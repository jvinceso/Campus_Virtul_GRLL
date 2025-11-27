using Campus_Virtul_GRLL.Models.Enums;

namespace Campus_Virtul_GRLL.Models.ViewModels
{
    /// <summary>
    /// ViewModel para mostrar los cursos asignados a un profesor
    /// </summary>
    public class ProfesorCursosViewModel
    {
        // ============================================
        // INFORMACIÓN DEL PROFESOR
        // ============================================
        public int ProfesorId { get; set; }
        public string NombreCompleto { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Area { get; set; } = string.Empty;
        public string Telefono { get; set; } = string.Empty;
        public bool Estado { get; set; }

        // ============================================
        // CURSOS ASIGNADOS
        // ============================================
        public List<CursoAsignadoViewModel> Cursos { get; set; } = new List<CursoAsignadoViewModel>();

        // ============================================
        // ESTADÍSTICAS
        // ============================================
        public int TotalCursosAsignados { get; set; }
        public int CursosActivos { get; set; } // Publicados + EnCurso
        public int CursosEnCurso { get; set; }
        public int CursosFinalizados { get; set; }
        public int HorasTotales { get; set; }

        // ============================================
        // FILTROS
        // ============================================
        public EstadoCurso? FiltroEstado { get; set; }
        public string? Ordenamiento { get; set; }

        // ============================================
        // CONFIGURACIÓN
        // ============================================
        public bool EsVistaAdmin { get; set; } // True si es admin viendo, False si es el profesor
    }

    /// <summary>
    /// Representa un curso asignado al profesor
    /// </summary>
    public class CursoAsignadoViewModel
    {
        public int Id { get; set; }
        public string Codigo { get; set; } = string.Empty;
        public string Titulo { get; set; } = string.Empty;
        public string Categoria { get; set; } = string.Empty;
        public EstadoCurso Estado { get; set; }
        public NivelCurso Nivel { get; set; }
        public Modalidad Modalidad { get; set; }
        public int Duracion { get; set; }
        public int CapacidadMaxima { get; set; }
        public DateTime? FechaInicio { get; set; }
        public DateTime? FechaFin { get; set; }
        public DateTime? FechaAsignacion { get; set; }
        public int? NumeroInscritos { get; set; } // Futuro
        public int? NumeroMateriales { get; set; } // Futuro
    }
}
