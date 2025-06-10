namespace ApiCriptoParcialI.DTOs
{
    public class TransactionDTOs
    {
        public int Id { get; set; }
        public string CryptoCode { get; set; }
        public string Action { get; set; }
        public decimal CryptoAmount { get; set; }
        public decimal Money { get; set; }
        public DateTime Datetime { get; set; }
        public int ClientId { get; set; }
    }
}
