using Campus_Virtul_GRLL.Models.Enums;

namespace Campus_Virtul_GRLL.Models.ViewModels
{
    /// <summary>
    /// ViewModel para la vista Index con lista de cursos y filtros
    /// </summary>
    public class CursoIndexViewModel
    {
        // ============================================
        // LISTA DE CURSOS
        // ============================================
        public List<CursoCardViewModel> Cursos { get; set; } = new List<CursoCardViewModel>();

        // ============================================
        // ESTADÍSTICAS
        // ============================================
        public int TotalCursos { get; set; }
        public int CursosPublicados { get; set; }
        public int CursosEnCurso { get; set; }
        public int CursosFinalizados { get; set; }

        // ============================================
        // FILTROS
        // ============================================
        public string? BusquedaTexto { get; set; }
        public EstadoCurso? FiltroEstado { get; set; }
        public Modalidad? FiltroModalidad { get; set; }
        public string? FiltroCategoria { get; set; }
        public NivelCurso? FiltroNivel { get; set; }
        public string? Ordenamiento { get; set; } // "recientes", "alfabetico", "proximosIniciar"

        // ============================================
        // PAGINACIÓN
        // ============================================
        public int PaginaActual { get; set; } = 1;
        public int CursosPorPagina { get; set; } = 9;
        public int TotalPaginas { get; set; }

        // ============================================
        // DATOS PARA FILTROS
        // ============================================
        public List<string> CategoriasDisponibles { get; set; } = new List<string>
        {
            "Gestión y Administración Pública",
            "Tecnologías de la Información",
            "Desarrollo Personal y Liderazgo",
            "Gestión de Proyectos",
            "Recursos Humanos",
            "Finanzas y Presupuesto",
            "Comunicación y Atención al Ciudadano",
            "Medio Ambiente y Recursos Naturales",
            "Infraestructura y Obras Públicas",
            "Legal y Normativa"
        };
    }

    /// <summary>
    /// ViewModel para mostrar un curso en formato de tarjeta (card)
    /// </summary>
    public class CursoCardViewModel
    {
        public int Id { get; set; }
        public string Codigo { get; set; } = string.Empty;
        public string Titulo { get; set; } = string.Empty;
        public string Categoria { get; set; } = string.Empty;
        public NivelCurso Nivel { get; set; }
        public EstadoCurso Estado { get; set; }
        public int Duracion { get; set; }
        public int CapacidadMaxima { get; set; }
        public Modalidad Modalidad { get; set; }
        public string? ImagenUrl { get; set; }
        public DateTime? FechaInicio { get; set; }
        public string? NombreProfesor { get; set; }
    }
}
