using EmployeesMVC_Core_8.Enum;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EmployeesMVC_Core_8.Models
{
    public class Employee
    {
        [Key]
        public int EmployeeId { get; set; }

        [Required]
        [StringLength(100)]
        public string Name { get; set; } = string.Empty;

        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        public int DepartmentId { get; set; }

        [ForeignKey("DepartmentId")]
        public virtual Department Department { get; set; }
        public EmployeeStatus Status { get; set; } = EmployeeStatus.Active;
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        [Required]
        [StringLength(15)]
        public string PhoneNumber { get; set; } = string.Empty;

    }
}
