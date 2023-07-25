using Newtonsoft.Json;
using System.Text.Json.Serialization;

namespace foodTrackerApi.Models
{
    public class User
    {
        [JsonProperty("custom:householdId")]
        [JsonPropertyName("custom:householdId")]
        public string HouseholdId { get; set; }

        [JsonPropertyName("given_name")]
        [JsonProperty("given_name")]
        public string GivenName { get; set; }

        [JsonPropertyName("family_name")]
        [JsonProperty("family_name")]
        public string FamilyName { get; set; }

        [JsonPropertyName("email")]
        [JsonProperty("email")]
        public string Email { get; set; }

        [JsonPropertyName("username")]
        [JsonProperty("username")]
        public string Username { get; set; }

        public bool IsHouseholdAdmin { get; set; }

        public static string Identifier => "user";
    }
}
