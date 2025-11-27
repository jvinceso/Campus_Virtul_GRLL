using Campus_Virtul_GRLL.Utilities;

namespace Campus_Virtul_GRLL.Services
{
    /// <summary>
    /// Implementación del servicio de almacenamiento de archivos
    /// </summary>
    public class FileStorageService : IFileStorageService
    {
        private readonly IWebHostEnvironment _environment;
        private readonly ILogger<FileStorageService> _logger;
        private const string UploadBasePath = "uploads/cursos";

        public FileStorageService(IWebHostEnvironment environment, ILogger<FileStorageService> logger)
        {
            _environment = environment;
            _logger = logger;
        }

        /// <summary>
        /// Guarda un archivo en el sistema de archivos
        /// </summary>
        public async Task<string> SaveFileAsync(IFormFile file, int cursoId)
        {
            try
            {
                // Generar nombre único
                var uniqueFileName = FileHelper.GenerateUniqueFileName(file.FileName);

                // Crear ruta relativa: uploads/cursos/{cursoId}/materiales/{uniqueFileName}
                var relativePath = Path.Combine(UploadBasePath, cursoId.ToString(), "materiales", uniqueFileName);

                // Obtener ruta física completa
                var physicalPath = Path.Combine(_environment.WebRootPath, relativePath);

                // Crear directorio si no existe
                var directory = Path.GetDirectoryName(physicalPath);
                if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                }

                // Guardar archivo
                using (var stream = new FileStream(physicalPath, FileMode.Create))
                {
                    await file.CopyToAsync(stream);
                }

                _logger.LogInformation($"✅ Archivo guardado: {relativePath}");

                // Retornar ruta relativa (para guardar en BD)
                return relativePath.Replace("\\", "/"); // Normalizar separadores
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"❌ Error al guardar archivo: {file.FileName}");
                throw new InvalidOperationException("Error al guardar el archivo", ex);
            }
        }

        /// <summary>
        /// Elimina un archivo del sistema de archivos
        /// </summary>
        public async Task DeleteFileAsync(string rutaArchivo)
        {
            try
            {
                var physicalPath = GetPhysicalPath(rutaArchivo);

                if (File.Exists(physicalPath))
                {
                    await Task.Run(() => File.Delete(physicalPath));
                    _logger.LogInformation($"✅ Archivo eliminado: {rutaArchivo}");
                }
                else
                {
                    _logger.LogWarning($"⚠️ Archivo no encontrado para eliminar: {rutaArchivo}");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"❌ Error al eliminar archivo: {rutaArchivo}");
                throw new InvalidOperationException("Error al eliminar el archivo", ex);
            }
        }

        /// <summary>
        /// Obtiene un stream del archivo
        /// </summary>
        public async Task<Stream?> GetFileStreamAsync(string rutaArchivo)
        {
            try
            {
                var physicalPath = GetPhysicalPath(rutaArchivo);

                if (!File.Exists(physicalPath))
                {
                    _logger.LogWarning($"⚠️ Archivo no encontrado: {rutaArchivo}");
                    return null;
                }

                var memory = new MemoryStream();
                using (var stream = new FileStream(physicalPath, FileMode.Open, FileAccess.Read))
                {
                    await stream.CopyToAsync(memory);
                }
                memory.Position = 0;

                return memory;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"❌ Error al leer archivo: {rutaArchivo}");
                return null;
            }
        }

        /// <summary>
        /// Verifica si un archivo existe
        /// </summary>
        public bool FileExists(string rutaArchivo)
        {
            var physicalPath = GetPhysicalPath(rutaArchivo);
            return File.Exists(physicalPath);
        }

        /// <summary>
        /// Obtiene la ruta física completa
        /// </summary>
        public string GetPhysicalPath(string rutaArchivo)
        {
            // Normalizar separadores
            rutaArchivo = rutaArchivo.Replace("/", "\\");
            return Path.Combine(_environment.WebRootPath, rutaArchivo);
        }
    }
}
