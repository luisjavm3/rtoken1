using System.Text.Json.Serialization;

namespace rtoken1.Model
{
    public class RefreshToken
    {
        public int Id { get; set; }
        public string Value { get; set; }
        public DateTime CreatedAt { get; set; }
        public string CreatedByIp { get; set; }
        public DateTime ExpiresAt { get; set; }
        public DateTime? RevokedAt { get; set; }
        public string RevokedByIp { get; set; }
        public string ReasonRevoked { get; set; }
        public string FirstSessionToken { get; set; }
        public bool IsExpired => ExpiresAt <= DateTime.Now;
        public bool IsRevoked => RevokedAt != null;
        public bool IsActive => !IsExpired && !IsRevoked;
        [JsonIgnore]
        public User User { get; set; }
    }
}