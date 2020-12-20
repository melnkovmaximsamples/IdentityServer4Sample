using IdentityModel.Client;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace UsersApi.Controllers
{
    [Route("[controller]")]
    public class SiteController : Controller
    {
        private readonly IHttpClientFactory _httpClientFactory;

        public SiteController(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        [Route("[action]")]
        public IActionResult Index()
        {
            return View();
        }

        [Route("[action]")]
        public async Task<IActionResult> GetOrders()
        {
            // retrieve to IdentityServer
            var authClient = _httpClientFactory.CreateClient();
            var discoveryDocument = await authClient.GetDiscoveryDocumentAsync("https://localhost:15554/");
            var token = await authClient.RequestClientCredentialsTokenAsync(
                new ClientCredentialsTokenRequest
                {
                    Address = discoveryDocument.TokenEndpoint,
                    ClientId = "client_id",
                    ClientSecret = "client_secret",
                    Scope = "OrdersAPI"
                });

            // retrieve to Orders
            var ordersClient = _httpClientFactory.CreateClient();

            ordersClient.SetBearerToken(token.AccessToken);

            var response = await ordersClient.GetAsync("https://localhost:11110/site/secret");

            if (!response.IsSuccessStatusCode)
            {
                ViewBag.Message = response.StatusCode.ToString();
                return View();
            }

            var message = await response.Content.ReadAsStringAsync();

            ViewBag.Message = message;
            return View();
        }
    }
}
