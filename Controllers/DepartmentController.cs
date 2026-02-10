using EmployeesMVC_Core_8.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EmployeesMVC_Core_8.Controllers
{
    public class DepartmentController : Controller
    {
        private readonly AppDbContext _context;
        public DepartmentController(AppDbContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            return View();
        }

        // DataTables AJAX
        [HttpGet]
        public async Task<IActionResult> GetDepartments()
        {
            var department = await _context.Departments
                .Select(d => new {
                    d.DepartmentId, d.Name })
                .ToListAsync();

            return Json(new { data = department });
        }

        [HttpPost]
        public async Task<IActionResult> AddDepartment(string name)
        {
            if (string.IsNullOrEmpty(name))
                return Json(new { success = false, message = "Name is required" });

            _context.Departments.Add(new Department { Name = name });
            await _context.SaveChangesAsync();
            return Json(new { success = true });
        }

        [HttpPost]
        public async Task<IActionResult> EditDepartment(int id, string name)
        {
            var dept = await _context.Departments.FindAsync(id);
            if (dept == null) return Json(new { success = false, message = "Not found" });

            dept.Name = name;
            await _context.SaveChangesAsync();
            return Json(new { success = true });
        }

        [HttpPost]
        public async Task<IActionResult> DeleteDepartment(int id)
        {
            var dept = await _context.Departments.FindAsync(id);
            if (dept == null) return Json(new { success = false, message = "Not found" });

            _context.Departments.Remove(dept);
            await _context.SaveChangesAsync();
            return Json(new { success = true });
        }
    }
}
