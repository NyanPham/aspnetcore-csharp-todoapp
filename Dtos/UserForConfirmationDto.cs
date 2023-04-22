namespace todoapp.Dtos
{
    public class UserForConfirmationDto
    {
        public string Email { get; set; }
        public byte[] PasswordHash { get; set; }
        public byte[] PasswordSalt { get; set; }

        public UserForConfirmationDto()
        {
            if (Email == null) Email = "";
        }

    }
}