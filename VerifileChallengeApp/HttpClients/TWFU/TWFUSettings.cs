namespace VerifileChallengeApp.HttpClients.TWFU
{
    public class TWFUSettings
    {
        public const string SectionName = "TWFU";

        public required string BaseUrl { get; set; }
        public required string SecretKey { get; set; }
        public required string DefaultPersonId { get; set; }
    }
}
