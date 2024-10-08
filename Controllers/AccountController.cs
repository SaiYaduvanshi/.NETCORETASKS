using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using UserProfileApp.Models;

namespace UserProfileApp.Controllers
{
    /// <summary>
    /// Controller responsible for managing user account-related actions such as registration, login, and logout.
    /// </summary>
    public class AccountController : Controller
    {
        private readonly SignInManager<IdentityUser> _signInManager;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly IEmailSender _emailSender;
        /// <summary>
        /// Initializes a new instance of the <see cref="AccountController"/> class with injected SignInManager and UserManager.
        /// </summary>
        /// <param name="signInManager">The SignInManager service for handling user sign-in operations.</param>
        /// <param name="userManager">The UserManager service for managing users.</param>
        public AccountController(SignInManager<IdentityUser> signInManager, UserManager<IdentityUser> userManager, IEmailSender emailSender)
        {
            _signInManager = signInManager;
            _userManager = userManager;
            _emailSender = emailSender;
        }

        /// <summary>
        /// Displays the registration page.
        /// </summary>
        /// <returns>The registration view.</returns>
        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }

        /// <summary>
        /// Handles the registration of a new user.
        /// </summary>
        /// <param name="model">The view model containing registration information.</param>
        /// <returns>The result of the registration process, redirecting to the profile page on success.</returns>
        [HttpPost]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = new IdentityUser { UserName = model.Username, Email = model.Email };
                var result = await _userManager.CreateAsync(user, model.Password);

                if (result.Succeeded)
                {
                    await _signInManager.SignInAsync(user, isPersistent: false);
                    return RedirectToAction("Index", "Profile");
                }

                // Add errors to ModelState for display.
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
            }

            return View(model);
        }

        /// <summary>
        /// Displays the login page.
        /// </summary>
        /// <returns>The login view.</returns>
        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        /// <summary>
        /// Handles user login requests.
        /// </summary>
        /// <param name="model">The view model containing login information.</param>
        /// <param name="returnUrl">The URL to redirect to after successful login.</param>
        /// <returns>The result of the login process, redirecting on success or returning errors on failure.</returns>
        [HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> Login(LoginViewModel model, string returnUrl = null)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var result = await _signInManager.PasswordSignInAsync(model.Username, model.Password, model.RememberMe, lockoutOnFailure: true);

            if (result.Succeeded)
            {
                return RedirectToLocal(returnUrl);
            }
            else if (result.IsLockedOut)
            {
                TempData["ErrorMessage"] = "Your account is locked out.";
                return View(model);
            }
            else
            {
                TempData["ErrorMessage"] = "Invalid login attempt.";
                return View(model);
            }
        }

        /// <summary>
        /// Logs out the currently logged-in user.
        /// </summary>
        /// <returns>Redirects to the login page after successful logout.</returns>
        [HttpPost]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            return RedirectToAction("Login", "Account");
        }

        [HttpGet]
        public IActionResult ForgotPassword()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> ForgotPassword(ForgotPasswordViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var user = await _userManager.FindByEmailAsync(model.Email);
            if (user == null)
            {
                // Don't reveal that the user does not exist
                return RedirectToAction(nameof(ForgotPasswordConfirmation));
            }

            var token = await _userManager.GeneratePasswordResetTokenAsync(user);
            var callbackUrl = Url.Action(nameof(ResetPassword), "Account", new { token, email = model.Email }, Request.Scheme);

            //comment below line if nos smtp configured.test like below url

            // callbackUrl-https://localhost:44323/Account/ResetPassword?token=CfDJ8GE8x%2BCIOUpFnlYgOXWNCII%2Bncs19ss4sMcjDXDKX%2Fmv%2Bb3Ax1zRMTCsd0NPnsv%2FAT%2Bt24MwGFKftNHq5eUnlNLdGVMmIS9tdLkS1nNG%2FmF4vifLwe12G6Qv2QngCbZGN%2BIe6PxSkSfkL4l5bjvfRVmiyS0IYJqmJa%2B0jc7o2bRbfTo%2FcAuAYXmLx0OgjeXnqC56sbQvenxtix%2Fl1pJBTsosU03YYE1yZG50Sz1369Af&email=sai.yaduvanshi@yahoo.com





            // Send email
            await _emailSender.SendEmailAsync(
                model.Email,
                "Reset Password",
                $"Please reset your password by <a href='{callbackUrl}'>clicking here</a>.");

            return RedirectToAction(nameof(ForgotPasswordConfirmation));
        }

        [HttpGet]
        public IActionResult ForgotPasswordConfirmation()
        {
            return View();
        }

        [HttpGet]
        public IActionResult ResetPassword(string token, string email)
        {
            if (token == null || email == null)
            {
                return BadRequest("A token must be supplied for password reset.");
            }

            var model = new ResetPasswordViewModel { Token = token, Email = email };
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> ResetPassword(ResetPasswordViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var user = await _userManager.FindByEmailAsync(model.Email);
            if (user == null)
            {
                // Don't reveal that the user does not exist
                return RedirectToAction(nameof(ResetPasswordConfirmation));
            }

            var result = await _userManager.ResetPasswordAsync(user, model.Token, model.Password);
            if (result.Succeeded)
            {
                return RedirectToAction(nameof(ResetPasswordConfirmation));
            }

            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }

            return View();
        }

        [HttpGet]
        public IActionResult ResetPasswordConfirmation()
        {
            return View();
        }

        /// <summary>
        /// Redirects to a local URL if the URL is valid, or to the profile index page.
        /// </summary>
        /// <param name="returnUrl">The URL to redirect to after login.</param>
        /// <returns>Redirects to the specified URL or the profile index page.</returns>
        private IActionResult RedirectToLocal(string returnUrl)
        {
            if (Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }
            else
            {
                return RedirectToAction(nameof(ProfileController.Index), "Profile");
            }
        }
    }
}
