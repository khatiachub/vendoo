namespace vendoo.Models
{
    public class GetBasketModel
    {
        public List<string>? Image_path { get; set; }
        public string Title { get; set; }
        public string Product_name { get; set; }
        public int Vaucher_price { get; set; }
        public int Total_quantity { get; set; }
        public int Vaucher_quantity { get; set; }
        public int Total_price { get; set; }
        public int Id { get; set; }
        public int Current_price { get; set; }
        public int Starting_price { get; set; }
    }
}
