using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ProyectoRH2025.Models
{
    [Table("tblRolusuario")]
    public class TblRolusuario
    {
        [Key]
        public int idRol { get; set; }

        public string RolNombre { get; set; }

        // Un rol puede tener muchos usuarios
        public virtual ICollection<Usuario> Usuarios { get; set; }
    }
}
