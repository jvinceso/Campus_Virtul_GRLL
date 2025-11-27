using System.ComponentModel.DataAnnotations;

namespace Campus_Virtul_GRLL.Models.ViewModels
{
    /// <summary>
    /// ViewModel para la vista Index de gestión de roles
    /// </summary>
    public class GestionRolesIndexViewModel
    {
        public List<RolUsuarioViewModel> Usuarios { get; set; } = new List<RolUsuarioViewModel>();

        // Estadísticas
        public int TotalUsuarios { get; set; }
        public Dictionary<string, int> UsuariosPorRol { get; set; } = new Dictionary<string, int>();

        // Filtros
        public string? BusquedaTexto { get; set; }
        public int? FiltroRol { get; set; }

        // Paginación
        public int PaginaActual { get; set; } = 1;
        public int UsuariosPorPagina { get; set; } = 15;
        public int TotalPaginas { get; set; }

        // Roles disponibles para filtro
        public List<Rol> RolesDisponibles { get; set; } = new List<Rol>();
    }

    /// <summary>
    /// Representa un usuario con su rol para la lista
    /// </summary>
    public class RolUsuarioViewModel
    {
        public int IdUsuario { get; set; }
        public string NombreCompleto { get; set; } = string.Empty;
        public string DNI { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Area { get; set; } = string.Empty;
        public string RolActual { get; set; } = string.Empty;
        public int IdRolActual { get; set; }
        public bool Estado { get; set; }
        public DateOnly FechaCreacion { get; set; }
        public bool EsUsuarioActual { get; set; }

        // Para mostrar información adicional
        public bool PuedeCambiarRol => !EsUsuarioActual;
    }

    /// <summary>
    /// ViewModel para cambiar el rol de un usuario
    /// </summary>
    public class CambiarRolViewModel
    {
        public int IdUsuario { get; set; }

        [Display(Name = "Nombre Completo")]
        public string NombreCompleto { get; set; } = string.Empty;

        [Display(Name = "DNI")]
        public string DNI { get; set; } = string.Empty;

        [Display(Name = "Email")]
        public string Email { get; set; } = string.Empty;

        [Display(Name = "Área")]
        public string Area { get; set; } = string.Empty;

        [Display(Name = "Rol Actual")]
        public string RolActual { get; set; } = string.Empty;

        public int IdRolActual { get; set; }

        [Required(ErrorMessage = "Debe seleccionar un nuevo rol")]
        [Display(Name = "Nuevo Rol")]
        public int IdRolNuevo { get; set; }

        [Display(Name = "Motivo del cambio")]
        [StringLength(500, ErrorMessage = "El motivo no puede exceder los 500 caracteres")]
        public string? Motivo { get; set; }

        // Para validaciones
        public bool EsUsuarioActual { get; set; }

        // Para el dropdown
        public List<Rol> RolesDisponibles { get; set; } = new List<Rol>();
    }
}
