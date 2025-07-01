using Microsoft.EntityFrameworkCore;
using ProyectoRH2025.Models; // Para tus modelos existentes si están en este namespace (ej. Empleado, Usuario)
using ProyectoRH2025.MODELS;   // Para los nuevos modelos PodRecord, PodEvidenciaImagen

// Asegúrate que el namespace sea exactamente "ProyectoRH2025.Data"
namespace ProyectoRH2025.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        // Tus DbSets existentes
        // Asegúrate que los tipos aquí (Empleado, ImagenEmpleado, etc.) estén correctamente definidos
        // en los namespaces referenciados por tus directivas 'using' (Models o MODELS).
        public DbSet<Empleado> Empleados { get; set; }
        public DbSet<ImagenEmpleado> ImagenesEmpleados { get; set; }
        public DbSet<PuestoEmpleado> PuestoEmpleados { get; set; } = null!;
        public DbSet<Usuario> TblUsuarios { get; set; }
        public DbSet<TblHistoricoPass> TblHistoricoPass { get; set; }
        public DbSet<TblRolusuario> TblRolusuario { get; set; }
        public DbSet<TblSellos> TblSellos { get; set; }
        public DbSet<TblUnidades> TblUnidades { get; set; }
        public DbSet<TblAsigSellos> TblAsigSellos { get; set; }
        public DbSet<TblTipoAsignacion> TblTipoAsignacion { get; set; }
        public DbSet<ViviendaEmpleado> tblViviendaEmple { get; set; }
        public DbSet<TblHuellasEmpleados> TblHuellasEmpleados { get; set; }
        // public DbSet<ImagenAsignacion> ImagenAsignacion { get; set; } // Verifica si esta entidad es parte de tu contexto

        // --- NUEVOS DBSETS PARA LIQUIDACIONES ---
        // Estas entidades (PodRecord, PodEvidenciaImagen) deben estar definidas en el namespace ProyectoRH2025.MODELS
        public DbSet<PodRecord> PodRecords { get; set; }
        public DbSet<PodEvidenciaImagen> PodEvidenciasImagenes { get; set; }
        // -----------------------------------------

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Tu configuración existente
            modelBuilder.Entity<PuestoEmpleado>()
                .ToTable("tblPuestoEmpleados");

            // Configuraciones para nuevos modelos (si son necesarias en el futuro)
            // modelBuilder.Entity<PodRecord>()
            // .HasMany(p => p.PodEvidenciasImagenes)
            // .WithOne(e => e.PodRecord)
            // .HasForeignKey(e => e.POD_ID_FK);

            // modelBuilder.Entity<PodEvidenciaImagen>()
            // .HasOne(e => e.PodRecord)
            // .WithMany(p => p.PodEvidenciasImagenes)
            // .HasForeignKey(e => e.POD_ID_FK);
        }
    }
}
