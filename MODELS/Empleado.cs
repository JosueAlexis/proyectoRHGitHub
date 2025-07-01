using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ProyectoRH2025.Models
{
    [Table("tblempleados")]
    public class Empleado
    {
        [Key]
        public int Id { get; set; }

        public int? Reloj { get; set; }

        [StringLength(30)]
        public string? Names { get; set; }

        [StringLength(30)]
        public string? Apellido { get; set; }

        [StringLength(30)]
        public string? Apellido2 { get; set; }

        public DateTime? Fingreso { get; set; }

        public int? TipEmpleado { get; set; }

        public int? Jinmediato { get; set; }

        public DateTime? Fnacimiento { get; set; }

        public int? Puesto { get; set; }

        public DateTime? Fegreso { get; set; }

        [StringLength(200)]
        public string? Email { get; set; }

        public string? Telefono { get; set; }

        public string? TelEmergencia { get; set; }

        [StringLength(40)]
        public string? Rfc { get; set; }

        [StringLength(40)]
        public string? Curp { get; set; }

        [StringLength(20)]
        public string? NumSSocial { get; set; }

        [StringLength(10)]
        public string? CodClientes { get; set; }

        public DateTime? FechaAlta { get; set; }

        public int? IdUsuarioAlta { get; set; }

        public int? Editor { get; set; }

        public int? Status { get; set; }


    }
}