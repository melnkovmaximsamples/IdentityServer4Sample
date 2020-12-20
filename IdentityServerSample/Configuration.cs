using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using IdentityModel;
using IdentityServer4;
using IdentityServer4.Models;

namespace IdentityServerSample
{
    public static class Configuration
    {
        public static IEnumerable<Client> GetClients() => 
        new List<Client>()
        {
            new Client()
            {
                ClientId = "client_id",
                ClientSecrets = { new Secret("client_secret".ToSha256()) },
                AllowedGrantTypes = GrantTypes.ClientCredentials,
                AllowedScopes =
                {
                    "OrdersAPI"
                }
            },
            new Client()
            {
                ClientId = "client_id_mvc",
                ClientSecrets = { new Secret("client_secret_mvc".ToSha256()) },
                AllowedGrantTypes = GrantTypes.Code,
                AllowedScopes =
                {
                    "OrdersAPI",
                    IdentityServerConstants.StandardScopes.OpenId,
                    IdentityServerConstants.StandardScopes.Profile
                },

                RedirectUris = { "https://localhost:13334/signin-oidc"}
            }
        };

        public static IEnumerable<ApiResource> GetApiResources() => 
        new List<ApiResource>()
        {
            new ApiResource("OrdersAPI")
        };

        public static IEnumerable<IdentityResource> GetIdentityResources() =>
        new List<IdentityResource>()
        {
            new IdentityResources.OpenId(),
            new IdentityResources.Profile()
        };

        public static IEnumerable<ApiScope> GetApiScopes() 
        {
            yield return new ApiScope("OrdersAPI");
        }
    }
}
