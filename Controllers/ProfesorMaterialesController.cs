using Campus_Virtul_GRLL.Data;
using Campus_Virtul_GRLL.Helpers;
using Campus_Virtul_GRLL.Models;
using Campus_Virtul_GRLL.Models.Enums;
using Campus_Virtul_GRLL.Models.ViewModels;
using Campus_Virtul_GRLL.Services;
using Campus_Virtul_GRLL.Utilities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Campus_Virtul_GRLL.Controllers
{
    [Authorize(Roles = "Administrador,Profesor")]
    public class ProfesorMaterialesController : Controller
    {
        private readonly AppDBContext _context;
        private readonly IFileStorageService _fileStorageService;
        private readonly ILogger<ProfesorMaterialesController> _logger;

        public ProfesorMaterialesController(
            AppDBContext context,
            IFileStorageService fileStorageService,
            ILogger<ProfesorMaterialesController> logger)
        {
            _context = context;
            _fileStorageService = fileStorageService;
            _logger = logger;
        }

        // ============================================
        // INDEX - Listar Materiales del Curso
        // ============================================
        [HttpGet]
        public async Task<IActionResult> Index(int idCurso)
        {
            try
            {
                var curso = await _context.Cursos
                    .FirstOrDefaultAsync(c => c.Id == idCurso && !c.EsEliminado);

                if (curso == null)
                {
                    TempData["Error"] = "Curso no encontrado";
                    return RedirectToAction("Index", "AdminCursos");
                }

                // Verificar que el profesor tenga acceso al curso
                var usuarioId = User.GetUserId();
                if (!User.EsAdministrador())
                {
                    var esProfesorDelCurso = await _context.Cursos
                        .AnyAsync(c => c.Id == idCurso && c.ProfesorAsignado == usuarioId);

                    if (!esProfesorDelCurso)
                    {
                        TempData["Error"] = "No tiene permisos para gestionar este curso";
                        return RedirectToAction("Index", "AdminCursos");
                    }
                }

                // Obtener módulos con materiales
                var modulos = await _context.ModulosCurso
                    .Where(m => m.IdCurso == idCurso && !m.EsEliminado)
                    .OrderBy(m => m.Orden)
                    .Select(m => new ModuloCursoViewModel
                    {
                        IdModuloCurso = m.IdModuloCurso,
                        Titulo = m.Titulo,
                        Descripcion = m.Descripcion,
                        Orden = m.Orden,
                        Materiales = m.Materiales
                            .Where(mat => !mat.EsEliminado)
                            .OrderBy(mat => mat.Orden)
                            .Select(mat => new MaterialCardViewModel
                            {
                                IdMaterial = mat.IdMaterial,
                                Titulo = mat.Titulo,
                                Descripcion = mat.Descripcion,
                                Tipo = mat.Tipo,
                                Extension = mat.Extension,
                                TamanoBytes = mat.TamanoBytes,
                                UrlExterna = mat.UrlExterna,
                                EsVisible = mat.EsVisible,
                                FechaSubida = mat.FechaSubida,
                                NumeroDescargas = mat.NumeroDescargas,
                                Orden = mat.Orden
                            })
                            .ToList()
                    })
                    .ToListAsync();

                // Obtener materiales sin módulo
                var materialesSinModulo = await _context.Materiales
                    .Where(m => m.IdCurso == idCurso && m.IdModuloCurso == null && !m.EsEliminado)
                    .OrderBy(m => m.Orden)
                    .Select(m => new MaterialCardViewModel
                    {
                        IdMaterial = m.IdMaterial,
                        Titulo = m.Titulo,
                        Descripcion = m.Descripcion,
                        Tipo = m.Tipo,
                        Extension = m.Extension,
                        TamanoBytes = m.TamanoBytes,
                        UrlExterna = m.UrlExterna,
                        EsVisible = m.EsVisible,
                        FechaSubida = m.FechaSubida,
                        NumeroDescargas = m.NumeroDescargas,
                        Orden = m.Orden
                    })
                    .ToListAsync();

                var viewModel = new MaterialesProfesorViewModel
                {
                    IdCurso = curso.Id,
                    TituloCurso = curso.Titulo,
                    Modulos = modulos,
                    MaterialesSinModulo = materialesSinModulo,
                    TotalMateriales = modulos.Sum(m => m.Materiales.Count) + materialesSinModulo.Count,
                    TamanoTotalBytes = await _context.Materiales
                        .Where(m => m.IdCurso == idCurso && !m.EsEliminado && m.TamanoBytes.HasValue)
                        .SumAsync(m => m.TamanoBytes ?? 0)
                };

                return View(viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al cargar materiales del curso {IdCurso}", idCurso);
                TempData["Error"] = "Error al cargar los materiales";
                return RedirectToAction("Index", "AdminCursos");
            }
        }

        // ============================================
        // SUBIR - Formulario de Subida
        // ============================================
        [HttpGet]
        public async Task<IActionResult> Subir(int idCurso)
        {
            try
            {
                var curso = await _context.Cursos
                    .FirstOrDefaultAsync(c => c.Id == idCurso && !c.EsEliminado);

                if (curso == null)
                {
                    TempData["Error"] = "Curso no encontrado";
                    return RedirectToAction("Index", "AdminCursos");
                }

                // Verificar permisos
                var usuarioId = User.GetUserId();
                if (!User.EsAdministrador())
                {
                    var esProfesorDelCurso = await _context.Cursos
                        .AnyAsync(c => c.Id == idCurso && c.ProfesorAsignado == usuarioId);

                    if (!esProfesorDelCurso)
                    {
                        TempData["Error"] = "No tiene permisos para gestionar este curso";
                        return RedirectToAction("Index", "AdminCursos");
                    }
                }

                var modulos = await _context.ModulosCurso
                    .Where(m => m.IdCurso == idCurso && !m.EsEliminado)
                    .OrderBy(m => m.Orden)
                    .ToListAsync();

                var viewModel = new SubirMaterialViewModel
                {
                    IdCurso = curso.Id,
                    TituloCurso = curso.Titulo,
                    ModulosDisponibles = modulos,
                    EsVisible = true
                };

                return View(viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al cargar formulario de subida para curso {IdCurso}", idCurso);
                TempData["Error"] = "Error al cargar el formulario";
                return RedirectToAction("Index", new { idCurso });
            }
        }

        // ============================================
        // SUBIR - Procesar Subida (POST)
        // ============================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Subir(SubirMaterialViewModel model)
        {
            try
            {
                // Validar que sea archivo O enlace (no ambos ni ninguno)
                if (model.Archivo == null && string.IsNullOrWhiteSpace(model.UrlExterna))
                {
                    ModelState.AddModelError("", "Debe subir un archivo o proporcionar un enlace externo");
                }

                if (model.Archivo != null && !string.IsNullOrWhiteSpace(model.UrlExterna))
                {
                    ModelState.AddModelError("", "Debe elegir solo una opción: archivo o enlace externo");
                }

                // Validar archivo si existe
                if (model.Archivo != null)
                {
                    var (isValid, error) = FileValidator.ValidateFile(model.Archivo);
                    if (!isValid)
                    {
                        ModelState.AddModelError("Archivo", error);
                    }
                }

                // Validar URL si existe
                if (!string.IsNullOrWhiteSpace(model.UrlExterna))
                {
                    var (isValidUrl, errorUrl) = FileHelper.ValidateUrl(model.UrlExterna);
                    if (!isValidUrl)
                    {
                        ModelState.AddModelError("UrlExterna", errorUrl);
                    }
                }

                if (!ModelState.IsValid)
                {
                    // Recargar módulos disponibles
                    model.ModulosDisponibles = await _context.ModulosCurso
                        .Where(m => m.IdCurso == model.IdCurso && !m.EsEliminado)
                        .OrderBy(m => m.Orden)
                        .ToListAsync();
                    return View(model);
                }

                // Obtener siguiente orden
                var siguienteOrden = await ObtenerSiguienteOrden(model.IdCurso, model.IdModuloCurso);

                // Crear entidad Material
                var material = new Material
                {
                    IdCurso = model.IdCurso,
                    Titulo = model.Titulo,
                    Descripcion = model.Descripcion,
                    IdModuloCurso = model.IdModuloCurso,
                    EsVisible = model.EsVisible,
                    FechaSubida = DateTime.Now,
                    SubidoPorId = User.GetUserId(),
                    Orden = siguienteOrden,
                    NumeroDescargas = 0,
                    EsEliminado = false
                };

                // Procesar archivo
                if (model.Archivo != null)
                {
                    var rutaArchivo = await _fileStorageService.SaveFileAsync(model.Archivo, model.IdCurso);
                    material.NombreArchivo = model.Archivo.FileName;
                    material.RutaArchivo = rutaArchivo;
                    material.Extension = Path.GetExtension(model.Archivo.FileName);
                    material.TamanoBytes = model.Archivo.Length;
                    material.Tipo = DeterminarTipoMaterial(material.Extension);
                }
                // Procesar enlace externo
                else if (!string.IsNullOrWhiteSpace(model.UrlExterna))
                {
                    material.UrlExterna = model.UrlExterna;
                    material.Tipo = model.TipoMaterial ?? TipoMaterial.Enlace;
                }

                _context.Materiales.Add(material);
                await _context.SaveChangesAsync();

                _logger.LogInformation("✅ Material '{Titulo}' subido por usuario {UsuarioId} al curso {IdCurso}",
                    material.Titulo, User.GetUserId(), model.IdCurso);

                TempData["Exito"] = "Material subido exitosamente";
                return RedirectToAction(nameof(Index), new { idCurso = model.IdCurso });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error al subir material al curso {IdCurso}", model.IdCurso);
                TempData["Error"] = "Error al subir el material: " + ex.Message;

                // Recargar módulos disponibles
                model.ModulosDisponibles = await _context.ModulosCurso
                    .Where(m => m.IdCurso == model.IdCurso && !m.EsEliminado)
                    .OrderBy(m => m.Orden)
                    .ToListAsync();
                return View(model);
            }
        }

        // ============================================
        // EDITAR - Formulario de Edición
        // ============================================
        [HttpGet]
        public async Task<IActionResult> Editar(int id)
        {
            try
            {
                var material = await _context.Materiales
                    .Include(m => m.Curso)
                    .FirstOrDefaultAsync(m => m.IdMaterial == id && !m.EsEliminado);

                if (material == null)
                {
                    TempData["Error"] = "Material no encontrado";
                    return RedirectToAction("Index", "AdminCursos");
                }

                // Verificar permisos
                var usuarioId = User.GetUserId();
                if (!User.EsAdministrador())
                {
                    var esProfesorDelCurso = await _context.Cursos
                        .AnyAsync(c => c.Id == material.IdCurso && c.ProfesorAsignado == usuarioId);

                    if (!esProfesorDelCurso)
                    {
                        TempData["Error"] = "No tiene permisos para editar este material";
                        return RedirectToAction("Index", new { idCurso = material.IdCurso });
                    }
                }

                var modulos = await _context.ModulosCurso
                    .Where(m => m.IdCurso == material.IdCurso && !m.EsEliminado)
                    .OrderBy(m => m.Orden)
                    .ToListAsync();

                var viewModel = new EditarMaterialViewModel
                {
                    IdMaterial = material.IdMaterial,
                    IdCurso = material.IdCurso,
                    TituloCurso = material.Curso.Titulo,
                    Titulo = material.Titulo,
                    Descripcion = material.Descripcion,
                    IdModuloCurso = material.IdModuloCurso,
                    EsVisible = material.EsVisible,
                    Orden = material.Orden,
                    UrlExterna = material.UrlExterna,
                    NombreArchivo = material.NombreArchivo,
                    Extension = material.Extension,
                    TamanoBytes = material.TamanoBytes,
                    Tipo = material.Tipo,
                    EsEnlace = !string.IsNullOrEmpty(material.UrlExterna),
                    ModulosDisponibles = modulos
                };

                return View(viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al cargar formulario de edición para material {IdMaterial}", id);
                TempData["Error"] = "Error al cargar el formulario";
                return RedirectToAction("Index", "AdminCursos");
            }
        }

        // ============================================
        // EDITAR - Procesar Edición (POST)
        // ============================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Editar(EditarMaterialViewModel model)
        {
            try
            {
                // Validar URL si es enlace
                if (model.EsEnlace && !string.IsNullOrWhiteSpace(model.UrlExterna))
                {
                    var (isValidUrl, errorUrl) = FileHelper.ValidateUrl(model.UrlExterna);
                    if (!isValidUrl)
                    {
                        ModelState.AddModelError("UrlExterna", errorUrl);
                    }
                }

                if (!ModelState.IsValid)
                {
                    model.ModulosDisponibles = await _context.ModulosCurso
                        .Where(m => m.IdCurso == model.IdCurso && !m.EsEliminado)
                        .OrderBy(m => m.Orden)
                        .ToListAsync();
                    return View(model);
                }

                var material = await _context.Materiales
                    .FirstOrDefaultAsync(m => m.IdMaterial == model.IdMaterial && !m.EsEliminado);

                if (material == null)
                {
                    TempData["Error"] = "Material no encontrado";
                    return RedirectToAction(nameof(Index), new { idCurso = model.IdCurso });
                }

                // Actualizar campos
                material.Titulo = model.Titulo;
                material.Descripcion = model.Descripcion;
                material.IdModuloCurso = model.IdModuloCurso;
                material.EsVisible = model.EsVisible;
                material.Orden = model.Orden;

                // Actualizar URL si es enlace
                if (model.EsEnlace)
                {
                    material.UrlExterna = model.UrlExterna;
                }

                await _context.SaveChangesAsync();

                _logger.LogInformation("✅ Material {IdMaterial} editado por usuario {UsuarioId}",
                    material.IdMaterial, User.GetUserId());

                TempData["Exito"] = "Material actualizado exitosamente";
                return RedirectToAction(nameof(Index), new { idCurso = model.IdCurso });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error al editar material {IdMaterial}", model.IdMaterial);
                TempData["Error"] = "Error al actualizar el material";

                model.ModulosDisponibles = await _context.ModulosCurso
                    .Where(m => m.IdCurso == model.IdCurso && !m.EsEliminado)
                    .OrderBy(m => m.Orden)
                    .ToListAsync();
                return View(model);
            }
        }

        // ============================================
        // ELIMINAR - Soft Delete
        // ============================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Eliminar(int id)
        {
            try
            {
                var material = await _context.Materiales
                    .FirstOrDefaultAsync(m => m.IdMaterial == id && !m.EsEliminado);

                if (material == null)
                {
                    return Json(new { success = false, message = "Material no encontrado" });
                }

                // Verificar permisos
                var usuarioId = User.GetUserId();
                if (!User.EsAdministrador())
                {
                    var esProfesorDelCurso = await _context.Cursos
                        .AnyAsync(c => c.Id == material.IdCurso && c.ProfesorAsignado == usuarioId);

                    if (!esProfesorDelCurso)
                    {
                        return Json(new { success = false, message = "No tiene permisos" });
                    }
                }

                // Soft delete
                material.EsEliminado = true;
                material.FechaEliminacion = DateTime.Now;
                material.EliminadoPorId = usuarioId;

                await _context.SaveChangesAsync();

                _logger.LogInformation("✅ Material {IdMaterial} eliminado por usuario {UsuarioId}",
                    id, usuarioId);

                return Json(new { success = true, message = "Material eliminado exitosamente" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error al eliminar material {IdMaterial}", id);
                return Json(new { success = false, message = "Error al eliminar el material" });
            }
        }

        // ============================================
        // TOGGLE VISIBILIDAD
        // ============================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleVisibilidad(int id)
        {
            try
            {
                var material = await _context.Materiales
                    .FirstOrDefaultAsync(m => m.IdMaterial == id && !m.EsEliminado);

                if (material == null)
                {
                    return Json(new { success = false, message = "Material no encontrado" });
                }

                // Verificar permisos
                var usuarioId = User.GetUserId();
                if (!User.EsAdministrador())
                {
                    var esProfesorDelCurso = await _context.Cursos
                        .AnyAsync(c => c.Id == material.IdCurso && c.ProfesorAsignado == usuarioId);

                    if (!esProfesorDelCurso)
                    {
                        return Json(new { success = false, message = "No tiene permisos" });
                    }
                }

                material.EsVisible = !material.EsVisible;
                await _context.SaveChangesAsync();

                _logger.LogInformation("✅ Visibilidad de material {IdMaterial} cambiada a {Visible}",
                    id, material.EsVisible);

                return Json(new
                {
                    success = true,
                    esVisible = material.EsVisible,
                    message = material.EsVisible ? "Material visible para estudiantes" : "Material oculto"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error al cambiar visibilidad del material {IdMaterial}", id);
                return Json(new { success = false, message = "Error al cambiar visibilidad" });
            }
        }

        // ============================================
        // DESCARGAR - Descarga con Estadísticas
        // ============================================
        [HttpGet]
        public async Task<IActionResult> Descargar(int id)
        {
            try
            {
                var material = await _context.Materiales
                    .FirstOrDefaultAsync(m => m.IdMaterial == id && !m.EsEliminado);

                if (material == null || string.IsNullOrEmpty(material.RutaArchivo))
                {
                    TempData["Error"] = "Archivo no encontrado";
                    return RedirectToAction("Index", new { idCurso = material?.IdCurso });
                }

                // Verificar que el archivo existe físicamente
                if (!_fileStorageService.FileExists(material.RutaArchivo))
                {
                    _logger.LogWarning("⚠️ Archivo físico no encontrado: {RutaArchivo}", material.RutaArchivo);
                    TempData["Error"] = "El archivo no está disponible";
                    return RedirectToAction("Index", new { idCurso = material.IdCurso });
                }

                // Actualizar estadísticas
                material.NumeroDescargas++;
                material.FechaUltimaDescarga = DateTime.Now;
                await _context.SaveChangesAsync();

                // Obtener stream del archivo
                var fileStream = await _fileStorageService.GetFileStreamAsync(material.RutaArchivo);

                if (fileStream == null)
                {
                    TempData["Error"] = "Error al leer el archivo";
                    return RedirectToAction("Index", new { idCurso = material.IdCurso });
                }

                var contentType = GetContentType(material.Extension ?? "");
                var fileName = material.NombreArchivo ?? "archivo" + material.Extension;

                _logger.LogInformation("✅ Material {IdMaterial} descargado por usuario {UsuarioId}",
                    id, User.GetUserId());

                return File(fileStream, contentType, fileName);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error al descargar material {IdMaterial}", id);
                TempData["Error"] = "Error al descargar el archivo";
                return RedirectToAction("Index", "AdminCursos");
            }
        }

        // ============================================
        // HELPERS PRIVADOS
        // ============================================

        /// <summary>
        /// Obtiene el siguiente número de orden para un material
        /// </summary>
        private async Task<int> ObtenerSiguienteOrden(int idCurso, int? idModuloCurso)
        {
            var query = _context.Materiales.Where(m => m.IdCurso == idCurso && !m.EsEliminado);

            if (idModuloCurso.HasValue)
            {
                query = query.Where(m => m.IdModuloCurso == idModuloCurso.Value);
            }
            else
            {
                query = query.Where(m => m.IdModuloCurso == null);
            }

            var maxOrden = await query.MaxAsync(m => (int?)m.Orden);
            return (maxOrden ?? 0) + 1;
        }

        /// <summary>
        /// Determina el tipo de material basado en la extensión
        /// </summary>
        private TipoMaterial DeterminarTipoMaterial(string extension)
        {
            return extension.ToLower() switch
            {
                ".pdf" => TipoMaterial.PDF,
                ".doc" or ".docx" => TipoMaterial.Documento,
                ".xls" or ".xlsx" => TipoMaterial.Documento,
                ".ppt" or ".pptx" => TipoMaterial.Presentacion,
                ".jpg" or ".jpeg" or ".png" or ".gif" => TipoMaterial.Imagen,
                ".mp4" or ".avi" => TipoMaterial.Video,
                _ => TipoMaterial.Otro
            };
        }

        /// <summary>
        /// Obtiene el content type para descargas
        /// </summary>
        private string GetContentType(string extension)
        {
            return extension.ToLower() switch
            {
                ".pdf" => "application/pdf",
                ".doc" => "application/msword",
                ".docx" => "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
                ".xls" => "application/vnd.ms-excel",
                ".xlsx" => "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                ".ppt" => "application/vnd.ms-powerpoint",
                ".pptx" => "application/vnd.openxmlformats-officedocument.presentationml.presentation",
                ".jpg" or ".jpeg" => "image/jpeg",
                ".png" => "image/png",
                ".gif" => "image/gif",
                ".mp4" => "video/mp4",
                ".avi" => "video/x-msvideo",
                ".zip" => "application/zip",
                ".rar" => "application/x-rar-compressed",
                _ => "application/octet-stream"
            };
        }
    }
}
