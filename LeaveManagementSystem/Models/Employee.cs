using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace LeaveManagementSystem.Models
{
    public class Employee: IdentityUser
    {
        [Required]
        [StringLength(50)]
        public string FirstName { get; set; }

        [Required]
        [StringLength(50)]
        public string LastName { get; set; }

        [StringLength(20)]
        public string? EmployeeId { get; set; }

        [StringLength(100)]
        public string? Department { get; set; }

        [StringLength(100)]
        public string? Designation { get; set; }

        public DateTime DateOfJoining { get; set; }

        [StringLength(200)]
        public string? Address { get; set; }

        public string? ManagerId { get; set; }

        public virtual Employee Manager { get; set; }

        public virtual ICollection<Employee> Subordinates { get; set; } = new List<Employee>();

        public virtual ICollection<LeaveRequest> LeaveRequests { get; set; } = new List<LeaveRequest>();

        public virtual ICollection<LeaveRequest> ManagedLeaveRequests { get; set; } = new List<LeaveRequest>();

        public string FullName => $"{FirstName} {LastName}";
    }
   
}
    