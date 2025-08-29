using LeaveManagementSystem.Data;
using LeaveManagementSystem.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LeaveManagementSystem.Controllers
{
    [Authorize]
    public class DashboardController : Controller
    {
        private readonly UserManager<Employee> _userManager;
        private readonly ApplicationDbContext _context;

        public DashboardController(UserManager<Employee> userManager, ApplicationDbContext context)
        {
            _userManager = userManager;
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var user = await _userManager.GetUserAsync(User);
            var roles = await _userManager.GetRolesAsync(user);

            ViewBag.UserRole = roles.FirstOrDefault();
            ViewBag.UserName = user.FullName;

            // Dashboard statistics
            var totalEmployees = await _context.Users.CountAsync();
            var pendingRequests = await _context.LeaveRequests
                .Where(lr => lr.Status == LeaveStatus.Pending)
                .CountAsync();

            var myRequests = await _context.LeaveRequests
                .Where(lr => lr.EmployeeId == user.Id)
                .CountAsync();

            var pendingApprovals = await _context.LeaveRequests
                .Where(lr => lr.ApproverId == user.Id  && lr.Status == LeaveStatus.Pending)
                .CountAsync();

            ViewBag.TotalEmployees = totalEmployees;
            ViewBag.PendingRequests = pendingRequests;
            ViewBag.MyRequests = myRequests;
            ViewBag.PendingApprovals = pendingApprovals;

            return View();
        }
    }
}
