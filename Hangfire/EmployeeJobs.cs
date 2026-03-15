using EmployeesMVC_Core_8.Enum;
using EmployeesMVC_Core_8.Hubs;
using SharedModels.Models;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;

namespace EmployeesMVC_Core_8.Hangfire
{
    public class EmployeeJobs
    {
        private readonly AppDbContext _context;
        private readonly IHubContext<EmployeeHub> _hub;

        public EmployeeJobs(AppDbContext context, IHubContext<EmployeeHub> hub)
        {
            _context = context;
            _hub = hub;
        }

        public async Task CheckBlockedEmployees()
        {
            var blockedEmployees = await _context.Employees
                .Where(e => e.Status == EmployeeStatus.Blocked)
                .ToListAsync();

            if (blockedEmployees.Count == 0) return;

            blockedEmployees.ForEach(emp => emp.Status = EmployeeStatus.Active);

            await _context.SaveChangesAsync();
            var updatedEmployees = blockedEmployees.Select(emp => new
            {
                emp.EmployeeId,
                emp.Name,
                emp.Email,
                DepartmentName = _context.Departments
                           .Where(d => d.DepartmentId == emp.DepartmentId)
                           .Select(d => d.Name)
                           .FirstOrDefault(),
                emp.Status,
                emp.Latitude,
                emp.Longitude
            }).ToList();

            await _hub.Clients.All.SendAsync("EmployeeStatusUpdated", updatedEmployees);
        }
    }
}