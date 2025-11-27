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
    /// Controlador para gestión de roles y permisos
    /// Solo accesible para Administradores
    /// </summary>
    [Authorize(Roles = "Administrador")]
    public class AdminRolesController : Controller
    {
        private readonly AppDBContext _context;
        private readonly ILogger<AdminRolesController> _logger;

        public AdminRolesController(AppDBContext context, ILogger<AdminRolesController> logger)
        {
            _context = context;
            _logger = logger;
        }

        // ============================================
        // INDEX - Lista de usuarios con sus roles
        // ============================================
        [HttpGet]
        public async Task<IActionResult> Index(string? busqueda, int? filtroRol, int pagina = 1)
        {
            _logger.LogInformation($"👥 Admin {User.GetUserName()} accedió a gestión de roles");

            // Query base: usuarios NO eliminados
            var query = _context.Usuarios
                .Include(u => u.Rol)
                .Where(u => !u.EsEliminado);

            // Aplicar búsqueda
            if (!string.IsNullOrWhiteSpace(busqueda))
            {
                busqueda = busqueda.Trim().ToLower();
                query = query.Where(u =>
                    u.Nombres.ToLower().Contains(busqueda) ||
                    u.Apellidos.ToLower().Contains(busqueda) ||
                    u.DNI.Contains(busqueda) ||
                    u.CorreoElectronico.ToLower().Contains(busqueda));
            }

            // Aplicar filtro por rol
            if (filtroRol.HasValue)
            {
                query = query.Where(u => u.IdRol == filtroRol.Value);
            }

            // Obtener roles disponibles para el filtro
            var rolesDisponibles = await _context.Rols
                .Where(r => r.Estado)
                .OrderBy(r => r.NombreRol)
                .ToListAsync();

            // Calcular estadísticas
            var totalUsuarios = await query.CountAsync();
            var usuariosPorRol = await _context.Usuarios
                .Where(u => !u.EsEliminado && u.Estado)
                .Include(u => u.Rol)
                .GroupBy(u => u.Rol!.NombreRol)
                .Select(g => new { Rol = g.Key, Cantidad = g.Count() })
                .ToListAsync();

            // Paginación
            var usuariosPorPagina = 15;
            var totalPaginas = (int)Math.Ceiling(totalUsuarios / (double)usuariosPorPagina);
            pagina = Math.Max(1, Math.Min(pagina, totalPaginas == 0 ? 1 : totalPaginas));

            var usuarios = await query
                .OrderBy(u => u.Rol!.NombreRol)
                .ThenBy(u => u.Apellidos)
                .Skip((pagina - 1) * usuariosPorPagina)
                .Take(usuariosPorPagina)
                .Select(u => new RolUsuarioViewModel
                {
                    IdUsuario = u.IdUsuario,
                    NombreCompleto = $"{u.Nombres} {u.Apellidos}",
                    DNI = u.DNI,
                    Email = u.CorreoElectronico,
                    Area = u.Area,
                    RolActual = u.Rol!.NombreRol,
                    IdRolActual = u.IdRol,
                    Estado = u.Estado,
                    FechaCreacion = u.FechaCreacion,
                    EsUsuarioActual = u.IdUsuario == User.GetUserId()
                })
                .ToListAsync();

            var viewModel = new GestionRolesIndexViewModel
            {
                Usuarios = usuarios,
                TotalUsuarios = totalUsuarios,
                BusquedaTexto = busqueda,
                FiltroRol = filtroRol,
                PaginaActual = pagina,
                TotalPaginas = totalPaginas,
                RolesDisponibles = rolesDisponibles,
                UsuariosPorRol = usuariosPorRol.ToDictionary(x => x.Rol, x => x.Cantidad)
            };

            return View(viewModel);
        }

        // ============================================
        // CAMBIAR ROL - GET
        // ============================================
        [HttpGet]
        public async Task<IActionResult> CambiarRol(int id)
        {
            var usuario = await _context.Usuarios
                .Include(u => u.Rol)
                .FirstOrDefaultAsync(u => u.IdUsuario == id && !u.EsEliminado);

            if (usuario == null)
            {
                _logger.LogWarning($"⚠️ Intento de cambiar rol de usuario inexistente ID: {id}");
                TempData["Error"] = "Usuario no encontrado";
                return RedirectToAction(nameof(Index));
            }

            var rolesDisponibles = await _context.Rols
                .Where(r => r.Estado)
                .OrderBy(r => r.NombreRol)
                .ToListAsync();

            var viewModel = new CambiarRolViewModel
            {
                IdUsuario = usuario.IdUsuario,
                NombreCompleto = $"{usuario.Nombres} {usuario.Apellidos}",
                DNI = usuario.DNI,
                Email = usuario.CorreoElectronico,
                Area = usuario.Area,
                RolActual = usuario.Rol!.NombreRol,
                IdRolActual = usuario.IdRol,
                IdRolNuevo = usuario.IdRol,
                RolesDisponibles = rolesDisponibles,
                EsUsuarioActual = usuario.IdUsuario == User.GetUserId()
            };

            return View(viewModel);
        }

        // ============================================
        // CAMBIAR ROL - POST
        // ============================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CambiarRol(CambiarRolViewModel model)
        {
            if (!ModelState.IsValid)
            {
                model.RolesDisponibles = await _context.Rols.Where(r => r.Estado).ToListAsync();
                return View(model);
            }

            try
            {
                var usuario = await _context.Usuarios
                    .Include(u => u.Rol)
                    .FirstOrDefaultAsync(u => u.IdUsuario == model.IdUsuario && !u.EsEliminado);

                if (usuario == null)
                {
                    _logger.LogWarning($"⚠️ Usuario ID {model.IdUsuario} no encontrado para cambio de rol");
                    TempData["Error"] = "Usuario no encontrado";
                    return RedirectToAction(nameof(Index));
                }

                var usuarioActualId = User.GetUserId();

                // ============================================
                // VALIDACIONES DE SEGURIDAD
                // ============================================

                // 1. No puede cambiar su propio rol si es administrador
                if (usuario.IdUsuario == usuarioActualId && usuario.Rol?.NombreRol == "Administrador")
                {
                    var nuevoRol = await _context.Rols.FindAsync(model.IdRolNuevo);
                    if (nuevoRol?.NombreRol != "Administrador")
                    {
                        _logger.LogWarning($"⚠️ Admin {User.GetUserName()} intentó quitarse el rol de administrador");
                        ModelState.AddModelError("IdRolNuevo", "No puede quitarse el rol de Administrador a sí mismo");
                        model.RolesDisponibles = await _context.Rols.Where(r => r.Estado).ToListAsync();
                        return View(model);
                    }
                }

                // 2. No puede cambiar el rol del último administrador
                if (usuario.Rol?.NombreRol == "Administrador")
                {
                    var totalAdmins = await _context.Usuarios
                        .CountAsync(u => u.Rol!.NombreRol == "Administrador" && !u.EsEliminado && u.Estado);

                    if (totalAdmins <= 1)
                    {
                        var nuevoRol = await _context.Rols.FindAsync(model.IdRolNuevo);
                        if (nuevoRol?.NombreRol != "Administrador")
                        {
                            _logger.LogWarning($"⚠️ Intento de cambiar rol del último administrador");
                            ModelState.AddModelError("IdRolNuevo", "No puede cambiar el rol del último administrador del sistema");
                            model.RolesDisponibles = await _context.Rols.Where(r => r.Estado).ToListAsync();
                            return View(model);
                        }
                    }
                }

                // 3. Verificar que el rol nuevo existe
                var rolNuevo = await _context.Rols.FindAsync(model.IdRolNuevo);
                if (rolNuevo == null || !rolNuevo.Estado)
                {
                    _logger.LogWarning($"⚠️ Intento de asignar rol inválido ID: {model.IdRolNuevo}");
                    ModelState.AddModelError("IdRolNuevo", "El rol seleccionado no es válido");
                    model.RolesDisponibles = await _context.Rols.Where(r => r.Estado).ToListAsync();
                    return View(model);
                }

                // ============================================
                // REALIZAR CAMBIO DE ROL
                // ============================================

                var rolAnterior = usuario.Rol!.NombreRol;
                usuario.IdRol = model.IdRolNuevo;
                usuario.FechaActualizacion = DateOnly.FromDateTime(DateTime.Now);

                await _context.SaveChangesAsync();

                // Log del cambio
                _logger.LogInformation($"✅ Cambio de rol exitoso - Usuario: {usuario.Nombres} {usuario.Apellidos} (ID: {usuario.IdUsuario}) | " +
                    $"Rol anterior: {rolAnterior} → Nuevo rol: {rolNuevo.NombreRol} | " +
                    $"Realizado por: {User.GetUserName()} (ID: {usuarioActualId})");

                TempData["Mensaje"] = $"Rol cambiado exitosamente de '{rolAnterior}' a '{rolNuevo.NombreRol}' para {usuario.Nombres} {usuario.Apellidos}";
                return RedirectToAction(nameof(Index));
            }
            catch (DbUpdateException ex)
            {
                _logger.LogError(ex, $"❌ Error de BD al cambiar rol del usuario ID: {model.IdUsuario}");
                ModelState.AddModelError("", $"Error de base de datos: {ex.InnerException?.Message ?? ex.Message}");
                model.RolesDisponibles = await _context.Rols.Where(r => r.Estado).ToListAsync();
                return View(model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"❌ Error inesperado al cambiar rol del usuario ID: {model.IdUsuario}");
                ModelState.AddModelError("", "Ocurrió un error inesperado al cambiar el rol");
                model.RolesDisponibles = await _context.Rols.Where(r => r.Estado).ToListAsync();
                return View(model);
            }
        }

        // ============================================
        // VER PERMISOS DE UN ROL (Informativo)
        // ============================================
        [HttpGet]
        public IActionResult VerPermisos(string rolNombre)
        {
            _logger.LogInformation($"👁️ Consultando permisos del rol: {rolNombre}");

            var permisos = ObtenerPermisosPorRol(rolNombre);

            ViewBag.RolNombre = rolNombre;
            ViewBag.Permisos = permisos;

            return View();
        }

        // ============================================
        // HELPER: Obtener permisos por rol
        // ============================================
        private Dictionary<string, List<string>> ObtenerPermisosPorRol(string rolNombre)
        {
            var permisos = new Dictionary<string, List<string>>();

            switch (rolNombre)
            {
                case "Administrador":
                    permisos = new Dictionary<string, List<string>>
                    {
                        ["Usuarios"] = new List<string> { "Crear", "Editar", "Ver", "Eliminar", "Cambiar Estado", "Cambiar Contraseña" },
                        ["Roles"] = new List<string> { "Ver", "Asignar", "Cambiar" },
                        ["Cursos"] = new List<string> { "Crear", "Editar", "Ver", "Eliminar", "Publicar" },
                        ["Profesores"] = new List<string> { "Asignar a cursos", "Ver todos", "Gestionar" },
                        ["Materiales"] = new List<string> { "Ver todos", "Gestionar", "Eliminar" },
                        ["Solicitudes"] = new List<string> { "Aprobar", "Rechazar", "Ver todas" },
                        ["Reportes"] = new List<string> { "Ver", "Generar", "Exportar" },
                        ["Sistema"] = new List<string> { "Acceso completo", "Configuración" }
                    };
                    break;

                case "Profesor":
                    permisos = new Dictionary<string, List<string>>
                    {
                        ["Cursos"] = new List<string> { "Ver cursos asignados" },
                        ["Materiales"] = new List<string> { "Subir", "Editar propios", "Eliminar propios", "Ver de sus cursos" },
                        ["Evaluaciones"] = new List<string> { "Crear", "Editar", "Calificar", "Ver resultados" },
                        ["Participantes"] = new List<string> { "Ver de sus cursos", "Calificar" }
                    };
                    break;

                case "Colaborador":
                    permisos = new Dictionary<string, List<string>>
                    {
                        ["Cursos"] = new List<string> { "Ver catálogo", "Inscribirse", "Ver inscritos" },
                        ["Materiales"] = new List<string> { "Ver de cursos inscritos", "Descargar" },
                        ["Evaluaciones"] = new List<string> { "Realizar", "Ver resultados propios" }
                    };
                    break;

                case "Practicante":
                    permisos = new Dictionary<string, List<string>>
                    {
                        ["Cursos"] = new List<string> { "Ver catálogo", "Inscribirse", "Ver inscritos" },
                        ["Materiales"] = new List<string> { "Ver de cursos inscritos", "Descargar" },
                        ["Evaluaciones"] = new List<string> { "Realizar", "Ver resultados propios" }
                    };
                    break;

                default:
                    permisos = new Dictionary<string, List<string>>
                    {
                        ["Sin permisos"] = new List<string> { "Rol no reconocido" }
                    };
                    break;
            }

            return permisos;
        }
    }
}
