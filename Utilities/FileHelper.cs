namespace Campus_Virtul_GRLL.Utilities
{
    /// <summary>
    /// Helper para operaciones con archivos
    /// </summary>
    public static class FileHelper
    {
        /// <summary>
        /// Sanitiza un nombre de archivo removiendo caracteres peligrosos
        /// </summary>
        public static string SanitizeFileName(string fileName)
        {
            if (string.IsNullOrWhiteSpace(fileName))
                return "archivo";

            // Remover caracteres inválidos
            var invalidChars = Path.GetInvalidFileNameChars();
            var sanitized = string.Join("_", fileName.Split(invalidChars, StringSplitOptions.RemoveEmptyEntries));

            // Remover espacios múltiples y caracteres especiales adicionales
            sanitized = System.Text.RegularExpressions.Regex.Replace(sanitized, @"\s+", "_");
            sanitized = System.Text.RegularExpressions.Regex.Replace(sanitized, @"[^\w\.-]", "_");

            // Limitar longitud
            if (sanitized.Length > 100)
            {
                var extension = Path.GetExtension(sanitized);
                var nameWithoutExt = Path.GetFileNameWithoutExtension(sanitized);
                sanitized = nameWithoutExt.Substring(0, Math.Min(100 - extension.Length, nameWithoutExt.Length)) + extension;
            }

            return sanitized;
        }

        /// <summary>
        /// Genera un nombre único para un archivo usando GUID
        /// </summary>
        public static string GenerateUniqueFileName(string originalFileName)
        {
            var extension = Path.GetExtension(originalFileName).ToLowerInvariant();
            var uniqueName = $"{Guid.NewGuid()}{extension}";
            return uniqueName;
        }

        /// <summary>
        /// Obtiene el icono de Bootstrap Icons según la extensión
        /// </summary>
        public static string GetIconByExtension(string? extension)
        {
            if (string.IsNullOrEmpty(extension))
                return "bi-file-earmark";

            return extension.ToLowerInvariant() switch
            {
                ".pdf" => "bi-file-earmark-pdf-fill text-danger",
                ".doc" or ".docx" => "bi-file-earmark-word-fill text-primary",
                ".xls" or ".xlsx" => "bi-file-earmark-excel-fill text-success",
                ".ppt" or ".pptx" => "bi-file-earmark-ppt-fill text-warning",
                ".jpg" or ".jpeg" or ".png" or ".gif" => "bi-file-earmark-image-fill text-info",
                ".mp4" or ".avi" => "bi-file-earmark-play-fill text-danger",
                ".zip" or ".rar" => "bi-file-earmark-zip-fill text-secondary",
                _ => "bi-file-earmark"
            };
        }

        /// <summary>
        /// Obtiene la clase de color de Bootstrap según el tipo de archivo
        /// </summary>
        public static string GetColorClassByExtension(string? extension)
        {
            if (string.IsNullOrEmpty(extension))
                return "secondary";

            return extension.ToLowerInvariant() switch
            {
                ".pdf" => "danger",
                ".doc" or ".docx" => "primary",
                ".xls" or ".xlsx" => "success",
                ".ppt" or ".pptx" => "warning",
                ".jpg" or ".jpeg" or ".png" or ".gif" => "info",
                ".mp4" or ".avi" => "danger",
                ".zip" or ".rar" => "secondary",
                _ => "secondary"
            };
        }

        /// <summary>
        /// Valida una URL externa
        /// </summary>
        public static (bool isValid, string error) ValidateUrl(string? url)
        {
            if (string.IsNullOrWhiteSpace(url))
                return (false, "La URL no puede estar vacía");

            if (!Uri.TryCreate(url, UriKind.Absolute, out var uriResult))
                return (false, "La URL no tiene un formato válido");

            if (uriResult.Scheme != Uri.UriSchemeHttp && uriResult.Scheme != Uri.UriSchemeHttps)
                return (false, "La URL debe comenzar con http:// o https://");

            return (true, string.Empty);
        }

        /// <summary>
        /// Extrae el ID de video de una URL de YouTube
        /// </summary>
        public static string? ExtractYouTubeVideoId(string url)
        {
            if (string.IsNullOrWhiteSpace(url))
                return null;

            // Patrón para URLs de YouTube
            var patterns = new[]
            {
                @"(?:youtube\.com\/watch\?v=|youtu\.be\/)([a-zA-Z0-9_-]{11})",
                @"youtube\.com\/embed\/([a-zA-Z0-9_-]{11})"
            };

            foreach (var pattern in patterns)
            {
                var match = System.Text.RegularExpressions.Regex.Match(url, pattern);
                if (match.Success)
                    return match.Groups[1].Value;
            }

            return null;
        }

        /// <summary>
        /// Verifica si una URL es de YouTube
        /// </summary>
        public static bool IsYouTubeUrl(string? url)
        {
            if (string.IsNullOrWhiteSpace(url))
                return false;

            return url.Contains("youtube.com") || url.Contains("youtu.be");
        }
    }
}
