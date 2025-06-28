namespace NSSLServer.Models
{
    public interface IDatabaseProduct
    {
        string Name { get; set; }
        decimal? Quantity { get; set; }
        string Unit { get; set; }
    }
}
