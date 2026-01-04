using System.Text.Json.Serialization;

namespace VerifileChallengeApp.HttpClients.TWFU.Response;

public class Person
{
    [JsonPropertyName("member_id")]
    public string MemberId { get; set; }
    
    [JsonPropertyName("house")]
    public string House { get; set; }

    [JsonPropertyName("constituency")]
    public string Constituency { get; set; }

    [JsonPropertyName("party")] 
    public string Party { get; set; }

    [JsonPropertyName("person_id")] 
    public int PersonId { get; set; }

    [JsonPropertyName("lastupdate")]
    [JsonConverter(typeof(CustomDateTimeConverter))]
    public DateTime LastUpdate { get; set; }

    [JsonPropertyName("given_name")] 
    public string GivenName { get; set; }

    [JsonPropertyName("family_name")] 
    public string FamilyName { get; set; } 
}
