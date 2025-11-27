namespace Campus_Virtul_GRLL.Models.ViewModels
{
    /// <summary>
    /// ViewModel para la vista Index de usuarios
    /// </summary>
    public class UsuariosIndexViewModel
    {
        // ============================================
        // LISTA DE USUARIOS
        // ============================================
        public List<UsuarioCardViewModel> Usuarios { get; set; } = new List<UsuarioCardViewModel>();

        // ============================================
        // ESTADÍSTICAS
        // ============================================
        public int TotalUsuarios { get; set; }
        public int UsuariosActivos { get; set; }
        public int UsuariosInactivos { get; set; }
        public int TotalAdministradores { get; set; }
        public int TotalProfesores { get; set; }
        public int TotalColaboradores { get; set; }
        public int TotalPracticantes { get; set; }

        // ============================================
        // FILTROS
        // ============================================
        public string? BusquedaTexto { get; set; }
        public int? FiltroRol { get; set; }
        public bool? FiltroEstado { get; set; }
        public string? Ordenamiento { get; set; }
        public string? OrdenActual { get; set; }

        // ============================================
        // PAGINACIÓN
        // ============================================
        public int PaginaActual { get; set; } = 1;
        public int UsuariosPorPagina { get; set; } = 15;
        public int TotalPaginas { get; set; }

        // ============================================
        // DATOS PARA FILTROS
        // ============================================
        public List<Rol> RolesDisponibles { get; set; } = new List<Rol>();
    }

    /// <summary>
    /// Representa un usuario en la tarjeta/lista
    /// </summary>
    public class UsuarioCardViewModel
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
        public string Rol => NombreRol; // Alias para compatibilidad con la vista
        public int IdRol { get; set; }
        public bool Estado { get; set; }
        public DateOnly FechaCreacion { get; set; }
        public bool EsUsuarioActual { get; set; } // Para resaltar al usuario logueado
    }
}
