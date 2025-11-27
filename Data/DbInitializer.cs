using Campus_Virtul_GRLL.Models;

namespace Campus_Virtul_GRLL.Data
{
    /// <summary>
    /// Clase para inicializar datos de prueba en la base de datos
    /// </summary>
    public static class DbInitializer
    {
        /// <summary>
        /// Inicializa la base de datos con datos de prueba si no existen
        /// </summary>
        public static void Initialize(AppDBContext context)
        {
            // Asegurar que la BD existe
            context.Database.EnsureCreated();

            // ============================================
            // 1. VERIFICAR SI YA HAY DATOS
            // ============================================
            if (context.Usuarios.Any())
            {
                return; // Ya hay datos, no hacer nada
            }

            // ============================================
            // 2. CREAR ROLES (si no existen)
            // ============================================
            if (!context.Rols.Any())
            {
                var roles = new Rol[]
                {
                    new Rol
                    {
                        NombreRol = "Administrador",
                        Descripcion = "Acceso total al sistema",
                        Estado = true
                    },
                    new Rol
                    {
                        NombreRol = "Profesor",
                        Descripcion = "Gestión de cursos y contenidos",
                        Estado = true
                    },
                    new Rol
                    {
                        NombreRol = "Colaborador",
                        Descripcion = "Colaborador del sistema",
                        Estado = true
                    },
                    new Rol
                    {
                        NombreRol = "Practicante",
                        Descripcion = "Acceso limitado para practicantes",
                        Estado = true
                    }
                };

                context.Rols.AddRange(roles);
                context.SaveChanges();
            }

            // Obtener IDs de roles
            var rolAdmin = context.Rols.First(r => r.NombreRol == "Administrador");
            var rolProfesor = context.Rols.First(r => r.NombreRol == "Profesor");
            var rolColaborador = context.Rols.First(r => r.NombreRol == "Colaborador");
            var rolPracticante = context.Rols.First(r => r.NombreRol == "Practicante");

            // ============================================
            // 3. CREAR USUARIOS DE PRUEBA
            // ============================================
            var usuarios = new Usuario[]
            {
                // ADMINISTRADORES (2)
                new Usuario
                {
                    Nombres = "Carlos",
                    Apellidos = "Rodríguez Pérez",
                    DNI = "12345678",
                    CorreoElectronico = "admin@campusvirtual.com",
                    Telefono = "987654321",
                    Area = "Administración",
                    IdRol = rolAdmin.IdRol,
                    ClaveTemporal = "admin123",
                    ClavePermanente = "admin123",
                    PrimerInicio = false,
                    Estado = true,
                    FechaCreacion = DateOnly.FromDateTime(DateTime.Now.AddMonths(-6)),
                    FechaActualizacion = DateOnly.FromDateTime(DateTime.Now),
                    EsEliminado = false
                },
                new Usuario
                {
                    Nombres = "María",
                    Apellidos = "González Sánchez",
                    DNI = "23456789",
                    CorreoElectronico = "maria.gonzalez@campusvirtual.com",
                    Telefono = "987654322",
                    Area = "Tecnología",
                    IdRol = rolAdmin.IdRol,
                    ClaveTemporal = "admin123",
                    ClavePermanente = "admin123",
                    PrimerInicio = false,
                    Estado = true,
                    FechaCreacion = DateOnly.FromDateTime(DateTime.Now.AddMonths(-5)),
                    FechaActualizacion = DateOnly.FromDateTime(DateTime.Now),
                    EsEliminado = false
                },

                // PROFESORES (5)
                new Usuario
                {
                    Nombres = "Juan",
                    Apellidos = "Martínez López",
                    DNI = "34567890",
                    CorreoElectronico = "juan.martinez@campusvirtual.com",
                    Telefono = "987654323",
                    Area = "Capacitación",
                    IdRol = rolProfesor.IdRol,
                    ClaveTemporal = "profesor123",
                    ClavePermanente = "profesor123",
                    PrimerInicio = false,
                    Estado = true,
                    FechaCreacion = DateOnly.FromDateTime(DateTime.Now.AddMonths(-4)),
                    FechaActualizacion = DateOnly.FromDateTime(DateTime.Now),
                    EsEliminado = false
                },
                new Usuario
                {
                    Nombres = "Ana",
                    Apellidos = "Fernández Torres",
                    DNI = "45678901",
                    CorreoElectronico = "ana.fernandez@campusvirtual.com",
                    Telefono = "987654324",
                    Area = "Desarrollo Humano",
                    IdRol = rolProfesor.IdRol,
                    ClaveTemporal = "profesor123",
                    ClavePermanente = "profesor123",
                    PrimerInicio = false,
                    Estado = true,
                    FechaCreacion = DateOnly.FromDateTime(DateTime.Now.AddMonths(-3)),
                    FechaActualizacion = DateOnly.FromDateTime(DateTime.Now),
                    EsEliminado = false
                },
                new Usuario
                {
                    Nombres = "Pedro",
                    Apellidos = "Ramírez Castro",
                    DNI = "56789012",
                    CorreoElectronico = "pedro.ramirez@campusvirtual.com",
                    Telefono = "987654325",
                    Area = "Tecnología",
                    IdRol = rolProfesor.IdRol,
                    ClaveTemporal = "profesor123",
                    ClavePermanente = "profesor123",
                    PrimerInicio = false,
                    Estado = true,
                    FechaCreacion = DateOnly.FromDateTime(DateTime.Now.AddMonths(-3)),
                    FechaActualizacion = DateOnly.FromDateTime(DateTime.Now),
                    EsEliminado = false
                },
                new Usuario
                {
                    Nombres = "Laura",
                    Apellidos = "Díaz Morales",
                    DNI = "67890123",
                    CorreoElectronico = "laura.diaz@campusvirtual.com",
                    Telefono = "987654326",
                    Area = "Finanzas",
                    IdRol = rolProfesor.IdRol,
                    ClaveTemporal = "profesor123",
                    ClavePermanente = "profesor123",
                    PrimerInicio = false,
                    Estado = false, // Profesor inactivo
                    FechaCreacion = DateOnly.FromDateTime(DateTime.Now.AddMonths(-2)),
                    FechaActualizacion = DateOnly.FromDateTime(DateTime.Now),
                    EsEliminado = false
                },
                new Usuario
                {
                    Nombres = "Roberto",
                    Apellidos = "Vega Núñez",
                    DNI = "78901234",
                    CorreoElectronico = "roberto.vega@campusvirtual.com",
                    Telefono = "987654327",
                    Area = "Legal y Normativa",
                    IdRol = rolProfesor.IdRol,
                    ClaveTemporal = "temporal123",
                    ClavePermanente = "",
                    PrimerInicio = true, // Pendiente primer inicio
                    Estado = true,
                    FechaCreacion = DateOnly.FromDateTime(DateTime.Now.AddDays(-15)),
                    FechaActualizacion = DateOnly.FromDateTime(DateTime.Now.AddDays(-15)),
                    EsEliminado = false
                },

                // COLABORADORES (4)
                new Usuario
                {
                    Nombres = "Isabel",
                    Apellidos = "Ruiz Herrera",
                    DNI = "89012345",
                    CorreoElectronico = "isabel.ruiz@campusvirtual.com",
                    Telefono = "987654328",
                    Area = "Recursos Humanos",
                    IdRol = rolColaborador.IdRol,
                    ClaveTemporal = "colaborador123",
                    ClavePermanente = "colaborador123",
                    PrimerInicio = false,
                    Estado = true,
                    FechaCreacion = DateOnly.FromDateTime(DateTime.Now.AddMonths(-2)),
                    FechaActualizacion = DateOnly.FromDateTime(DateTime.Now),
                    EsEliminado = false
                },
                new Usuario
                {
                    Nombres = "Miguel",
                    Apellidos = "Castro Jiménez",
                    DNI = "90123456",
                    CorreoElectronico = "miguel.castro@campusvirtual.com",
                    Telefono = "987654329",
                    Area = "Infraestructura",
                    IdRol = rolColaborador.IdRol,
                    ClaveTemporal = "colaborador123",
                    ClavePermanente = "colaborador123",
                    PrimerInicio = false,
                    Estado = true,
                    FechaCreacion = DateOnly.FromDateTime(DateTime.Now.AddMonths(-1)),
                    FechaActualizacion = DateOnly.FromDateTime(DateTime.Now),
                    EsEliminado = false
                },
                new Usuario
                {
                    Nombres = "Carmen",
                    Apellidos = "Ortiz Mendoza",
                    DNI = "01234567",
                    CorreoElectronico = "carmen.ortiz@campusvirtual.com",
                    Telefono = "987654330",
                    Area = "Medio Ambiente",
                    IdRol = rolColaborador.IdRol,
                    ClaveTemporal = "colaborador123",
                    ClavePermanente = "colaborador123",
                    PrimerInicio = false,
                    Estado = true,
                    FechaCreacion = DateOnly.FromDateTime(DateTime.Now.AddMonths(-1)),
                    FechaActualizacion = DateOnly.FromDateTime(DateTime.Now),
                    EsEliminado = false
                },
                new Usuario
                {
                    Nombres = "Fernando",
                    Apellidos = "Silva Paredes",
                    DNI = "11223344",
                    CorreoElectronico = "fernando.silva@campusvirtual.com",
                    Telefono = "987654331",
                    Area = "Planificación",
                    IdRol = rolColaborador.IdRol,
                    ClaveTemporal = "colaborador123",
                    ClavePermanente = "colaborador123",
                    PrimerInicio = false,
                    Estado = false, // Colaborador inactivo
                    FechaCreacion = DateOnly.FromDateTime(DateTime.Now.AddDays(-45)),
                    FechaActualizacion = DateOnly.FromDateTime(DateTime.Now),
                    EsEliminado = false
                },

                // PRACTICANTES (3)
                new Usuario
                {
                    Nombres = "Sofía",
                    Apellidos = "Vargas Rojas",
                    DNI = "22334455",
                    CorreoElectronico = "sofia.vargas@campusvirtual.com",
                    Telefono = "987654332",
                    Area = "Tecnología",
                    IdRol = rolPracticante.IdRol,
                    ClaveTemporal = "temporal123",
                    ClavePermanente = "",
                    PrimerInicio = true, // Pendiente primer inicio
                    Estado = true,
                    FechaCreacion = DateOnly.FromDateTime(DateTime.Now.AddDays(-10)),
                    FechaActualizacion = DateOnly.FromDateTime(DateTime.Now.AddDays(-10)),
                    EsEliminado = false
                },
                new Usuario
                {
                    Nombres = "Diego",
                    Apellidos = "Campos Ríos",
                    DNI = "33445566",
                    CorreoElectronico = "diego.campos@campusvirtual.com",
                    Telefono = "987654333",
                    Area = "Capacitación",
                    IdRol = rolPracticante.IdRol,
                    ClaveTemporal = "practicante123",
                    ClavePermanente = "practicante123",
                    PrimerInicio = false,
                    Estado = true,
                    FechaCreacion = DateOnly.FromDateTime(DateTime.Now.AddDays(-30)),
                    FechaActualizacion = DateOnly.FromDateTime(DateTime.Now),
                    EsEliminado = false
                },
                new Usuario
                {
                    Nombres = "Valentina",
                    Apellidos = "Flores Medina",
                    DNI = "44556677",
                    CorreoElectronico = "valentina.flores@campusvirtual.com",
                    Telefono = "987654334",
                    Area = "Recursos Humanos",
                    IdRol = rolPracticante.IdRol,
                    ClaveTemporal = "practicante123",
                    ClavePermanente = "practicante123",
                    PrimerInicio = false,
                    Estado = true,
                    FechaCreacion = DateOnly.FromDateTime(DateTime.Now.AddDays(-20)),
                    FechaActualizacion = DateOnly.FromDateTime(DateTime.Now),
                    EsEliminado = false
                }
            };

            context.Usuarios.AddRange(usuarios);
            context.SaveChanges();
        }
    }
}
