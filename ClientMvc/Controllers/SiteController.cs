using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClientMvc.Controllers
{
    [Route("[controller]")]
    public class SiteController : Controller
    {
        [Route("[action]")]
        public IActionResult Index()
        {
            return View();
        }

        [Authorize]
        [Route("[action]")]
        public IActionResult Secret()
        {
            return View();
        }
    }
}
