using System.ComponentModel.DataAnnotations;

namespace LeaveManagementSystem.ViewModels
{
    public class EmployeeProfileViewModel
    {
        public string Id { get; set; }

        [Required]
        [StringLength(50)]
        public string FirstName { get; set; }

        [Required]
        [StringLength(50)]
        public string LastName { get; set; }

        [Required]
        [EmailAddress]
        public string Email { get; set; }

        [Phone]
        public string PhoneNumber { get; set; }

        [StringLength(20)]
        public string EmployeeId { get; set; }

        [StringLength(100)]
        public string Department { get; set; }

        [StringLength(100)]
        public string Designation { get; set; }

        [DataType(DataType.Date)]
        public DateTime DateOfJoining { get; set; }

        [StringLength(200)]
        public string Address { get; set; }

        public int CurrentPage { get; set; }
        public int TotalPages { get; set; }

        public IFormFile? ProfilePicture { get; set; }
        public string? ProfileImagePath { get; set; }
    }
}
