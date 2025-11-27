using Campus_Virtul_GRLL.Data;
using Campus_Virtul_GRLL.Services;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// ============================================
// 1. CONFIGURAR BASE DE DATOS
// ============================================
builder.Services.AddDbContext<AppDBContext>(options =>
{
    options.UseSqlServer(builder.Configuration.GetConnectionString("ConexionSQL"));
});

// ============================================
// 2. CONFIGURAR AUTENTICACI�N CON COOKIES
// ============================================
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Login/Index"; // P�gina de login
        options.LogoutPath = "/Login/Logout"; // Cerrar sesi�n
        options.AccessDeniedPath = "/Login/AccesoDenegado"; // Sin permisos
        options.ExpireTimeSpan = TimeSpan.FromSeconds(1); // Cookie v�lida por 8 horas
        options.SlidingExpiration = true; // Renovar autom�ticamente
        options.Cookie.Name = "CampusVirtualAuth";
        options.Cookie.HttpOnly = true; // Protecci�n contra XSS
        options.Cookie.IsEssential = true; // Cookie esencial
    });

// ============================================
// 3. CONFIGURAR POL�TICAS DE AUTORIZACI�N
// ============================================
builder.Services.AddAuthorization(options =>
{
    // Pol�tica para Administradores
    options.AddPolicy("EsAdministrador", policy =>
        policy.RequireRole("Administrador"));

    // Pol�tica para Profesores
    options.AddPolicy("EsProfesor", policy =>
        policy.RequireRole("Profesor"));

    // Pol�tica para Colaboradores
    options.AddPolicy("EsColaborador", policy =>
        policy.RequireRole("Colaborador"));

    // Pol�tica para Practicantes
    options.AddPolicy("EsPracticante", policy =>
        policy.RequireRole("Practicante"));

    // Pol�tica para Admin o Profesor (gesti�n de cursos)
    options.AddPolicy("EsAdminOProfesor", policy =>
        policy.RequireRole("Administrador", "Profesor"));

    // Pol�tica para Colaborador o Practicante (estudiantes)
    options.AddPolicy("EsEstudiante", policy =>
        policy.RequireRole("Colaborador", "Practicante"));

    // Pol�tica para usuarios activos
    options.AddPolicy("UsuarioActivo", policy =>
        policy.RequireClaim("Estado", "True"));
});

// ============================================
// 4. CONFIGURAR SUBIDA DE ARCHIVOS
// ============================================
builder.Services.Configure<FormOptions>(options =>
{
    options.MultipartBodyLengthLimit = 52428800; // 50 MB
    options.ValueLengthLimit = int.MaxValue;
    options.MultipartHeadersLengthLimit = int.MaxValue;
});

// ============================================
// 5. REGISTRAR SERVICIOS
// ============================================
builder.Services.AddScoped<IFileStorageService, FileStorageService>();

// ============================================
// 6. AGREGAR CONTROLADORES Y VISTAS
// ============================================
builder.Services.AddControllersWithViews();

var app = builder.Build();

// ============================================
// 7. INICIALIZAR BASE DE DATOS CON DATOS SEED
// ============================================
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var context = services.GetRequiredService<AppDBContext>();
        DbInitializer.Initialize(context);
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "❌ Error al inicializar la base de datos con datos seed");
    }
}

// ============================================
// 8. CONFIGURAR PIPELINE HTTP
// ============================================
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

// ============================================
// 9. CONFIGURAR RUTAS
// ============================================
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Solicitud}/{action=Solicitud}/{id?}");

app.Run();
