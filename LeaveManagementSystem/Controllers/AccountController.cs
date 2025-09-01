using LeaveManagementSystem.Models;
using LeaveManagementSystem.Repository.Interface;
using LeaveManagementSystem.ViewModels;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.EntityFrameworkCore;
using System.Text;


namespace LeaveManagementSystem.Controllers
{
    public class AccountController : Controller
    {
        private readonly UserManager<Employee> _userManager;
        private readonly SignInManager<Employee> _signInManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private IEmailSender _emailSender;

        public AccountController(
            UserManager<Employee> userManager,
            SignInManager<Employee> signInManager,
            RoleManager<IdentityRole> roleManager,
            IEmailSender emailSender)

        {
            _userManager = userManager;
            _signInManager = signInManager;
            _roleManager = roleManager;
            _emailSender = emailSender;
        }

        [HttpGet]
        public IActionResult Login()
        {
            // new line add.
            if (User.Identity != null && User.Identity.IsAuthenticated)
            {
                // User is already logged in, redirect to Home
                return RedirectToAction("Index", "Home");
            }
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (ModelState.IsValid)
            {
                var result = await _signInManager.PasswordSignInAsync(
                    model.Email, model.Password, model.RememberMe, false);

                if (result.Succeeded)
                {
                    return RedirectToAction("Index", "Dashboard");
                }

                ModelState.AddModelError(string.Empty, "Invalid login attempt.");
            }

            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> Register()
        {
            ViewBag.Roles = new SelectList(await _roleManager.Roles.ToListAsync(), "Name", "Name");

            var managers = await _userManager.GetUsersInRoleAsync("Manager");
            var model = new RegisterViewModel()
            {
                ForManager = managers
                            .Select(u => new SelectListItem
                            {
                                Value = u.Id,
                                Text = $"{u.FirstName} {u.LastName} - ({u.Email})"
                            }).ToList()
            };

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = new Employee
                {
                    UserName = model.Email,
                    Email = model.Email,
                    FirstName = model.FirstName,
                    LastName = model.LastName,
                    EmployeeId = model.EmployeeId,
                    ManagerId = model.ManagerId,
                    DateOfJoining = DateTime.Now,
                    EmailConfirmed = true
                };

                var result = await _userManager.CreateAsync(user, model.Password);

                if (result.Succeeded)
                {
                    
                    await _userManager.AddToRoleAsync(user, model.Role);
                    
                    await _signInManager.SignInAsync(user, isPersistent: false);
                    string Subject = "Account Created";
                    string body = $"Congratulation Your user name is {user}, Your account has been successfully created." + $"Your password is {model.Password}.";
                    var isEmailSent = await _emailSender.EmailSendAsync(model.Email,Subject, body);
                    TempData["SuccessMessage"] = "User Registered Successfully";
                    return RedirectToAction("Index", "Dashboard");
                }

                

                //foreach (var error in result.Errors)
                //{
                //    ModelState.AddModelError(string.Empty, error.Description);
                //}
            }

            var managers = await _userManager.GetUsersInRoleAsync("Manager");
            model.ForManager = managers
                            .Select(u => new SelectListItem
                            {
                                Value = u.Id,
                                Text = $"{u.FirstName} {u.LastName} - ({u.Email})"
                            }).ToList();
            ViewBag.Roles = new SelectList(await _roleManager.Roles.ToListAsync(), "Name", "Name");
            return View(model);
        }

        public IActionResult VerifyEmail()
        {
            return View();
        }
        [HttpPost]
        public async Task<IActionResult> VerifyEmail(VerifyEmailViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = await _userManager.FindByNameAsync(model.Email);

                if (user == null)
                {
                    ModelState.AddModelError("", "Something is wrong!");
                    return View(model);
                }
                else
                {
                    return RedirectToAction("ChangePassword", "Account", new { username = user.UserName });

                }
            }
            return View(model);

        }
        [HttpGet]
        public IActionResult ForgotPassword()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> ForgotPassword(ForgotPasswordViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = await _userManager.FindByEmailAsync(model.Email);
                if (user == null)
                {                    
                    return RedirectToAction("ForgotPasswordConfirmation");
                }
                var token = await _userManager.GeneratePasswordResetTokenAsync(user);
                var callbackUrl = Url.Action("ResetPassword", "Account",
                    new { token, email = model.Email }, Request.Scheme);

                await _emailSender.EmailSendAsync(model.Email, "Reset Password",
               $"Please reset your password by clicking <a href='{callbackUrl}'>here</a>.");

               return RedirectToAction("ForgotPasswordConfirmation");
            }
            return View(model);
        }
        [HttpGet]
        public IActionResult ForgotPasswordConfirmation()
        {
            return View();
        }

        [HttpGet]
        public IActionResult ResetPassword(string token = null, string email = null)
        {
            if (token == null || email == null)
            {
                ModelState.AddModelError(string.Empty, "Invalid password reset token.");
                return View("Error");
            }

            var model = new ResetPasswordViewModel
            {
                Token = token,
                Email = email
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ResetPassword(ResetPasswordViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var user = await _userManager.FindByEmailAsync(model.Email);
            if (user == null)
            {
                // Don't reveal that the user does not exist
                return RedirectToAction(nameof(ForgotPasswordConfirmation));
            }

            // Decode the token
            var decodedToken = Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(model.Token));
            decodedToken = model.Token;
            // Reset the password
            var result = await _userManager.ResetPasswordAsync(user, decodedToken, model.NewPassword);

            if (result.Succeeded)
            {
                TempData["SuccessMessage"] = "Reset Password updated successfully!";
                //_logger.LogInformation($"Password reset successful for user {model.Email}");
                return RedirectToAction("Login", "Account");
            }
            
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            return RedirectToAction("Login");
        }
    }
}

