namespace Campus_Virtul_GRLL.Models
{
    /// <summary>
    /// Solicitud de creación de cuenta de usuario
    /// </summary>
    public class Solicitud
    {
        public int IdSolicitud { get; set; }

        public int IdRol { get; set; }

        public required string Nombres { get; set; }

        public required string Apellidos { get; set; }

        public required string DNI { get; set; }

        public required string Telefono { get; set; }

        public required string CorreoElectronico { get; set; }

        public required string Area { get; set; }

        public DateOnly FechaSolicitud { get; set; } = DateOnly.FromDateTime(DateTime.Now);

        public EstadoSolicitud Estado { get; set; } = EstadoSolicitud.Pendiente;

        // Campos para aprobación/rechazo
        public DateTime? FechaRespuesta { get; set; }

        public int? RespuestaDe { get; set; } // FK al Usuario (admin) que respondió

        public string? MotivoRechazo { get; set; }

        public int? UsuarioCreado { get; set; } // FK al Usuario creado si fue aprobada

        // Navegación
        public Rol Rol { get; set; } = null!;

        public Usuario? UsuarioRespondio { get; set; }

        public Usuario? UsuarioGenerado { get; set; }

        public string? NombreRol => Rol?.NombreRol;

        public string NombreCompleto => $"{Nombres} {Apellidos}";
    }

    /// <summary>
    /// Estados posibles de una solicitud
    /// </summary>
    public enum EstadoSolicitud
    {
        Pendiente,      // Solicitud recién creada, esperando revisión
        EnRevision,     // Admin está revisando (opcional, puede ir directo a Aprobada/Rechazada)
        Aprobada,       // Solicitud aprobada, usuario creado
        Rechazada       // Solicitud rechazada
    }
}
