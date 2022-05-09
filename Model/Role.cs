using System.Text.Json.Serialization;

namespace rtoken1.Model
{
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum Role
    {
        User,
        Admin,
        Super
    }
}