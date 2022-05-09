using System.Text.Json.Serialization;
using rtoken1.Model;

namespace rtoken1.Dtos.Auth
{
    public class LoginResponse
    {
        public int Id { get; set; }
        public string Username { get; set; }
        public Role Role { get; set; }
        public string AccessToken { get; set; }
        [JsonIgnore]
        public string RToken { get; set; }
    }
}