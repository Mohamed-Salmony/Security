namespace NetMvcApp.Models
{
    public class User
    {
        public int UserId { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public string HashedPassword { get; set; }
        public string PhoneNumber { get; set; }
        public string RSAPublicKey { get; set; }
        // The private key will be stored separately for security, linked by UserId
    }
}

