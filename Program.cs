using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using NetMvcApp.Services; // Ensure this namespace is correct for your services
using System;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

// Register custom services
builder.Services.AddSingleton<FileStorageService>(); // Or Scoped/Transient as appropriate
builder.Services.AddSingleton<EncryptionService>();

// Configure session services
builder.Services.AddDistributedMemoryCache(); // Required for session state
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30); // Set session timeout
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true; // Make the session cookie essential
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error"); // A generic error handler page
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseSession(); // IMPORTANT: Call UseSession before UseAuthorization and MapControllerRoute

app.UseAuthorization(); // If you add authorization later

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Account}/{action=Login}/{id?}"); // Default to Login page

// Ensure data directory exists
string dataPath = System.IO.Path.Combine(app.Environment.ContentRootPath, "Data");
if (!System.IO.Directory.Exists(dataPath))
{
    System.IO.Directory.CreateDirectory(dataPath);
}

app.Run();

