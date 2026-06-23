namespace anamoly_detection_api.Models
{
    public class LoginDto
    {
        public string Email { get; set; }

        public string Password { get; set; }

        public string DisplayName { get; set; } = string.Empty;
    }
}