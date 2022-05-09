using System.Text.Json.Serialization;

namespace rtoken1.Model
{
    public class User
    {
        public int Id { get; set; }
        public string Username { get; set; }
        public Role Role { get; set; } = Role.User;
        public byte[] PasswordHash { get; set; }
        public byte[] PasswordSalt { get; set; }
        [JsonIgnore]
        public List<RefreshToken> RTokens { get; set; }
    }
}