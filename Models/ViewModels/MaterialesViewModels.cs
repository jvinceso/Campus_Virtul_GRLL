using Campus_Virtul_GRLL.Models.Enums;
using System.ComponentModel.DataAnnotations;

namespace Campus_Virtul_GRLL.Models.ViewModels
{
    // ============================================
    // Lista de Materiales del Curso (Profesor)
    // ============================================
    public class MaterialesProfesorViewModel
    {
        public int IdCurso { get; set; }
        public string TituloCurso { get; set; } = string.Empty;
        public List<ModuloCursoViewModel> Modulos { get; set; } = new List<ModuloCursoViewModel>();
        public List<MaterialCardViewModel> MaterialesSinModulo { get; set; } = new List<MaterialCardViewModel>();
        public int TotalMateriales { get; set; }
        public long TamanoTotalBytes { get; set; }
    }

    public class ModuloCursoViewModel
    {
        public int IdModuloCurso { get; set; }
        public string Titulo { get; set; } = string.Empty;
        public string? Descripcion { get; set; }
        public int Orden { get; set; }
        public List<MaterialCardViewModel> Materiales { get; set; } = new List<MaterialCardViewModel>();
    }

    public class MaterialCardViewModel
    {
        public int IdMaterial { get; set; }
        public string Titulo { get; set; } = string.Empty;
        public string? Descripcion { get; set; }
        public TipoMaterial Tipo { get; set; }
        public string? Extension { get; set; }
        public long? TamanoBytes { get; set; }
        public string? UrlExterna { get; set; }
        public bool EsVisible { get; set; }
        public DateTime FechaSubida { get; set; }
        public int NumeroDescargas { get; set; }
        public int Orden { get; set; }
        public bool EsEnlace => !string.IsNullOrEmpty(UrlExterna);
        public string TamanoFormateado => TamanoBytes.HasValue
            ? Campus_Virtul_GRLL.Utilities.FileValidator.FormatFileSize(TamanoBytes.Value)
            : "N/A";
    }

    // ============================================
    // Subir Material
    // ============================================
    public class SubirMaterialViewModel
    {
        public int IdCurso { get; set; }
        public string TituloCurso { get; set; } = string.Empty;

        [Required(ErrorMessage = "El título es obligatorio")]
        [StringLength(200, ErrorMessage = "El título no puede exceder 200 caracteres")]
        [Display(Name = "Título del Material")]
        public string Titulo { get; set; } = string.Empty;

        [StringLength(1000, ErrorMessage = "La descripción no puede exceder 1000 caracteres")]
        [Display(Name = "Descripción")]
        public string? Descripcion { get; set; }

        [Display(Name = "Módulo (opcional)")]
        public int? IdModuloCurso { get; set; }

        [Display(Name = "Visible para estudiantes")]
        public bool EsVisible { get; set; } = true;

        // Para archivo
        [Display(Name = "Archivo")]
        public IFormFile? Archivo { get; set; }

        // Para enlace externo
        [StringLength(1000, ErrorMessage = "La URL no puede exceder 1000 caracteres")]
        [Display(Name = "URL Externa (YouTube, Google Drive, etc.)")]
        public string? UrlExterna { get; set; }

        [Display(Name = "Tipo de Material")]
        public TipoMaterial? TipoMaterial { get; set; }

        // Lista de módulos disponibles
        public List<ModuloCurso> ModulosDisponibles { get; set; } = new List<ModuloCurso>();

        // Indicador de modo
        public bool EsModoEnlace { get; set; }
    }

    // ============================================
    // Editar Material
    // ============================================
    public class EditarMaterialViewModel
    {
        public int IdMaterial { get; set; }
        public int IdCurso { get; set; }
        public string TituloCurso { get; set; } = string.Empty;

        [Required(ErrorMessage = "El título es obligatorio")]
        [StringLength(200, ErrorMessage = "El título no puede exceder 200 caracteres")]
        [Display(Name = "Título")]
        public string Titulo { get; set; } = string.Empty;

        [StringLength(1000, ErrorMessage = "La descripción no puede exceder 1000 caracteres")]
        [Display(Name = "Descripción")]
        public string? Descripcion { get; set; }

        [Display(Name = "Módulo")]
        public int? IdModuloCurso { get; set; }

        [Display(Name = "Visible")]
        public bool EsVisible { get; set; }

        [Display(Name = "Orden")]
        public int Orden { get; set; }

        // Para enlaces: permitir editar URL
        [StringLength(1000, ErrorMessage = "La URL no puede exceder 1000 caracteres")]
        [Display(Name = "URL Externa")]
        public string? UrlExterna { get; set; }

        // Información del archivo actual (solo lectura)
        public string? NombreArchivo { get; set; }
        public string? Extension { get; set; }
        public long? TamanoBytes { get; set; }
        public TipoMaterial Tipo { get; set; }
        public bool EsEnlace { get; set; }

        // Lista de módulos disponibles
        public List<ModuloCurso> ModulosDisponibles { get; set; } = new List<ModuloCurso>();
    }

    // ============================================
    // Materiales para Estudiantes
    // ============================================
    public class MaterialesEstudianteViewModel
    {
        public int IdCurso { get; set; }
        public string TituloCurso { get; set; } = string.Empty;
        public string? Descripcion { get; set; }
        public List<ModuloCursoViewModel> Modulos { get; set; } = new List<ModuloCursoViewModel>();
        public List<MaterialCardViewModel> MaterialesSinModulo { get; set; } = new List<MaterialCardViewModel>();
        public bool EstaInscrito { get; set; }
    }
}
