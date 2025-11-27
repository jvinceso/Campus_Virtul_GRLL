using Campus_Virtul_GRLL.Data;
using Campus_Virtul_GRLL.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace Campus_Virtul_GRLL.Controllers
{
    public class LoginController : Controller
    {
        private readonly AppDBContext _appContext;
        private readonly ILogger<LoginController> _logger;

        public LoginController(AppDBContext appContext, ILogger<LoginController> logger)
        {
            _appContext = appContext;
            _logger = logger;
        }

        /// Muestra el formulario de login
        [HttpGet]
        public IActionResult Index()
        {
            return View("Login");
        }

        /// Procesa el login del usuario (con DNI o Email)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(string DNI, string Contrasena)
        {
            try
            {
                // ============================================
                // 1. VALIDACIONES INICIALES
                // ============================================
                if (string.IsNullOrWhiteSpace(DNI) || string.IsNullOrWhiteSpace(Contrasena))
                {
                    TempData["Error"] = "Por favor, complete todos los campos.";
                    return View("Login");
                }

                string credencial = DNI.Trim();
                Contrasena = Contrasena.Trim();

                _logger.LogInformation($"Intento de login con credencial: {credencial}");

                // ============================================
                // 2. BUSCAR USUARIO EN LA BASE DE DATOS (por DNI o Email)
                // ============================================
                Usuario? usuario = null;

                // Determinar si es DNI (8 dígitos) o Email
                bool esDNI = credencial.Length == 8 && credencial.All(char.IsDigit);

                if (esDNI)
                {
                    // Buscar por DNI
                    usuario = await _appContext.Usuarios
                        .Include(u => u.Rol)
                        .FirstOrDefaultAsync(u => u.DNI == credencial);
                }
                else
                {
                    // Buscar por Email
                    usuario = await _appContext.Usuarios
                        .Include(u => u.Rol)
                        .FirstOrDefaultAsync(u => u.CorreoElectronico == credencial);
                }

                // Validar que el usuario existe
                if (usuario == null)
                {
                    _logger.LogWarning($"Usuario no encontrado - Credencial: {credencial}");
                    TempData["Error"] = esDNI
                        ? "El DNI ingresado no está registrado en el sistema."
                        : "El Email ingresado no está registrado en el sistema.";
                    return View("Login");
                }

                // Validar que el usuario esté activo (Estado = 1 o true)
                if (!usuario.Estado)
                {
                    _logger.LogWarning($"Usuario inactivo - Credencial: {credencial}, ID: {usuario.IdUsuario}");
                    TempData["Error"] = "Su cuenta está inactiva. Contacte al administrador.";
                    return View("Login");
                }

                // ============================================
                // 3. VERIFICAR CONTRASEÑA
                // ============================================
                bool contrasenaCorrecta = false;

                // Si es primer inicio, verificar con clave temporal
                if (usuario.PrimerInicio && Contrasena == usuario.ClaveTemporal)
                {
                    contrasenaCorrecta = true;
                    _logger.LogInformation($"Login con clave temporal - Usuario ID: {usuario.IdUsuario}");
                }
                // Si no es primer inicio, verificar con clave permanente
                else if (!usuario.PrimerInicio && Contrasena == usuario.ClavePermanente)
                {
                    contrasenaCorrecta = true;
                    _logger.LogInformation($"Login con clave permanente - Usuario ID: {usuario.IdUsuario}");
                }

                if (!contrasenaCorrecta)
                {
                    _logger.LogWarning($"Contraseña incorrecta - Credencial: {credencial}");
                    TempData["Error"] = "Credenciales incorrectas.";
                    return View("Login");
                }

                // ============================================
                // 4. CREAR CLAIMS (Información del usuario)
                // ============================================
                var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.NameIdentifier, usuario.IdUsuario.ToString()),
                    new Claim(ClaimTypes.Name, $"{usuario.Nombres} {usuario.Apellidos}"),
                    new Claim(ClaimTypes.Email, usuario.CorreoElectronico),
                    new Claim(ClaimTypes.Role, usuario.Rol?.NombreRol ?? "Usuario"),
                    new Claim("DNI", usuario.DNI),
                    new Claim("RolId", usuario.IdRol.ToString()),
                    new Claim("Area", usuario.Area),
                    new Claim("Nombres", usuario.Nombres),
                    new Claim("Apellidos", usuario.Apellidos),
                    new Claim("Telefono", usuario.Telefono),
                    new Claim("PrimerInicio", usuario.PrimerInicio.ToString())
                };

                // ============================================
                // 5. CREAR IDENTITY Y AUTENTICAR
                // ============================================
                var claimsIdentity = new ClaimsIdentity(
                    claims,
                    CookieAuthenticationDefaults.AuthenticationScheme
                );

                var claimsPrincipal = new ClaimsPrincipal(claimsIdentity);

                var authProperties = new AuthenticationProperties
                {
                    IsPersistent = true,
                    ExpiresUtc = DateTimeOffset.UtcNow.AddHours(8),
                    AllowRefresh = true,
                    IssuedUtc = DateTimeOffset.UtcNow
                };

                // Crear Cookie de autenticación
                await HttpContext.SignInAsync(
                    CookieAuthenticationDefaults.AuthenticationScheme,
                    claimsPrincipal,
                    authProperties
                );

                _logger.LogInformation($"✅ Usuario autenticado exitosamente - ID: {usuario.IdUsuario}, Nombre: {usuario.Nombres} {usuario.Apellidos}");

                // ============================================
                // 6. REDIRECCIONAR SEGÚN EL ESTADO DEL USUARIO
                // ============================================

                // Si es primer inicio, debe cambiar su contraseña
                if (usuario.PrimerInicio)
                {
                    TempData["Mensaje"] = "Bienvenido. Por seguridad, debe cambiar su contraseña temporal.";
                    _logger.LogInformation($"Redirigiendo a cambio de contraseña - Usuario ID: {usuario.IdUsuario}");

                    ViewBag.NombreUsuario = $"{usuario.Nombres} {usuario.Apellidos}";
                    ViewBag.EsPrimerInicio = usuario.PrimerInicio;
                    return View("CambiarContrasena");
                }

                // Login exitoso normal
                TempData["Mensaje"] = $"¡Bienvenido {usuario.Nombres}!";
                _logger.LogInformation($"Redirigiendo al Dashboard - Usuario ID: {usuario.IdUsuario}");
                return RedirectToAction("Index", "Dashboard");
            }
            catch (DbUpdateException ex)
            {
                _logger.LogError(ex, "❌ Error de base de datos durante el login");
                var errorMessage = ex.InnerException?.Message ?? ex.Message;
                TempData["Error"] = $"Error de base de datos: {errorMessage}";
                return View("Login");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error crítico durante el proceso de login");
                var errorMessage = ex.InnerException?.Message ?? ex.Message;
                TempData["Error"] = $"Error inesperado: {errorMessage}";
                return View("Login");
            }
        }

        /// Procesar cambio de contraseña 
        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CambiarContrasena(
            string ContrasenaActual,
            string ContrasenaNueva,
            string ConfirmarContrasena)
        {
            try
            {
                var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
                var usuario = await _appContext.Usuarios
                    .Include(u => u.Rol)
                    .FirstOrDefaultAsync(u => u.IdUsuario == userId);

                if (usuario == null)
                {
                    TempData["Error"] = "Usuario no encontrado.";
                    return RedirectToAction("Index", "Login");
                }

                // ============================================
                // 1. VALIDACIONES
                // ============================================
                if (string.IsNullOrWhiteSpace(ContrasenaActual) ||
                    string.IsNullOrWhiteSpace(ContrasenaNueva) ||
                    string.IsNullOrWhiteSpace(ConfirmarContrasena))
                {
                    TempData["Error"] = "Todos los campos son obligatorios.";
                    ViewBag.NombreUsuario = $"{usuario.Nombres} {usuario.Apellidos}";
                    ViewBag.EsPrimerInicio = usuario.PrimerInicio;
                    return View("CambiarContrasena");
                }

                // Validar que la nueva contraseña coincida con la confirmación
                if (ContrasenaNueva.Trim() != ConfirmarContrasena.Trim())
                {
                    TempData["Error"] = "La nueva contraseña y la confirmación no coinciden.";
                    ViewBag.NombreUsuario = $"{usuario.Nombres} {usuario.Apellidos}";
                    ViewBag.EsPrimerInicio = usuario.PrimerInicio;
                    return View("CambiarContrasena");
                }

                // Validar longitud de la nueva contraseña
                if (ContrasenaNueva.Trim().Length < 6)
                {
                    TempData["Error"] = "La nueva contraseña debe tener al menos 6 caracteres.";
                    ViewBag.NombreUsuario = $"{usuario.Nombres} {usuario.Apellidos}";
                    ViewBag.EsPrimerInicio = usuario.PrimerInicio;
                    return View("CambiarContrasena");
                }

                // ============================================
                // 2. VERIFICAR CONTRASEÑA ACTUAL
                // ============================================
                bool contrasenaActualCorrecta = false;

                if (usuario.PrimerInicio && ContrasenaActual.Trim() == usuario.ClaveTemporal)
                {
                    contrasenaActualCorrecta = true;
                }
                else if (!usuario.PrimerInicio && ContrasenaActual.Trim() == usuario.ClavePermanente)
                {
                    contrasenaActualCorrecta = true;
                }

                if (!contrasenaActualCorrecta)
                {
                    TempData["Error"] = "La contraseña actual es incorrecta.";
                    _logger.LogWarning($"Intento fallido de cambio de contraseña - Usuario ID: {userId}");
                    ViewBag.NombreUsuario = $"{usuario.Nombres} {usuario.Apellidos}";
                    ViewBag.EsPrimerInicio = usuario.PrimerInicio;
                    return View("CambiarContrasena");
                }

                // ============================================
                // 3. ACTUALIZAR CONTRASEÑA
                // ============================================
                usuario.ClavePermanente = ContrasenaNueva.Trim();
                usuario.PrimerInicio = false;
                usuario.FechaActualizacion = DateOnly.FromDateTime(DateTime.Now);

                _appContext.Usuarios.Update(usuario);
                await _appContext.SaveChangesAsync();

                _logger.LogInformation($"✅ Contraseña actualizada exitosamente - Usuario ID: {userId}");

                // ============================================
                // 4. ACTUALIZAR CLAIMS (Cookie de autenticación)
                // ============================================
                var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.NameIdentifier, usuario.IdUsuario.ToString()),
                    new Claim(ClaimTypes.Name, $"{usuario.Nombres} {usuario.Apellidos}"),
                    new Claim(ClaimTypes.Email, usuario.CorreoElectronico),
                    new Claim(ClaimTypes.Role, usuario.Rol?.NombreRol ?? "Usuario"),
                    new Claim("DNI", usuario.DNI),
                    new Claim("RolId", usuario.IdRol.ToString()),
                    new Claim("Area", usuario.Area),
                    new Claim("Nombres", usuario.Nombres),
                    new Claim("Apellidos", usuario.Apellidos),
                    new Claim("Telefono", usuario.Telefono),
                    new Claim("PrimerInicio", "False")
                };

                var claimsIdentity = new ClaimsIdentity(
                    claims,
                    CookieAuthenticationDefaults.AuthenticationScheme
                );

                var claimsPrincipal = new ClaimsPrincipal(claimsIdentity);

                var authProperties = new AuthenticationProperties
                {
                    IsPersistent = true,
                    ExpiresUtc = DateTimeOffset.UtcNow.AddHours(8),
                    AllowRefresh = true
                };

                // Actualizar cookie de autenticación
                await HttpContext.SignInAsync(
                    CookieAuthenticationDefaults.AuthenticationScheme,
                    claimsPrincipal,
                    authProperties
                );

                TempData["Mensaje"] = "Contraseña actualizada exitosamente.";
                return RedirectToAction("Index", "Login");
            }
            catch (DbUpdateException ex)
            {
                _logger.LogError(ex, "❌ Error de base de datos al cambiar contraseña");
                var errorMessage = ex.InnerException?.Message ?? ex.Message;
                TempData["Error"] = $"Error al guardar en la base de datos: {errorMessage}";
                return View("CambiarContrasena");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error al cambiar contraseña");
                var errorMessage = ex.InnerException?.Message ?? ex.Message;
                TempData["Error"] = $"Error inesperado: {errorMessage}";
                return View("CambiarContrasena");
            }
        }

        /// <summary>
        /// Cierra la sesión del usuario
        /// GET: /Login/Logout
        /// </summary>
        [HttpGet]
        [Authorize]
        public async Task<IActionResult> Logout()
        {
            try
            {
                var userName = User.Identity?.Name ?? "Usuario desconocido";
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0";

                _logger.LogInformation($"🚪 Cerrando sesión - Usuario: {userName} (ID: {userId})");

                // Eliminar cookie de autenticación
                await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);

                _logger.LogInformation($"✅ Sesión cerrada exitosamente - Usuario: {userName}");

                TempData["Mensaje"] = "Sesión cerrada correctamente.";
                return RedirectToAction("Index", "Login");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error al cerrar sesión");
                TempData["Error"] = "Error al cerrar la sesión. Por favor, intente nuevamente.";
                return RedirectToAction("Index", "Dashboard");
            }
        }

        /// Página cuando el usuario no tiene permisos
        [HttpGet]
        public IActionResult AccesoDenegado()
        {
            ViewBag.Mensaje = "No tiene permisos para acceder a esta página.";
            return View();
        }
    }
}