using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ProyectoRH2025.Models
{
    [Table("tblHuellasEmpleados")]
    public class TblHuellasEmpleados
    {
        [Key]
        public int id { get; set; }
        public string Huella { get; set; }
        public int? OpcHuella { get; set; }
        public int? idEmpleado { get; set; }
        public int? idUsuario { get; set; }
        public string Dedo { get; set; }  // <-- NUEVO
    }

}
