using IdentityModel.Client;
using MbCoreApp.ServiceModel.Vehicle;
using Newtonsoft.Json.Linq;
using ServiceStack;
using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace DemoIdSvrClient
{
    public class Constants
    {
        public const string Authority = "http://127.0.0.1:65048";
        public const string SampleApi = "http://127.0.0.1:6000";
    }

    class Program
    {
        public static void Main(string[] args) => MainAsync().GetAwaiter().GetResult();

        private static async Task MainAsync()
        {

            Console.WriteLine("============== CallResourceOwnerFlow ");
            await CallResourceOwnerFlow();

            Console.WriteLine("============== CallClientCredentials ");
            await CallClientCredentials();

            
            Console.ReadLine();

        }

        #region Resource Owner Flow

        static async Task CallResourceOwnerFlow()
        {
            var response = await RequestTokenAsync_CallResourceOwnerFlow();
            await CallServiceAsync(response.AccessToken, nameof(CallResourceOwnerFlow));
        }

        static async Task<TokenResponse> RequestTokenAsync_CallResourceOwnerFlow()
        {
            var client = new HttpClient();

            var disco = await client.GetDiscoveryDocumentAsync(Constants.Authority);
            if (disco.IsError) throw new Exception(disco.Error);

            var response = await client.RequestPasswordTokenAsync(new PasswordTokenRequest
            {
                Address = disco.TokenEndpoint,

                ClientId = "roclient",
                ClientSecret = "secret",

                UserName = "bob",
                Password = "password",

                Scope = "apiMB openid",
            });

            if (response.IsError)
            {
                Console.WriteLine(response.Error);
            }

            Console.WriteLine(response.Json);
            Console.WriteLine("\n\n");
            return response;
        }

        #endregion

        #region Call Client Credentials

        static async Task CallClientCredentials()
        {
            var response = await RequestTokenAsync_CallClientCredentials();
            await CallServiceAsync(response.AccessToken, nameof(CallClientCredentials));
        }

        static async Task<TokenResponse> RequestTokenAsync_CallClientCredentials()
        {
            var client = new HttpClient();

            // discover endpoints from metadata MbIdSvrHost
            var disco = await client.GetDiscoveryDocumentAsync(Constants.Authority);
            if (disco.IsError) throw new Exception(disco.Error);

            // request token
            var response = await client.RequestClientCredentialsTokenAsync(new ClientCredentialsTokenRequest
            {
                Address = disco.TokenEndpoint,

                ClientId = "client",
                ClientSecret = "secret",
                Scope = "apiMB"
            });

            if (response.IsError)
            {
                Console.WriteLine(response.Error);
            }

            Console.WriteLine(response.Json);
            Console.WriteLine("\n\n");
            return response;
        }

        #endregion

        #region Calling the Secure Identity Api

        static async Task CallServiceAsync(string token, string callerName)
        {
            var baseAddress = Constants.SampleApi;

            var client = new HttpClient
            {
                BaseAddress = new Uri(baseAddress)
            };

            client.SetBearerToken(token);
            var response = await client.GetStringAsync("identity");

            Console.WriteLine($"============= {callerName} : Service Claims");
            Console.WriteLine(JArray.Parse(response));


            // ================================================================

            //Call the SECURE Vehicle api (.Net Core api)
            await CallServiceVehicle(token, callerName);

            //call the SECURE ServiceStack Vehicle api (ServiceStack version)
            await CallServiceStackVehicle(token, callerName);


            // ================================================================
        }

        #endregion

        #region Calling the ServiceStack Secure Vehicle Api

        static async Task CallServiceVehicle(string token, string callerName)
        {
            var baseAddress = Constants.SampleApi;

            var client = new HttpClient
            {
                BaseAddress = new Uri(baseAddress)
            };

            client.SetBearerToken(token);
            var response = await client.GetStringAsync("vehicle");

            Console.WriteLine($"============= {callerName} : Vehicles");
            Console.WriteLine($"Vehicles: {response}");
            
        }

        static async Task CallServiceStackVehicle(string token, string callerName)
        {
            var serviceStackClient = new JsonServiceClient(Constants.SampleApi);
            serviceStackClient.BearerToken = token;

            GetVehiclesResponse response = await serviceStackClient.GetAsync(new GetVehicles());

            Console.WriteLine($"============= {callerName} : Vehicles");
            foreach (var item in response.Results)
            {
                Console.WriteLine($"Vehicle: {item.Id} {item.Description}");
            }
        }

        #endregion

    }
}