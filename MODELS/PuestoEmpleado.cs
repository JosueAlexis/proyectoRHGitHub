using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ProyectoRH2025.Models
{
    [Table("tblPuestoEmpleados")]
    public class PuestoEmpleado
    {
        [Key]
        [Column("id")]
        public int id { get; set; }

        [Column("Puesto")]
        public string Puesto { get; set; } = string.Empty;

        [Column("idtipempleado")]
        public int idtipempleado { get; set; }
    }
}
