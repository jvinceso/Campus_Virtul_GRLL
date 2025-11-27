-- ============================================
-- SCRIPT DE DATOS INICIALES PARA CAMPUS VIRTUAL GRLL
-- Incluye: Roles, Módulos, Permisos y Usuarios de Prueba
-- ============================================

USE Campu_Virtul_GRLLDB;
GO

-- ============================================
-- 1. LIMPIAR DATOS EXISTENTES (OPCIONAL - COMENTAR SI NO SE QUIERE LIMPIAR)
-- ============================================
/*
DELETE FROM Permisos;
DELETE FROM Usuario;
DELETE FROM Curso;
DELETE FROM Modulo;
DELETE FROM Rol;
DBCC CHECKIDENT ('Rol', RESEED, 0);
DBCC CHECKIDENT ('Modulo', RESEED, 0);
DBCC CHECKIDENT ('Permisos', RESEED, 0);
DBCC CHECKIDENT ('Usuario', RESEED, 0);
DBCC CHECKIDENT ('Curso', RESEED, 0);
*/

-- ============================================
-- 2. INSERTAR ROLES
-- ============================================
SET IDENTITY_INSERT Rol ON;

INSERT INTO Rol (IdRol, NombreRol, Descripcion, Estado)
VALUES
    (1, 'Administrador', 'Administrador del sistema con acceso total', 1),
    (2, 'Profesor', 'Profesor que imparte cursos', 1),
    (3, 'Colaborador', 'Colaborador de la institución', 1),
    (4, 'Practicante', 'Practicante en formación', 1);

SET IDENTITY_INSERT Rol OFF;
GO

-- ============================================
-- 3. INSERTAR MÓDULOS DEL SISTEMA
-- ============================================
SET IDENTITY_INSERT Modulo ON;

INSERT INTO Modulo (IdModulo, Titulo, Descripcion, Estado)
VALUES
    (1, 'Gestión de Usuarios', 'Administración de usuarios del sistema', 1),
    (2, 'Gestión de Cursos', 'Administración del catálogo de cursos', 1),
    (3, 'Gestión de Solicitudes', 'Administración de solicitudes de acceso', 1),
    (4, 'Gestión de Roles', 'Administración de roles y permisos', 1),
    (5, 'Asignación Profesor-Curso', 'Asignación de profesores a cursos', 1),
    (6, 'Gestión de Materiales', 'Administración de materiales de cursos', 1),
    (7, 'Reportes', 'Generación de reportes del sistema', 1);

SET IDENTITY_INSERT Modulo OFF;
GO

-- ============================================
-- 4. INSERTAR PERMISOS
-- ============================================
SET IDENTITY_INSERT Permisos ON;

-- Permisos para Administrador (acceso completo a todo)
INSERT INTO Permisos (IdPermisos, IdRol, IdModulo, Crear, Editar, Revisar, Aprobar, Visualizar)
VALUES
    (1, 1, 1, 1, 1, 1, 1, 1),  -- Gestión de Usuarios
    (2, 1, 2, 1, 1, 1, 1, 1),  -- Gestión de Cursos
    (3, 1, 3, 1, 1, 1, 1, 1),  -- Gestión de Solicitudes
    (4, 1, 4, 1, 1, 1, 1, 1),  -- Gestión de Roles
    (5, 1, 5, 1, 1, 1, 1, 1),  -- Asignación Profesor-Curso
    (6, 1, 6, 1, 1, 1, 1, 1),  -- Gestión de Materiales
    (7, 1, 7, 1, 1, 1, 1, 1);  -- Reportes

-- Permisos para Profesor
INSERT INTO Permisos (IdPermisos, IdRol, IdModulo, Crear, Editar, Revisar, Aprobar, Visualizar)
VALUES
    (8, 2, 2, 0, 1, 0, 0, 1),  -- Ver y editar cursos asignados
    (9, 2, 6, 1, 1, 0, 0, 1);  -- Gestionar materiales de sus cursos

-- Permisos para Colaborador
INSERT INTO Permisos (IdPermisos, IdRol, IdModulo, Crear, Editar, Revisar, Aprobar, Visualizar)
VALUES
    (10, 3, 2, 0, 0, 0, 0, 1), -- Solo visualizar cursos
    (11, 3, 6, 0, 0, 0, 0, 1); -- Solo visualizar materiales

-- Permisos para Practicante
INSERT INTO Permisos (IdPermisos, IdRol, IdModulo, Crear, Editar, Revisar, Aprobar, Visualizar)
VALUES
    (12, 4, 2, 0, 0, 0, 0, 1); -- Solo visualizar cursos

SET IDENTITY_INSERT Permisos OFF;
GO

-- ============================================
-- 5. INSERTAR USUARIOS DE PRUEBA
-- ============================================
SET IDENTITY_INSERT Usuario ON;

-- Usuario 1: Administrador (para pruebas del CRUD de Cursos)
INSERT INTO Usuario (IdUsuario, IdRol, Nombres, Apellidos, DNI, Telefono, CorreoElectronico, Area,
                     PrimerInicio, ClaveTemporal, ClavePermanente, FechaCreacion, FechaActualizacion, Estado)
VALUES
(1, 1, 'Juan Carlos', 'García López', '12345678', '987654321', 'admin@grll.gob.pe',
 'Administración', 0, 'temp123', 'admin123', CAST(GETDATE() AS DATE), CAST(GETDATE() AS DATE), 1);

-- Usuario 2: Administrador con primer inicio (para probar cambio de contraseña)
INSERT INTO Usuario (IdUsuario, IdRol, Nombres, Apellidos, DNI, Telefono, CorreoElectronico, Area,
                     PrimerInicio, ClaveTemporal, ClavePermanente, FechaCreacion, FechaActualizacion, Estado)
VALUES
(2, 1, 'María Elena', 'Rodríguez Pérez', '87654321', '987654322', 'admin2@grll.gob.pe',
 'Tecnología', 1, 'temp456', 'admin456', CAST(GETDATE() AS DATE), CAST(GETDATE() AS DATE), 1);

-- Usuario 3: Profesor (Pedro Martínez - le asignaremos 3 cursos: Excel, Proyectos, Medio Ambiente)
INSERT INTO Usuario (IdUsuario, IdRol, Nombres, Apellidos, DNI, Telefono, CorreoElectronico, Area,
                     PrimerInicio, ClaveTemporal, ClavePermanente, FechaCreacion, FechaActualizacion, Estado)
VALUES
(3, 2, 'Pedro José', 'Martínez Silva', '11223344', '987654323', 'profesor1@grll.gob.pe',
 'Capacitación', 0, 'temp789', 'prof123', CAST(GETDATE() AS DATE), CAST(GETDATE() AS DATE), 1);

-- Usuario 4: Profesor (Ana Torres - le asignaremos 3 cursos: Liderazgo, Comunicación, Infraestructura)
INSERT INTO Usuario (IdUsuario, IdRol, Nombres, Apellidos, DNI, Telefono, CorreoElectronico, Area,
                     PrimerInicio, ClaveTemporal, ClavePermanente, FechaCreacion, FechaActualizacion, Estado)
VALUES
(4, 2, 'Ana María', 'Torres Vega', '44332211', '987654324', 'profesor2@grll.gob.pe',
 'Desarrollo Humano', 0, 'temp101', 'prof456', CAST(GETDATE() AS DATE), CAST(GETDATE() AS DATE), 1);

-- Usuario 5: Colaborador
INSERT INTO Usuario (IdUsuario, IdRol, Nombres, Apellidos, DNI, Telefono, CorreoElectronico, Area,
                     PrimerInicio, ClaveTemporal, ClavePermanente, FechaCreacion, FechaActualizacion, Estado)
VALUES
(5, 3, 'Luis Alberto', 'Fernández Cruz', '55667788', '987654325', 'colaborador1@grll.gob.pe',
 'Finanzas', 0, 'temp202', 'colab123', CAST(GETDATE() AS DATE), CAST(GETDATE() AS DATE), 1);

-- Usuario 6: Practicante
INSERT INTO Usuario (IdUsuario, IdRol, Nombres, Apellidos, DNI, Telefono, CorreoElectronico, Area,
                     PrimerInicio, ClaveTemporal, ClavePermanente, FechaCreacion, FechaActualizacion, Estado)
VALUES
(6, 4, 'Carmen Rosa', 'Sánchez Díaz', '99887766', '987654326', 'practicante1@grll.gob.pe',
 'Recursos Humanos', 0, 'temp303', 'pract123', CAST(GETDATE() AS DATE), CAST(GETDATE() AS DATE), 1);

SET IDENTITY_INSERT Usuario OFF;
GO

-- ============================================
-- 6. INSERTAR CURSOS DE PRUEBA (OPCIONAL)
-- ============================================
SET IDENTITY_INSERT Curso ON;

INSERT INTO Curso (Id, Codigo, Titulo, Descripcion, Objetivos, Duracion, Modalidad, Categoria, Nivel,
                   CapacidadMaxima, Estado, FechaInicio, FechaFin, FechaPublicacion, ImagenUrl,
                   ProfesorAsignado, CreadoPor, FechaCreacion, FechaUltimaActualizacion, EsEliminado, FechaEliminacion)
VALUES
-- Curso 1: Borrador
(1, 'CURSO-2024-001', 'Introducción a la Gestión Pública',
 'Curso diseñado para proporcionar los fundamentos básicos de la gestión pública moderna, incluyendo principios de administración, normativa básica y buenas prácticas en el sector público.',
 'Comprender los fundamentos de la gestión pública. Aplicar principios de administración en el sector público. Conocer la normativa básica del Estado.',
 40, 'Presencial', 'Gestión y Administración Pública', 'Basico', 30, 'Borrador',
 NULL, NULL, NULL, 'https://placehold.co/400x250/007bff/ffffff?text=Gestion+Publica',
 NULL, 1, GETDATE(), NULL, 0, NULL),

-- Curso 2: Publicado (Profesor: Pedro Martínez - ID 3)
(2, 'CURSO-2024-002', 'Excel Avanzado para el Sector Público',
 'Curso práctico de Excel avanzado orientado a las necesidades específicas del sector público, incluyendo análisis de datos, tablas dinámicas, macros y automatización de reportes.',
 'Dominar funciones avanzadas de Excel. Crear tablas dinámicas complejas. Automatizar reportes con macros. Analizar datos del sector público.',
 32, 'Virtual', 'Tecnologías de la Información', 'Intermedio', 25, 'Publicado',
 '2024-12-15', '2025-01-30', GETDATE(), 'https://placehold.co/400x250/28a745/ffffff?text=Excel+Avanzado',
 3, 1, GETDATE(), NULL, 0, NULL),

-- Curso 3: En Curso (Profesor: Ana Torres - ID 4)
(3, 'CURSO-2024-003', 'Liderazgo Transformacional',
 'Programa integral de desarrollo de habilidades de liderazgo para funcionarios públicos, enfocado en el liderazgo transformacional, gestión de equipos y motivación.',
 'Desarrollar habilidades de liderazgo efectivo. Gestionar equipos de alto rendimiento. Aplicar técnicas de motivación. Liderar procesos de cambio.',
 48, 'Hibrido', 'Desarrollo Personal y Liderazgo', 'Avanzado', 20, 'EnCurso',
 '2024-11-01', '2024-12-20', '2024-10-25', 'https://placehold.co/400x250/ffc107/000000?text=Liderazgo',
 4, 1, GETDATE(), GETDATE(), 0, NULL),

-- Curso 4: Publicado (Profesor: Pedro Martínez - ID 3)
(4, 'CURSO-2024-004', 'Gestión de Proyectos con Metodologías Ágiles',
 'Curso completo sobre gestión de proyectos utilizando metodologías ágiles (Scrum, Kanban), adaptadas al contexto del sector público.',
 'Implementar Scrum en proyectos públicos. Utilizar Kanban para gestión visual. Aplicar principios ágiles en el Estado. Mejorar la entrega de valor.',
 60, 'Virtual', 'Gestión de Proyectos', 'Intermedio', 35, 'Publicado',
 '2025-01-10', '2025-03-15', GETDATE(), 'https://placehold.co/400x250/17a2b8/ffffff?text=Proyectos+Agiles',
 3, 1, GETDATE(), NULL, 0, NULL),

-- Curso 5: Finalizado (Profesor: Ana Torres - ID 4)
(5, 'CURSO-2024-005', 'Comunicación Efectiva y Atención al Ciudadano',
 'Programa de capacitación en técnicas de comunicación efectiva y protocolo de atención al ciudadano para mejorar la calidad del servicio público.',
 'Mejorar habilidades de comunicación interpersonal. Aplicar protocolos de atención al ciudadano. Resolver conflictos efectivamente. Brindar servicio de calidad.',
 24, 'Presencial', 'Comunicación y Atención al Ciudadano', 'Basico', 40, 'Finalizado',
 '2024-09-01', '2024-10-15', '2024-08-25', 'https://placehold.co/400x250/dc3545/ffffff?text=Comunicacion',
 4, 1, GETDATE(), GETDATE(), 0, NULL),

-- Curso 6: Publicado
(6, 'CURSO-2024-006', 'Gestión Financiera y Presupuesto Público',
 'Curso especializado en gestión financiera del sector público, incluyendo elaboración de presupuestos, ejecución presupuestal y control financiero.',
 'Elaborar presupuestos públicos. Ejecutar presupuesto según normativa. Realizar control financiero efectivo. Generar reportes de ejecución.',
 56, 'Hibrido', 'Finanzas y Presupuesto', 'Avanzado', 25, 'Publicado',
 '2025-02-01', '2025-04-15', GETDATE(), 'https://placehold.co/400x250/6f42c1/ffffff?text=Finanzas',
 NULL, 1, GETDATE(), NULL, 0, NULL),

-- Curso 7: Borrador
(7, 'CURSO-2024-007', 'Recursos Humanos: Gestión del Talento',
 'Programa de gestión del talento humano en el sector público, incluyendo reclutamiento, selección, desarrollo y retención de personal.',
 'Aplicar técnicas de reclutamiento efectivo. Desarrollar planes de carrera. Implementar programas de capacitación. Retener talento clave.',
 45, 'Virtual', 'Recursos Humanos', 'Intermedio', 30, 'Borrador',
 NULL, NULL, NULL, 'https://placehold.co/400x250/fd7e14/ffffff?text=RRHH',
 NULL, 1, GETDATE(), NULL, 0, NULL),

-- Curso 8: Cancelado
(8, 'CURSO-2024-008', 'Normativa Legal del Sector Público',
 'Curso de normativa legal básica del sector público, incluyendo leyes fundamentales, procedimientos administrativos y jurisprudencia relevante.',
 'Conocer la normativa básica del Estado. Aplicar procedimientos administrativos correctamente. Interpretar jurisprudencia relevante.',
 36, 'Presencial', 'Legal y Normativa', 'Basico', 28, 'Cancelado',
 '2024-10-01', '2024-11-15', '2024-09-20', NULL,
 NULL, 1, GETDATE(), GETDATE(), 0, NULL),

-- Curso 9: Publicado (Profesor: Pedro Martínez - ID 3)
(9, 'CURSO-2024-009', 'Gestión Ambiental y Desarrollo Sostenible',
 'Curso sobre gestión ambiental en el sector público, incluyendo evaluación de impacto ambiental, políticas ambientales y desarrollo sostenible.',
 'Evaluar impactos ambientales. Implementar políticas ambientales. Promover desarrollo sostenible. Cumplir normativa ambiental.',
 40, 'Hibrido', 'Medio Ambiente y Recursos Naturales', 'Intermedio', 22, 'Publicado',
 '2024-12-05', '2025-01-25', GETDATE(), 'https://placehold.co/400x250/20c997/ffffff?text=Medio+Ambiente',
 3, 1, GETDATE(), NULL, 0, NULL),

-- Curso 10: En Curso (Profesor: Ana Torres - ID 4)
(10, 'CURSO-2024-010', 'Gestión de Infraestructura y Obras Públicas',
 'Programa de gestión de proyectos de infraestructura pública, incluyendo planificación, ejecución, supervisión y entrega de obras.',
 'Planificar proyectos de infraestructura. Supervisar ejecución de obras. Asegurar calidad en construcción. Gestionar cronogramas y presupuestos.',
 64, 'Presencial', 'Infraestructura y Obras Públicas', 'Avanzado', 18, 'EnCurso',
 '2024-10-15', '2025-01-15', '2024-10-01', 'https://placehold.co/400x250/6c757d/ffffff?text=Infraestructura',
 4, 1, GETDATE(), GETDATE(), 0, NULL);

SET IDENTITY_INSERT Curso OFF;
GO

-- ============================================
-- 7. VERIFICAR DATOS INSERTADOS
-- ============================================
PRINT '============================================';
PRINT 'RESUMEN DE DATOS INSERTADOS';
PRINT '============================================';
PRINT '';

SELECT 'Roles' AS Tabla, COUNT(*) AS Total FROM Rol;
SELECT 'Módulos' AS Tabla, COUNT(*) AS Total FROM Modulo;
SELECT 'Permisos' AS Tabla, COUNT(*) AS Total FROM Permisos;
SELECT 'Usuarios' AS Tabla, COUNT(*) AS Total FROM Usuario;
SELECT 'Cursos' AS Tabla, COUNT(*) AS Total FROM Curso;

PRINT '';
PRINT '============================================';
PRINT 'USUARIOS DE PRUEBA CREADOS';
PRINT '============================================';
PRINT '';

SELECT
    u.DNI,
    u.Nombres + ' ' + u.Apellidos AS NombreCompleto,
    r.NombreRol AS Rol,
    u.CorreoElectronico AS Email,
    CASE WHEN u.PrimerInicio = 1 THEN u.ClaveTemporal ELSE u.ClavePermanente END AS Contraseña,
    CASE WHEN u.PrimerInicio = 1 THEN 'Sí (debe cambiar)' ELSE 'No' END AS PrimerInicio
FROM Usuario u
INNER JOIN Rol r ON u.IdRol = r.IdRol
ORDER BY u.IdUsuario;

PRINT '';
PRINT '============================================';
PRINT 'INSTRUCCIONES DE USO';
PRINT '============================================';
PRINT '';
PRINT '1. Para probar el CRUD de Cursos, use el usuario Administrador:';
PRINT '   DNI: 12345678';
PRINT '   Contraseña: admin123';
PRINT '';
PRINT '2. Acceda a: /AdminCursos/Index';
PRINT '';
PRINT '3. Otros usuarios disponibles:';
PRINT '   - Admin con primer inicio: DNI 87654321, Contraseña temp456';
PRINT '   - Profesor: DNI 11223344, Contraseña prof123';
PRINT '   - Colaborador: DNI 55667788, Contraseña colab123';
PRINT '';
PRINT '4. Se crearon 10 cursos de prueba con diferentes estados';
PRINT '';
PRINT '5. ASIGNACIÓN DE PROFESORES A CURSOS:';
PRINT '   - Pedro Martínez (ID 3): 3 cursos asignados (132 horas)';
PRINT '     * Excel Avanzado (32h) - Publicado';
PRINT '     * Proyectos Ágiles (60h) - Publicado';
PRINT '     * Medio Ambiente (40h) - Publicado';
PRINT '';
PRINT '   - Ana Torres (ID 4): 3 cursos asignados (136 horas)';
PRINT '     * Liderazgo (48h) - En Curso';
PRINT '     * Comunicación (24h) - Finalizado';
PRINT '     * Infraestructura (64h) - En Curso';
PRINT '';
PRINT '   Ambos profesores tienen carga alta (>120 horas)';
PRINT '';
PRINT '============================================';
GO

-- ============================================
-- 8. RESUMEN DE ASIGNACIONES PROFESOR-CURSO
-- ============================================
SELECT
    u.Nombres + ' ' + u.Apellidos AS Profesor,
    COUNT(c.Id) AS CursosAsignados,
    SUM(c.Duracion) AS HorasTotales,
    CASE
        WHEN SUM(c.Duracion) > 120 THEN 'SOBRECARGADO ⚠️'
        WHEN SUM(c.Duracion) > 80 THEN 'Carga alta'
        WHEN SUM(c.Duracion) > 60 THEN 'Carga moderada'
        ELSE 'Carga baja'
    END AS EstadoCarga
FROM Usuario u
INNER JOIN Rol r ON u.IdRol = r.IdRol
LEFT JOIN Curso c ON c.ProfesorAsignado = u.IdUsuario AND c.EsEliminado = 0
WHERE r.NombreRol = 'Profesor'
GROUP BY u.IdUsuario, u.Nombres, u.Apellidos
ORDER BY HorasTotales DESC;
GO
