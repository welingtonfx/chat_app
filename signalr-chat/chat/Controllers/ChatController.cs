using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authorization;
using chat.Models;

namespace chat.Controllers
{
    public class ChatController : Controller
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly SignInManager<AppUser> _signInManager;
        public ChatController(UserManager<AppUser> userManager, SignInManager<AppUser> signInManager)
        {
            _userManager = userManager;
            _signInManager = signInManager;
        }

        public IActionResult Index()
        {
            return View();
        }


        [Authorize]
        public async Task<IActionResult> UserInfo()
        {
            var user = await _userManager.GetUserAsync(HttpContext.User).ConfigureAwait(false);


            if (user == null)
            {
                RedirectToAction("Login");
            }

            return View(user);
        }

        [Authorize]
        public async Task<IActionResult> Room()
        {
            var user = await _userManager.GetUserAsync(HttpContext.User).ConfigureAwait(false);


            if (user == null)
            {
                RedirectToAction("Login");
            }

            return View(user);
        }

        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(AppUser appUser)
        {
            var user = await _userManager.FindByNameAsync(appUser.UserName);

            if (user != null)
            {
                var signInResult = await _signInManager.PasswordSignInAsync(user, appUser.Password, false, false);

                if (signInResult.Succeeded)
                {
                    return RedirectToAction("Room");
                }
            }

            return RedirectToAction("Register");
        }

        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Register(AppUser appUser)
        {
            var randomId = new Random();
            var user = new AppUser
            {
                Id = randomId.Next(1, 50000).ToString(),
                UserName = appUser.UserName,
                Email = appUser.Email,
                Name = appUser.Name,
                Password = appUser.Password
            };

            var result = await _userManager.CreateAsync(user, user.Password);


            if (result.Succeeded)
            {
                var signInResult = await _signInManager.PasswordSignInAsync(user, user.Password, false, false);

                if (signInResult.Succeeded)
                {
                    return RedirectToAction("Login");
                }
            }

            return View();
        }
    }
}
