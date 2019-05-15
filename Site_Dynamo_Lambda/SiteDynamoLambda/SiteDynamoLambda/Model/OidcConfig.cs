namespace SiteDynamoLambda.Model
{
    public class OidcConfig
    {
        public string MetadataAddress { get; set; }
        public string ResponseType { get; set; }
        public string ClientId { get; set; }
        public string ClientSecretKey { get; set; }
        public string LogoutUrl { get; set; }
        public string BaseUrl { get; set; }
    }
}
