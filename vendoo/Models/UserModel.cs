namespace vendoo.Models
{
    public class UserModel
    {
        public string Name { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public string? Password { get; set; }
        public int Mobile { get; set; }
        public int Role_id { get; set; }
        public int Id { get; set; }
    }
}
