﻿namespace vendoo.Models
{
    public class GetProductModel
    {
        public int Id { get; set; }
        public string Product_name { get; set; }
        public int Category_id { get; set; }
        public string Full_Description { get; set; }
        public int Price { get; set; }
        public int Contact { get; set; }
        public List<string>? Image_path { get; set; }
        public int Vaucher { get; set; }
        public int Sale {  get; set; }
        public int Current_price { get; set; }
        public string Title { get; set; }
    }
}
