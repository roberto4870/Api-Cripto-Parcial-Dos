using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace ApiCriptoParcialI.Models
{
    public class Transaction
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string CryptoCode { get; set; }

        [Required]
        public string Action { get; set; }

        [Required]
        public int ClienteId { get; set; }

        [ForeignKey("ClienteId")]
        public Cliente? Cliente { get; set; }

        [Required]
        [Precision(18, 8)] // Hasta 8 decimales (recomendado para criptos)
        public decimal CryptoAmount { get; set; }

        [Required]
        [Precision(18, 2)] // Monto en pesos (con 2 decimales)
        public decimal Money { get; set; }

        [Required]
        public DateTime DateTime { get; set; }
    }

}
