using LeaveManagementSystem.Data;
using LeaveManagementSystem.Models;
using LeaveManagementSystem.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace LeaveManagementSystem.Controllers
{
    [Authorize]
    public class LeaveController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<Employee> _userManager;

        public LeaveController(ApplicationDbContext context, UserManager<Employee> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // GET: Leave/MyRequests
        public async Task<IActionResult> MyRequests()
        {
            var user = await _userManager.GetUserAsync(User);
            var requests = await _context.LeaveRequests
                .Where(lr => lr.EmployeeId == user.Id)
                .OrderByDescending(lr => lr.RequestDate)
                .Select(lr => new LeaveRequestViewModel
                {
                    Id = lr.Id,
                    StartDate = lr.StartDate,
                    EndDate = lr.EndDate,
                    LeaveType = lr.LeaveType,

                    Reason = lr.Reason,
                    Status = lr.Status,
                    Comments = lr.Comments,
                    RequestDate = lr.RequestDate,
                    LeaveDays = lr.LeaveDays
                })
                .ToListAsync();

            return View(requests);
        }

        // GET: Leave/Create
        //public IActionResult Create()
        //{
        //    return View();
        //}

        // POST: Leave/Create


        public async Task<IActionResult> Create()
        {
            //var model = new LeaveRequestViewModel
            //{
            //    ForApproval = new List<SelectListItem>
            //    {
            //        new SelectListItem { Value = "1", Text = "User1" },
            //        new SelectListItem { Value = "2", Text = "User2" },
            //        new SelectListItem { Value = "3", Text = "User3" }
            //    }
            //};
            var user = await _userManager.GetUserAsync(User);
            var roles = await _userManager.GetRolesAsync(user);


            var model = new LeaveRequestViewModel();

            if (roles.Contains("Manager"))
            {
                // Agar Manager hai, to Admin ko default approver set karna hai
                var admins = await _userManager.GetUsersInRoleAsync("Admin");
                var admin = admins.FirstOrDefault(); // Pehle Admin ko select kar liya

                if (admin != null)
                {
                    model.ApproverId = admin.Id; // Default assign
                    model.ForApproval = new List<SelectListItem> {
                        new SelectListItem {
                            Value = admin.Id,
                            Text = $"{admin.FirstName} {admin.LastName} ({admin.Email})"
                        }
                    };
                }

            }

            else
            {
                var managers = await _userManager.GetUsersInRoleAsync("Manager");
                model.ForApproval = managers
                           .Select(u => new SelectListItem
                           {
                               Value = u.Id,
                               Text = $"{u.FirstName} {u.LastName} - ({u.Email})"
                           }).ToList();
            }

            return View(model);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(LeaveRequestViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = await _userManager.GetUserAsync(User);
                var roles = await _userManager.GetRolesAsync(user);

                if (roles.Contains("Manager"))
                {
                    var admins = await _userManager.GetUsersInRoleAsync("Admin");
                    var admin = admins.FirstOrDefault();
                    if (admin != null)
                    {
                        model.ApproverId = admin.Id;
                    }
                }
                // Calculate leave days
                var leaveDays = CalculateLeaveDays(model.StartDate, model.EndDate);

                string approverId = model.ApproverId;

                var leaveRequest = new LeaveRequest
                {
                    EmployeeId = user.Id,
                    StartDate = model.StartDate,
                    EndDate = model.EndDate,
                    LeaveType = model.LeaveType,
                    ApproverId = model.ApproverId,
                    Reason = model.Reason,
                    LeaveDays = leaveDays,
                    RequestDate = DateTime.Now,
                    Status = LeaveStatus.Pending
                };

                _context.LeaveRequests.Add(leaveRequest);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "Leave request submitted successfully!";
                return RedirectToAction(nameof(MyRequests));
            }
            var managers = await _userManager.GetUsersInRoleAsync("Manager");
            model.ForApproval = managers
                            .Select(u => new SelectListItem
                            {
                                Value = u.Id,
                                Text = $"{u.FirstName} {u.LastName} - ({u.Email})"
                            }).ToList();
            return View(model);
        }

        // GET: Leave/PendingRequests (Manager/HR)
        [Authorize(Roles = "Manager,HR,Admin")]
        public async Task<IActionResult> PendingRequests()
        {
            var user = await _userManager.GetUserAsync(User);
            var roles = await _userManager.GetRolesAsync(user);

            IQueryable<LeaveRequest> query = _context.LeaveRequests
                .Include(lr => lr.Employee)
                .Where(lr => lr.Status == LeaveStatus.Pending);

            // Managers see only their subordinates' requests
            if (roles.Contains("Manager") && !roles.Contains("HR") && !roles.Contains("Admin"))
            {
                //query = query.Where(lr => lr.Employee.ManagerId == user.Id);
                query = query.Where(lr => lr.ApproverId == user.Id && lr.EmployeeId != user.Id);
            }

            var requests = await query
                .OrderByDescending(lr => lr.RequestDate)
                .Select(lr => new LeaveRequestViewModel
                {
                    Id = lr.Id,
                    StartDate = lr.StartDate,
                    EndDate = lr.EndDate,
                    LeaveType = lr.LeaveType,

                    Reason = lr.Reason,
                    Status = lr.Status,
                    EmployeeName = lr.Employee.FullName,
                    RequestDate = lr.RequestDate,
                    LeaveDays = lr.LeaveDays
                })
                .ToListAsync();

            return View(requests);
        }

        // POST: Leave/Approve
        [HttpPost]
        [Authorize(Roles = "Manager,HR,Admin")]
        public async Task<IActionResult> Approve(int id, string comments = "")
        {
            //var leaveRequest = await _context.LeaveRequests.FindAsync(id);
            var leaveRequest = await _context.LeaveRequests
                .Include(lr => lr.Employee)
                .FirstOrDefaultAsync(lr => lr.Id == id);
            if (leaveRequest != null)
            {
                var user = await _userManager.GetUserAsync(User);

                var roles = await _userManager.GetRolesAsync(user);
                var applicantRoles = await _userManager.GetRolesAsync(leaveRequest.Employee);

                if (applicantRoles.Contains("Manager") && roles.Contains("Manager") && !roles.Contains("Admin") && !roles.Contains("HR"))
                {
                    TempData["ErrorMessage"] = "Manager leave request can only be rejected by Admin or HR.";
                    return RedirectToAction(nameof(PendingRequests));
                }

                leaveRequest.Status = LeaveStatus.Approved;
                leaveRequest.ApprovedBy = user.Id;
                leaveRequest.ApprovalDate = DateTime.Now;
                leaveRequest.Comments = comments;

                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Leave request approved successfully!";
            }

            return RedirectToAction(nameof(PendingRequests));
        }

        // POST: Leave/Reject
        [HttpPost]
        [Authorize(Roles = "Manager,HR,Admin")]
        public async Task<IActionResult> Reject(int id, string comments = "")
        {
            //var leaveRequest = await _context.LeaveRequests.FindAsync(id);

            var leaveRequest = await _context.LeaveRequests
                .Include(lr => lr.Employee)
                .FirstOrDefaultAsync(lr => lr.Id == id);

            if (leaveRequest != null)
            {
                var user = await _userManager.GetUserAsync(User);

                var roles = await _userManager.GetRolesAsync(user);

                var applicantRoles = await _userManager.GetRolesAsync(leaveRequest.Employee);
                if (applicantRoles.Contains("Manager") && roles.Contains("Manager") && !roles.Contains("Admin") && !roles.Contains("HR"))
                {
                    TempData["ErrorMessage"] = "Manager leave request can only be rejected by Admin or HR.";
                    return RedirectToAction(nameof(PendingRequests));
                }

                leaveRequest.Status = LeaveStatus.Rejected;
                leaveRequest.ApprovedBy = user.Id;
                leaveRequest.ApprovalDate = DateTime.Now;
                leaveRequest.Comments = comments;

                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Leave request rejected.";
            }

            return RedirectToAction(nameof(PendingRequests));
        }

        private int CalculateLeaveDays(DateTime startDate, DateTime endDate)
        {
            int days = 0;
            for (var date = startDate; date <= endDate; date = date.AddDays(1))
            {
                if (date.DayOfWeek != DayOfWeek.Saturday && date.DayOfWeek != DayOfWeek.Sunday)
                {
                    days++;
                }
            }
            return days;
        }
    }
}
