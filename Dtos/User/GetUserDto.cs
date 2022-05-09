using rtoken1.Model;

namespace rtoken1.Dtos.User
{
    public class GetUserDto
    {
        public int Id { get; set; }
        public string Username { get; set; }
        public Role Role { get; set; }
    }
}