using Campus_Virtul_GRLL.Models.Enums;

namespace Campus_Virtul_GRLL.Models
{
    /// <summary>
    /// Material educativo de un curso (archivos o enlaces)
    /// </summary>
    public class Material
    {
        public int IdMaterial { get; set; }

        // Relación con Curso
        public int IdCurso { get; set; }
        public Curso Curso { get; set; } = null!;

        // Información del Material
        public required string Titulo { get; set; }
        public string? Descripcion { get; set; }
        public TipoMaterial Tipo { get; set; }

        // Archivo (para materiales subidos)
        public string? NombreArchivo { get; set; }        // Nombre original del archivo
        public string? RutaArchivo { get; set; }          // Ruta relativa en servidor
        public string? Extension { get; set; }            // .pdf, .docx, etc.
        public long? TamanoBytes { get; set; }

        // URL Externa (para videos de YouTube, enlaces, etc.)
        public string? UrlExterna { get; set; }

        // Organización
        public int? IdModuloCurso { get; set; }
        public ModuloCurso? ModuloCurso { get; set; }
        public int Orden { get; set; }                    // Orden dentro del curso/módulo

        // Visibilidad
        public bool EsVisible { get; set; }               // Mostrar/Ocultar a estudiantes

        // Metadatos
        public DateTime FechaSubida { get; set; }
        public int SubidoPorId { get; set; }
        public Usuario SubidoPor { get; set; } = null!;

        // Estadísticas
        public int NumeroDescargas { get; set; }
        public DateTime? FechaUltimaDescarga { get; set; }

        // Soft delete
        public bool EsEliminado { get; set; }
        public DateTime? FechaEliminacion { get; set; }
        public int? EliminadoPorId { get; set; }
        public Usuario? EliminadoPor { get; set; }
    }
}
