namespace Campus_Virtul_GRLL.Utilities
{
    /// <summary>
    /// Validador de archivos para uploads
    /// </summary>
    public static class FileValidator
    {
        // Extensiones permitidas
        private static readonly string[] AllowedExtensions = {
            ".pdf", ".doc", ".docx", ".xls", ".xlsx", ".ppt", ".pptx",
            ".jpg", ".jpeg", ".png", ".gif",
            ".mp4", ".avi",
            ".zip", ".rar"
        };

        // Content types permitidos
        private static readonly string[] AllowedContentTypes = {
            "application/pdf",
            "application/msword",
            "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
            "application/vnd.ms-excel",
            "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
            "application/vnd.ms-powerpoint",
            "application/vnd.openxmlformats-officedocument.presentationml.presentation",
            "image/jpeg",
            "image/png",
            "image/gif",
            "video/mp4",
            "video/x-msvideo",
            "application/zip",
            "application/x-zip-compressed",
            "application/x-rar-compressed",
            "application/octet-stream" // Para algunos archivos comprimidos
        };

        // Tamaño máximo: 50 MB
        private const long MaxFileSize = 50 * 1024 * 1024;

        /// <summary>
        /// Valida un archivo subido
        /// </summary>
        public static (bool isValid, string error) ValidateFile(IFormFile? file)
        {
            if (file == null || file.Length == 0)
                return (false, "No se proporcionó ningún archivo");

            if (file.Length > MaxFileSize)
                return (false, $"El archivo es muy grande. Tamaño máximo: {MaxFileSize / (1024 * 1024)} MB");

            var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
            if (string.IsNullOrEmpty(extension))
                return (false, "El archivo no tiene extensión");

            if (!AllowedExtensions.Contains(extension))
                return (false, $"Tipo de archivo no permitido: {extension}");

            if (!AllowedContentTypes.Contains(file.ContentType.ToLowerInvariant()))
            {
                // Validación adicional para algunos tipos
                if (extension == ".rar" || extension == ".zip")
                {
                    // Permitir para archivos comprimidos aunque el content-type sea genérico
                    return (true, string.Empty);
                }
                return (false, $"Tipo de contenido no válido: {file.ContentType}");
            }

            return (true, string.Empty);
        }

        /// <summary>
        /// Obtiene el tamaño formateado de un archivo
        /// </summary>
        public static string FormatFileSize(long bytes)
        {
            string[] sizes = { "B", "KB", "MB", "GB" };
            double len = bytes;
            int order = 0;

            while (len >= 1024 && order < sizes.Length - 1)
            {
                order++;
                len = len / 1024;
            }

            return $"{len:0.##} {sizes[order]}";
        }

        /// <summary>
        /// Verifica si una extensión es una imagen
        /// </summary>
        public static bool IsImage(string extension)
        {
            string[] imageExtensions = { ".jpg", ".jpeg", ".png", ".gif" };
            return imageExtensions.Contains(extension.ToLowerInvariant());
        }

        /// <summary>
        /// Verifica si una extensión es un video
        /// </summary>
        public static bool IsVideo(string extension)
        {
            string[] videoExtensions = { ".mp4", ".avi" };
            return videoExtensions.Contains(extension.ToLowerInvariant());
        }

        /// <summary>
        /// Verifica si una extensión es un documento
        /// </summary>
        public static bool IsDocument(string extension)
        {
            string[] docExtensions = { ".pdf", ".doc", ".docx", ".xls", ".xlsx", ".ppt", ".pptx" };
            return docExtensions.Contains(extension.ToLowerInvariant());
        }
    }
}
