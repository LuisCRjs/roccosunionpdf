using DocumentManager.Core.Services.Interfaces;
using PdfSharp.Drawing;
using PdfSharp.Pdf;
using PdfSharp.Pdf.IO;

namespace DocumentManager.Infrastructure.Services;

public sealed class PdfService : IPdfService
{
    private static readonly HashSet<string> SupportedImageExtensions =
        new(StringComparer.OrdinalIgnoreCase) { ".jpg", ".jpeg", ".png", ".bmp" };

    public Task ValidatePdfAsync(string path, CancellationToken cancellationToken = default) =>
        Task.Run(() => ValidatePdf(path, cancellationToken), cancellationToken);

    public Task ConvertImagesToPdfAsync(
        IReadOnlyList<string> imagePaths,
        string destinationPdfPath,
        CancellationToken cancellationToken = default) =>
        Task.Run(
            () => ConvertImagesToPdf(imagePaths, destinationPdfPath, cancellationToken),
            cancellationToken);

    public Task MergeAsync(
        IReadOnlyList<string> sourcePdfPaths,
        string destinationPdfPath,
        CancellationToken cancellationToken = default) =>
        Task.Run(() => Merge(sourcePdfPaths, destinationPdfPath, cancellationToken), cancellationToken);

    private static void ValidatePdf(string path, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        EnsureExistingFile(path, ".pdf");

        using var document = PdfReader.Open(path, PdfDocumentOpenMode.Import);
        if (document.PageCount == 0)
        {
            throw new InvalidDataException("El PDF no contiene páginas.");
        }
    }

    private static void ConvertImagesToPdf(
        IReadOnlyList<string> imagePaths,
        string destinationPdfPath,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(imagePaths);
        if (imagePaths.Count == 0)
        {
            throw new ArgumentException("Se requiere al menos una imagen.", nameof(imagePaths));
        }

        EnsureDestinationIsAvailable(destinationPdfPath);
        var partialPath = CreatePartialPath(destinationPdfPath);

        try
        {
            using var output = new PdfDocument();
            foreach (var imagePath in imagePaths)
            {
                cancellationToken.ThrowIfCancellationRequested();
                EnsureSupportedImage(imagePath);

                using var image = XImage.FromFile(imagePath);
                var page = output.AddPage();
                page.Width = XUnit.FromPoint(image.PointWidth);
                page.Height = XUnit.FromPoint(image.PointHeight);

                using var graphics = XGraphics.FromPdfPage(page);
                graphics.DrawImage(image, 0, 0, page.Width.Point, page.Height.Point);
            }

            output.Save(partialPath);
            File.Move(partialPath, destinationPdfPath, overwrite: false);
        }
        finally
        {
            DeletePartialFile(partialPath);
        }
    }

    private static void Merge(
        IReadOnlyList<string> sourcePdfPaths,
        string destinationPdfPath,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(sourcePdfPaths);
        if (sourcePdfPaths.Count == 0)
        {
            throw new ArgumentException("Se requiere al menos un PDF.", nameof(sourcePdfPaths));
        }

        EnsureDestinationIsAvailable(destinationPdfPath);
        var partialPath = CreatePartialPath(destinationPdfPath);

        try
        {
            using var output = new PdfDocument();
            foreach (var sourcePath in sourcePdfPaths)
            {
                cancellationToken.ThrowIfCancellationRequested();
                EnsureExistingFile(sourcePath, ".pdf");

                using var input = PdfReader.Open(sourcePath, PdfDocumentOpenMode.Import);
                if (input.PageCount == 0)
                {
                    throw new InvalidDataException($"El archivo '{Path.GetFileName(sourcePath)}' no contiene páginas.");
                }

                for (var pageIndex = 0; pageIndex < input.PageCount; pageIndex++)
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    output.AddPage(input.Pages[pageIndex]);
                }
            }

            output.Save(partialPath);
            File.Move(partialPath, destinationPdfPath, overwrite: false);
        }
        finally
        {
            DeletePartialFile(partialPath);
        }
    }

    private static void EnsureExistingFile(string path, string requiredExtension)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(path);
        if (!File.Exists(path))
        {
            throw new FileNotFoundException("No se encontró el archivo seleccionado.", path);
        }

        if (!string.Equals(Path.GetExtension(path), requiredExtension, StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidDataException($"El archivo debe tener extensión {requiredExtension}.");
        }
    }

    private static void EnsureSupportedImage(string path)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(path);
        if (!File.Exists(path))
        {
            throw new FileNotFoundException("No se encontró la imagen escaneada.", path);
        }

        if (!SupportedImageExtensions.Contains(Path.GetExtension(path)))
        {
            throw new InvalidDataException("La imagen debe estar en formato JPG, PNG o BMP.");
        }
    }

    private static void EnsureDestinationIsAvailable(string destinationPath)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(destinationPath);
        var directory = Path.GetDirectoryName(Path.GetFullPath(destinationPath));
        if (string.IsNullOrWhiteSpace(directory))
        {
            throw new ArgumentException("La ruta de destino no es válida.", nameof(destinationPath));
        }

        Directory.CreateDirectory(directory);
        if (File.Exists(destinationPath))
        {
            throw new IOException("Ya existe un expediente con el mismo nombre.");
        }
    }

    private static string CreatePartialPath(string destinationPath) =>
        $"{destinationPath}.{Guid.NewGuid():N}.partial";

    private static void DeletePartialFile(string path)
    {
        try
        {
            if (File.Exists(path))
            {
                File.Delete(path);
            }
        }
        catch (IOException)
        {
            // No se oculta el error original de generación por un fallo de limpieza.
        }
    }
}

