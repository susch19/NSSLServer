namespace NSSL.Models
{
    public class Product
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Unit { get; set; }
        public decimal? Quantity { get; set; }
        public string ExternalId { get; set; }
        public short ProviderKey { get; set; }
        public short Fitness { get; set; }
        

    }
}
