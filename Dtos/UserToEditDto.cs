namespace todoapp.Dtos
{
    public class UserToEditDto
    {
        public string Email { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Gender { get; set; }
        public bool Active { get; set; }
        

        public UserToEditDto()
        {
            if (Email == null) Email = "";
            if (FirstName == null) FirstName = "";
            if (LastName == null) LastName = "";
            if (Gender == null) Gender = "";
        }
    }
}