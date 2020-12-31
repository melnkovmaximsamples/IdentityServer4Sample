using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Authorization.Client.Mvc.ViewModels;
using IdentityModel.Client;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;

namespace ClientMvc.Controllers
{
    public class SiteController : Controller
    {
        private readonly IHttpClientFactory _httpClientFactory;

        public SiteController(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        public IActionResult Index()
        {
            return View();
        }

        // проблема. работает только при audience отключенном. т.к. на добавляется в access_token "OrdersAPI" в aud.
        [Authorize]
        public async Task<IActionResult> Secret()
        {
            var model = new ClaimManager(HttpContext, User);

            try
            {
                ViewBag.Message = await GetSecretAsync(model);
                return View(model);
            }
            catch (Exception ex)
            {
                await RefreshToken(model.RefreshToken);
                var model2 = new ClaimManager(HttpContext, User);
                ViewBag.Message = await GetSecretAsync(model2);
            }

            return View(model);
        }

        private async Task RefreshToken(string refreshToken)
        {
            var refreshClient = _httpClientFactory.CreateClient();
            var resultRefreshToken = await refreshClient.RequestRefreshTokenAsync(new RefreshTokenRequest()
            {
                ClientId = "client_id_mvc",
                ClientSecret = "client_secret_mvc",
                Address = "http://localhost:15555/connect/token",
                RefreshToken = refreshToken,
                Scope = "openid ordersAPI offline_access"
            });

            await UpdateAuthContextAsync(resultRefreshToken.AccessToken, resultRefreshToken.RefreshToken);
        }

        private async Task<string> GetSecretAsync(ClaimManager model)
        {
            var client = _httpClientFactory.CreateClient();

            client.SetBearerToken(model.AccessToken);

            return await client.GetStringAsync("http://localhost:11111/site/secret");
        }

        private async Task UpdateAuthContextAsync(string accessToken, string refreshToken)
        {
            var authenticate = await HttpContext.AuthenticateAsync(CookieAuthenticationDefaults.AuthenticationScheme);

            authenticate.Properties.UpdateTokenValue("access_token", accessToken);
            authenticate.Properties.UpdateTokenValue("refresh_token", refreshToken);

            await HttpContext.SignInAsync(authenticate.Principal, authenticate.Properties);
        }
        
        [Authorize(Policy = "HasDateOfBirth")]
        [Route("[action]")]
        public IActionResult Secret1()
        {
            var model = new ClaimManager(HttpContext, User);

            return View("Secret", model);
        }

        [Authorize(Policy = "OlderThan")]
        [Route("[action]")]
        public IActionResult Secret2()
        {
            var model = new ClaimManager(HttpContext, User);

            return View("Secret", model);
        }

        [Route("[action]")]
        public IActionResult Logout()
        {
            var parameters = new AuthenticationProperties()
            {
                RedirectUri = "/Site/Secret"
            };
            return SignOut(
                parameters,
                CookieAuthenticationDefaults.AuthenticationScheme,
                OpenIdConnectDefaults.AuthenticationScheme);
        }
    }
}
