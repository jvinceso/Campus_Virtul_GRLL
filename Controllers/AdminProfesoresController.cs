using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Campus_Virtul_GRLL.Data;
using Campus_Virtul_GRLL.Models;
using Campus_Virtul_GRLL.Models.Enums;
using Campus_Virtul_GRLL.Models.ViewModels;
using Campus_Virtul_GRLL.Helpers;

namespace Campus_Virtul_GRLL.Controllers
{
    /// <summary>
    /// Controlador para la gestión de profesores y asignación de cursos
    /// Solo accesible para Administradores
    /// </summary>
    [Authorize(Roles = "Administrador")]
    public class AdminProfesoresController : Controller
    {
        private readonly AppDBContext _context;
        private readonly ILogger<AdminProfesoresController> _logger;

        public AdminProfesoresController(AppDBContext context, ILogger<AdminProfesoresController> logger)
        {
            _context = context;
            _logger = logger;
        }

        // ============================================
        // LISTAR PROFESORES
        // ============================================

        /// <summary>
        /// Vista principal con lista de profesores y sus cursos
        /// GET: /AdminProfesores/Index
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> Index(
            string? busqueda,
            bool? soloActivos,
            bool? soloConCursos,
            string? area,
            string? orden,
            int pagina = 1)
        {
            try
            {
                _logger.LogInformation($"📚 Cargando lista de profesores - Página {pagina}");

                // Obtener el rol Profesor
                var rolProfesor = await _context.Rols
                    .FirstOrDefaultAsync(r => r.NombreRol == "Profesor");

                if (rolProfesor == null)
                {
                    _logger.LogWarning("⚠️ No se encontró el rol Profesor");
                    TempData["Error"] = "No se encontró el rol Profesor en el sistema";
                    return View(new ProfesoresIndexViewModel());
                }

                // Query base: usuarios con rol Profesor
                var query = _context.Usuarios
                    .Where(u => u.IdRol == rolProfesor.IdRol);

                // Aplicar búsqueda
                if (!string.IsNullOrWhiteSpace(busqueda))
                {
                    busqueda = busqueda.ToLower().Trim();
                    query = query.Where(u =>
                        u.Nombres.ToLower().Contains(busqueda) ||
                        u.Apellidos.ToLower().Contains(busqueda) ||
                        u.CorreoElectronico.ToLower().Contains(busqueda));
                }

                // Aplicar filtros
                if (soloActivos.HasValue && soloActivos.Value)
                {
                    query = query.Where(u => u.Estado == true);
                }

                if (!string.IsNullOrWhiteSpace(area))
                {
                    query = query.Where(u => u.Area == area);
                }

                // Obtener todos los cursos para calcular estadísticas
                var todosCursos = await _context.Cursos
                    .Where(c => !c.EsEliminado)
                    .ToListAsync();

                // Aplicar ordenamiento
                query = orden switch
                {
                    "cursos" => query.OrderByDescending(u => todosCursos.Count(c => c.ProfesorAsignado == u.IdUsuario)),
                    "area" => query.OrderBy(u => u.Area),
                    _ => query.OrderBy(u => u.Nombres) // "nombre" por defecto
                };

                // Obtener profesores
                var profesores = await query.ToListAsync();

                // Filtrar por cursos asignados si es necesario
                if (soloConCursos.HasValue)
                {
                    if (soloConCursos.Value)
                    {
                        profesores = profesores.Where(p => todosCursos.Any(c => c.ProfesorAsignado == p.IdUsuario)).ToList();
                    }
                    else
                    {
                        profesores = profesores.Where(p => !todosCursos.Any(c => c.ProfesorAsignado == p.IdUsuario)).ToList();
                    }
                }

                // Calcular estadísticas globales
                int totalProfesores = profesores.Count;
                int profesoresActivos = profesores.Count(p => p.Estado);
                int profesoresConCursos = profesores.Count(p => todosCursos.Any(c => c.ProfesorAsignado == p.IdUsuario));
                int profesoresSinCursos = totalProfesores - profesoresConCursos;

                // Paginación
                int profesoresPorPagina = 12;
                int totalPaginas = (int)Math.Ceiling(totalProfesores / (double)profesoresPorPagina);
                var profesoresPaginados = profesores
                    .Skip((pagina - 1) * profesoresPorPagina)
                    .Take(profesoresPorPagina)
                    .ToList();

                // Mapear a ViewModels
                var profesoresViewModel = profesoresPaginados.Select(p =>
                {
                    var cursosProfesor = todosCursos.Where(c => c.ProfesorAsignado == p.IdUsuario).ToList();
                    var cursosActivos = cursosProfesor.Where(c => c.Estado == EstadoCurso.Publicado || c.Estado == EstadoCurso.EnCurso).ToList();

                    return new ProfesorCardViewModel
                    {
                        IdUsuario = p.IdUsuario,
                        Nombres = p.Nombres,
                        Apellidos = p.Apellidos,
                        Email = p.CorreoElectronico,
                        Area = p.Area,
                        Telefono = p.Telefono,
                        Estado = p.Estado,
                        TotalCursosAsignados = cursosProfesor.Count,
                        CursosActivos = cursosActivos.Count,
                        CursosFinalizados = cursosProfesor.Count(c => c.Estado == EstadoCurso.Finalizado),
                        HorasCargaTrabajo = cursosActivos.Sum(c => c.Duracion)
                    };
                }).ToList();

                // Obtener áreas distintas para filtros
                var areasDisponibles = await _context.Usuarios
                    .Where(u => u.IdRol == rolProfesor.IdRol)
                    .Select(u => u.Area)
                    .Distinct()
                    .OrderBy(a => a)
                    .ToListAsync();

                var viewModel = new ProfesoresIndexViewModel
                {
                    Profesores = profesoresViewModel,
                    TotalProfesores = totalProfesores,
                    ProfesoresActivos = profesoresActivos,
                    ProfesoresConCursos = profesoresConCursos,
                    ProfesoresSinCursos = profesoresSinCursos,
                    BusquedaTexto = busqueda,
                    SoloActivos = soloActivos,
                    SoloConCursos = soloConCursos,
                    FiltroArea = area,
                    Ordenamiento = orden,
                    PaginaActual = pagina,
                    ProfesoresPorPagina = profesoresPorPagina,
                    TotalPaginas = totalPaginas,
                    AreasDisponibles = areasDisponibles
                };

                _logger.LogInformation($"✅ Lista cargada: {profesoresViewModel.Count} profesores en página {pagina} de {totalPaginas}");
                return View(viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error al cargar lista de profesores");
                TempData["Error"] = "Error al cargar la lista de profesores";
                return View(new ProfesoresIndexViewModel());
            }
        }

        // ============================================
        // VER CURSOS DE UN PROFESOR
        // ============================================

        /// <summary>
        /// Vista con los cursos asignados a un profesor específico
        /// GET: /AdminProfesores/Cursos/{id}
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> Cursos(int id, EstadoCurso? estado, string? orden)
        {
            try
            {
                var profesor = await _context.Usuarios
                    .Include(u => u.Rol)
                    .FirstOrDefaultAsync(u => u.IdUsuario == id);

                if (profesor == null)
                {
                    _logger.LogWarning($"⚠️ Profesor con ID {id} no encontrado");
                    TempData["Error"] = "Profesor no encontrado";
                    return RedirectToAction(nameof(Index));
                }

                if (profesor.Rol?.NombreRol != "Profesor")
                {
                    _logger.LogWarning($"⚠️ Usuario {id} no es profesor");
                    TempData["Error"] = "El usuario no es un profesor";
                    return RedirectToAction(nameof(Index));
                }

                // Obtener cursos asignados
                var query = _context.Cursos
                    .Where(c => c.ProfesorAsignado == id && !c.EsEliminado);

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
                    FechaAsignacion = c.FechaCreacion // Por ahora usamos fecha de creación
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
                    EsVistaAdmin = true
                };

                _logger.LogInformation($"✅ Mostrando {totalCursos} cursos del profesor {profesor.Nombres} {profesor.Apellidos}");
                return View(viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"❌ Error al cargar cursos del profesor ID {id}");
                TempData["Error"] = "Error al cargar los cursos del profesor";
                return RedirectToAction(nameof(Index));
            }
        }

        // ============================================
        // DESASIGNAR PROFESOR
        // ============================================

        /// <summary>
        /// Desasignar profesor de un curso
        /// POST: /AdminProfesores/DesasignarProfesor
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DesasignarProfesor(int cursoId, string? motivo)
        {
            try
            {
                var curso = await _context.Cursos
                    .Include(c => c.Profesor)
                    .FirstOrDefaultAsync(c => c.Id == cursoId && !c.EsEliminado);

                if (curso == null)
                {
                    return Json(new { success = false, mensaje = "Curso no encontrado" });
                }

                if (curso.ProfesorAsignado == null)
                {
                    return Json(new { success = false, mensaje = "El curso no tiene profesor asignado" });
                }

                // Advertir si curso está en curso
                if (curso.Estado == EstadoCurso.EnCurso)
                {
                    _logger.LogWarning($"⚠️ Desasignando profesor de curso EnCurso: {curso.Codigo}");
                }

                var nombreProfesor = curso.Profesor != null ? $"{curso.Profesor.Nombres} {curso.Profesor.Apellidos}" : "Desconocido";

                // Desasignar
                curso.ProfesorAsignado = null;
                curso.FechaUltimaActualizacion = DateTime.Now;

                await _context.SaveChangesAsync();

                _logger.LogInformation($"✅ Profesor {nombreProfesor} desasignado del curso {curso.Codigo}. Motivo: {motivo}");

                return Json(new
                {
                    success = true,
                    mensaje = $"Profesor {nombreProfesor} desasignado exitosamente"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"❌ Error al desasignar profesor del curso ID {cursoId}");
                return Json(new { success = false, mensaje = "Error al desasignar el profesor" });
            }
        }
    }
}
