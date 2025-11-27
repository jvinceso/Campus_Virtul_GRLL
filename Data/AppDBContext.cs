using Microsoft.EntityFrameworkCore;
using Campus_Virtul_GRLL.Models;

namespace Campus_Virtul_GRLL.Data
{
    public class AppDBContext: DbContext
    {
        public AppDBContext(DbContextOptions<AppDBContext> options) : base(options)
        {

        }

        public DbSet<Rol> Rols { get; set; }
        public DbSet<Permisos> Permisos { get; set; }
        public DbSet<Modulo> Modulos { get; set; }
        public DbSet <Solicitud> Solicituds { get; set; }
        public DbSet<SolicitudRevisión> SolicitudsRevision { get; set; }
        public DbSet<Usuario> Usuarios { get; set; }
        public DbSet<Curso> Cursos { get; set; }
        
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Rol>(tabla =>
            {
                tabla.HasKey(columna => columna.IdRol);

                tabla.Property(columna => columna.IdRol)
                .ValueGeneratedOnAdd();

                tabla.Property(columna => columna.NombreRol)
                .HasMaxLength(50)
                .IsRequired();

                tabla.Property(columna => columna.Descripcion)
                .HasMaxLength(100)
                .IsRequired();

                tabla.Property(columna => columna.Estado)
                .IsRequired();
            });

            modelBuilder.Entity<Permisos>(tabla =>
            {
                tabla.HasKey(columna => columna.IdPermisos);

                tabla.HasOne(columna => columna.Rol)
                .WithMany()
                .HasForeignKey(columna => columna.IdRol)
                .OnDelete(DeleteBehavior.Restrict);

                tabla.HasOne(columna => columna.Modulos)
                .WithMany()
                .HasForeignKey(columna => columna.IdModulo)
                .OnDelete(DeleteBehavior.Restrict);

                tabla.Property(columna => columna.Crear)
                .HasDefaultValue(false);

                tabla.Property(columna => columna.Editar)
                .HasDefaultValue(false);

                tabla.Property(columna => columna.Revisar)
                .HasDefaultValue(false);

                tabla.Property(columna => columna.Aprobar)
                .HasDefaultValue(false);

                tabla.Property(columna => columna.Visualizar)
                .HasDefaultValue(false);
            });

            modelBuilder.Entity<Modulo>(tabla =>
            {
                tabla.HasKey(columna => columna.IdModulo);

                tabla.Property(columna => columna.Titulo)
                .HasMaxLength(100)
                .IsRequired();

                tabla.Property(columna => columna.Descripcion)
                .HasMaxLength(100)
                .IsRequired();

                tabla.Property(columna => columna.Estado)
                .IsRequired();
            });

            modelBuilder.Entity<Solicitud>(tabla =>
            {
                tabla.HasKey(columna => columna.IdSolicitud);

                tabla.Property(columna => columna.IdSolicitud)
                .UseIdentityColumn()
                .ValueGeneratedOnAdd();

                tabla.Property(columna => columna.Nombres)
                .HasMaxLength(50)
                .IsRequired();

                tabla.Property(columna => columna.Apellidos)
                .HasMaxLength(50)
                .IsRequired();

                tabla.Property(columna => columna.DNI)
                .HasMaxLength(8)
                .IsRequired();

                tabla.Property(columna => columna.Telefono)
                .HasMaxLength(9)
                .IsRequired();

                tabla.Property(columna => columna.CorreoElectronico)
                .HasMaxLength(100)
                .IsRequired();

                tabla.Property(columna => columna.Area)
                .HasMaxLength(50)
                .IsRequired();

                tabla.Property(columna => columna.FechaSolicitud)
                .HasColumnType("date")
                .IsRequired();

                tabla.Property(columna => columna.Estado)
                .HasConversion<string>()
                .HasMaxLength(50)
                .IsRequired();

                tabla.HasOne(columna => columna.Rol)
                .WithMany()
                .HasForeignKey(columna => columna.IdRol)
                .OnDelete(DeleteBehavior.Restrict);

                // Configuración de campos de respuesta
                tabla.Property(columna => columna.FechaRespuesta)
                .IsRequired(false);

                tabla.Property(columna => columna.RespuestaDe)
                .IsRequired(false);

                tabla.Property(columna => columna.MotivoRechazo)
                .HasMaxLength(500)
                .IsRequired(false);

                tabla.Property(columna => columna.UsuarioCreado)
                .IsRequired(false);

                // Relaciones con Usuario
                tabla.HasOne(columna => columna.UsuarioRespondio)
                .WithMany()
                .HasForeignKey(columna => columna.RespuestaDe)
                .OnDelete(DeleteBehavior.Restrict);

                tabla.HasOne(columna => columna.UsuarioGenerado)
                .WithMany()
                .HasForeignKey(columna => columna.UsuarioCreado)
                .OnDelete(DeleteBehavior.Restrict);

                tabla.Ignore(columna => columna.NombreRol);
                tabla.Ignore(columna => columna.NombreCompleto);
            });

            modelBuilder.Entity<SolicitudRevisión>(tabla =>
            {
                tabla.HasKey(columna => columna.IdSolicitudRevision);

                tabla.Property(columna => columna.IdSolicitudRevision)
                .UseIdentityColumn()
                .ValueGeneratedOnAdd();

                tabla.HasOne(columna => columna.Solicitud)
                .WithMany()
                .HasForeignKey(columna => columna.IdSolicitud)
                .OnDelete(DeleteBehavior.Restrict);

                tabla.Property(columna => columna.fechaRevision)
                .HasColumnType("date")
                .IsRequired();

                tabla.Property(columna => columna.observaciones)
                .HasMaxLength(50)
                .IsRequired();

                tabla.Property(columna => columna.Estado)
                .HasConversion<string>()
                .HasMaxLength(50)
                .IsRequired();
            });

            modelBuilder.Entity<Usuario>(tabla =>
            {
                tabla.HasKey(columna => columna.IdUsuario);

                tabla.Property(columna => columna.IdUsuario)
                .UseIdentityColumn()
                .ValueGeneratedOnAdd();

                tabla.HasOne(columna => columna.Rol)
                .WithMany()
                .HasForeignKey(columna => columna.IdRol)
                .OnDelete(DeleteBehavior.Restrict);

                // Configuración de Soft Delete
                tabla.HasOne(columna => columna.UsuarioEliminador)
                .WithMany()
                .HasForeignKey(columna => columna.EliminadoPor)
                .OnDelete(DeleteBehavior.Restrict);

                tabla.Property(columna => columna.EsEliminado)
                .HasDefaultValue(false);

                tabla.Property(columna => columna.FechaEliminacion)
                .IsRequired(false);

                tabla.Property(columna => columna.EliminadoPor)
                .IsRequired(false);

                tabla.Property(columna => columna.Nombres)
                .HasMaxLength(50)
                .IsRequired();

                tabla.Property(columna => columna.Apellidos)
                .HasMaxLength(50)
                .IsRequired();

                tabla.Property(columna => columna.DNI)
                .HasMaxLength(8)
                .IsRequired();

                tabla.Property(columna => columna.Telefono)
                .HasMaxLength(15)
                .IsRequired();

                tabla.Property(columna => columna.CorreoElectronico)
                .HasMaxLength(100)
                .IsRequired();

                tabla.Property(columna => columna.Area)
                .HasMaxLength(50)
                .IsRequired();

                tabla.Property(columna => columna.PrimerInicio)
                .HasDefaultValue(false);

                tabla.Property(columna => columna.ClaveTemporal)
                .HasMaxLength(100)
                .IsRequired();

                tabla.Property(columna => columna.ClavePermanente)
                .HasMaxLength(100)
                .IsRequired();

                tabla.Property(columna => columna.FechaCreacion)
                .HasColumnType("date")
                .IsRequired();

                tabla.Property(columna => columna.FechaActualizacion)
                .HasColumnType("date")
                .IsRequired();

                tabla.Property(columna => columna.Estado)
                .IsRequired();
            });

            // ============================================
            // CONFIGURACIÓN DE CURSO
            // ============================================
            modelBuilder.Entity<Curso>(tabla =>
            {
                tabla.HasKey(columna => columna.Id);

                tabla.Property(columna => columna.Id)
                .UseIdentityColumn()
                .ValueGeneratedOnAdd();

                // Índice único en el código del curso
                tabla.HasIndex(columna => columna.Codigo)
                .IsUnique();

                tabla.Property(columna => columna.Codigo)
                .HasMaxLength(20)
                .IsRequired();

                tabla.Property(columna => columna.Titulo)
                .HasMaxLength(200)
                .IsRequired();

                tabla.Property(columna => columna.Descripcion)
                .HasMaxLength(2000)
                .IsRequired();

                tabla.Property(columna => columna.Objetivos)
                .HasMaxLength(1000);

                tabla.Property(columna => columna.Duracion)
                .IsRequired();

                tabla.Property(columna => columna.Modalidad)
                .HasConversion<string>()
                .HasMaxLength(20)
                .IsRequired();

                tabla.Property(columna => columna.Categoria)
                .HasMaxLength(100)
                .IsRequired();

                tabla.Property(columna => columna.Nivel)
                .HasConversion<string>()
                .HasMaxLength(20)
                .IsRequired();

                tabla.Property(columna => columna.CapacidadMaxima)
                .IsRequired();

                tabla.Property(columna => columna.Estado)
                .HasConversion<string>()
                .HasMaxLength(20)
                .IsRequired();

                tabla.Property(columna => columna.ImagenUrl)
                .HasMaxLength(500);

                tabla.Property(columna => columna.FechaCreacion)
                .IsRequired();

                tabla.Property(columna => columna.EsEliminado)
                .HasDefaultValue(false)
                .IsRequired();

                // Relación con Usuario (Profesor)
                tabla.HasOne(columna => columna.Profesor)
                .WithMany()
                .HasForeignKey(columna => columna.ProfesorAsignado)
                .OnDelete(DeleteBehavior.Restrict);

                // Relación con Usuario (Creador)
                tabla.HasOne(columna => columna.Creador)
                .WithMany()
                .HasForeignKey(columna => columna.CreadoPor)
                .OnDelete(DeleteBehavior.Restrict);
            });

            modelBuilder.Entity<Solicitud>().ToTable("Solicitud");
            modelBuilder.Entity<SolicitudRevisión>().ToTable("SolicitudRevisión");
            modelBuilder.Entity<Usuario>().ToTable("Usuario");
            modelBuilder.Entity<Rol>().ToTable("Rol");
            modelBuilder.Entity<Permisos>().ToTable("Permisos");
            modelBuilder.Entity<Modulo>().ToTable("Modulo");
            modelBuilder.Entity<Curso>().ToTable("Curso");
        }
    }        
}
