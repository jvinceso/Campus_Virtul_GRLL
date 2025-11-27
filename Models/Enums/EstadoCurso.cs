namespace Campus_Virtul_GRLL.Models.Enums
{
    /// <summary>
    /// Define los estados del ciclo de vida de un curso
    /// </summary>
    public enum EstadoCurso
    {
        /// <summary>
        /// Curso en proceso de creación, no visible para usuarios
        /// </summary>
        Borrador,

        /// <summary>
        /// Curso visible y disponible para inscripción
        /// </summary>
        Publicado,

        /// <summary>
        /// Curso actualmente en ejecución
        /// </summary>
        EnCurso,

        /// <summary>
        /// Curso que ha terminado su ejecución
        /// </summary>
        Finalizado,

        /// <summary>
        /// Curso cancelado antes de finalizar
        /// </summary>
        Cancelado
    }
}
