using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ProyectoRH2025.Models
{
    [Table("tblAsigSellos")]
    public class TblAsigSellos
    {
        [Key]
        public int id { get; set; }

        public int idSello { get; set; }
        public int idUsuario { get; set; }
        public DateTime Fentrega { get; set; }
        public int idOperador { get; set; }
        public int idUnidad { get; set; }
        public string? Ruta { get; set; }
        public string? Comentarios { get; set; }
        public string? Caja { get; set; }
        public int Status { get; set; }
        public string? editor { get; set; }
        [Column("idSeAsigno")]
        public int idSeAsigno { get; set; }

        public DateTime? FechaStatus4 { get; set; }
        public int? idOperador2 { get; set; }
        public int TipoAsignacion { get; set; }

    }
}
