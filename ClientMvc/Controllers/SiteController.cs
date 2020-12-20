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
using Microsoft.AspNetCore.Authentication;

namespace ClientMvc.Controllers
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

        // проблема. работает только при audience отключенном. т.к. на добавляется в access_token "OrdersAPI" в aud.
        [Authorize]
        [Route("[action]")]
        public async Task<IActionResult> Secret()
        {
            var model = new ClaimManager(HttpContext, User);

            try
            {
                var client = _httpClientFactory.CreateClient();

                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", model.AccessToken);

                var result = await client.GetStringAsync("http://localhost:11111/site/secret");
                
                ViewBag.Message = result;
            }
            catch (Exception ex)
            {
                ViewBag.Message = ex.Message;
            }

            return View(model);
        }        
        
        [Authorize(Policy = "HasDateOfBirth")]
        [Route("[action]")]
        public async Task<IActionResult> Secret1()
        {
            var model = new ClaimManager(HttpContext, User);

            return View(model);
        }

        [Authorize(Policy = "OlderThan10")]
        [Route("[action]")]
        public async Task<IActionResult> Secret2()
        {
            var model = new ClaimManager(HttpContext, User);

            return View(model);
        }
    }
}
