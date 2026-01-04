namespace VerifileChallengeApp.Database.Models
{
    public class Person
    {
        public int Id { get; set; }
        public string GivenName { get; set; }
        public string FamilyName { get; set; }
        public DateTime LastUpdate { get; set; }

        public static Person FromDto(HttpClients.TWFU.Response.Person dto) =>
            new Person
            {
                Id = dto.PersonId,
                GivenName = dto.GivenName,
                FamilyName = dto.FamilyName,
                LastUpdate = dto.LastUpdate
            };
    }
}
