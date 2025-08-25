using LeaveManagementSystem.Data;
using LeaveManagementSystem.Models;
using LeaveManagementSystem.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LeaveManagementSystem.Controllers
{
    [Authorize]
    public class EmployeeController : Controller
    {
        private readonly UserManager<Employee> _userManager;
        private readonly ApplicationDbContext _context;

        public EmployeeController(UserManager<Employee> userManager, ApplicationDbContext context)
        {
            _userManager = userManager;
            _context = context;
        }

        // GET: Employee/Profile
        public async Task<IActionResult> Profile()
        {
            var user = await _userManager.GetUserAsync(User);

            var model = new EmployeeProfileViewModel
            {
                Id = user.Id,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Email = user.Email,
                PhoneNumber = user.PhoneNumber,
                EmployeeId = user.EmployeeId,
                Department = user.Department,
                Designation = user.Designation,
                DateOfJoining = user.DateOfJoining,
                Address = user.Address
            };

            return View(model);
        }

        // POST: Employee/Profile
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Profile(EmployeeProfileViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = await _userManager.GetUserAsync(User);

                user.FirstName = model.FirstName;
                user.LastName = model.LastName;
                user.PhoneNumber = model.PhoneNumber;
                user.Department = model.Department;
                user.Designation = model.Designation;
                user.Address = model.Address;

                var result = await _userManager.UpdateAsync(user);

                if (result.Succeeded)
                {
                    TempData["SuccessMessage"] = "Profile updated successfully!";
                    return RedirectToAction(nameof(Profile));
                }

                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
            }

            return View(model);
        }

        // GET: Employee/List (HR/Admin only)
        [Authorize(Roles = "HR,Admin")]
        public async Task<IActionResult> List()
        {
            var employees = await _userManager.Users
                .Select(e => new EmployeeProfileViewModel
                {
                    Id = e.Id,
                    FirstName = e.FirstName,
                    LastName = e.LastName,
                    Email = e.Email,
                    EmployeeId = e.EmployeeId,
                    Department = e.Department,
                    Designation = e.Designation,
                    DateOfJoining = e.DateOfJoining
                })
                .ToListAsync();

            return View(employees);
        }
    }
}
