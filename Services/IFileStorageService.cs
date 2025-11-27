namespace Campus_Virtul_GRLL.Services
{
    /// <summary>
    /// Servicio para almacenamiento de archivos
    /// </summary>
    public interface IFileStorageService
    {
        /// <summary>
        /// Guarda un archivo en el sistema de archivos
        /// </summary>
        /// <param name="file">Archivo a guardar</param>
        /// <param name="cursoId">ID del curso</param>
        /// <returns>Ruta relativa del archivo guardado</returns>
        Task<string> SaveFileAsync(IFormFile file, int cursoId);

        /// <summary>
        /// Elimina un archivo del sistema de archivos
        /// </summary>
        /// <param name="rutaArchivo">Ruta relativa del archivo</param>
        Task DeleteFileAsync(string rutaArchivo);

        /// <summary>
        /// Obtiene un archivo del sistema de archivos
        /// </summary>
        /// <param name="rutaArchivo">Ruta relativa del archivo</param>
        /// <returns>Stream del archivo</returns>
        Task<Stream?> GetFileStreamAsync(string rutaArchivo);

        /// <summary>
        /// Verifica si un archivo existe
        /// </summary>
        /// <param name="rutaArchivo">Ruta relativa del archivo</param>
        bool FileExists(string rutaArchivo);

        /// <summary>
        /// Obtiene la ruta física completa de un archivo
        /// </summary>
        /// <param name="rutaArchivo">Ruta relativa del archivo</param>
        string GetPhysicalPath(string rutaArchivo);
    }
}
