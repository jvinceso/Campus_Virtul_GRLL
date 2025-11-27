namespace Campus_Virtul_GRLL.Models
{
    public class Usuario
    {
       
        public int IdUsuario { get; set; }

        public int IdRol { get; set; }

        public required string Nombres { get; set; }
        public required string Apellidos { get; set; }
        public required string DNI { get; set; }
        public required string Telefono { get; set; }
        public string? NombreRol => Rol?.NombreRol;
        public required string CorreoElectronico { get; set; }
        public required string Area { get; set; }

        public bool PrimerInicio { get; set; }
        public required string ClaveTemporal { get; set; }
        public required string ClavePermanente { get; set; }

        public DateOnly FechaCreacion { get; set; }
        public DateOnly FechaActualizacion { get; set; }

        public bool Estado { get; set; }

        // Campos para Soft Delete
        public bool EsEliminado { get; set; } = false;
        public DateTime? FechaEliminacion { get; set; }
        public int? EliminadoPor { get; set; }

        // Navegación
        public Rol? Rol { get; set; }
        public Usuario? UsuarioEliminador { get; set; }
    }
}
