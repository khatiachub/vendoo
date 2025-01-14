namespace vendoo.Models
{
    public class OrderModel
    {
        public int id { get; set; }
        public string name { get; set; }
        public int quantity { get; set; }
        public List<int> product_id { get; set; } // Change to List<int>
    }
}
