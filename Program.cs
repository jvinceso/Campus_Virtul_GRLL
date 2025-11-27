using Campus_Virtul_GRLL.Data;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Builder;
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
// 3. AGREGAR CONTROLADORES Y VISTAS
// ============================================
builder.Services.AddControllersWithViews();

var app = builder.Build();

// ============================================
// 4. INICIALIZAR BASE DE DATOS CON DATOS SEED
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
// 5. CONFIGURAR PIPELINE HTTP
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
// 6. CONFIGURAR RUTAS
// ============================================
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Solicitud}/{action=Solicitud}/{id?}");

app.Run();
