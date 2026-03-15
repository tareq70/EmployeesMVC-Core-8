using EmployeesMVC_Core_8.Hangfire;
using EmployeesMVC_Core_8.Hubs;
using SharedModels.Models;
using EmployeesMVC_Core_8.Services.Email;
using EmployeesMVC_Core_8.Services.Firebase_Notifications;
using EmployeesMVC_Core_8.Services.WhatsApp;
using FirebaseAdmin;
using Google.Apis.Auth.OAuth2;
using Hangfire;
using Hangfire.MemoryStorage;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));


builder.Services.AddScoped<IEmailHelper, EmailHelper>();
builder.Services.AddScoped<IFirebaseService, FirebaseService>();
builder.Services.AddHttpClient<IWhatsAppService, WhatsAppService>();

//solve error with camelCase in JSON responses
builder.Services.AddControllersWithViews()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.PropertyNamingPolicy = null; 
    });

builder.Services.AddSignalR()
    .AddJsonProtocol(options =>
    {
        options.PayloadSerializerOptions.PropertyNamingPolicy = null; 
    });

builder.Services.AddHangfire(config =>
    config.SetDataCompatibilityLevel(CompatibilityLevel.Version_170)
          .UseSimpleAssemblyNameTypeSerializer()
          .UseRecommendedSerializerSettings()
          .UseMemoryStorage() 
);

FirebaseApp.Create(new AppOptions()
{
    Credential = GoogleCredential.FromFile("wwwroot/firebase/service-account.json")
});

builder.Services.AddHangfireServer();

builder.Services.AddSignalR();

var app = builder.Build();
app.UseHangfireDashboard();

RecurringJob.AddOrUpdate<EmployeeJobs>(
    job => job.CheckBlockedEmployees(),
"*/1 * * * *");

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.MapHub<EmployeeHub>("/employeeHub");
app.MapHub<DepartmentHub>("/departmentHub");


app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
