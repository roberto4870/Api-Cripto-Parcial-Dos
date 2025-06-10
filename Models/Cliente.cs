using System.ComponentModel.DataAnnotations;

namespace ApiCriptoParcialI.Models
{
    public class Cliente
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string Name { get; set; }

        [Required]
        [EmailAddress]
        public string Email { get; set; }

        public List<Transaction> Transactions { get; set; } = new List<Transaction>();

    }
}
