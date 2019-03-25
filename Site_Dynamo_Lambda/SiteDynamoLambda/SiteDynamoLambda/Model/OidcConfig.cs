using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SiteDynamoLambda.Model
{
    public class OidcConfig
    {
        public string MetadataAddress { get; set; }
        public string ResponseType { get; set; }
        public string ClientId { get; set; }
        public string ClientSecret { get; set; }
        public string LogoutUrl { get; set; }
        public string BaseUrl { get; set; }
    }
}
