using IdentityModel;
using IdentityServer4;
using IdentityServer4.Models;
using IdentityServer4.Test;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using static IdentityServer4.IdentityServerConstants;

namespace MbIdSvrHost
{
    /// <summary>
    /// IdentityServer configuration
    /// </summary>
    public class Config
    {
        /// <summary>
        /// Define the api resources (as a collection) we want to protect
        /// </summary>
        /// <returns></returns>
        public static IEnumerable<ApiResource> GetApiResources()
        {
            return new List<ApiResource>
            {
                new ApiResource
                {
                    Name = "apiMB",
                    Enabled = true,
                    ApiSecrets = new List<Secret>
                    {
                        new Secret("secret".Sha256())
                    },
                    Scopes = new List<Scope>
                    {
                        new Scope("apiMB")
                    }
                }
            };
        }

        /// <summary>
        /// Define clients that can access these api resources.
        /// </summary>
        /// <returns></returns>
        public static IEnumerable<Client> GetClients()
        {
            return new List<Client>
            {
                //Service web client
                new Client
                {
                    ClientId = "client",
                    ClientName = "client",              // The Client Identifier matching the IdentityServerAuthFeature.ClientId call
                                                        // in the ServiceStack AppHost Configure() method   

                    Enabled = true,
                    AccessTokenType = AccessTokenType.Jwt,

                    // no interactive user, use the clientid/secret for authentication
                    AllowedGrantTypes = GrantTypes.ClientCredentials,  

                    // secret for authentication
                    ClientSecrets =
                    {
                        new Secret("secret".Sha256())
                    },

                    RedirectUris = new List<string>
                    {
                        "http://127.0.0.1:6000/auth/IdentityServer"                  // The Address and Provider Uri of the ServiceStack Instance
                    },

                    //AllowOfflineAccess = false,          // Set to true if requesting the offline_access scope

                    RequireConsent = false,              // Don't bother prompting for consent

                    // scopes that client has access to
                    AllowedScopes = new List<string>                                    // The requested scopes. Should match IdentityServerAuthFeature.Scopes
                    {
                        IdentityServerConstants.StandardScopes.OpenId,
                        "apiMB"
                    },

                },

                // resource owner password grant client
                new Client
                {
                    ClientId = "roclient",
                    AllowedGrantTypes = GrantTypes.ResourceOwnerPasswordAndClientCredentials,

                    Enabled = true,

                    AccessTokenType = AccessTokenType.Jwt,                              // The AccessToken encryption type

                    ClientSecrets =
                    {
                        new Secret("secret".Sha256())
                    },

                    //RedirectUris = new List<string>
                    //{
                    //    "http://127.0.0.1:6000/auth/IdentityServer"                     // The Address and Provider Uri of the ServiceStack Instance
                    //},

                    AllowedScopes = new List<string>                                    // The requested scopes. Should match IdentityServerAuthFeature.Scopes
                    {
                        IdentityServerConstants.StandardScopes.OpenId,
                        "apiMB"
                    },

                    RequireConsent = false
                }
            };
        }

        public static IEnumerable<IdentityResource> GetIdentityResources()
        {
            return new List<IdentityResource>
            {
                new IdentityResources.OpenId(),
                new IdentityResources.Profile(),
                new IdentityResource
                {
                    Name = "resourceapiMB",
                    Enabled = true
                }
            };
        }

        public static List<TestUser> GetUsers()
        {
            return new List<TestUser>
            {
                new TestUser
                {
                    SubjectId = "1",
                    Username = "alice",
                    Password = "password",
                    IsActive = true
                },
                new TestUser
                {
                    SubjectId = "2",
                    Username = "bob",
                    Password = "password",
                    IsActive = true
                }
            };
        }

        //public static IEnumerable<IdentityResource> GetIdentityResources()
        //{
        //    return new List<IdentityResource>
        //    {
        //        new IdentityServer4.Models.IdentityResources.OpenId(),
        //        new IdentityResource
        //        {
        //            Name = "apiMB",
        //            Enabled = true,
        //            UserClaims = new List<string>
        //            {
        //                JwtClaimTypes.Subject,
        //                JwtClaimTypes.PreferredUserName
        //            }
        //        }
        //    };
        //}
    }
}
