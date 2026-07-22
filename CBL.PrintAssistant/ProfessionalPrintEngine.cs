using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Printing;
using System.Text;

namespace CBL.PrintAssistant
{
    public sealed class ProfessionalPrintEngine
    {
        public Task PrintFromUrlAsync(
            string imageUrl,
            EffectivePrintSettings settings,
            bool stripMode,
            CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(imageUrl))
                throw new ArgumentException("A URL da imagem é obrigatória.", nameof(imageUrl));

            return DownloadAndPrintAsync(imageUrl, settings, stripMode, cancellationToken);
        }

        private static async Task DownloadAndPrintAsync(
            string imageUrl,
            EffectivePrintSettings settings,
            bool stripMode,
            CancellationToken cancellationToken)
        {
            string tempFile = Path.Combine(Path.GetTempPath(), $"cbl_print_v2_{Guid.NewGuid():N}.img");

            try
            {
                using var client = new HttpClient { Timeout = TimeSpan.FromSeconds(45) };
                using var response = await client.GetAsync(
                    imageUrl,
                    HttpCompletionOption.ResponseHeadersRead,
                    cancellationToken);
                response.EnsureSuccessStatusCode();

                await using (var output = new FileStream(
                    tempFile,
                    FileMode.CreateNew,
                    FileAccess.Write,
                    FileShare.None,
                    81920,
                    useAsync: true))
                {
                    await response.Content.CopyToAsync(output, cancellationToken);
                }

                cancellationToken.ThrowIfCancellationRequested();
                PrintFile(tempFile, settings, stripMode);
            }
            finally
            {
                try
                {
                    if (File.Exists(tempFile)) File.Delete(tempFile);
                }
                catch
                {
                }
            }
        }

        public static void PrintFile(
            string imagePath,
            EffectivePrintSettings settings,
            bool stripMode)
        {
            Validate(settings);

            using Image source = Image.FromFile(imagePath);
            using Bitmap rotated = new Bitmap(source);
            ApplyRotation(rotated, settings.RotationMode);
            rotated.SetResolution(settings.Dpi, settings.Dpi);

            using Bitmap printable = stripMode
                ? CreateStripSheet(rotated, settings.Dpi)
                : new Bitmap(rotated);
            printable.SetResolution(settings.Dpi, settings.Dpi);

            using var document = new PrintDocument();
            document.PrinterSettings.PrinterName = settings.PrinterName;
            if (!document.PrinterSettings.IsValid)
                throw new InvalidOperationException($"Impressora inválida ou indisponível: {settings.PrinterName}");

            PaperSize? paper = FindPaperSize(
                document,
                settings.PaperName,
                settings.FallbackPaperName);

            if (paper is null)
            {
                throw new InvalidOperationException(
                    $"Papel não encontrado na impressora. Solicitado: {settings.PaperName}; " +
                    $"fallback local: {settings.FallbackPaperName}.");
            }

            document.DefaultPageSettings.PaperSize = paper;
            document.DefaultPageSettings.Landscape = ResolveLandscape(settings.Orientation, printable);
            document.DefaultPageSettings.Margins = new Margins(
                settings.MarginLeft,
                settings.MarginRight,
                settings.MarginTop,
                settings.MarginBottom);
            document.OriginAtMargins = false;

            document.PrintPage += (_, args) =>
            {
                ConfigureGraphics(args.Graphics);
                Rectangle target = CalculateTargetRectangle(args, printable, settings);
                args.Graphics.DrawImage(printable, target);
                args.HasMorePages = false;
            };

            document.Print();
        }

        private static void Validate(EffectivePrintSettings settings)
        {
            if (string.IsNullOrWhiteSpace(settings.PrinterName))
                throw new InvalidOperationException("Nenhuma impressora foi definida.");
            if (string.IsNullOrWhiteSpace(settings.PaperName) &&
                string.IsNullOrWhiteSpace(settings.FallbackPaperName))
                throw new InvalidOperationException("Nenhum papel foi definido.");
            if (settings.Dpi is < 72 or > 1200)
                throw new InvalidOperationException("DPI fora do intervalo permitido (72–1200).");
            if (settings.Bleed is < -500 or > 500)
                throw new InvalidOperationException("Sangria fora do intervalo permitido.");
            if (settings.OffsetX is < -1000 or > 1000 || settings.OffsetY is < -1000 or > 1000)
                throw new InvalidOperationException("Offset fora do intervalo permitido.");
        }

        private static bool ResolveLandscape(string orientation, Image image)
        {
            if (orientation.Equals("Landscape", StringComparison.OrdinalIgnoreCase) ||
                orientation.Equals("Paisagem", StringComparison.OrdinalIgnoreCase))
                return true;

            if (orientation.Equals("Portrait", StringComparison.OrdinalIgnoreCase) ||
                orientation.Equals("Retrato", StringComparison.OrdinalIgnoreCase))
                return false;

            return image.Width >= image.Height;
        }

        private static Rectangle CalculateTargetRectangle(
            PrintPageEventArgs args,
            Image image,
            EffectivePrintSettings settings)
        {
            int hardX = (int)Math.Round(args.PageSettings.HardMarginX);
            int hardY = (int)Math.Round(args.PageSettings.HardMarginY);

            int left = -hardX + settings.MarginLeft - settings.Bleed + settings.OffsetX;
            int top = -hardY + settings.MarginTop - settings.Bleed + settings.OffsetY;
            int width = args.PageBounds.Width + (hardX * 2)
                - settings.MarginLeft - settings.MarginRight + (settings.Bleed * 2);
            int height = args.PageBounds.Height + (hardY * 2)
                - settings.MarginTop - settings.MarginBottom + (settings.Bleed * 2);

            if (width <= 0 || height <= 0)
                throw new InvalidOperationException("Margens e sangria resultaram em uma área de impressão inválida.");

            var bounds = new Rectangle(left, top, width, height);
            string fit = settings.FitMode.Trim();

            if (fit.Equals("Stretch", StringComparison.OrdinalIgnoreCase) ||
                fit.Equals("Esticar", StringComparison.OrdinalIgnoreCase))
                return bounds;

            float imageRatio = (float)image.Width / image.Height;
            float boundsRatio = (float)bounds.Width / bounds.Height;
            bool cover = fit.Equals("Cover", StringComparison.OrdinalIgnoreCase) ||
                         fit.Equals("Preencher", StringComparison.OrdinalIgnoreCase);

            int drawWidth;
            int drawHeight;
            if ((cover && imageRatio > boundsRatio) || (!cover && imageRatio < boundsRatio))
            {
                drawHeight = bounds.Height;
                drawWidth = (int)Math.Round(bounds.Height * imageRatio);
            }
            else
            {
                drawWidth = bounds.Width;
                drawHeight = (int)Math.Round(bounds.Width / imageRatio);
            }

            return new Rectangle(
                bounds.X + ((bounds.Width - drawWidth) / 2),
                bounds.Y + ((bounds.Height - drawHeight) / 2),
                drawWidth,
                drawHeight);
        }

        private static void ConfigureGraphics(Graphics graphics)
        {
            graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
            graphics.SmoothingMode = SmoothingMode.HighQuality;
            graphics.PixelOffsetMode = PixelOffsetMode.HighQuality;
            graphics.CompositingQuality = CompositingQuality.HighQuality;
        }

        private static PaperSize? FindPaperSize(
            PrintDocument document,
            string requestedPaperName,
            string fallbackPaperName)
        {
            PaperSize[] papers = document.PrinterSettings.PaperSizes
                .Cast<PaperSize>()
                .ToArray();

            PaperSize? requested = FindPaperByNameOrAlias(papers, requestedPaperName);
            if (requested is not null)
                return requested;

            if (!string.Equals(
                    requestedPaperName,
                    fallbackPaperName,
                    StringComparison.OrdinalIgnoreCase))
            {
                PaperSize? fallback = FindPaperByNameOrAlias(papers, fallbackPaperName);
                if (fallback is not null)
                    return fallback;
            }

            return null;
        }

        private static PaperSize? FindPaperByNameOrAlias(
            IEnumerable<PaperSize> papers,
            string paperName)
        {
            if (string.IsNullOrWhiteSpace(paperName))
                return null;

            string normalizedRequest = NormalizePaperName(paperName);
            PaperSize[] candidates = papers.ToArray();

            foreach (PaperSize paper in candidates)
            {
                if (paper.PaperName.Equals(paperName, StringComparison.OrdinalIgnoreCase))
                    return paper;
            }

            foreach (PaperSize paper in candidates)
            {
                string normalizedPaper = NormalizePaperName(paper.PaperName);
                if (normalizedPaper == normalizedRequest)
                    return paper;
            }

            foreach (PaperSize paper in candidates)
            {
                string normalizedPaper = NormalizePaperName(paper.PaperName);
                if (IsSafeAliasMatch(normalizedRequest, normalizedPaper))
                    return paper;
            }

            return null;
        }

        private static bool IsSafeAliasMatch(string requested, string installed)
        {
            if (string.IsNullOrWhiteSpace(requested) || string.IsNullOrWhiteSpace(installed))
                return false;

            if (requested == "6x4" || requested == "4x6")
            {
                return installed.StartsWith("6x4", StringComparison.Ordinal) ||
                       installed.StartsWith("4x6", StringComparison.Ordinal) ||
                       installed.Contains("152x102", StringComparison.Ordinal) ||
                       installed.Contains("102x152", StringComparison.Ordinal);
            }

            if (requested == "6x2type1" || requested == "6x2x2type1")
            {
                return installed.Contains("6x2", StringComparison.Ordinal) &&
                       installed.Contains("type1", StringComparison.Ordinal);
            }

            if (requested == "a4" || requested == "isoa4")
            {
                return installed == "a4" ||
                       installed.StartsWith("isoa4", StringComparison.Ordinal) ||
                       installed.Contains("210x297", StringComparison.Ordinal) ||
                       installed.Contains("297x210", StringComparison.Ordinal);
            }

            if (requested == "a3" || requested == "isoa3")
            {
                return installed == "a3" ||
                       installed.StartsWith("isoa3", StringComparison.Ordinal) ||
                       installed.Contains("297x420", StringComparison.Ordinal) ||
                       installed.Contains("420x297", StringComparison.Ordinal);
            }

            return false;
        }

        private static string NormalizePaperName(string value)
        {
            string normalized = value.Trim().ToLowerInvariant();
            var builder = new StringBuilder(normalized.Length);

            foreach (char c in normalized)
            {
                if (char.IsLetterOrDigit(c))
                    builder.Append(c);
                else if (c == '×')
                    builder.Append('x');
            }

            return builder.ToString();
        }

        private static void ApplyRotation(Bitmap bitmap, string rotation)
        {
            switch (rotation.Trim().ToLowerInvariant())
            {
                case "90":
                case "90°":
                    bitmap.RotateFlip(RotateFlipType.Rotate90FlipNone);
                    break;
                case "180":
                case "180°":
                    bitmap.RotateFlip(RotateFlipType.Rotate180FlipNone);
                    break;
                case "270":
                case "270°":
                    bitmap.RotateFlip(RotateFlipType.Rotate270FlipNone);
                    break;
            }
        }

        private static Bitmap CreateStripSheet(Bitmap source, int dpi)
        {
            const int canvasWidth = 1800;
            const int canvasHeight = 1200;
            const int gap = 12;

            var canvas = new Bitmap(canvasWidth, canvasHeight);
            canvas.SetResolution(dpi, dpi);

            using Graphics graphics = Graphics.FromImage(canvas);
            graphics.Clear(Color.White);
            ConfigureGraphics(graphics);

            using var strip = new Bitmap(source);
            if (strip.Height >= strip.Width)
                strip.RotateFlip(RotateFlipType.Rotate90FlipNone);

            var top = new Rectangle(0, 0, canvasWidth, (canvasHeight / 2) - (gap / 2));
            var bottom = new Rectangle(0, (canvasHeight / 2) + (gap / 2), canvasWidth, (canvasHeight / 2) - (gap / 2));
            DrawContain(graphics, strip, top);
            DrawContain(graphics, strip, bottom);
            return canvas;
        }

        private static void DrawContain(Graphics graphics, Image image, Rectangle bounds)
        {
            float imageRatio = (float)image.Width / image.Height;
            float boundsRatio = (float)bounds.Width / bounds.Height;

            int width;
            int height;
            if (imageRatio > boundsRatio)
            {
                width = bounds.Width;
                height = (int)Math.Round(bounds.Width / imageRatio);
            }
            else
            {
                height = bounds.Height;
                width = (int)Math.Round(bounds.Height * imageRatio);
            }

            var target = new Rectangle(
                bounds.X + ((bounds.Width - width) / 2),
                bounds.Y + ((bounds.Height - height) / 2),
                width,
                height);
            graphics.DrawImage(image, target);
        }
    }
}