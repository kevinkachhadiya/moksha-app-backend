namespace MAPI.Models
{
    public class LoginModel
    {
        public string? Username { get; set; }
        public string? Password { get; set; }
    }
    public class User
    {
        public int Id { get; set; } // User ID, could be int, string, Guid, etc.
        public string? Username { get; set; } // Username for the user
        public bool IsAdmin { get; set; } // Flag to indicate if the user is an admin
        public string? PasswordHash { get; set; } // Hashed password (typically a string)
    }

}
