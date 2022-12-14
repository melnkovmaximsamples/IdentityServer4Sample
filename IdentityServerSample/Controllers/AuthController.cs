using System.Threading.Tasks;
using IdentityServer4.Services;
using IdentityServerSample.ViewModels;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace IdentityServerSample.Controllers
{
    [Route("[controller]")]
    public class AuthController : Controller
    {
        private readonly IIdentityServerInteractionService _interactionService;
        private readonly SignInManager<IdentityUser> _signInManager;
        private readonly UserManager<IdentityUser> _userManager;

        public AuthController(
            IIdentityServerInteractionService interactionService,
            SignInManager<IdentityUser> signInManager,
            UserManager<IdentityUser> userManager)
        {
            _interactionService = interactionService;
            _signInManager = signInManager;
            _userManager = userManager;
        }

        [Route("[action]")]
        public IActionResult Login(string returnUrl)
        {
            return View(new LoginViewModel()
            {
                UserName = "User",
                Password = "123qwe",
                ReturnUrl = returnUrl
            });
        }

        [Route("[action]")]
        public async Task<IActionResult> Logout(string logoutId)
        {
            await _signInManager.SignOutAsync();
            var logoutResult = await _interactionService.GetLogoutContextAsync(logoutId);
            if (string.IsNullOrEmpty(logoutResult.PostLogoutRedirectUri))
            {
                return RedirectToAction("Index", "Server");
            }

            return Redirect(logoutResult.PostLogoutRedirectUri);
        }

        [Route("[action]")]
        [HttpPost]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var user = await _userManager.FindByNameAsync(model.UserName);

            if (user == null)
            {
                ModelState.AddModelError("UserName", "User not found");
                return View(model);
            }

            var signInResult = await _signInManager.PasswordSignInAsync(user, model.Password, false, false);

            if (signInResult.Succeeded)
            {
                return Redirect(model.ReturnUrl);
            }

            ModelState.AddModelError("UserName", "Something went wrong");

            return View(model);
        }
    }
}
