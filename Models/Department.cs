using System.ComponentModel.DataAnnotations;

namespace EmployeesMVC_Core_8.Models
{
    public class Department
    {
        [Key]
        public int DepartmentId { get; set; }

        [Required]
        [StringLength(100)]
        public string Name { get; set; } = string.Empty;

        public virtual ICollection<Employee> Employees { get; set; }
    }
}
