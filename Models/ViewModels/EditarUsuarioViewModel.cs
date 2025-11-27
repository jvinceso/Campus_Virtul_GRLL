using System.ComponentModel.DataAnnotations;

namespace Campus_Virtul_GRLL.Models.ViewModels
{
    /// <summary>
    /// ViewModel para editar un usuario existente
    /// </summary>
    public class EditarUsuarioViewModel
    {
        public int IdUsuario { get; set; }

        [Required(ErrorMessage = "El nombre es obligatorio")]
        [StringLength(100, ErrorMessage = "El nombre no puede exceder los 100 caracteres")]
        [Display(Name = "Nombres")]
        public string Nombres { get; set; } = string.Empty;

        [Required(ErrorMessage = "Los apellidos son obligatorios")]
        [StringLength(100, ErrorMessage = "Los apellidos no pueden exceder los 100 caracteres")]
        [Display(Name = "Apellidos")]
        public string Apellidos { get; set; } = string.Empty;

        // DNI es readonly - no se puede editar
        [Display(Name = "DNI")]
        public string DNI { get; set; } = string.Empty;

        [Required(ErrorMessage = "El email es obligatorio")]
        [EmailAddress(ErrorMessage = "El formato del email no es válido")]
        [StringLength(150, ErrorMessage = "El email no puede exceder los 150 caracteres")]
        [Display(Name = "Correo Electrónico")]
        public string Email { get; set; } = string.Empty;

        [StringLength(20, ErrorMessage = "El teléfono no puede exceder los 20 caracteres")]
        [Display(Name = "Teléfono")]
        public string Telefono { get; set; } = string.Empty;

        [Required(ErrorMessage = "El área es obligatoria")]
        [StringLength(100, ErrorMessage = "El área no puede exceder los 100 caracteres")]
        [Display(Name = "Área")]
        public string Area { get; set; } = string.Empty;

        [Required(ErrorMessage = "Debe seleccionar un rol")]
        [Display(Name = "Rol")]
        public int IdRol { get; set; }

        [Required(ErrorMessage = "Debe seleccionar un estado")]
        [Display(Name = "Estado")]
        public bool Estado { get; set; }

        // Para validaciones especiales
        public bool EsUsuarioActual { get; set; } // No puede quitarse admin o desactivarse
        public string RolActual { get; set; } = string.Empty;

        // Para los dropdowns
        public List<Rol> RolesDisponibles { get; set; } = new List<Rol>();

        public List<string> AreasDisponibles { get; set; } = new List<string>
        {
            "Administración",
            "Tecnología",
            "Capacitación",
            "Desarrollo Humano",
            "Finanzas",
            "Recursos Humanos",
            "Infraestructura",
            "Legal y Normativa",
            "Medio Ambiente",
            "Planificación"
        };
    }
}
