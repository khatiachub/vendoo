namespace vendoo.Models
{
    public class ChildCategoryModel
    {
        public string? category_name { get; set; }
        public string? first_child_name { get; set; }
        public string? sec_child_name { get; set; }
        public int Category_id { get; set; }
        public string Description { get; set; }
        public int Price { get; set; }
        public IFormFile Image {  get; set; }
    }
}
