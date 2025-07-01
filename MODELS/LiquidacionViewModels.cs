using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

// Asegúrate que el namespace sea el correcto para tu proyecto.
// Si tu carpeta de modelos está directamente bajo ProyectoRH2025, entonces es ProyectoRH2025.MODELS
namespace ProyectoRH2025.MODELS
{
    // Modelo para la tabla POD_Records
    [Table("POD_Records")]
    public class PodRecord
    {
        [Key]
        public int POD_ID { get; set; }

        [StringLength(255)]
        public string? Folio { get; set; }

        [StringLength(255)]
        public string? Cliente { get; set; }

        [StringLength(255)]
        public string? Tractor { get; set; }

        [StringLength(255)]
        public string? Remolque { get; set; }

        public DateTime? FechaSalida { get; set; }

        [StringLength(255)]
        public string? Frontera { get; set; }

        [StringLength(255)]
        public string? Origen { get; set; }

        [StringLength(255)]
        public string? Destino { get; set; }

        [StringLength(255)]
        public string? DriverName { get; set; }

        public DateTime? CaptureDate { get; set; }

        [StringLength(100)]
        public byte? Status { get; set; }

        public DateTime? CreatedAt { get; set; }

        [StringLength(1024)]
        public string? ImageUrl { get; set; }

        public int? ImageSequence { get; set; }

        public virtual ICollection<PodEvidenciaImagen> PodEvidenciasImagenes { get; set; } = new List<PodEvidenciaImagen>();
    }

    // Modelo para la tabla POD_Evidencias_Imagenes
    [Table("POD_Evidencias_Imagenes")]
    public class PodEvidenciaImagen
    {
        [Key]
        public int EvidenciaID { get; set; }

        public int POD_ID_FK { get; set; }

        public byte[]? ImageData { get; set; }

        public int? ImageSequence { get; set; }

        [StringLength(50)]
        public string? MimeType { get; set; }

        [StringLength(255)]
        public string? FileName { get; set; }

        public DateTime? CaptureDate { get; set; }

        [ForeignKey("POD_ID_FK")]
        public virtual PodRecord? PodRecord { get; set; }
    }

    // --- ViewModel para la página de Liquidaciones ---
    public class LiquidacionViewModel
    {
        public int PodId { get; set; }
        public string? Folio { get; set; }
        public string? Cliente { get; set; }
        public string? Tractor { get; set; }
        public string? Remolque { get; set; }
        public DateTime? FechaSalida { get; set; }
        public string? Origen { get; set; }
        public string? Destino { get; set; }
        public string? DriverName { get; set; }
        public string? Status { get; set; }
        public DateTime? PodRecordCaptureDate { get; set; }
        public string? PodRecordImageUrl { get; set; }
        public List<EvidenciaViewModel> Evidencias { get; set; } = new List<EvidenciaViewModel>();
    }

    public class EvidenciaViewModel
    {
        public int EvidenciaId { get; set; }
        public string? FileName { get; set; }
        public string? MimeType { get; set; }
        public DateTime? EvidenciaCaptureDate { get; set; }
        public int? ImageSequence { get; set; }
        public bool HasImageData { get; set; }
    }
}
