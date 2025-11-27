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
    /// Controlador para la gestión completa de cursos (CRUD)
    /// Solo accesible para Administradores
    /// </summary>
    [Authorize(Roles = "Administrador")]
    public class AdminCursosController : Controller
    {
        private readonly AppDBContext _context;
        private readonly ILogger<AdminCursosController> _logger;

        public AdminCursosController(AppDBContext context, ILogger<AdminCursosController> logger)
        {
            _context = context;
            _logger = logger;
        }

        // ============================================
        // LISTAR CURSOS (INDEX)
        // ============================================

        /// <summary>
        /// Vista principal con grid de cursos, filtros y búsqueda
        /// GET: /AdminCursos/Index
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> Index(
            string? busqueda,
            EstadoCurso? estado,
            Modalidad? modalidad,
            string? categoria,
            NivelCurso? nivel,
            string? orden,
            int pagina = 1)
        {
            try
            {
                _logger.LogInformation($"📚 Cargando lista de cursos - Página {pagina}");

                // Query base: solo cursos no eliminados
                var query = _context.Cursos
                    .Include(c => c.Profesor)
                    .Where(c => !c.EsEliminado);

                // Aplicar búsqueda por texto
                if (!string.IsNullOrWhiteSpace(busqueda))
                {
                    busqueda = busqueda.ToLower().Trim();
                    query = query.Where(c =>
                        c.Titulo.ToLower().Contains(busqueda) ||
                        c.Codigo.ToLower().Contains(busqueda) ||
                        c.Categoria.ToLower().Contains(busqueda));
                }

                // Aplicar filtros
                if (estado.HasValue)
                {
                    query = query.Where(c => c.Estado == estado.Value);
                }

                if (modalidad.HasValue)
                {
                    query = query.Where(c => c.Modalidad == modalidad.Value);
                }

                if (!string.IsNullOrWhiteSpace(categoria))
                {
                    query = query.Where(c => c.Categoria == categoria);
                }

                if (nivel.HasValue)
                {
                    query = query.Where(c => c.Nivel == nivel.Value);
                }

                // Aplicar ordenamiento
                query = orden switch
                {
                    "alfabetico" => query.OrderBy(c => c.Titulo),
                    "proximosIniciar" => query.OrderBy(c => c.FechaInicio ?? DateTime.MaxValue),
                    _ => query.OrderByDescending(c => c.FechaCreacion) // "recientes" por defecto
                };

                // Calcular estadísticas
                var todosLosCursos = await _context.Cursos
                    .Where(c => !c.EsEliminado)
                    .ToListAsync();

                int totalCursos = todosLosCursos.Count;
                int cursosPublicados = todosLosCursos.Count(c => c.Estado == EstadoCurso.Publicado);
                int cursosEnCurso = todosLosCursos.Count(c => c.Estado == EstadoCurso.EnCurso);
                int cursosFinalizados = todosLosCursos.Count(c => c.Estado == EstadoCurso.Finalizado);

                // Paginación
                int cursosPorPagina = 9;
                int totalItems = await query.CountAsync();
                int totalPaginas = (int)Math.Ceiling(totalItems / (double)cursosPorPagina);

                var cursos = await query
                    .Skip((pagina - 1) * cursosPorPagina)
                    .Take(cursosPorPagina)
                    .Select(c => new CursoCardViewModel
                    {
                        Id = c.Id,
                        Codigo = c.Codigo,
                        Titulo = c.Titulo,
                        Categoria = c.Categoria,
                        Nivel = c.Nivel,
                        Estado = c.Estado,
                        Duracion = c.Duracion,
                        CapacidadMaxima = c.CapacidadMaxima,
                        Modalidad = c.Modalidad,
                        ImagenUrl = c.ImagenUrl,
                        FechaInicio = c.FechaInicio,
                        NombreProfesor = c.Profesor != null ? $"{c.Profesor.Nombres} {c.Profesor.Apellidos}" : null
                    })
                    .ToListAsync();

                var viewModel = new CursoIndexViewModel
                {
                    Cursos = cursos,
                    TotalCursos = totalCursos,
                    CursosPublicados = cursosPublicados,
                    CursosEnCurso = cursosEnCurso,
                    CursosFinalizados = cursosFinalizados,
                    BusquedaTexto = busqueda,
                    FiltroEstado = estado,
                    FiltroModalidad = modalidad,
                    FiltroCategoria = categoria,
                    FiltroNivel = nivel,
                    Ordenamiento = orden,
                    PaginaActual = pagina,
                    CursosPorPagina = cursosPorPagina,
                    TotalPaginas = totalPaginas
                };

                _logger.LogInformation($"✅ Lista cargada: {cursos.Count} cursos en página {pagina} de {totalPaginas}");
                return View(viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error al cargar lista de cursos");
                TempData["Error"] = "Error al cargar la lista de cursos";
                return View(new CursoIndexViewModel());
            }
        }

        // ============================================
        // VER DETALLE DE CURSO
        // ============================================

        /// <summary>
        /// Vista detallada de un curso
        /// GET: /AdminCursos/Detalle/{id}
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> Detalle(int id)
        {
            try
            {
                var curso = await _context.Cursos
                    .Include(c => c.Profesor)
                    .Include(c => c.Creador)
                    .FirstOrDefaultAsync(c => c.Id == id && !c.EsEliminado);

                if (curso == null)
                {
                    _logger.LogWarning($"⚠️ Curso con ID {id} no encontrado");
                    TempData["Error"] = "Curso no encontrado";
                    return RedirectToAction(nameof(Index));
                }

                var viewModel = new DetalleCursoViewModel
                {
                    Id = curso.Id,
                    Codigo = curso.Codigo,
                    Titulo = curso.Titulo,
                    Descripcion = curso.Descripcion,
                    Objetivos = curso.Objetivos,
                    Duracion = curso.Duracion,
                    Modalidad = curso.Modalidad,
                    Categoria = curso.Categoria,
                    Nivel = curso.Nivel,
                    CapacidadMaxima = curso.CapacidadMaxima,
                    Estado = curso.Estado,
                    ImagenUrl = curso.ImagenUrl,
                    FechaInicio = curso.FechaInicio,
                    FechaFin = curso.FechaFin,
                    FechaPublicacion = curso.FechaPublicacion,
                    ProfesorAsignado = curso.ProfesorAsignado,
                    NombreProfesor = curso.Profesor != null ? $"{curso.Profesor.Nombres} {curso.Profesor.Apellidos}" : null,
                    EmailProfesor = curso.Profesor?.CorreoElectronico,
                    CreadoPor = curso.CreadoPor,
                    NombreCreador = $"{curso.Creador.Nombres} {curso.Creador.Apellidos}",
                    FechaCreacion = curso.FechaCreacion,
                    FechaUltimaActualizacion = curso.FechaUltimaActualizacion,
                    PermitirEdicion = curso.Estado != EstadoCurso.Finalizado,
                    PermitirEliminacion = curso.Estado != EstadoCurso.EnCurso,
                    PermitirCambioEstado = true,
                    MensajeAdvertencia = curso.Estado == EstadoCurso.EnCurso
                        ? "Este curso está actualmente en ejecución. Algunos cambios están restringidos."
                        : null
                };

                _logger.LogInformation($"✅ Mostrando detalle del curso: {curso.Codigo}");
                return View(viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"❌ Error al cargar detalle del curso ID {id}");
                TempData["Error"] = "Error al cargar el detalle del curso";
                return RedirectToAction(nameof(Index));
            }
        }

        // ============================================
        // CREAR CURSO
        // ============================================

        /// <summary>
        /// Formulario para crear un nuevo curso
        /// GET: /AdminCursos/Crear
        /// </summary>
        [HttpGet]
        public IActionResult Crear()
        {
            var viewModel = new CrearCursoViewModel
            {
                Estado = EstadoCurso.Borrador,
                Codigo = GenerarCodigoCurso() // Auto-generar código
            };

            return View(viewModel);
        }

        /// <summary>
        /// Procesar la creación de un nuevo curso
        /// POST: /AdminCursos/Crear
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Crear(CrearCursoViewModel model)
        {
            try
            {
                // Validaciones personalizadas
                if (ModelState.IsValid)
                {
                    // Validar fechas
                    if (model.FechaInicio.HasValue && model.FechaFin.HasValue)
                    {
                        if (model.FechaFin.Value <= model.FechaInicio.Value)
                        {
                            ModelState.AddModelError("FechaFin", "La fecha de fin debe ser posterior a la fecha de inicio");
                            return View(model);
                        }
                    }

                    // Generar código si no se proporcionó
                    if (string.IsNullOrWhiteSpace(model.Codigo))
                    {
                        model.Codigo = GenerarCodigoCurso();
                    }

                    // Verificar código único
                    var codigoExiste = await _context.Cursos
                        .AnyAsync(c => c.Codigo == model.Codigo);

                    if (codigoExiste)
                    {
                        ModelState.AddModelError("Codigo", "Este código de curso ya existe");
                        return View(model);
                    }

                    // Crear entidad Curso
                    var curso = new Curso
                    {
                        Codigo = model.Codigo!,
                        Titulo = model.Titulo,
                        Descripcion = model.Descripcion,
                        Objetivos = model.Objetivos,
                        Duracion = model.Duracion,
                        Modalidad = model.Modalidad,
                        Categoria = model.Categoria,
                        Nivel = model.Nivel,
                        CapacidadMaxima = model.CapacidadMaxima,
                        Estado = model.Estado,
                        ImagenUrl = model.ImagenUrl,
                        FechaInicio = model.FechaInicio,
                        FechaFin = model.FechaFin,
                        FechaPublicacion = model.Estado == EstadoCurso.Publicado ? DateTime.Now : null,
                        CreadoPor = User.GetUserId(),
                        FechaCreacion = DateTime.Now,
                        EsEliminado = false
                    };

                    _context.Cursos.Add(curso);
                    await _context.SaveChangesAsync();

                    _logger.LogInformation($"✅ Curso creado exitosamente: {curso.Codigo} por usuario {User.GetUserName()}");
                    TempData["Exito"] = $"Curso '{curso.Titulo}' creado exitosamente";

                    return RedirectToAction(nameof(Detalle), new { id = curso.Id });
                }

                return View(model);
            }
            catch (DbUpdateException ex)
            {
                _logger.LogError(ex, "❌ Error de base de datos al crear curso");
                TempData["Error"] = "Error al guardar el curso en la base de datos";
                return View(model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error inesperado al crear curso");
                TempData["Error"] = "Error inesperado al crear el curso";
                return View(model);
            }
        }

        // ============================================
        // EDITAR CURSO
        // ============================================

        /// <summary>
        /// Formulario para editar un curso existente
        /// GET: /AdminCursos/Editar/{id}
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> Editar(int id)
        {
            try
            {
                var curso = await _context.Cursos
                    .FirstOrDefaultAsync(c => c.Id == id && !c.EsEliminado);

                if (curso == null)
                {
                    _logger.LogWarning($"⚠️ Curso con ID {id} no encontrado para edición");
                    TempData["Error"] = "Curso no encontrado";
                    return RedirectToAction(nameof(Index));
                }

                var viewModel = new EditarCursoViewModel
                {
                    Id = curso.Id,
                    Codigo = curso.Codigo,
                    Titulo = curso.Titulo,
                    Descripcion = curso.Descripcion,
                    Objetivos = curso.Objetivos,
                    Duracion = curso.Duracion,
                    Modalidad = curso.Modalidad,
                    Categoria = curso.Categoria,
                    Nivel = curso.Nivel,
                    CapacidadMaxima = curso.CapacidadMaxima,
                    Estado = curso.Estado,
                    ImagenUrl = curso.ImagenUrl,
                    FechaInicio = curso.FechaInicio,
                    FechaFin = curso.FechaFin,
                    EstadoActual = curso.Estado,
                    PermitirCambioFechas = curso.Estado != EstadoCurso.EnCurso,
                    PermitirEdicionCompleta = curso.Estado != EstadoCurso.Finalizado
                };

                return View(viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"❌ Error al cargar formulario de edición para curso ID {id}");
                TempData["Error"] = "Error al cargar el formulario de edición";
                return RedirectToAction(nameof(Index));
            }
        }

        /// <summary>
        /// Procesar la edición de un curso
        /// POST: /AdminCursos/Editar/{id}
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Editar(int id, EditarCursoViewModel model)
        {
            if (id != model.Id)
            {
                return NotFound();
            }

            try
            {
                if (ModelState.IsValid)
                {
                    var curso = await _context.Cursos
                        .FirstOrDefaultAsync(c => c.Id == id && !c.EsEliminado);

                    if (curso == null)
                    {
                        TempData["Error"] = "Curso no encontrado";
                        return RedirectToAction(nameof(Index));
                    }

                    // Validaciones según estado
                    if (curso.Estado == EstadoCurso.EnCurso)
                    {
                        if (model.FechaInicio != curso.FechaInicio || model.FechaFin != curso.FechaFin)
                        {
                            ModelState.AddModelError("", "No se pueden cambiar las fechas de un curso en curso");
                            return View(model);
                        }
                    }

                    // Validar fechas
                    if (model.FechaInicio.HasValue && model.FechaFin.HasValue)
                    {
                        if (model.FechaFin.Value <= model.FechaInicio.Value)
                        {
                            ModelState.AddModelError("FechaFin", "La fecha de fin debe ser posterior a la fecha de inicio");
                            return View(model);
                        }
                    }

                    // Actualizar propiedades
                    curso.Titulo = model.Titulo;
                    curso.Descripcion = model.Descripcion;
                    curso.Objetivos = model.Objetivos;
                    curso.Duracion = model.Duracion;
                    curso.Modalidad = model.Modalidad;
                    curso.Categoria = model.Categoria;
                    curso.Nivel = model.Nivel;
                    curso.CapacidadMaxima = model.CapacidadMaxima;
                    curso.ImagenUrl = model.ImagenUrl;
                    curso.FechaUltimaActualizacion = DateTime.Now;

                    // Actualizar estado y fechas si aplica
                    if (curso.Estado != EstadoCurso.Finalizado)
                    {
                        var estadoAnterior = curso.Estado;
                        curso.Estado = model.Estado;

                        // Si cambia de Borrador a Publicado, registrar fecha de publicación
                        if (estadoAnterior == EstadoCurso.Borrador && model.Estado == EstadoCurso.Publicado)
                        {
                            curso.FechaPublicacion = DateTime.Now;
                        }
                    }

                    if (curso.Estado != EstadoCurso.EnCurso)
                    {
                        curso.FechaInicio = model.FechaInicio;
                        curso.FechaFin = model.FechaFin;
                    }

                    await _context.SaveChangesAsync();

                    _logger.LogInformation($"✅ Curso actualizado: {curso.Codigo}");
                    TempData["Exito"] = "Curso actualizado exitosamente";

                    return RedirectToAction(nameof(Detalle), new { id = curso.Id });
                }

                return View(model);
            }
            catch (DbUpdateConcurrencyException ex)
            {
                _logger.LogError(ex, $"❌ Error de concurrencia al actualizar curso ID {id}");
                TempData["Error"] = "El curso fue modificado por otro usuario";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"❌ Error al actualizar curso ID {id}");
                TempData["Error"] = "Error al actualizar el curso";
                return View(model);
            }
        }

        // ============================================
        // ELIMINAR CURSO (SOFT DELETE)
        // ============================================

        /// <summary>
        /// Confirmar eliminación de curso
        /// POST: /AdminCursos/Eliminar/{id}
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Eliminar(int id)
        {
            try
            {
                var curso = await _context.Cursos
                    .FirstOrDefaultAsync(c => c.Id == id && !c.EsEliminado);

                if (curso == null)
                {
                    _logger.LogWarning($"⚠️ Intento de eliminar curso inexistente ID {id}");
                    TempData["Error"] = "Curso no encontrado";
                    return RedirectToAction(nameof(Index));
                }

                // Validar que no esté en curso
                if (curso.Estado == EstadoCurso.EnCurso)
                {
                    _logger.LogWarning($"⚠️ Intento de eliminar curso en curso: {curso.Codigo}");
                    TempData["Error"] = "No se puede eliminar un curso que está en ejecución. Primero debe cancelarlo.";
                    return RedirectToAction(nameof(Detalle), new { id });
                }

                // Soft delete
                curso.EsEliminado = true;
                curso.FechaEliminacion = DateTime.Now;

                await _context.SaveChangesAsync();

                _logger.LogInformation($"✅ Curso eliminado (soft delete): {curso.Codigo} por usuario {User.GetUserName()}");
                TempData["Exito"] = $"Curso '{curso.Titulo}' eliminado exitosamente";

                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"❌ Error al eliminar curso ID {id}");
                TempData["Error"] = "Error al eliminar el curso";
                return RedirectToAction(nameof(Detalle), new { id });
            }
        }

        // ============================================
        // CAMBIAR ESTADO DE CURSO
        // ============================================

        /// <summary>
        /// Cambiar el estado de un curso
        /// POST: /AdminCursos/CambiarEstado
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CambiarEstado(int id, EstadoCurso nuevoEstado)
        {
            try
            {
                var curso = await _context.Cursos
                    .FirstOrDefaultAsync(c => c.Id == id && !c.EsEliminado);

                if (curso == null)
                {
                    return Json(new { success = false, mensaje = "Curso no encontrado" });
                }

                var estadoAnterior = curso.Estado;

                // Validar transiciones de estado
                var transicionValida = ValidarTransicionEstado(estadoAnterior, nuevoEstado, curso);
                if (!transicionValida.esValida)
                {
                    return Json(new { success = false, mensaje = transicionValida.mensaje });
                }

                // Aplicar cambio de estado
                curso.Estado = nuevoEstado;
                curso.FechaUltimaActualizacion = DateTime.Now;

                // Actualizar fecha de publicación si aplica
                if (estadoAnterior == EstadoCurso.Borrador && nuevoEstado == EstadoCurso.Publicado)
                {
                    curso.FechaPublicacion = DateTime.Now;
                }

                await _context.SaveChangesAsync();

                _logger.LogInformation($"✅ Estado del curso {curso.Codigo} cambiado de {estadoAnterior} a {nuevoEstado}");

                return Json(new
                {
                    success = true,
                    mensaje = $"Estado cambiado exitosamente de {estadoAnterior} a {nuevoEstado}",
                    nuevoEstado = nuevoEstado.ToString()
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"❌ Error al cambiar estado del curso ID {id}");
                return Json(new { success = false, mensaje = "Error al cambiar el estado del curso" });
            }
        }

        // ============================================
        // MÉTODOS AUXILIARES
        // ============================================

        /// <summary>
        /// Genera un código único para un nuevo curso
        /// Formato: CURSO-{AÑO}-{NÚMERO}
        /// Ejemplo: CURSO-2024-001
        /// </summary>
        private string GenerarCodigoCurso()
        {
            var año = DateTime.Now.Year;
            var ultimoCurso = _context.Cursos
                .Where(c => c.Codigo.StartsWith($"CURSO-{año}"))
                .OrderByDescending(c => c.Id)
                .FirstOrDefault();

            int numero = 1;
            if (ultimoCurso != null)
            {
                var partes = ultimoCurso.Codigo.Split('-');
                if (partes.Length == 3 && int.TryParse(partes[2], out int ultimoNumero))
                {
                    numero = ultimoNumero + 1;
                }
            }

            return $"CURSO-{año}-{numero:D3}";
        }

        /// <summary>
        /// Valida si una transición de estado es permitida
        /// </summary>
        private (bool esValida, string mensaje) ValidarTransicionEstado(
            EstadoCurso estadoActual,
            EstadoCurso nuevoEstado,
            Curso curso)
        {
            // No cambiar si es el mismo estado
            if (estadoActual == nuevoEstado)
            {
                return (false, "El curso ya está en ese estado");
            }

            // Validar transiciones permitidas
            switch (estadoActual)
            {
                case EstadoCurso.Borrador:
                    if (nuevoEstado != EstadoCurso.Publicado)
                    {
                        return (false, "Un curso en Borrador solo puede pasar a Publicado");
                    }
                    // Validar que tenga toda la información necesaria
                    if (string.IsNullOrWhiteSpace(curso.Descripcion) || curso.Duracion == 0)
                    {
                        return (false, "El curso debe tener toda la información completa antes de publicarse");
                    }
                    break;

                case EstadoCurso.Publicado:
                    if (nuevoEstado == EstadoCurso.EnCurso)
                    {
                        if (!curso.FechaInicio.HasValue)
                        {
                            return (false, "El curso debe tener una fecha de inicio definida");
                        }
                        if (curso.FechaInicio.Value > DateTime.Now)
                        {
                            return (false, "No se puede iniciar el curso antes de la fecha programada");
                        }
                    }
                    else if (nuevoEstado != EstadoCurso.Cancelado && nuevoEstado != EstadoCurso.Borrador)
                    {
                        return (false, "Un curso Publicado solo puede pasar a EnCurso, Cancelado o volver a Borrador");
                    }
                    break;

                case EstadoCurso.EnCurso:
                    if (nuevoEstado == EstadoCurso.Finalizado)
                    {
                        if (curso.FechaFin.HasValue && curso.FechaFin.Value > DateTime.Now)
                        {
                            return (false, "El curso no puede finalizarse antes de la fecha programada");
                        }
                    }
                    else if (nuevoEstado != EstadoCurso.Cancelado)
                    {
                        return (false, "Un curso EnCurso solo puede pasar a Finalizado o Cancelado");
                    }
                    break;

                case EstadoCurso.Finalizado:
                    return (false, "No se puede cambiar el estado de un curso finalizado");

                case EstadoCurso.Cancelado:
                    return (false, "No se puede cambiar el estado de un curso cancelado");
            }

            return (true, "Transición válida");
        }

        // ============================================
        // ASIGNAR PROFESOR AL CURSO
        // ============================================

        /// <summary>
        /// Muestra el formulario para asignar profesor a un curso
        /// GET: /AdminCursos/AsignarProfesor/{id}
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> AsignarProfesor(int id)
        {
            try
            {
                var curso = await _context.Cursos
                    .Include(c => c.Profesor)
                    .FirstOrDefaultAsync(c => c.Id == id && !c.EsEliminado);

                if (curso == null)
                {
                    _logger.LogWarning($"⚠️ Curso con ID {id} no encontrado para asignación");
                    TempData["Error"] = "Curso no encontrado";
                    return RedirectToAction(nameof(Index));
                }

                // No permitir asignar a cursos cancelados o eliminados
                if (curso.Estado == EstadoCurso.Cancelado)
                {
                    TempData["Error"] = "No se puede asignar profesor a un curso cancelado";
                    return RedirectToAction(nameof(Detalle), new { id });
                }

                // Obtener rol Profesor
                var rolProfesor = await _context.Rols
                    .FirstOrDefaultAsync(r => r.NombreRol == "Profesor");

                if (rolProfesor == null)
                {
                    TempData["Error"] = "No se encontró el rol Profesor en el sistema";
                    return RedirectToAction(nameof(Detalle), new { id });
                }

                // Obtener profesores disponibles
                var profesores = await _context.Usuarios
                    .Where(u => u.IdRol == rolProfesor.IdRol && u.Estado == true)
                    .OrderBy(u => u.Nombres)
                    .ToListAsync();

                // Obtener cursos activos de cada profesor para calcular carga
                var todosCursos = await _context.Cursos
                    .Where(c => !c.EsEliminado && (c.Estado == EstadoCurso.Publicado || c.Estado == EstadoCurso.EnCurso))
                    .ToListAsync();

                var profesoresDisponibles = profesores.Select(p =>
                {
                    var cursosProfesor = todosCursos.Where(c => c.ProfesorAsignado == p.IdUsuario).ToList();
                    return new ProfesorDisponibleItem
                    {
                        IdUsuario = p.IdUsuario,
                        NombreCompleto = $"{p.Nombres} {p.Apellidos}",
                        Email = p.CorreoElectronico,
                        Area = p.Area,
                        CursosAsignados = cursosProfesor.Count,
                        HorasCargaTrabajo = cursosProfesor.Sum(c => c.Duracion)
                    };
                }).ToList();

                var viewModel = new AsignarProfesorViewModel
                {
                    CursoId = curso.Id,
                    CodigoCurso = curso.Codigo,
                    TituloCurso = curso.Titulo,
                    CategoriaCurso = curso.Categoria,
                    ProfesorActualId = curso.ProfesorAsignado,
                    NombreProfesorActual = curso.Profesor != null ? $"{curso.Profesor.Nombres} {curso.Profesor.Apellidos}" : null,
                    FechaAsignacionActual = curso.FechaCreacion, // Temporal, idealmente debería ser FechaAsignacion
                    ProfesoresDisponibles = profesoresDisponibles
                };

                return View(viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"❌ Error al cargar formulario de asignación para curso ID {id}");
                TempData["Error"] = "Error al cargar el formulario de asignación";
                return RedirectToAction(nameof(Index));
            }
        }

        /// <summary>
        /// Procesa la asignación de profesor a un curso
        /// POST: /AdminCursos/AsignarProfesor
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AsignarProfesor(AsignarProfesorViewModel model)
        {
            try
            {
                var curso = await _context.Cursos
                    .Include(c => c.Profesor)
                    .FirstOrDefaultAsync(c => c.Id == model.CursoId && !c.EsEliminado);

                if (curso == null)
                {
                    TempData["Error"] = "Curso no encontrado";
                    return RedirectToAction(nameof(Index));
                }

                // Si se selecciona "Sin asignar"
                if (model.NuevoProfesorId == null || model.NuevoProfesorId == 0)
                {
                    if (curso.ProfesorAsignado != null)
                    {
                        var profesorAnterior = curso.Profesor != null ? $"{curso.Profesor.Nombres} {curso.Profesor.Apellidos}" : "Desconocido";
                        curso.ProfesorAsignado = null;
                        curso.FechaUltimaActualizacion = DateTime.Now;

                        await _context.SaveChangesAsync();

                        _logger.LogInformation($"✅ Profesor {profesorAnterior} desasignado del curso {curso.Codigo}");
                        TempData["Exito"] = $"Profesor {profesorAnterior} desasignado exitosamente";
                    }
                    else
                    {
                        TempData["Error"] = "El curso no tiene profesor asignado";
                    }

                    return RedirectToAction(nameof(Detalle), new { id = curso.Id });
                }

                // Validar que el profesor existe y es profesor
                var nuevoProfesor = await _context.Usuarios
                    .Include(u => u.Rol)
                    .FirstOrDefaultAsync(u => u.IdUsuario == model.NuevoProfesorId.Value);

                if (nuevoProfesor == null)
                {
                    TempData["Error"] = "Profesor no encontrado";
                    return RedirectToAction(nameof(AsignarProfesor), new { id = curso.Id });
                }

                if (nuevoProfesor.Rol?.NombreRol != "Profesor")
                {
                    TempData["Error"] = "El usuario seleccionado no es un profesor";
                    return RedirectToAction(nameof(AsignarProfesor), new { id = curso.Id });
                }

                if (!nuevoProfesor.Estado)
                {
                    TempData["Error"] = "El profesor seleccionado no está activo";
                    return RedirectToAction(nameof(AsignarProfesor), new { id = curso.Id });
                }

                // Registrar cambio
                var profesorAnteriorNombre = curso.Profesor != null ? $"{curso.Profesor.Nombres} {curso.Profesor.Apellidos}" : null;
                var nuevoProfesorNombre = $"{nuevoProfesor.Nombres} {nuevoProfesor.Apellidos}";

                // Asignar nuevo profesor
                curso.ProfesorAsignado = nuevoProfesor.IdUsuario;
                curso.FechaUltimaActualizacion = DateTime.Now;

                await _context.SaveChangesAsync();

                if (profesorAnteriorNombre != null)
                {
                    _logger.LogInformation($"✅ Profesor del curso {curso.Codigo} cambiado de {profesorAnteriorNombre} a {nuevoProfesorNombre}");
                    TempData["Exito"] = $"Profesor cambiado exitosamente de {profesorAnteriorNombre} a {nuevoProfesorNombre}";
                }
                else
                {
                    _logger.LogInformation($"✅ Profesor {nuevoProfesorNombre} asignado al curso {curso.Codigo}");
                    TempData["Exito"] = $"Profesor {nuevoProfesorNombre} asignado exitosamente al curso";
                }

                return RedirectToAction(nameof(Detalle), new { id = curso.Id });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"❌ Error al asignar profesor al curso ID {model.CursoId}");
                TempData["Error"] = "Error al asignar el profesor";
                return RedirectToAction(nameof(AsignarProfesor), new { id = model.CursoId });
            }
        }
    }
}
