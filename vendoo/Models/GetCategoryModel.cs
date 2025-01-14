namespace vendoo.Models
{
    public class GetCategoryModel
    {
        public string? category_name { get; set; }
        public string? first_child_name { get; set; }
        public string? sec_child_name { get; set; }
        //public int Category_id { get; set; }
        public string Description { get; set; }
        public int Price { get; set; }
        public string Image { get; set; }
        public int location_id { get; set; }
        public int Guest_id { get; set; }
        public int Id { get; set; }
    }
}
