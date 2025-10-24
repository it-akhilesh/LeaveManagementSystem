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
            var dashboardViewModel = new DashboardViewModel();
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

            int pendingApprovals;

            if (roles.FirstOrDefault() == "Admin")
            {
                
                pendingApprovals = await _context.LeaveRequests
                    .Where(lr => lr.Status == LeaveStatus.Pending)
                    .CountAsync();
            }
            else
            {
                
                pendingApprovals = await _context.LeaveRequests
                    .Where(lr => lr.ApproverId == user.Id && lr.Status == LeaveStatus.Pending)
                    .CountAsync();
            }

            if (roles.FirstOrDefault() == "Admin" )
            {

                var oneWeekAgo = DateTime.Now.AddDays(-7);
                var activityInfo = await (from l in _context.LeaveRequests
                                          join u in _context.Users on l.EmployeeId equals u.Id
                                          join ur in _context.UserRoles on u.Id equals ur.UserId
                                          join r in _context.Roles on ur.RoleId equals r.Id
                                          where l.RequestDate >= oneWeekAgo
                                         && (r.Name == "Manager" || r.Name == "Employee")
                                          orderby l.RequestDate,l.Id ascending
                                          select new LeaveCreationActivity
                                          {
                                              Id = l.Id,
                                              Name = l.Employee.FirstName + " " + l.Employee.LastName,
                                              EmailId = l.Employee.Email,
                                              Designation = r.Name,   // from AspNetRoles
                                              Date = l.RequestDate,
                                              Status = l.Status
                                          })
                                  .ToListAsync();
                dashboardViewModel.leaveCreationActivities = activityInfo;

            }
            
            

            ViewBag.TotalEmployees = totalEmployees;
            ViewBag.PendingRequests = pendingRequests;
            ViewBag.MyRequests = myRequests;
            ViewBag.PendingApprovals = pendingApprovals;
            
            
            return View(dashboardViewModel);
        }
    }
}
