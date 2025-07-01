using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ProyectoRH2025.Models
{
    [Table("tblHistoricoPass")]
    public class TblHistoricoPass
    {
        [Key]
        public int ID { get; set; }

        public int idUsuario { get; set; }

        public byte[] PassAnterior { get; set; }
    }
}
