using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Campus_Virtul_GRLL.Data;
using Campus_Virtul_GRLL.Models;
using Campus_Virtul_GRLL.Models.ViewModels;
using Campus_Virtul_GRLL.Helpers;
using System.Security.Claims;

namespace Campus_Virtul_GRLL.Controllers
{
    /// <summary>
    /// Controlador para la gestión completa de usuarios (CRUD)
    /// Solo accesible para Administradores
    /// </summary>
    [Authorize(Roles = "Administrador")]
    public class AdminUsuariosController : Controller
    {
        private readonly AppDBContext _context;
        private readonly ILogger<AdminUsuariosController> _logger;

        public AdminUsuariosController(AppDBContext context, ILogger<AdminUsuariosController> logger)
        {
            _context = context;
            _logger = logger;
        }

        // ============================================
        // LISTAR USUARIOS (INDEX)
        // ============================================

        /// <summary>
        /// Vista principal con lista de usuarios
        /// GET: /AdminUsuarios/Index
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> Index(
            string? busqueda,
            int? filtroRol,
            bool? filtroEstado,
            string? orden,
            int pagina = 1)
        {
            try
            {
                _logger.LogInformation($"📋 Cargando lista de usuarios - Página {pagina}");

                var usuarioActualId = User.GetUserId();

                // Query base: usuarios NO eliminados
                var query = _context.Usuarios
                    .Include(u => u.Rol)
                    .Where(u => !u.EsEliminado);

                // Aplicar búsqueda
                if (!string.IsNullOrWhiteSpace(busqueda))
                {
                    busqueda = busqueda.ToLower().Trim();
                    query = query.Where(u =>
                        u.Nombres.ToLower().Contains(busqueda) ||
                        u.Apellidos.ToLower().Contains(busqueda) ||
                        u.DNI.Contains(busqueda) ||
                        u.CorreoElectronico.ToLower().Contains(busqueda));
                }

                // Aplicar filtro de rol
                if (filtroRol.HasValue)
                {
                    query = query.Where(u => u.IdRol == filtroRol.Value);
                }

                // Aplicar filtro de estado
                if (filtroEstado.HasValue)
                {
                    query = query.Where(u => u.Estado == filtroEstado.Value);
                }

                // Aplicar ordenamiento
                query = orden switch
                {
                    "nombre" => query.OrderBy(u => u.Nombres),
                    "dni" => query.OrderBy(u => u.DNI),
                    "email" => query.OrderBy(u => u.CorreoElectronico),
                    "rol" => query.OrderBy(u => u.Rol!.NombreRol),
                    "fecha" => query.OrderByDescending(u => u.FechaCreacion),
                    _ => query.OrderBy(u => u.Nombres) // Por defecto
                };

                // Obtener todos los usuarios filtrados
                var todosUsuarios = await query.ToListAsync();

                // Calcular estadísticas
                int totalUsuarios = todosUsuarios.Count;
                int usuariosActivos = todosUsuarios.Count(u => u.Estado);
                int usuariosInactivos = totalUsuarios - usuariosActivos;
                int totalAdmins = todosUsuarios.Count(u => u.Rol?.NombreRol == "Administrador");
                int totalProfesores = todosUsuarios.Count(u => u.Rol?.NombreRol == "Profesor");
                int totalColaboradores = todosUsuarios.Count(u => u.Rol?.NombreRol == "Colaborador");
                int totalPracticantes = todosUsuarios.Count(u => u.Rol?.NombreRol == "Practicante");

                // Paginación
                int usuariosPorPagina = 15;
                int totalPaginas = (int)Math.Ceiling(totalUsuarios / (double)usuariosPorPagina);
                var usuariosPaginados = todosUsuarios
                    .Skip((pagina - 1) * usuariosPorPagina)
                    .Take(usuariosPorPagina)
                    .ToList();

                // Mapear a ViewModels
                var usuariosViewModel = usuariosPaginados.Select(u => new UsuarioCardViewModel
                {
                    IdUsuario = u.IdUsuario,
                    Nombres = u.Nombres,
                    Apellidos = u.Apellidos,
                    DNI = u.DNI,
                    Email = u.CorreoElectronico,
                    Telefono = u.Telefono,
                    Area = u.Area,
                    NombreRol = u.Rol?.NombreRol ?? "Sin Rol",
                    IdRol = u.IdRol,
                    Estado = u.Estado,
                    FechaCreacion = u.FechaCreacion,
                    EsUsuarioActual = u.IdUsuario == usuarioActualId
                }).ToList();

                // Obtener roles para filtros
                var roles = await _context.Rols.OrderBy(r => r.NombreRol).ToListAsync();

                var viewModel = new UsuariosIndexViewModel
                {
                    Usuarios = usuariosViewModel,
                    TotalUsuarios = totalUsuarios,
                    UsuariosActivos = usuariosActivos,
                    UsuariosInactivos = usuariosInactivos,
                    TotalAdministradores = totalAdmins,
                    TotalProfesores = totalProfesores,
                    TotalColaboradores = totalColaboradores,
                    TotalPracticantes = totalPracticantes,
                    BusquedaTexto = busqueda,
                    FiltroRol = filtroRol,
                    FiltroEstado = filtroEstado,
                    Ordenamiento = orden,
                    PaginaActual = pagina,
                    UsuariosPorPagina = usuariosPorPagina,
                    TotalPaginas = totalPaginas,
                    RolesDisponibles = roles
                };

                _logger.LogInformation($"✅ Lista cargada: {usuariosViewModel.Count} usuarios en página {pagina} de {totalPaginas}");
                return View(viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error al cargar lista de usuarios");
                TempData["Error"] = "Error al cargar la lista de usuarios";
                return View(new UsuariosIndexViewModel());
            }
        }

        // ============================================
        // VER DETALLE DE USUARIO
        // ============================================

        /// <summary>
        /// Vista con el detalle completo de un usuario
        /// GET: /AdminUsuarios/Detalle/{id}
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> Detalle(int id)
        {
            try
            {
                var usuario = await _context.Usuarios
                    .Include(u => u.Rol)
                    .FirstOrDefaultAsync(u => u.IdUsuario == id && !u.EsEliminado);

                if (usuario == null)
                {
                    _logger.LogWarning($"⚠️ Usuario con ID {id} no encontrado o eliminado");
                    TempData["Error"] = "Usuario no encontrado";
                    return RedirectToAction(nameof(Index));
                }

                var usuarioActualId = User.GetUserId();

                // Contar cursos si es profesor
                int? cursosAsignados = null;
                int? cursosCreados = null;

                if (usuario.Rol?.NombreRol == "Profesor")
                {
                    cursosAsignados = await _context.Cursos
                        .CountAsync(c => c.ProfesorAsignado == id && !c.EsEliminado);
                }

                cursosCreados = await _context.Cursos
                    .CountAsync(c => c.CreadoPor == id && !c.EsEliminado);

                var viewModel = new DetalleUsuarioViewModel
                {
                    IdUsuario = usuario.IdUsuario,
                    Nombres = usuario.Nombres,
                    Apellidos = usuario.Apellidos,
                    DNI = usuario.DNI,
                    Email = usuario.CorreoElectronico,
                    Telefono = usuario.Telefono,
                    Area = usuario.Area,
                    NombreRol = usuario.Rol?.NombreRol ?? "Sin Rol",
                    IdRol = usuario.IdRol,
                    Estado = usuario.Estado,
                    PrimerInicio = usuario.PrimerInicio,
                    FechaCreacion = usuario.FechaCreacion,
                    FechaActualizacion = usuario.FechaActualizacion,
                    EsUsuarioActual = usuario.IdUsuario == usuarioActualId,
                    CursosAsignados = cursosAsignados,
                    CursosCreados = cursosCreados
                };

                _logger.LogInformation($"✅ Mostrando detalle del usuario {usuario.Nombres} {usuario.Apellidos}");
                return View(viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"❌ Error al cargar detalle del usuario ID {id}");
                TempData["Error"] = "Error al cargar el detalle del usuario";
                return RedirectToAction(nameof(Index));
            }
        }

        // ============================================
        // CREAR USUARIO
        // ============================================

        /// <summary>
        /// Vista para crear un nuevo usuario
        /// GET: /AdminUsuarios/Crear
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> Crear()
        {
            try
            {
                var roles = await _context.Rols.OrderBy(r => r.NombreRol).ToListAsync();

                var viewModel = new CrearUsuarioViewModel
                {
                    RolesDisponibles = roles,
                    Estado = true // Por defecto activo
                };

                return View(viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error al cargar formulario de crear usuario");
                TempData["Error"] = "Error al cargar el formulario";
                return RedirectToAction(nameof(Index));
            }
        }

        /// <summary>
        /// Procesa la creación de un nuevo usuario
        /// POST: /AdminUsuarios/Crear
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Crear(CrearUsuarioViewModel model)
        {
            try
            {
                // Recargar roles en caso de error
                model.RolesDisponibles = await _context.Rols.OrderBy(r => r.NombreRol).ToListAsync();

                if (!ModelState.IsValid)
                {
                    return View(model);
                }

                // Validar DNI único
                var dniExiste = await _context.Usuarios
                    .AnyAsync(u => u.DNI == model.DNI && !u.EsEliminado);

                if (dniExiste)
                {
                    ModelState.AddModelError("DNI", "Ya existe un usuario con este DNI");
                    return View(model);
                }

                // Validar Email único
                var emailExiste = await _context.Usuarios
                    .AnyAsync(u => u.CorreoElectronico == model.Email && !u.EsEliminado);

                if (emailExiste)
                {
                    ModelState.AddModelError("Email", "Ya existe un usuario con este email");
                    return View(model);
                }

                // Crear nuevo usuario
                var nuevoUsuario = new Usuario
                {
                    Nombres = model.Nombres.Trim(),
                    Apellidos = model.Apellidos.Trim(),
                    DNI = model.DNI.Trim(),
                    CorreoElectronico = model.Email.Trim(),
                    Telefono = model.Telefono?.Trim() ?? "",
                    Area = model.Area.Trim(),
                    IdRol = model.IdRol,
                    Estado = model.Estado,
                    PrimerInicio = false, // Usuario creado por admin, no requiere cambio de contraseña
                    ClaveTemporal = model.Contrasena, // TODO: Hashear en producción
                    ClavePermanente = model.Contrasena, // TODO: Hashear en producción
                    FechaCreacion = DateOnly.FromDateTime(DateTime.Now),
                    FechaActualizacion = DateOnly.FromDateTime(DateTime.Now),
                    EsEliminado = false
                };

                _context.Usuarios.Add(nuevoUsuario);
                await _context.SaveChangesAsync();

                _logger.LogInformation($"✅ Usuario creado: {nuevoUsuario.Nombres} {nuevoUsuario.Apellidos} (ID: {nuevoUsuario.IdUsuario})");

                TempData["Mensaje"] = $"Usuario {nuevoUsuario.Nombres} {nuevoUsuario.Apellidos} creado exitosamente";
                return RedirectToAction(nameof(Detalle), new { id = nuevoUsuario.IdUsuario });
            }
            catch (DbUpdateException ex)
            {
                _logger.LogError(ex, "❌ Error de base de datos al crear usuario");
                TempData["Error"] = "Error al guardar el usuario en la base de datos";
                return View(model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error al crear usuario");
                TempData["Error"] = "Error inesperado al crear el usuario";
                return View(model);
            }
        }

        // ============================================
        // EDITAR USUARIO
        // ============================================

        /// <summary>
        /// Vista para editar un usuario existente
        /// GET: /AdminUsuarios/Editar/{id}
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> Editar(int id)
        {
            try
            {
                var usuario = await _context.Usuarios
                    .Include(u => u.Rol)
                    .FirstOrDefaultAsync(u => u.IdUsuario == id && !u.EsEliminado);

                if (usuario == null)
                {
                    _logger.LogWarning($"⚠️ Usuario con ID {id} no encontrado");
                    TempData["Error"] = "Usuario no encontrado";
                    return RedirectToAction(nameof(Index));
                }

                var usuarioActualId = User.GetUserId();
                var roles = await _context.Rols.OrderBy(r => r.NombreRol).ToListAsync();

                var viewModel = new EditarUsuarioViewModel
                {
                    IdUsuario = usuario.IdUsuario,
                    Nombres = usuario.Nombres,
                    Apellidos = usuario.Apellidos,
                    DNI = usuario.DNI,
                    Email = usuario.CorreoElectronico,
                    Telefono = usuario.Telefono,
                    Area = usuario.Area,
                    IdRol = usuario.IdRol,
                    Estado = usuario.Estado,
                    EsUsuarioActual = usuario.IdUsuario == usuarioActualId,
                    RolActual = usuario.Rol?.NombreRol ?? "",
                    RolesDisponibles = roles
                };

                return View(viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"❌ Error al cargar formulario de editar usuario ID {id}");
                TempData["Error"] = "Error al cargar el formulario";
                return RedirectToAction(nameof(Index));
            }
        }

        /// <summary>
        /// Procesa la edición de un usuario
        /// POST: /AdminUsuarios/Editar
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Editar(EditarUsuarioViewModel model)
        {
            try
            {
                // Recargar datos en caso de error
                model.RolesDisponibles = await _context.Rols.OrderBy(r => r.NombreRol).ToListAsync();

                if (!ModelState.IsValid)
                {
                    return View(model);
                }

                var usuario = await _context.Usuarios
                    .Include(u => u.Rol)
                    .FirstOrDefaultAsync(u => u.IdUsuario == model.IdUsuario && !u.EsEliminado);

                if (usuario == null)
                {
                    TempData["Error"] = "Usuario no encontrado";
                    return RedirectToAction(nameof(Index));
                }

                var usuarioActualId = User.GetUserId();
                model.EsUsuarioActual = usuario.IdUsuario == usuarioActualId;

                // Validar que admin no se quite rol Administrador a sí mismo
                if (model.EsUsuarioActual && usuario.Rol?.NombreRol == "Administrador")
                {
                    var nuevoRol = await _context.Rols.FindAsync(model.IdRol);
                    if (nuevoRol?.NombreRol != "Administrador")
                    {
                        ModelState.AddModelError("IdRol", "No puede quitarse el rol de Administrador a sí mismo");
                        return View(model);
                    }
                }

                // Validar que admin no se desactive a sí mismo
                if (model.EsUsuarioActual && !model.Estado)
                {
                    ModelState.AddModelError("Estado", "No puede desactivarse a sí mismo");
                    return View(model);
                }

                // Validar Email único si cambió
                if (usuario.CorreoElectronico != model.Email)
                {
                    var emailExiste = await _context.Usuarios
                        .AnyAsync(u => u.CorreoElectronico == model.Email && u.IdUsuario != model.IdUsuario && !u.EsEliminado);

                    if (emailExiste)
                    {
                        ModelState.AddModelError("Email", "Ya existe otro usuario con este email");
                        return View(model);
                    }
                }

                // Actualizar usuario
                usuario.Nombres = model.Nombres.Trim();
                usuario.Apellidos = model.Apellidos.Trim();
                usuario.CorreoElectronico = model.Email.Trim();
                usuario.Telefono = model.Telefono?.Trim() ?? "";
                usuario.Area = model.Area.Trim();
                usuario.IdRol = model.IdRol;
                usuario.Estado = model.Estado;
                usuario.FechaActualizacion = DateOnly.FromDateTime(DateTime.Now);

                _context.Usuarios.Update(usuario);
                await _context.SaveChangesAsync();

                _logger.LogInformation($"✅ Usuario actualizado: {usuario.Nombres} {usuario.Apellidos} (ID: {usuario.IdUsuario})");

                TempData["Mensaje"] = $"Usuario {usuario.Nombres} {usuario.Apellidos} actualizado exitosamente";
                return RedirectToAction(nameof(Detalle), new { id = usuario.IdUsuario });
            }
            catch (DbUpdateException ex)
            {
                _logger.LogError(ex, $"❌ Error de base de datos al editar usuario ID {model.IdUsuario}");
                TempData["Error"] = "Error al actualizar el usuario en la base de datos";
                return View(model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"❌ Error al editar usuario ID {model.IdUsuario}");
                TempData["Error"] = "Error inesperado al actualizar el usuario";
                return View(model);
            }
        }

        // ============================================
        // ELIMINAR USUARIO (SOFT DELETE)
        // ============================================

        /// <summary>
        /// Elimina un usuario (soft delete)
        /// POST: /AdminUsuarios/Eliminar
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Eliminar(int id)
        {
            try
            {
                var usuario = await _context.Usuarios
                    .Include(u => u.Rol)
                    .FirstOrDefaultAsync(u => u.IdUsuario == id && !u.EsEliminado);

                if (usuario == null)
                {
                    return Json(new { success = false, mensaje = "Usuario no encontrado" });
                }

                var usuarioActualId = User.GetUserId();

                // NO permitir eliminar al usuario actual
                if (usuario.IdUsuario == usuarioActualId)
                {
                    return Json(new { success = false, mensaje = "No puede eliminarse a sí mismo" });
                }

                // NO permitir eliminar el último administrador
                if (usuario.Rol?.NombreRol == "Administrador")
                {
                    var totalAdmins = await _context.Usuarios
                        .CountAsync(u => u.Rol!.NombreRol == "Administrador" && !u.EsEliminado);

                    if (totalAdmins <= 1)
                    {
                        return Json(new { success = false, mensaje = "No puede eliminar el último administrador del sistema" });
                    }
                }

                // Soft Delete
                usuario.EsEliminado = true;
                usuario.FechaEliminacion = DateTime.Now;
                usuario.EliminadoPor = usuarioActualId;

                _context.Usuarios.Update(usuario);
                await _context.SaveChangesAsync();

                _logger.LogInformation($"✅ Usuario eliminado (soft delete): {usuario.Nombres} {usuario.Apellidos} (ID: {usuario.IdUsuario}) por usuario ID: {usuarioActualId}");

                return Json(new { success = true, mensaje = $"Usuario {usuario.Nombres} {usuario.Apellidos} eliminado exitosamente" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"❌ Error al eliminar usuario ID {id}");
                return Json(new { success = false, mensaje = "Error al eliminar el usuario" });
            }
        }

        // ============================================
        // CAMBIAR ESTADO (ACTIVAR/DESACTIVAR)
        // ============================================

        /// <summary>
        /// Cambia el estado de un usuario (activo/inactivo)
        /// POST: /AdminUsuarios/CambiarEstado
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CambiarEstado(int id)
        {
            try
            {
                var usuario = await _context.Usuarios
                    .Include(u => u.Rol)
                    .FirstOrDefaultAsync(u => u.IdUsuario == id && !u.EsEliminado);

                if (usuario == null)
                {
                    return Json(new { success = false, mensaje = "Usuario no encontrado" });
                }

                var usuarioActualId = User.GetUserId();

                // NO permitir desactivarse a sí mismo
                if (usuario.IdUsuario == usuarioActualId && usuario.Estado)
                {
                    return Json(new { success = false, mensaje = "No puede desactivarse a sí mismo" });
                }

                // NO permitir desactivar el último administrador activo
                if (usuario.Rol?.NombreRol == "Administrador" && usuario.Estado)
                {
                    var totalAdminsActivos = await _context.Usuarios
                        .CountAsync(u => u.Rol!.NombreRol == "Administrador" && u.Estado && !u.EsEliminado);

                    if (totalAdminsActivos <= 1)
                    {
                        return Json(new { success = false, mensaje = "No puede desactivar el último administrador activo" });
                    }
                }

                // Cambiar estado
                usuario.Estado = !usuario.Estado;
                usuario.FechaActualizacion = DateOnly.FromDateTime(DateTime.Now);

                _context.Usuarios.Update(usuario);
                await _context.SaveChangesAsync();

                var nuevoEstado = usuario.Estado ? "activado" : "desactivado";
                _logger.LogInformation($"✅ Usuario {nuevoEstado}: {usuario.Nombres} {usuario.Apellidos} (ID: {usuario.IdUsuario})");

                return Json(new
                {
                    success = true,
                    mensaje = $"Usuario {nuevoEstado} exitosamente",
                    nuevoEstado = usuario.Estado
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"❌ Error al cambiar estado del usuario ID {id}");
                return Json(new { success = false, mensaje = "Error al cambiar el estado del usuario" });
            }
        }

        // ============================================
        // CAMBIAR CONTRASEÑA
        // ============================================

        /// <summary>
        /// Vista para cambiar la contraseña de un usuario
        /// GET: /AdminUsuarios/CambiarPassword/{id}
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> CambiarPassword(int id)
        {
            try
            {
                var usuario = await _context.Usuarios
                    .FirstOrDefaultAsync(u => u.IdUsuario == id && !u.EsEliminado);

                if (usuario == null)
                {
                    _logger.LogWarning($"⚠️ Usuario con ID {id} no encontrado");
                    TempData["Error"] = "Usuario no encontrado";
                    return RedirectToAction(nameof(Index));
                }

                var viewModel = new CambiarPasswordViewModel
                {
                    IdUsuario = usuario.IdUsuario,
                    NombreCompleto = $"{usuario.Nombres} {usuario.Apellidos}",
                    DNI = usuario.DNI
                };

                return View(viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"❌ Error al cargar formulario de cambiar contraseña para usuario ID {id}");
                TempData["Error"] = "Error al cargar el formulario";
                return RedirectToAction(nameof(Index));
            }
        }

        /// <summary>
        /// Procesa el cambio de contraseña de un usuario
        /// POST: /AdminUsuarios/CambiarPassword
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CambiarPassword(CambiarPasswordViewModel model)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return View(model);
                }

                var usuario = await _context.Usuarios
                    .FirstOrDefaultAsync(u => u.IdUsuario == model.IdUsuario && !u.EsEliminado);

                if (usuario == null)
                {
                    TempData["Error"] = "Usuario no encontrado";
                    return RedirectToAction(nameof(Index));
                }

                // Actualizar contraseña
                // TODO: Hashear en producción
                usuario.ClavePermanente = model.NuevaContrasena;
                usuario.ClaveTemporal = model.NuevaContrasena;
                usuario.PrimerInicio = false;
                usuario.FechaActualizacion = DateOnly.FromDateTime(DateTime.Now);

                _context.Usuarios.Update(usuario);
                await _context.SaveChangesAsync();

                _logger.LogInformation($"✅ Contraseña cambiada para usuario: {usuario.Nombres} {usuario.Apellidos} (ID: {usuario.IdUsuario})");

                TempData["Mensaje"] = $"Contraseña actualizada exitosamente para {usuario.Nombres} {usuario.Apellidos}";
                return RedirectToAction(nameof(Detalle), new { id = usuario.IdUsuario });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"❌ Error al cambiar contraseña del usuario ID {model.IdUsuario}");
                TempData["Error"] = "Error al cambiar la contraseña";
                return View(model);
            }
        }
    }
}

