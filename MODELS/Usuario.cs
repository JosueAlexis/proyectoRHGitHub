using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ProyectoRH2025.Models
{
    [Table("tblUsuarios")]
    public class Usuario
    {
        [Key]
        public int idUsuario { get; set; }

        [Required(ErrorMessage = "El nombre de usuario es obligatorio.")]
        [Column("Usuario")]
        public string UsuarioNombre { get; set; }

        [Required(ErrorMessage = "El rol es obligatorio.")]
        public int idRol { get; set; }

        // Ya no lo requerimos porque se asigna desde el backend
        public byte[]? pass { get; set; }

        [Required(ErrorMessage = "El estado es obligatorio.")]
        public int Status { get; set; }

        // Opcionales
        public int? DefaultPassw { get; set; }
        public DateTime? CambioPass { get; set; }
        public string? TokenRecuperacion { get; set; }
        public int? idSucursal { get; set; }

        public string? NombreCompleto { get; set; }
        public string? CorreoElectronico { get; set; }

        // Si no usas lazy loading, no es necesario "virtual"
        [ForeignKey("idRol")]
        public TblRolusuario? Rol { get; set; }
    }
}
