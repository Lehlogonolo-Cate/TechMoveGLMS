using Microsoft.AspNetCore.Mvc;
using TechMoveGLMS.Web.Models;
using TechMoveGLMS.Web.Services;

namespace TechMoveGLMS.Web.Controllers
{
    public class AuthController : Controller
    {
        private readonly AuthService _authService;

        public AuthController(AuthService authService)
        {
            _authService = authService;
        }

        [HttpGet]
        public IActionResult Login()
        {
            if (!string.IsNullOrWhiteSpace(
                    HttpContext.Session.GetString("JwtToken")))
            {
                return RedirectToAction(
                    "Index",
                    "Home");
            }

            return View(new LoginViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(
            LoginViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            try
            {
                var loginResponse =
                    await _authService.LoginAsync(model);

                if (loginResponse == null ||
                    string.IsNullOrWhiteSpace(
                        loginResponse.Token))
                {
                    ModelState.AddModelError(
                        string.Empty,
                        "The username or password is incorrect.");

                    return View(model);
                }

                HttpContext.Session.SetString(
                    "JwtToken",
                    loginResponse.Token);

                HttpContext.Session.SetString(
                    "Username",
                    loginResponse.Username);

                HttpContext.Session.SetString(
                    "Role",
                    loginResponse.Role);

                TempData["SuccessMessage"] =
                    "You have logged in successfully.";

                return RedirectToAction(
                    "Index",
                    "Home");
            }
            catch (HttpRequestException)
            {
                ModelState.AddModelError(
                    string.Empty,
                    "The API is currently unavailable. Please try again.");

                return View(model);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Logout()
        {
            HttpContext.Session.Clear();

            return RedirectToAction(
                nameof(Login));
        }
    }
}