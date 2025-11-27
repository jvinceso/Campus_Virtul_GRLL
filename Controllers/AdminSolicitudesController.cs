using Campus_Virtul_GRLL.Data;
using Campus_Virtul_GRLL.Helpers;
using Campus_Virtul_GRLL.Models;
using Campus_Virtul_GRLL.Models.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Campus_Virtul_GRLL.Controllers
{
    /// <summary>
    /// Controlador para gestión de solicitudes de cuenta (Admin)
    /// Solo accesible para Administradores
    /// </summary>
    [Authorize(Roles = "Administrador")]
    public class AdminSolicitudesController : Controller
    {
        private readonly AppDBContext _context;
        private readonly ILogger<AdminSolicitudesController> _logger;

        public AdminSolicitudesController(AppDBContext context, ILogger<AdminSolicitudesController> logger)
        {
            _context = context;
            _logger = logger;
        }

        // ============================================
        // INDEX - Lista de solicitudes con filtros
        // ============================================
        [HttpGet]
        public async Task<IActionResult> Index(
            string? busqueda,
            EstadoSolicitud? filtroEstado,
            int? filtroRol,
            int pagina = 1)
        {
            _logger.LogInformation($"📋 Admin {User.GetUserName()} accedió a lista de solicitudes");

            // Query base
            var query = _context.Solicituds
                .Include(s => s.Rol)
                .Include(s => s.UsuarioRespondio)
                .AsQueryable();

            // Aplicar búsqueda
            if (!string.IsNullOrWhiteSpace(busqueda))
            {
                busqueda = busqueda.Trim().ToLower();
                query = query.Where(s =>
                    s.Nombres.ToLower().Contains(busqueda) ||
                    s.Apellidos.ToLower().Contains(busqueda) ||
                    s.DNI.Contains(busqueda) ||
                    s.CorreoElectronico.ToLower().Contains(busqueda));
            }

            // Aplicar filtro por estado
            if (filtroEstado.HasValue)
            {
                query = query.Where(s => s.Estado == filtroEstado.Value);
            }

            // Aplicar filtro por rol
            if (filtroRol.HasValue)
            {
                query = query.Where(s => s.IdRol == filtroRol.Value);
            }

            // Calcular estadísticas
            var totalSolicitudes = await query.CountAsync();
            var pendientes = await _context.Solicituds.CountAsync(s => s.Estado == EstadoSolicitud.Pendiente);
            var aprobadas = await _context.Solicituds.CountAsync(s => s.Estado == EstadoSolicitud.Aprobada);
            var rechazadas = await _context.Solicituds.CountAsync(s => s.Estado == EstadoSolicitud.Rechazada);

            // Obtener roles disponibles para el filtro
            var rolesDisponibles = await _context.Rols
                .Where(r => r.Estado && (r.NombreRol == "Colaborador" || r.NombreRol == "Practicante"))
                .OrderBy(r => r.NombreRol)
                .ToListAsync();

            // Paginación
            var solicitudesPorPagina = 10;
            var totalPaginas = (int)Math.Ceiling(totalSolicitudes / (double)solicitudesPorPagina);
            pagina = Math.Max(1, Math.Min(pagina, totalPaginas == 0 ? 1 : totalPaginas));

            var solicitudes = await query
                .OrderByDescending(s => s.FechaSolicitud)
                .ThenByDescending(s => s.IdSolicitud)
                .Skip((pagina - 1) * solicitudesPorPagina)
                .Take(solicitudesPorPagina)
                .Select(s => new SolicitudCardViewModel
                {
                    IdSolicitud = s.IdSolicitud,
                    NombreCompleto = $"{s.Nombres} {s.Apellidos}",
                    DNI = s.DNI,
                    Email = s.CorreoElectronico,
                    Area = s.Area,
                    RolSolicitado = s.Rol!.NombreRol,
                    FechaSolicitud = s.FechaSolicitud,
                    Estado = s.Estado,
                    FechaRespuesta = s.FechaRespuesta,
                    NombreAprobador = s.UsuarioRespondio != null
                        ? $"{s.UsuarioRespondio.Nombres} {s.UsuarioRespondio.Apellidos}"
                        : null
                })
                .ToListAsync();

            var viewModel = new SolicitudesIndexViewModel
            {
                Solicitudes = solicitudes,
                TotalSolicitudes = totalSolicitudes,
                SolicitudesPendientes = pendientes,
                SolicitudesAprobadas = aprobadas,
                SolicitudesRechazadas = rechazadas,
                BusquedaTexto = busqueda,
                FiltroEstado = filtroEstado,
                FiltroRol = filtroRol,
                PaginaActual = pagina,
                TotalPaginas = totalPaginas,
                RolesDisponibles = rolesDisponibles
            };

            return View(viewModel);
        }

        // ============================================
        // DETALLE - Ver solicitud completa
        // ============================================
        [HttpGet]
        public async Task<IActionResult> Detalle(int id)
        {
            var solicitud = await _context.Solicituds
                .Include(s => s.Rol)
                .Include(s => s.UsuarioRespondio)
                .Include(s => s.UsuarioGenerado)
                .FirstOrDefaultAsync(s => s.IdSolicitud == id);

            if (solicitud == null)
            {
                _logger.LogWarning($"⚠️ Intento de ver solicitud inexistente ID: {id}");
                TempData["Error"] = "Solicitud no encontrada";
                return RedirectToAction(nameof(Index));
            }

            // Verificar si ya existe usuario con ese DNI o Email
            var yaExisteUsuario = await _context.Usuarios
                .AnyAsync(u => (u.DNI == solicitud.DNI || u.CorreoElectronico == solicitud.CorreoElectronico)
                    && !u.EsEliminado);

            string? mensajeValidacion = null;
            if (yaExisteUsuario && solicitud.Estado == EstadoSolicitud.Pendiente)
            {
                mensajeValidacion = "⚠️ Ya existe un usuario activo con este DNI o Email. No se puede aprobar.";
            }

            var viewModel = new DetalleSolicitudViewModel
            {
                IdSolicitud = solicitud.IdSolicitud,
                Nombres = solicitud.Nombres,
                Apellidos = solicitud.Apellidos,
                DNI = solicitud.DNI,
                Email = solicitud.CorreoElectronico,
                Telefono = solicitud.Telefono,
                Area = solicitud.Area,
                RolSolicitado = solicitud.Rol!.NombreRol,
                IdRolSolicitado = solicitud.IdRol,
                FechaSolicitud = solicitud.FechaSolicitud,
                Estado = solicitud.Estado,
                FechaRespuesta = solicitud.FechaRespuesta,
                NombreAprobador = solicitud.UsuarioRespondio != null
                    ? $"{solicitud.UsuarioRespondio.Nombres} {solicitud.UsuarioRespondio.Apellidos}"
                    : null,
                MotivoRechazo = solicitud.MotivoRechazo,
                IdUsuarioCreado = solicitud.UsuarioCreado,
                YaExisteUsuario = yaExisteUsuario,
                MensajeValidacion = mensajeValidacion
            };

            _logger.LogInformation($"👁️ Admin {User.GetUserName()} vio detalle de solicitud ID: {id}");

            return View(viewModel);
        }

        // ============================================
        // APROBAR - Aprobar solicitud y crear usuario
        // ============================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Aprobar(int id)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                var solicitud = await _context.Solicituds
                    .Include(s => s.Rol)
                    .FirstOrDefaultAsync(s => s.IdSolicitud == id);

                if (solicitud == null)
                {
                    _logger.LogWarning($"⚠️ Intento de aprobar solicitud inexistente ID: {id}");
                    TempData["Error"] = "Solicitud no encontrada";
                    return RedirectToAction(nameof(Index));
                }

                // ============================================
                // VALIDACIONES
                // ============================================

                // 1. Verificar que esté pendiente
                if (solicitud.Estado != EstadoSolicitud.Pendiente)
                {
                    _logger.LogWarning($"⚠️ Intento de aprobar solicitud que no está pendiente. ID: {id}, Estado: {solicitud.Estado}");
                    TempData["Error"] = $"No se puede aprobar una solicitud en estado '{solicitud.Estado}'";
                    return RedirectToAction(nameof(Detalle), new { id });
                }

                // 2. Verificar que no exista usuario con ese DNI o Email
                var usuarioExistente = await _context.Usuarios
                    .FirstOrDefaultAsync(u => (u.DNI == solicitud.DNI || u.CorreoElectronico == solicitud.CorreoElectronico)
                        && !u.EsEliminado);

                if (usuarioExistente != null)
                {
                    _logger.LogWarning($"⚠️ Intento de aprobar solicitud con DNI/Email duplicado. DNI: {solicitud.DNI}, Email: {solicitud.CorreoElectronico}");
                    TempData["Error"] = "Ya existe un usuario activo con este DNI o correo electrónico";
                    return RedirectToAction(nameof(Detalle), new { id });
                }

                // ============================================
                // CREAR USUARIO
                // ============================================

                // Generar password temporal
                string passwordTemporal = GenerarPasswordTemporal();

                var nuevoUsuario = new Usuario
                {
                    Nombres = solicitud.Nombres,
                    Apellidos = solicitud.Apellidos,
                    DNI = solicitud.DNI,
                    CorreoElectronico = solicitud.CorreoElectronico,
                    Telefono = solicitud.Telefono,
                    Area = solicitud.Area,
                    IdRol = solicitud.IdRol,
                    ClaveTemporal = passwordTemporal,
                    ClavePermanente = string.Empty,
                    PrimerInicio = true, // Usuario debe cambiar contraseña en primer inicio
                    Estado = true,
                    FechaCreacion = DateOnly.FromDateTime(DateTime.Now),
                    FechaActualizacion = DateOnly.FromDateTime(DateTime.Now),
                    EsEliminado = false
                };

                _context.Usuarios.Add(nuevoUsuario);
                await _context.SaveChangesAsync();

                _logger.LogInformation($"✅ Usuario creado exitosamente - ID: {nuevoUsuario.IdUsuario}, DNI: {nuevoUsuario.DNI}, Rol: {solicitud.Rol!.NombreRol}");

                // ============================================
                // ACTUALIZAR SOLICITUD
                // ============================================

                solicitud.Estado = EstadoSolicitud.Aprobada;
                solicitud.FechaRespuesta = DateTime.Now;
                solicitud.RespuestaDe = User.GetUserId();
                solicitud.UsuarioCreado = nuevoUsuario.IdUsuario;

                await _context.SaveChangesAsync();

                // Commit transaction
                await transaction.CommitAsync();

                _logger.LogInformation($"✅ Solicitud aprobada exitosamente - ID: {id} | Usuario creado: {nuevoUsuario.IdUsuario} | " +
                    $"Aprobado por: {User.GetUserName()} | Password temporal: {passwordTemporal}");

                TempData["Mensaje"] = $"Solicitud aprobada exitosamente. Usuario creado con DNI: {nuevoUsuario.DNI}. " +
                    $"Password temporal: {passwordTemporal} (Compártelo con el usuario de forma segura)";

                return RedirectToAction(nameof(Detalle), new { id });
            }
            catch (DbUpdateException ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, $"❌ Error de BD al aprobar solicitud ID: {id}");
                TempData["Error"] = $"Error de base de datos: {ex.InnerException?.Message ?? ex.Message}";
                return RedirectToAction(nameof(Detalle), new { id });
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, $"❌ Error inesperado al aprobar solicitud ID: {id}");
                TempData["Error"] = "Ocurrió un error inesperado al aprobar la solicitud";
                return RedirectToAction(nameof(Detalle), new { id });
            }
        }

        // ============================================
        // RECHAZAR - GET (mostrar formulario)
        // ============================================
        [HttpGet]
        public async Task<IActionResult> Rechazar(int id)
        {
            var solicitud = await _context.Solicituds
                .FirstOrDefaultAsync(s => s.IdSolicitud == id);

            if (solicitud == null)
            {
                _logger.LogWarning($"⚠️ Intento de rechazar solicitud inexistente ID: {id}");
                TempData["Error"] = "Solicitud no encontrada";
                return RedirectToAction(nameof(Index));
            }

            if (solicitud.Estado != EstadoSolicitud.Pendiente)
            {
                _logger.LogWarning($"⚠️ Intento de rechazar solicitud que no está pendiente. ID: {id}, Estado: {solicitud.Estado}");
                TempData["Error"] = $"No se puede rechazar una solicitud en estado '{solicitud.Estado}'";
                return RedirectToAction(nameof(Detalle), new { id });
            }

            var viewModel = new RechazarSolicitudViewModel
            {
                IdSolicitud = solicitud.IdSolicitud,
                NombreCompleto = $"{solicitud.Nombres} {solicitud.Apellidos}",
                DNI = solicitud.DNI,
                Email = solicitud.CorreoElectronico
            };

            return View(viewModel);
        }

        // ============================================
        // RECHAZAR - POST
        // ============================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Rechazar(RechazarSolicitudViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            try
            {
                var solicitud = await _context.Solicituds
                    .FirstOrDefaultAsync(s => s.IdSolicitud == model.IdSolicitud);

                if (solicitud == null)
                {
                    _logger.LogWarning($"⚠️ Solicitud no encontrada al rechazar ID: {model.IdSolicitud}");
                    TempData["Error"] = "Solicitud no encontrada";
                    return RedirectToAction(nameof(Index));
                }

                // Validar que esté pendiente
                if (solicitud.Estado != EstadoSolicitud.Pendiente)
                {
                    _logger.LogWarning($"⚠️ Intento de rechazar solicitud que no está pendiente. ID: {model.IdSolicitud}, Estado: {solicitud.Estado}");
                    TempData["Error"] = $"No se puede rechazar una solicitud en estado '{solicitud.Estado}'";
                    return RedirectToAction(nameof(Detalle), new { id = model.IdSolicitud });
                }

                // Actualizar solicitud
                solicitud.Estado = EstadoSolicitud.Rechazada;
                solicitud.FechaRespuesta = DateTime.Now;
                solicitud.RespuestaDe = User.GetUserId();
                solicitud.MotivoRechazo = string.IsNullOrWhiteSpace(model.MotivoRechazo)
                    ? "Sin motivo especificado"
                    : model.MotivoRechazo.Trim();

                await _context.SaveChangesAsync();

                _logger.LogInformation($"✅ Solicitud rechazada exitosamente - ID: {model.IdSolicitud} | " +
                    $"Rechazado por: {User.GetUserName()} | Motivo: {solicitud.MotivoRechazo}");

                TempData["Mensaje"] = $"Solicitud rechazada exitosamente para {model.NombreCompleto}";
                return RedirectToAction(nameof(Index));
            }
            catch (DbUpdateException ex)
            {
                _logger.LogError(ex, $"❌ Error de BD al rechazar solicitud ID: {model.IdSolicitud}");
                ModelState.AddModelError("", $"Error de base de datos: {ex.InnerException?.Message ?? ex.Message}");
                return View(model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"❌ Error inesperado al rechazar solicitud ID: {model.IdSolicitud}");
                ModelState.AddModelError("", "Ocurrió un error inesperado al rechazar la solicitud");
                return View(model);
            }
        }

        // ============================================
        // HELPER: Generar password temporal
        // ============================================
        private string GenerarPasswordTemporal()
        {
            // Generar password aleatorio de 8 caracteres
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
            var random = new Random();
            var password = new string(Enumerable.Repeat(chars, 8)
                .Select(s => s[random.Next(s.Length)]).ToArray());

            // Agregar caracteres especiales para cumplir política
            return $"{password}!";
        }
    }
}
