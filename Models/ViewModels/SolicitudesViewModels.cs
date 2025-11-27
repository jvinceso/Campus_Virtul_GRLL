using System.ComponentModel.DataAnnotations;

namespace Campus_Virtul_GRLL.Models.ViewModels
{
    /// <summary>
    /// ViewModel para la vista Index de solicitudes (Admin)
    /// </summary>
    public class SolicitudesIndexViewModel
    {
        public List<SolicitudCardViewModel> Solicitudes { get; set; } = new List<SolicitudCardViewModel>();

        // Estadísticas
        public int TotalSolicitudes { get; set; }
        public int SolicitudesPendientes { get; set; }
        public int SolicitudesAprobadas { get; set; }
        public int SolicitudesRechazadas { get; set; }

        // Filtros
        public string? BusquedaTexto { get; set; }
        public EstadoSolicitud? FiltroEstado { get; set; }
        public int? FiltroRol { get; set; }

        // Paginación
        public int PaginaActual { get; set; } = 1;
        public int SolicitudesPorPagina { get; set; } = 10;
        public int TotalPaginas { get; set; }

        // Roles disponibles para filtro
        public List<Rol> RolesDisponibles { get; set; } = new List<Rol>();
    }

    /// <summary>
    /// Representa una solicitud en la tarjeta/lista
    /// </summary>
    public class SolicitudCardViewModel
    {
        public int IdSolicitud { get; set; }
        public string NombreCompleto { get; set; } = string.Empty;
        public string DNI { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Area { get; set; } = string.Empty;
        public string RolSolicitado { get; set; } = string.Empty;
        public DateOnly FechaSolicitud { get; set; }
        public EstadoSolicitud Estado { get; set; }
        public DateTime? FechaRespuesta { get; set; }
        public string? NombreAprobador { get; set; }
    }

    /// <summary>
    /// ViewModel para el detalle completo de una solicitud
    /// </summary>
    public class DetalleSolicitudViewModel
    {
        public int IdSolicitud { get; set; }

        // Datos del solicitante
        public string Nombres { get; set; } = string.Empty;
        public string Apellidos { get; set; } = string.Empty;
        public string NombreCompleto => $"{Nombres} {Apellidos}";
        public string DNI { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Telefono { get; set; } = string.Empty;
        public string Area { get; set; } = string.Empty;
        public string RolSolicitado { get; set; } = string.Empty;
        public int IdRolSolicitado { get; set; }

        // Datos de la solicitud
        public DateOnly FechaSolicitud { get; set; }
        public EstadoSolicitud Estado { get; set; }

        // Datos de respuesta (si ya fue procesada)
        public DateTime? FechaRespuesta { get; set; }
        public string? NombreAprobador { get; set; }
        public string? MotivoRechazo { get; set; }
        public int? IdUsuarioCreado { get; set; }

        // Validaciones
        public bool YaExisteUsuario { get; set; }
        public string? MensajeValidacion { get; set; }

        // Permisos
        public bool PuedeAprobar => Estado == EstadoSolicitud.Pendiente && !YaExisteUsuario;
        public bool PuedeRechazar => Estado == EstadoSolicitud.Pendiente;
    }

    /// <summary>
    /// ViewModel para rechazar una solicitud
    /// </summary>
    public class RechazarSolicitudViewModel
    {
        public int IdSolicitud { get; set; }

        [Display(Name = "Nombre del Solicitante")]
        public string NombreCompleto { get; set; } = string.Empty;

        [Display(Name = "DNI")]
        public string DNI { get; set; } = string.Empty;

        [Display(Name = "Email")]
        public string Email { get; set; } = string.Empty;

        [Display(Name = "Motivo del Rechazo")]
        [StringLength(500, ErrorMessage = "El motivo no puede exceder los 500 caracteres")]
        public string? MotivoRechazo { get; set; }
    }
}
