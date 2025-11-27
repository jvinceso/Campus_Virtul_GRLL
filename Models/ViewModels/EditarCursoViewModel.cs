using Campus_Virtul_GRLL.Models.Enums;
using System.ComponentModel.DataAnnotations;

namespace Campus_Virtul_GRLL.Models.ViewModels
{
    /// <summary>
    /// ViewModel para editar un curso existente
    /// </summary>
    public class EditarCursoViewModel
    {
        [Required]
        public int Id { get; set; }

        // ============================================
        // TAB 1: INFORMACIÓN BÁSICA
        // ============================================

        [Required(ErrorMessage = "El código del curso es requerido")]
        [Display(Name = "Código")]
        [StringLength(20, ErrorMessage = "El código no puede exceder 20 caracteres")]
        public string Codigo { get; set; } = string.Empty; // ReadOnly en el formulario

        [Required(ErrorMessage = "El título del curso es requerido")]
        [Display(Name = "Título")]
        [StringLength(200, ErrorMessage = "El título no puede exceder 200 caracteres")]
        public string Titulo { get; set; } = string.Empty;

        [Required(ErrorMessage = "La categoría es requerida")]
        [Display(Name = "Categoría")]
        [StringLength(100, ErrorMessage = "La categoría no puede exceder 100 caracteres")]
        public string Categoria { get; set; } = string.Empty;

        [Required(ErrorMessage = "El nivel es requerido")]
        [Display(Name = "Nivel")]
        public NivelCurso Nivel { get; set; }

        [Required(ErrorMessage = "La modalidad es requerida")]
        [Display(Name = "Modalidad")]
        public Modalidad Modalidad { get; set; }

        // ============================================
        // TAB 2: DESCRIPCIÓN
        // ============================================

        [Required(ErrorMessage = "La descripción es requerida")]
        [Display(Name = "Descripción")]
        [StringLength(2000, MinimumLength = 20, ErrorMessage = "La descripción debe tener entre 20 y 2000 caracteres")]
        public string Descripcion { get; set; } = string.Empty;

        [Display(Name = "Objetivos del Curso")]
        [StringLength(1000, ErrorMessage = "Los objetivos no pueden exceder 1000 caracteres")]
        public string? Objetivos { get; set; }

        // ============================================
        // TAB 3: DETALLES Y FECHAS
        // ============================================

        [Required(ErrorMessage = "La duración es requerida")]
        [Display(Name = "Duración (horas)")]
        [Range(1, 1000, ErrorMessage = "La duración debe estar entre 1 y 1000 horas")]
        public int Duracion { get; set; }

        [Required(ErrorMessage = "La capacidad máxima es requerida")]
        [Display(Name = "Capacidad Máxima")]
        [Range(1, 500, ErrorMessage = "La capacidad debe estar entre 1 y 500 personas")]
        public int CapacidadMaxima { get; set; }

        [Display(Name = "Fecha de Inicio")]
        [DataType(DataType.Date)]
        public DateTime? FechaInicio { get; set; }

        [Display(Name = "Fecha de Fin")]
        [DataType(DataType.Date)]
        public DateTime? FechaFin { get; set; }

        // ============================================
        // TAB 4: IMAGEN Y OTROS
        // ============================================

        [Display(Name = "URL de la Imagen")]
        [StringLength(500, ErrorMessage = "La URL no puede exceder 500 caracteres")]
        [Url(ErrorMessage = "La URL no es válida")]
        public string? ImagenUrl { get; set; }

        [Required(ErrorMessage = "El estado es requerido")]
        [Display(Name = "Estado")]
        public EstadoCurso Estado { get; set; }

        // ============================================
        // PROPIEDADES DE SOLO LECTURA (para contexto)
        // ============================================

        public EstadoCurso EstadoActual { get; set; } // Para validaciones
        public bool PermitirCambioFechas { get; set; } = true;
        public bool PermitirEdicionCompleta { get; set; } = true;

        // ============================================
        // LISTAS PARA DROPDOWNS
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
}
