using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ProyectoRH2025.Models
{
    [Table("tblImagenes")]
    public class ImagenEmpleado
    {
        [Key]
        public int id { get; set; }
        public string? Imagen { get; set; }

        [Column("idEmpleado")]
        public int idEmpleado { get; set; }
    }
}