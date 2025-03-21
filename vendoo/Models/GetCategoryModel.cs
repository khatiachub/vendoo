namespace vendoo.Models
{
    public class GetCategoryModel
    {
        public string? category_name { get; set; }
        public string? first_child_name { get; set; }
        public string? title { get; set; }
        public int Category_id { get; set; }
        public int Main_category_id { get; set; }
        public string product_name { get; set; }
        public int Price { get; set; }
        public List<string>? Image_path { get; set; }
        public string location { get; set; }
        public string Guests { get; set; }
        public int Id { get; set; }
        public int Sale {  get; set; }
        public int Current_price { get; set; }
    }
}
