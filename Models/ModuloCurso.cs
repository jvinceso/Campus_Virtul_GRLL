namespace Campus_Virtul_GRLL.Models
{
    /// <summary>
    /// Módulo o sección dentro de un curso para organizar materiales
    /// </summary>
    public class ModuloCurso
    {
        public int IdModuloCurso { get; set; }

        // Relación con Curso
        public int IdCurso { get; set; }
        public Curso Curso { get; set; } = null!;

        // Información del Módulo
        public required string Titulo { get; set; }
        public string? Descripcion { get; set; }
        public int Orden { get; set; }

        // Materiales del módulo
        public ICollection<Material> Materiales { get; set; } = new List<Material>();

        // Metadatos
        public DateTime FechaCreacion { get; set; }

        // Soft delete
        public bool EsEliminado { get; set; }
        public DateTime? FechaEliminacion { get; set; }
    }
}
