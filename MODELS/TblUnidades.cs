using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ProyectoRH2025.Models
{
    [Table("tblUnidades")]
    public class TblUnidades
    {
        [Key]
        public int id { get; set; }

        public int NumUnidad { get; set; }
        public string? Placas { get; set; }
        public string? Pool { get; set; }
        public string? CodCliente { get; set; }
        public string? AnoUnidad { get; set; }
        public int? idSucursal { get; set; }
    }
}
