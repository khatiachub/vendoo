namespace vendoo.Models
{
    public class ItemDescriptionModel
    {
        public string Product_name { get; set; }
        public int Category_id { get; set; }
        public string Full_Description { get; set; }
        public int Price { get; set; }
        public int Contact {  get; set; }
        //public IFormFile[]? All_Images { get; set; }
        public List<IFormFile> Image_path { get; set; }
    }
}
