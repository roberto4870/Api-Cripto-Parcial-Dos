using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace ApiCriptoParcialI.Models
{
    public class Transaction
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string CryptoCode { get; set; }

        [Required]
        public string Action { get; set; } // "compra" o "venta"

        [Required]
        public int ClienteId { get; set; }

        [ForeignKey("ClientId")]
        public Cliente? Cliente { get; set; }

        [Required]
        public decimal CryptoAmount { get; set; }

        [Required]
        public decimal Money { get; set; } // Monto total en ARS

        [Required]
        public DateTime DateTime { get; set; }
    }
}
