namespace ImageCatalog.Api.Services;

/// <summary>
/// Abstraction for image processing libraries to support multiple implementations
/// </summary>
public interface IImageProcessor
{
    /// <summary>
    /// Determines if this processor can handle the given file extension
    /// </summary>
    bool CanProcess(string fileExtension);

    /// <summary>
    /// Converts an image to JPEG format without resizing (for upload conversion)
    /// </summary>
    /// <param name="imageStream">Input image stream</param>
    /// <param name="quality">JPEG quality (1-100), default 95 for high quality originals</param>
    /// <returns>JPEG stream and original dimensions</returns>
    Task<(Stream outputStream, int width, int height)> ConvertToJpegAsync(
        Stream imageStream, 
        int quality = 95);

    /// <summary>
    /// Resizes an image and returns it as a JPEG stream (for thumbnail generation)
    /// </summary>
    /// <param name="imageStream">Input image stream</param>
    /// <param name="targetWidth">Target width for the thumbnail</param>
    /// <param name="quality">JPEG quality (1-100)</param>
    /// <returns>Tuple containing the output stream and dimensions (width, height)</returns>
    Task<(Stream outputStream, int width, int height)> ResizeToJpegAsync(
        Stream imageStream, 
        int targetWidth, 
        int quality = 75);
}
