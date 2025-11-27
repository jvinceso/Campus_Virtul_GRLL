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

            var permisos = ObtenerPermisosDetalladosPorRol(rolNombre);

            var viewModel = new VerPermisosViewModel
            {
                NombreRol = rolNombre,
                PermisosPorModulo = permisos,
                ModulosConPermiso = permisos.Count,
                TotalPermisos = permisos.Values.Sum(p =>
                    (p.Crear ? 1 : 0) +
                    (p.Editar ? 1 : 0) +
                    (p.Revisar ? 1 : 0) +
                    (p.Aprobar ? 1 : 0) +
                    (p.Visualizar ? 1 : 0))
            };

            return View(viewModel);
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

        // ============================================
        // HELPER: Obtener permisos detallados por rol (para ViewModel)
        // ============================================
        private Dictionary<string, PermisosDetalle> ObtenerPermisosDetalladosPorRol(string rolNombre)
        {
            var permisos = new Dictionary<string, PermisosDetalle>();

            switch (rolNombre)
            {
                case "Administrador":
                    permisos = new Dictionary<string, PermisosDetalle>
                    {
                        ["Usuarios"] = new PermisosDetalle
                        {
                            Crear = true,
                            Editar = true,
                            Revisar = true,
                            Aprobar = true,
                            Visualizar = true
                        },
                        ["Roles"] = new PermisosDetalle
                        {
                            Crear = false,
                            Editar = true,
                            Revisar = true,
                            Aprobar = false,
                            Visualizar = true
                        },
                        ["Cursos"] = new PermisosDetalle
                        {
                            Crear = true,
                            Editar = true,
                            Revisar = true,
                            Aprobar = true,
                            Visualizar = true
                        },
                        ["Profesores"] = new PermisosDetalle
                        {
                            Crear = false,
                            Editar = true,
                            Revisar = true,
                            Aprobar = false,
                            Visualizar = true
                        },
                        ["Materiales"] = new PermisosDetalle
                        {
                            Crear = true,
                            Editar = true,
                            Revisar = true,
                            Aprobar = true,
                            Visualizar = true
                        },
                        ["Solicitudes"] = new PermisosDetalle
                        {
                            Crear = false,
                            Editar = false,
                            Revisar = true,
                            Aprobar = true,
                            Visualizar = true
                        },
                        ["Reportes"] = new PermisosDetalle
                        {
                            Crear = true,
                            Editar = false,
                            Revisar = true,
                            Aprobar = false,
                            Visualizar = true
                        },
                        ["Sistema"] = new PermisosDetalle
                        {
                            Crear = true,
                            Editar = true,
                            Revisar = true,
                            Aprobar = true,
                            Visualizar = true
                        }
                    };
                    break;

                case "Profesor":
                    permisos = new Dictionary<string, PermisosDetalle>
                    {
                        ["Cursos Asignados"] = new PermisosDetalle
                        {
                            Crear = false,
                            Editar = true,
                            Revisar = true,
                            Aprobar = false,
                            Visualizar = true
                        },
                        ["Materiales"] = new PermisosDetalle
                        {
                            Crear = true,
                            Editar = true,
                            Revisar = true,
                            Aprobar = false,
                            Visualizar = true
                        },
                        ["Evaluaciones"] = new PermisosDetalle
                        {
                            Crear = true,
                            Editar = true,
                            Revisar = true,
                            Aprobar = true,
                            Visualizar = true
                        },
                        ["Participantes"] = new PermisosDetalle
                        {
                            Crear = false,
                            Editar = false,
                            Revisar = true,
                            Aprobar = false,
                            Visualizar = true
                        }
                    };
                    break;

                case "Colaborador":
                    permisos = new Dictionary<string, PermisosDetalle>
                    {
                        ["Cursos"] = new PermisosDetalle
                        {
                            Crear = false,
                            Editar = false,
                            Revisar = false,
                            Aprobar = false,
                            Visualizar = true
                        },
                        ["Materiales"] = new PermisosDetalle
                        {
                            Crear = false,
                            Editar = false,
                            Revisar = false,
                            Aprobar = false,
                            Visualizar = true
                        },
                        ["Evaluaciones"] = new PermisosDetalle
                        {
                            Crear = false,
                            Editar = false,
                            Revisar = false,
                            Aprobar = false,
                            Visualizar = true
                        },
                        ["Mi Progreso"] = new PermisosDetalle
                        {
                            Crear = false,
                            Editar = false,
                            Revisar = true,
                            Aprobar = false,
                            Visualizar = true
                        }
                    };
                    break;

                case "Practicante":
                    permisos = new Dictionary<string, PermisosDetalle>
                    {
                        ["Cursos"] = new PermisosDetalle
                        {
                            Crear = false,
                            Editar = false,
                            Revisar = false,
                            Aprobar = false,
                            Visualizar = true
                        },
                        ["Materiales"] = new PermisosDetalle
                        {
                            Crear = false,
                            Editar = false,
                            Revisar = false,
                            Aprobar = false,
                            Visualizar = true
                        },
                        ["Evaluaciones"] = new PermisosDetalle
                        {
                            Crear = false,
                            Editar = false,
                            Revisar = false,
                            Aprobar = false,
                            Visualizar = true
                        },
                        ["Mi Progreso"] = new PermisosDetalle
                        {
                            Crear = false,
                            Editar = false,
                            Revisar = true,
                            Aprobar = false,
                            Visualizar = true
                        }
                    };
                    break;

                default:
                    permisos = new Dictionary<string, PermisosDetalle>
                    {
                        ["Sin permisos"] = new PermisosDetalle
                        {
                            Crear = false,
                            Editar = false,
                            Revisar = false,
                            Aprobar = false,
                            Visualizar = false
                        }
                    };
                    break;
            }

            return permisos;
        }

        // ============================================
        // CRUD DE ROLES
        // ============================================

        // ============================================
        // LISTADO DE ROLES
        // ============================================
        [HttpGet]
        public async Task<IActionResult> ListadoRoles()
        {
            _logger.LogInformation($"👥 Admin {User.GetUserName()} accedió a listado de roles");

            var roles = await _context.Rols
                .Select(r => new RolCardViewModel
                {
                    IdRol = r.IdRol,
                    NombreRol = r.NombreRol,
                    Descripcion = r.Descripcion,
                    Estado = r.Estado,
                    UsuariosConEsteRol = _context.Usuarios.Count(u => u.IdRol == r.IdRol && !u.EsEliminado)
                })
                .OrderBy(r => r.NombreRol)
                .ToListAsync();

            var viewModel = new ListadoRolesViewModel
            {
                Roles = roles,
                TotalRoles = roles.Count,
                RolesActivos = roles.Count(r => r.Estado),
                RolesInactivos = roles.Count(r => !r.Estado)
            };

            return View(viewModel);
        }

        // ============================================
        // CREAR ROL - GET
        // ============================================
        [HttpGet]
        public IActionResult CrearRol()
        {
            return View(new CrearRolViewModel());
        }

        // ============================================
        // CREAR ROL - POST
        // ============================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CrearRol(CrearRolViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            try
            {
                // Verificar que no exista un rol con el mismo nombre
                var rolExistente = await _context.Rols
                    .AnyAsync(r => r.NombreRol.ToLower() == model.NombreRol.Trim().ToLower());

                if (rolExistente)
                {
                    ModelState.AddModelError("NombreRol", "Ya existe un rol con este nombre");
                    return View(model);
                }

                var nuevoRol = new Rol
                {
                    NombreRol = model.NombreRol.Trim(),
                    Descripcion = model.Descripcion.Trim(),
                    Estado = model.Estado
                };

                _context.Rols.Add(nuevoRol);
                await _context.SaveChangesAsync();

                _logger.LogInformation($"✅ Rol creado: {nuevoRol.NombreRol} (ID: {nuevoRol.IdRol}) por {User.GetUserName()}");
                TempData["Mensaje"] = $"Rol '{nuevoRol.NombreRol}' creado exitosamente";

                return RedirectToAction(nameof(ListadoRoles));
            }
            catch (DbUpdateException ex)
            {
                _logger.LogError(ex, "❌ Error al crear rol");
                ModelState.AddModelError("", $"Error de base de datos: {ex.InnerException?.Message ?? ex.Message}");
                return View(model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error inesperado al crear rol");
                ModelState.AddModelError("", "Ocurrió un error inesperado");
                return View(model);
            }
        }

        // ============================================
        // EDITAR ROL - GET
        // ============================================
        [HttpGet]
        public async Task<IActionResult> EditarRol(int id)
        {
            var rol = await _context.Rols.FindAsync(id);

            if (rol == null)
            {
                _logger.LogWarning($"⚠️ Intento de editar rol inexistente ID: {id}");
                TempData["Error"] = "Rol no encontrado";
                return RedirectToAction(nameof(ListadoRoles));
            }

            var usuariosConRol = await _context.Usuarios
                .CountAsync(u => u.IdRol == id && !u.EsEliminado);

            var viewModel = new EditarRolViewModel
            {
                IdRol = rol.IdRol,
                NombreRol = rol.NombreRol,
                Descripcion = rol.Descripcion,
                Estado = rol.Estado,
                UsuariosConEsteRol = usuariosConRol
            };

            return View(viewModel);
        }

        // ============================================
        // EDITAR ROL - POST
        // ============================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditarRol(EditarRolViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            try
            {
                var rol = await _context.Rols.FindAsync(model.IdRol);

                if (rol == null)
                {
                    _logger.LogWarning($"⚠️ Rol ID {model.IdRol} no encontrado");
                    TempData["Error"] = "Rol no encontrado";
                    return RedirectToAction(nameof(ListadoRoles));
                }

                // Verificar que no exista otro rol con el mismo nombre
                var rolDuplicado = await _context.Rols
                    .AnyAsync(r => r.NombreRol.ToLower() == model.NombreRol.Trim().ToLower()
                        && r.IdRol != model.IdRol);

                if (rolDuplicado)
                {
                    ModelState.AddModelError("NombreRol", "Ya existe otro rol con este nombre");
                    return View(model);
                }

                // No permitir desactivar si hay usuarios con este rol
                if (!model.Estado && model.UsuariosConEsteRol > 0)
                {
                    ModelState.AddModelError("Estado",
                        $"No se puede desactivar este rol porque hay {model.UsuariosConEsteRol} usuario(s) asignado(s)");
                    return View(model);
                }

                var nombreAnterior = rol.NombreRol;
                rol.NombreRol = model.NombreRol.Trim();
                rol.Descripcion = model.Descripcion.Trim();
                rol.Estado = model.Estado;

                await _context.SaveChangesAsync();

                _logger.LogInformation($"✅ Rol editado: {nombreAnterior} → {rol.NombreRol} por {User.GetUserName()}");
                TempData["Mensaje"] = $"Rol '{rol.NombreRol}' actualizado exitosamente";

                return RedirectToAction(nameof(ListadoRoles));
            }
            catch (DbUpdateException ex)
            {
                _logger.LogError(ex, $"❌ Error al editar rol ID: {model.IdRol}");
                ModelState.AddModelError("", $"Error de base de datos: {ex.InnerException?.Message ?? ex.Message}");
                return View(model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"❌ Error inesperado al editar rol ID: {model.IdRol}");
                ModelState.AddModelError("", "Ocurrió un error inesperado");
                return View(model);
            }
        }

        // ============================================
        // ELIMINAR ROL - POST
        // ============================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EliminarRol(int id)
        {
            try
            {
                var rol = await _context.Rols.FindAsync(id);

                if (rol == null)
                {
                    _logger.LogWarning($"⚠️ Intento de eliminar rol inexistente ID: {id}");
                    TempData["Error"] = "Rol no encontrado";
                    return RedirectToAction(nameof(ListadoRoles));
                }

                // Verificar que no haya usuarios con este rol
                var usuariosConRol = await _context.Usuarios
                    .CountAsync(u => u.IdRol == id && !u.EsEliminado);

                if (usuariosConRol > 0)
                {
                    _logger.LogWarning($"⚠️ Intento de eliminar rol ID: {id} con {usuariosConRol} usuarios asignados");
                    TempData["Error"] = $"No se puede eliminar el rol '{rol.NombreRol}' porque tiene {usuariosConRol} usuario(s) asignado(s)";
                    return RedirectToAction(nameof(ListadoRoles));
                }

                // Eliminar permisos asociados
                var permisos = await _context.Permisos.Where(p => p.IdRol == id).ToListAsync();
                _context.Permisos.RemoveRange(permisos);

                // Eliminar rol
                _context.Rols.Remove(rol);
                await _context.SaveChangesAsync();

                _logger.LogInformation($"✅ Rol eliminado: {rol.NombreRol} (ID: {id}) por {User.GetUserName()}");
                TempData["Mensaje"] = $"Rol '{rol.NombreRol}' eliminado exitosamente";

                return RedirectToAction(nameof(ListadoRoles));
            }
            catch (DbUpdateException ex)
            {
                _logger.LogError(ex, $"❌ Error al eliminar rol ID: {id}");
                TempData["Error"] = $"Error de base de datos: {ex.InnerException?.Message ?? ex.Message}";
                return RedirectToAction(nameof(ListadoRoles));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"❌ Error inesperado al eliminar rol ID: {id}");
                TempData["Error"] = "Ocurrió un error inesperado al eliminar el rol";
                return RedirectToAction(nameof(ListadoRoles));
            }
        }

        // ============================================
        // GESTIONAR PERMISOS - GET
        // ============================================
        [HttpGet]
        public async Task<IActionResult> GestionarPermisos(int id)
        {
            var rol = await _context.Rols.FindAsync(id);

            if (rol == null)
            {
                _logger.LogWarning($"⚠️ Intento de gestionar permisos de rol inexistente ID: {id}");
                TempData["Error"] = "Rol no encontrado";
                return RedirectToAction(nameof(ListadoRoles));
            }

            // Obtener todos los módulos
            var modulos = await _context.Modulos
                .OrderBy(m => m.Titulo)
                .ToListAsync();

            // Obtener permisos actuales del rol
            var permisosActuales = await _context.Permisos
                .Where(p => p.IdRol == id)
                .ToListAsync();

            var viewModel = new GestionarPermisosViewModel
            {
                IdRol = rol.IdRol,
                NombreRol = rol.NombreRol,
                Modulos = modulos.Select(m =>
                {
                    var permiso = permisosActuales.FirstOrDefault(p => p.IdModulo == m.IdModulo);
                    return new ModuloPermisoViewModel
                    {
                        IdModulo = m.IdModulo,
                        NombreModulo = m.Titulo,
                        Descripcion = m.Descripcion,
                        IdPermiso = permiso?.IdPermisos,
                        Crear = permiso?.Crear ?? false,
                        Editar = permiso?.Editar ?? false,
                        Revisar = permiso?.Revisar ?? false,
                        Aprobar = permiso?.Aprobar ?? false,
                        Visualizar = permiso?.Visualizar ?? false
                    };
                }).ToList()
            };

            return View(viewModel);
        }

        // ============================================
        // GESTIONAR PERMISOS - POST
        // ============================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> GestionarPermisos(GestionarPermisosViewModel model)
        {
            try
            {
                var rol = await _context.Rols.FindAsync(model.IdRol);

                if (rol == null)
                {
                    TempData["Error"] = "Rol no encontrado";
                    return RedirectToAction(nameof(ListadoRoles));
                }

                // Obtener permisos actuales
                var permisosActuales = await _context.Permisos
                    .Where(p => p.IdRol == model.IdRol)
                    .ToListAsync();

                foreach (var modulo in model.Modulos)
                {
                    var permisoExistente = permisosActuales
                        .FirstOrDefault(p => p.IdModulo == modulo.IdModulo);

                    // Si hay al menos un permiso marcado
                    bool tieneAlgunPermiso = modulo.Crear || modulo.Editar ||
                        modulo.Revisar || modulo.Aprobar || modulo.Visualizar;

                    if (tieneAlgunPermiso)
                    {
                        if (permisoExistente != null)
                        {
                            // Actualizar permiso existente
                            permisoExistente.Crear = modulo.Crear;
                            permisoExistente.Editar = modulo.Editar;
                            permisoExistente.Revisar = modulo.Revisar;
                            permisoExistente.Aprobar = modulo.Aprobar;
                            permisoExistente.Visualizar = modulo.Visualizar;
                        }
                        else
                        {
                            // Crear nuevo permiso
                            var nuevoPermiso = new Permisos
                            {
                                IdRol = model.IdRol,
                                IdModulo = modulo.IdModulo,
                                Crear = modulo.Crear,
                                Editar = modulo.Editar,
                                Revisar = modulo.Revisar,
                                Aprobar = modulo.Aprobar,
                                Visualizar = modulo.Visualizar
                            };
                            _context.Permisos.Add(nuevoPermiso);
                        }
                    }
                    else if (permisoExistente != null)
                    {
                        // Si no hay permisos y existía un registro, eliminarlo
                        _context.Permisos.Remove(permisoExistente);
                    }
                }

                await _context.SaveChangesAsync();

                _logger.LogInformation($"✅ Permisos actualizados para rol: {rol.NombreRol} por {User.GetUserName()}");
                TempData["Mensaje"] = $"Permisos del rol '{rol.NombreRol}' actualizados exitosamente";

                return RedirectToAction(nameof(ListadoRoles));
            }
            catch (DbUpdateException ex)
            {
                _logger.LogError(ex, $"❌ Error al actualizar permisos del rol ID: {model.IdRol}");
                TempData["Error"] = $"Error de base de datos: {ex.InnerException?.Message ?? ex.Message}";
                return RedirectToAction(nameof(GestionarPermisos), new { id = model.IdRol });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"❌ Error inesperado al actualizar permisos del rol ID: {model.IdRol}");
                TempData["Error"] = "Ocurrió un error inesperado";
                return RedirectToAction(nameof(GestionarPermisos), new { id = model.IdRol });
            }
        }
    }
}
