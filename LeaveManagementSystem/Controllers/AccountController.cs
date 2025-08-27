using LeaveManagementSystem.Models;
using LeaveManagementSystem.Repository.Interface;
using LeaveManagementSystem.ViewModels;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

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

        [HttpPost]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            return RedirectToAction("Login");
        }
    }

    public class LoginViewModel
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; }

        [Required]
        [DataType(DataType.Password)]
        public string Password { get; set; }

        public bool RememberMe { get; set; }
    }
}

