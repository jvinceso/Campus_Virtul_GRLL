namespace Campus_Virtul_GRLL.Models.ViewModels
{
    /// <summary>
    /// ViewModel para la lista de profesores con sus cursos asignados
    /// </summary>
    public class ProfesoresIndexViewModel
    {
        // ============================================
        // LISTA DE PROFESORES
        // ============================================
        public List<ProfesorCardViewModel> Profesores { get; set; } = new List<ProfesorCardViewModel>();

        // ============================================
        // ESTADÍSTICAS GENERALES
        // ============================================
        public int TotalProfesores { get; set; }
        public int ProfesoresActivos { get; set; }
        public int ProfesoresConCursos { get; set; }
        public int ProfesoresSinCursos { get; set; }

        // ============================================
        // FILTROS
        // ============================================
        public string? BusquedaTexto { get; set; }
        public bool? SoloActivos { get; set; }
        public bool? SoloConCursos { get; set; }
        public string? FiltroArea { get; set; }
        public string? Ordenamiento { get; set; } // "nombre", "cursos", "area"

        // ============================================
        // PAGINACIÓN
        // ============================================
        public int PaginaActual { get; set; } = 1;
        public int ProfesoresPorPagina { get; set; } = 12;
        public int TotalPaginas { get; set; }

        // ============================================
        // DATOS PARA FILTROS
        // ============================================
        public List<string> AreasDisponibles { get; set; } = new List<string>();
    }

    /// <summary>
    /// Representa un profesor en formato de tarjeta
    /// </summary>
    public class ProfesorCardViewModel
    {
        public int IdUsuario { get; set; }
        public string Nombres { get; set; } = string.Empty;
        public string Apellidos { get; set; } = string.Empty;
        public string NombreCompleto => $"{Nombres} {Apellidos}";
        public string Email { get; set; } = string.Empty;
        public string Area { get; set; } = string.Empty;
        public string Telefono { get; set; } = string.Empty;
        public bool Estado { get; set; }

        // Estadísticas de cursos
        public int TotalCursosAsignados { get; set; }
        public int CursosActivos { get; set; } // Publicados + EnCurso
        public int CursosFinalizados { get; set; }
        public int HorasCargaTrabajo { get; set; } // Total horas de cursos activos
    }
}
