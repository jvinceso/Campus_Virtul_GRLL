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

    /// <summary>
    /// ViewModel para visualizar los permisos de un rol
    /// </summary>
    public class VerPermisosViewModel
    {
        public string NombreRol { get; set; } = string.Empty;
        public Dictionary<string, PermisosDetalle> PermisosPorModulo { get; set; } = new Dictionary<string, PermisosDetalle>();

        // Estadísticas
        public int TotalPermisos { get; set; }
        public int ModulosConPermiso { get; set; }
    }

    /// <summary>
    /// Detalle de permisos para un módulo específico
    /// </summary>
    public class PermisosDetalle
    {
        public bool Crear { get; set; }
        public bool Editar { get; set; }
        public bool Revisar { get; set; }
        public bool Aprobar { get; set; }
        public bool Visualizar { get; set; }
    }

    // ============================================
    // CRUD DE ROLES
    // ============================================

    /// <summary>
    /// ViewModel para el listado de roles (CRUD)
    /// </summary>
    public class ListadoRolesViewModel
    {
        public List<RolCardViewModel> Roles { get; set; } = new List<RolCardViewModel>();
        public int TotalRoles { get; set; }
        public int RolesActivos { get; set; }
        public int RolesInactivos { get; set; }
    }

    /// <summary>
    /// Representa un rol para el listado
    /// </summary>
    public class RolCardViewModel
    {
        public int IdRol { get; set; }
        public string NombreRol { get; set; } = string.Empty;
        public string Descripcion { get; set; } = string.Empty;
        public bool Estado { get; set; }
        public int UsuariosConEsteRol { get; set; }
        public bool PuedeEliminar => UsuariosConEsteRol == 0;
    }

    /// <summary>
    /// ViewModel para crear un nuevo rol
    /// </summary>
    public class CrearRolViewModel
    {
        [Required(ErrorMessage = "El nombre del rol es obligatorio")]
        [StringLength(50, ErrorMessage = "El nombre no puede exceder 50 caracteres")]
        [Display(Name = "Nombre del Rol")]
        public string NombreRol { get; set; } = string.Empty;

        [Required(ErrorMessage = "La descripción es obligatoria")]
        [StringLength(200, ErrorMessage = "La descripción no puede exceder 200 caracteres")]
        [Display(Name = "Descripción")]
        public string Descripcion { get; set; } = string.Empty;

        [Display(Name = "Rol Activo")]
        public bool Estado { get; set; } = true;
    }

    /// <summary>
    /// ViewModel para editar un rol existente
    /// </summary>
    public class EditarRolViewModel
    {
        public int IdRol { get; set; }

        [Required(ErrorMessage = "El nombre del rol es obligatorio")]
        [StringLength(50, ErrorMessage = "El nombre no puede exceder 50 caracteres")]
        [Display(Name = "Nombre del Rol")]
        public string NombreRol { get; set; } = string.Empty;

        [Required(ErrorMessage = "La descripción es obligatoria")]
        [StringLength(200, ErrorMessage = "La descripción no puede exceder 200 caracteres")]
        [Display(Name = "Descripción")]
        public string Descripcion { get; set; } = string.Empty;

        [Display(Name = "Rol Activo")]
        public bool Estado { get; set; }

        // Para mostrar información
        public int UsuariosConEsteRol { get; set; }
        public bool PuedeDesactivar => UsuariosConEsteRol == 0 || Estado == false;
    }

    /// <summary>
    /// ViewModel para gestionar permisos de un rol
    /// </summary>
    public class GestionarPermisosViewModel
    {
        public int IdRol { get; set; }
        public string NombreRol { get; set; } = string.Empty;

        // Lista de módulos disponibles con sus permisos actuales
        public List<ModuloPermisoViewModel> Modulos { get; set; } = new List<ModuloPermisoViewModel>();
    }

    /// <summary>
    /// Representa un módulo y sus permisos para edición
    /// </summary>
    public class ModuloPermisoViewModel
    {
        public int IdModulo { get; set; }
        public string NombreModulo { get; set; } = string.Empty;
        public string Descripcion { get; set; } = string.Empty;

        // ID del permiso (si existe)
        public int? IdPermiso { get; set; }

        // Permisos del rol en este módulo
        public bool Crear { get; set; }
        public bool Editar { get; set; }
        public bool Revisar { get; set; }
        public bool Aprobar { get; set; }
        public bool Visualizar { get; set; }
    }
}
