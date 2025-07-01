using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ProyectoRH2025.Models
{
    [Table("tblViviendaEmple")]
    public class ViviendaEmpleado
    {
        [Key]
        public int idEmpleado { get; set; }

        public string? Calle { get; set; }
        public string? Colonia { get; set; }
        public string? NoExterior { get; set; }
        public string? Ciudad { get; set; }
        public string? Estado { get; set; }
        public string? Pais { get; set; }
        public string? Municipio { get; set; }
        public string? codPostal { get; set; }
        public string? NoInterior { get; set; }
        public string? NoCredito { get; set; }
    }
}
