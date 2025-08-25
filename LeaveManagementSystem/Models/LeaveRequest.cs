using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace LeaveManagementSystem.Models
{
    public class LeaveRequest
    {
        public int Id { get; set; }

        [Required]
        public string EmployeeId { get; set; }

        [Required]
        public DateTime StartDate { get; set; }

        [Required]
        public DateTime EndDate { get; set; }

        [Required]
        public LeaveType LeaveType { get; set; }

        [StringLength(500)]
        public string Reason { get; set; }
        [Required]
        public string ApproverId { get; set; }

        public LeaveStatus Status { get; set; } = LeaveStatus.Pending;

        public string? ApprovedBy { get; set; }

        public DateTime? ApprovalDate { get; set; }

        [StringLength(500)]
        public string? Comments { get; set; }

        public DateTime RequestDate { get; set; } = DateTime.Now;

        public int LeaveDays { get; set; }

        public virtual Employee Employee { get; set; }

        public virtual Employee? Approver { get; set; }
    }

    public enum LeaveType
    {
        Annual,
        Sick,
        Maternity,
        Paternity,
        Emergency,
        Personal
    }

    public enum LeaveStatus
    {
        Pending,
        Approved,
        Rejected,
        Cancelled
    }
}
