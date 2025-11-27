namespace Campus_Virtul_GRLL.Models.ViewModels
{
    /// <summary>
    /// ViewModel para mostrar el detalle completo de un usuario
    /// </summary>
    public class DetalleUsuarioViewModel
    {
        public int IdUsuario { get; set; }
        public string Nombres { get; set; } = string.Empty;
        public string Apellidos { get; set; } = string.Empty;
        public string NombreCompleto => $"{Nombres} {Apellidos}";
        public string DNI { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Telefono { get; set; } = string.Empty;
        public string Area { get; set; } = string.Empty;
        public string NombreRol { get; set; } = string.Empty;
        public int IdRol { get; set; }
        public bool Estado { get; set; }
        public bool PrimerInicio { get; set; }
        public DateOnly FechaCreacion { get; set; }
        public DateOnly FechaActualizacion { get; set; }

        // Información adicional
        public bool EsUsuarioActual { get; set; } // Para deshabilitar ciertas acciones
        public int? CursosAsignados { get; set; } // Si es profesor
        public int? CursosCreados { get; set; } // Si creó cursos

        // Para acciones
        public bool PuedeEliminar => !EsUsuarioActual;
        public bool PuedeCambiarEstado => !EsUsuarioActual;
        public bool PuedeCambiarRol => !EsUsuarioActual;
    }
}
