using System.ComponentModel.DataAnnotations;

namespace Campus_Virtul_GRLL.Models.ViewModels
{
    /// <summary>
    /// ViewModel para asignar o cambiar el profesor de un curso
    /// </summary>
    public class AsignarProfesorViewModel
    {
        // ============================================
        // INFORMACIÓN DEL CURSO
        // ============================================
        [Required]
        public int CursoId { get; set; }
        public string CodigoCurso { get; set; } = string.Empty;
        public string TituloCurso { get; set; } = string.Empty;
        public string CategoriaCurso { get; set; } = string.Empty;

        // ============================================
        // PROFESOR ACTUAL (si existe)
        // ============================================
        public int? ProfesorActualId { get; set; }
        public string? NombreProfesorActual { get; set; }
        public DateTime? FechaAsignacionActual { get; set; }

        // ============================================
        // NUEVO PROFESOR
        // ============================================
        [Display(Name = "Profesor")]
        public int? NuevoProfesorId { get; set; }

        [Display(Name = "Comentario / Motivo")]
        [StringLength(500, ErrorMessage = "El comentario no puede exceder 500 caracteres")]
        public string? Comentario { get; set; }

        // ============================================
        // LISTA DE PROFESORES DISPONIBLES
        // ============================================
        public List<ProfesorDisponibleItem> ProfesoresDisponibles { get; set; } = new List<ProfesorDisponibleItem>();
    }

    /// <summary>
    /// Representa un profesor disponible para asignar
    /// </summary>
    public class ProfesorDisponibleItem
    {
        public int IdUsuario { get; set; }
        public string NombreCompleto { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Area { get; set; } = string.Empty;
        public int CursosAsignados { get; set; }
        public int HorasCargaTrabajo { get; set; } // Suma de duración de cursos activos
    }
}
