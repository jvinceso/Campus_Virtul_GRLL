namespace Campus_Virtul_GRLL.Models.Enums
{
    /// <summary>
    /// Tipos de materiales educativos soportados
    /// </summary>
    public enum TipoMaterial
    {
        PDF = 1,
        Documento = 2,        // Word, Excel
        Presentacion = 3,     // PowerPoint
        Video = 4,            // MP4, o link de YouTube
        Imagen = 5,
        Enlace = 6,           // Link externo
        Otro = 99
    }
}
