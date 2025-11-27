using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Campus_Virtul_GRLL.Data;
using Campus_Virtul_GRLL.Models.Enums;
using Campus_Virtul_GRLL.Models.ViewModels;
using Campus_Virtul_GRLL.Helpers;

namespace Campus_Virtul_GRLL.Controllers
{
    /// <summary>
    /// Controlador para profesores
    /// Permite a los profesores ver sus cursos asignados
    /// </summary>
    [Authorize(Roles = "Profesor")]
    public class ProfesorController : Controller
    {
        private readonly AppDBContext _context;
        private readonly ILogger<ProfesorController> _logger;

        public ProfesorController(AppDBContext context, ILogger<ProfesorController> logger)
        {
            _context = context;
            _logger = logger;
        }

        // ============================================
        // DASHBOARD DEL PROFESOR
        // ============================================

        /// <summary>
        /// Dashboard principal del profesor
        /// GET: /Profesor/Dashboard
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> Dashboard()
        {
            try
            {
                var profesorId = User.GetUserId();
                _logger.LogInformation($"📚 Cargando dashboard del profesor ID: {profesorId}");

                // Obtener información del profesor
                var profesor = await _context.Usuarios
                    .Include(u => u.Rol)
                    .FirstOrDefaultAsync(u => u.IdUsuario == profesorId);

                if (profesor == null)
                {
                    _logger.LogWarning($"⚠️ Profesor ID {profesorId} no encontrado");
                    return RedirectToAction("Index", "Login");
                }

                // Obtener cursos asignados
                var cursos = await _context.Cursos
                    .Where(c => c.ProfesorAsignado == profesorId && !c.EsEliminado)
                    .OrderByDescending(c => c.FechaCreacion)
                    .ToListAsync();

                // Mapear a ViewModels
                var cursosViewModel = cursos.Select(c => new CursoAsignadoViewModel
                {
                    Id = c.Id,
                    Codigo = c.Codigo,
                    Titulo = c.Titulo,
                    Categoria = c.Categoria,
                    Estado = c.Estado,
                    Nivel = c.Nivel,
                    Modalidad = c.Modalidad,
                    Duracion = c.Duracion,
                    CapacidadMaxima = c.CapacidadMaxima,
                    FechaInicio = c.FechaInicio,
                    FechaFin = c.FechaFin,
                    FechaAsignacion = c.FechaCreacion
                }).ToList();

                // Calcular estadísticas
                int totalCursos = cursosViewModel.Count;
                int cursosActivos = cursosViewModel.Count(c => c.Estado == EstadoCurso.Publicado || c.Estado == EstadoCurso.EnCurso);
                int cursosEnCurso = cursosViewModel.Count(c => c.Estado == EstadoCurso.EnCurso);
                int cursosFinalizados = cursosViewModel.Count(c => c.Estado == EstadoCurso.Finalizado);
                int horasTotales = cursosViewModel.Sum(c => c.Duracion);

                var viewModel = new ProfesorCursosViewModel
                {
                    ProfesorId = profesor.IdUsuario,
                    NombreCompleto = $"{profesor.Nombres} {profesor.Apellidos}",
                    Email = profesor.CorreoElectronico,
                    Area = profesor.Area,
                    Telefono = profesor.Telefono,
                    Estado = profesor.Estado,
                    Cursos = cursosViewModel,
                    TotalCursosAsignados = totalCursos,
                    CursosActivos = cursosActivos,
                    CursosEnCurso = cursosEnCurso,
                    CursosFinalizados = cursosFinalizados,
                    HorasTotales = horasTotales,
                    EsVistaAdmin = false // Vista del profesor
                };

                _logger.LogInformation($"✅ Dashboard cargado: {totalCursos} cursos para profesor {profesor.Nombres} {profesor.Apellidos}");
                return View(viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error al cargar dashboard del profesor");
                TempData["Error"] = "Error al cargar el dashboard";
                return RedirectToAction("PanelProfesor", "Dashboard");
            }
        }

        // ============================================
        // MIS CURSOS (con filtros)
        // ============================================

        /// <summary>
        /// Vista detallada de cursos con filtros
        /// GET: /Profesor/MisCursos
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> MisCursos(EstadoCurso? estado, string? orden)
        {
            try
            {
                var profesorId = User.GetUserId();
                _logger.LogInformation($"📚 Cargando cursos del profesor ID: {profesorId}");

                // Obtener información del profesor
                var profesor = await _context.Usuarios
                    .Include(u => u.Rol)
                    .FirstOrDefaultAsync(u => u.IdUsuario == profesorId);

                if (profesor == null)
                {
                    _logger.LogWarning($"⚠️ Profesor ID {profesorId} no encontrado");
                    return RedirectToAction("Index", "Login");
                }

                // Obtener cursos asignados
                var query = _context.Cursos
                    .Where(c => c.ProfesorAsignado == profesorId && !c.EsEliminado);

                // Aplicar filtro de estado
                if (estado.HasValue)
                {
                    query = query.Where(c => c.Estado == estado.Value);
                }

                // Aplicar ordenamiento
                query = orden switch
                {
                    "inicio" => query.OrderBy(c => c.FechaInicio ?? DateTime.MaxValue),
                    "estado" => query.OrderBy(c => c.Estado),
                    "titulo" => query.OrderBy(c => c.Titulo),
                    _ => query.OrderByDescending(c => c.FechaCreacion) // "recientes" por defecto
                };

                var cursos = await query.ToListAsync();

                // Mapear a ViewModels
                var cursosViewModel = cursos.Select(c => new CursoAsignadoViewModel
                {
                    Id = c.Id,
                    Codigo = c.Codigo,
                    Titulo = c.Titulo,
                    Categoria = c.Categoria,
                    Estado = c.Estado,
                    Nivel = c.Nivel,
                    Modalidad = c.Modalidad,
                    Duracion = c.Duracion,
                    CapacidadMaxima = c.CapacidadMaxima,
                    FechaInicio = c.FechaInicio,
                    FechaFin = c.FechaFin,
                    FechaAsignacion = c.FechaCreacion
                }).ToList();

                // Calcular estadísticas
                int totalCursos = cursosViewModel.Count;
                int cursosActivos = cursosViewModel.Count(c => c.Estado == EstadoCurso.Publicado || c.Estado == EstadoCurso.EnCurso);
                int cursosEnCurso = cursosViewModel.Count(c => c.Estado == EstadoCurso.EnCurso);
                int cursosFinalizados = cursosViewModel.Count(c => c.Estado == EstadoCurso.Finalizado);
                int horasTotales = cursosViewModel.Sum(c => c.Duracion);

                var viewModel = new ProfesorCursosViewModel
                {
                    ProfesorId = profesor.IdUsuario,
                    NombreCompleto = $"{profesor.Nombres} {profesor.Apellidos}",
                    Email = profesor.CorreoElectronico,
                    Area = profesor.Area,
                    Telefono = profesor.Telefono,
                    Estado = profesor.Estado,
                    Cursos = cursosViewModel,
                    TotalCursosAsignados = totalCursos,
                    CursosActivos = cursosActivos,
                    CursosEnCurso = cursosEnCurso,
                    CursosFinalizados = cursosFinalizados,
                    HorasTotales = horasTotales,
                    FiltroEstado = estado,
                    Ordenamiento = orden,
                    EsVistaAdmin = false // Vista del profesor
                };

                _logger.LogInformation($"✅ Cursos cargados: {totalCursos} cursos para profesor {profesor.Nombres} {profesor.Apellidos}");
                return View(viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error al cargar cursos del profesor");
                TempData["Error"] = "Error al cargar sus cursos";
                return RedirectToAction("PanelProfesor", "Dashboard");
            }
        }

        // ============================================
        // VER DETALLE DE UN CURSO
        // ============================================

        /// <summary>
        /// Ver detalle de un curso asignado
        /// GET: /Profesor/VerCurso/{id}
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> VerCurso(int id)
        {
            try
            {
                var profesorId = User.GetUserId();

                var curso = await _context.Cursos
                    .Include(c => c.Profesor)
                    .Include(c => c.Creador)
                    .FirstOrDefaultAsync(c => c.Id == id && !c.EsEliminado);

                if (curso == null)
                {
                    _logger.LogWarning($"⚠️ Curso ID {id} no encontrado");
                    TempData["Error"] = "Curso no encontrado";
                    return RedirectToAction(nameof(MisCursos));
                }

                // Verificar que el curso esté asignado a este profesor
                if (curso.ProfesorAsignado != profesorId)
                {
                    _logger.LogWarning($"⚠️ Profesor {profesorId} intentó acceder a curso {id} que no le pertenece");
                    TempData["Error"] = "No tiene permiso para ver este curso";
                    return RedirectToAction(nameof(MisCursos));
                }

                // Redirigir al detalle del curso en AdminCursos (read-only para profesor)
                return RedirectToAction("Detalle", "AdminCursos", new { id = curso.Id });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"❌ Error al cargar curso ID {id}");
                TempData["Error"] = "Error al cargar el curso";
                return RedirectToAction(nameof(MisCursos));
            }
        }
    }
}
