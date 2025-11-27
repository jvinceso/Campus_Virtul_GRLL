using Campus_Virtul_GRLL.Models.Enums;

namespace Campus_Virtul_GRLL.Models.ViewModels
{
    /// <summary>
    /// ViewModel para mostrar el detalle completo de un curso
    /// </summary>
    public class DetalleCursoViewModel
    {
        // ============================================
        // INFORMACIÓN DEL CURSO
        // ============================================
        public int Id { get; set; }
        public string Codigo { get; set; } = string.Empty;
        public string Titulo { get; set; } = string.Empty;
        public string Descripcion { get; set; } = string.Empty;
        public string? Objetivos { get; set; }
        public int Duracion { get; set; }
        public Modalidad Modalidad { get; set; }
        public string Categoria { get; set; } = string.Empty;
        public NivelCurso Nivel { get; set; }
        public int CapacidadMaxima { get; set; }
        public EstadoCurso Estado { get; set; }
        public string? ImagenUrl { get; set; }

        // ============================================
        // FECHAS
        // ============================================
        public DateTime? FechaInicio { get; set; }
        public DateTime? FechaFin { get; set; }
        public DateTime? FechaPublicacion { get; set; }

        // ============================================
        // PROFESOR ASIGNADO
        // ============================================
        public int? ProfesorAsignado { get; set; }
        public string? NombreProfesor { get; set; }
        public string? EmailProfesor { get; set; }

        // ============================================
        // METADATOS DE AUDITORÍA
        // ============================================
        public int CreadoPor { get; set; }
        public string NombreCreador { get; set; } = string.Empty;
        public DateTime FechaCreacion { get; set; }
        public DateTime? FechaUltimaActualizacion { get; set; }

        // ============================================
        // INFORMACIÓN DE ESTADO
        // ============================================
        public bool PermitirEdicion { get; set; } = true;
        public bool PermitirEliminacion { get; set; } = true;
        public bool PermitirCambioEstado { get; set; } = true;
        public string? MensajeAdvertencia { get; set; }

        // ============================================
        // ESTADÍSTICAS (para futuras implementaciones)
        // ============================================
        public int? NumeroInscritos { get; set; }
        public int? NumeroMateriales { get; set; }
        public int? NumeroEvaluaciones { get; set; }
    }
}
