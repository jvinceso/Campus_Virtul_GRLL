using Campus_Virtul_GRLL.Models.Enums;
using System.ComponentModel.DataAnnotations;

namespace Campus_Virtul_GRLL.Models
{
    /// <summary>
    /// Entidad que representa un curso en el Campus Virtual
    /// Es la entidad central del sistema
    /// </summary>
    public class Curso
    {
        // ============================================
        // PROPIEDADES PRINCIPALES
        // ============================================

        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "El código del curso es requerido")]
        [StringLength(20, ErrorMessage = "El código no puede exceder 20 caracteres")]
        public required string Codigo { get; set; }

        [Required(ErrorMessage = "El título del curso es requerido")]
        [StringLength(200, ErrorMessage = "El título no puede exceder 200 caracteres")]
        public required string Titulo { get; set; }

        [Required(ErrorMessage = "La descripción del curso es requerida")]
        [StringLength(2000, ErrorMessage = "La descripción no puede exceder 2000 caracteres")]
        public required string Descripcion { get; set; }

        [StringLength(1000, ErrorMessage = "Los objetivos no pueden exceder 1000 caracteres")]
        public string? Objetivos { get; set; }

        [Required(ErrorMessage = "La duración es requerida")]
        [Range(1, int.MaxValue, ErrorMessage = "La duración debe ser al menos 1 hora")]
        public int Duracion { get; set; } // En horas

        [Required(ErrorMessage = "La modalidad es requerida")]
        public Modalidad Modalidad { get; set; }

        [Required(ErrorMessage = "La categoría es requerida")]
        [StringLength(100, ErrorMessage = "La categoría no puede exceder 100 caracteres")]
        public required string Categoria { get; set; }

        [Required(ErrorMessage = "El nivel es requerido")]
        public NivelCurso Nivel { get; set; }

        [Required(ErrorMessage = "La capacidad máxima es requerida")]
        [Range(1, int.MaxValue, ErrorMessage = "La capacidad debe ser al menos 1 persona")]
        public int CapacidadMaxima { get; set; }

        [Required(ErrorMessage = "El estado es requerido")]
        public EstadoCurso Estado { get; set; }

        // ============================================
        // FECHAS
        // ============================================

        public DateTime? FechaInicio { get; set; }

        public DateTime? FechaFin { get; set; }

        public DateTime? FechaPublicacion { get; set; }

        // ============================================
        // MULTIMEDIA
        // ============================================

        [StringLength(500, ErrorMessage = "La URL de la imagen no puede exceder 500 caracteres")]
        [Url(ErrorMessage = "La URL de la imagen no es válida")]
        public string? ImagenUrl { get; set; }

        // ============================================
        // RELACIONES
        // ============================================

        /// <summary>
        /// ID del profesor asignado al curso (relación con Usuario)
        /// </summary>
        public int? ProfesorAsignado { get; set; }

        /// <summary>
        /// Navegación al profesor asignado
        /// </summary>
        public Usuario? Profesor { get; set; }

        /// <summary>
        /// ID del administrador que creó el curso
        /// </summary>
        [Required]
        public int CreadoPor { get; set; }

        /// <summary>
        /// Navegación al usuario que creó el curso
        /// </summary>
        public Usuario Creador { get; set; } = null!;

        // ============================================
        // AUDITORÍA
        // ============================================

        [Required]
        public DateTime FechaCreacion { get; set; } = DateTime.Now;

        public DateTime? FechaUltimaActualizacion { get; set; }

        // ============================================
        // SOFT DELETE
        // ============================================

        [Required]
        public bool EsEliminado { get; set; } = false;

        public DateTime? FechaEliminacion { get; set; }
    }
}
