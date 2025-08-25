using LeaveManagementSystem.Models;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace LeaveManagementSystem.ViewModels
{
    public class LeaveRequestViewModel
    {
        public int Id { get; set; }

        [Required]
        [DataType(DataType.Date)]
        public DateTime StartDate { get; set; }

        [Required]
        [DataType(DataType.Date)]
        public DateTime EndDate { get; set; }

        [Required]
        public LeaveType LeaveType { get; set; }

        [Required]
        [StringLength(500)]
        public string Reason { get; set; }

        public string ApproverId { get; set; }
        public List<SelectListItem>? ForApproval { get; set; }

        public LeaveStatus Status { get; set; }

        public string? Comments { get; set; }

        public string? EmployeeName { get; set; }

        public DateTime RequestDate { get; set; }

        public int LeaveDays { get; set; }
    }
}
