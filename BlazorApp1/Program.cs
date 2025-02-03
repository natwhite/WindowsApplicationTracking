using BlazorApp1;
using BlazorApp1.Components;
using BlazorApp1.Controllers;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Connection string from appsettings.json or secrets
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
Console.WriteLine($"Connection string: {connectionString}");

// Add services to the container.
// builder.Services.AddRazorComponents()
// .AddInteractiveServerComponents();

// var app = builder.Build();

// Configure the HTTP request pipeline.
// if (!app.Environment.IsDevelopment())
// {
//     app.UseExceptionHandler("/Error", createScopeForErrors: true);
//     // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
//     app.UseHsts();
// }

// app.UseHttpsRedirection();
//
//
// app.UseAntiforgery();
//
// app.MapStaticAssets();
// app.MapRazorComponents<App>()
//     .AddInteractiveServerRenderMode();
//
// app.Run();


// Add EF Core
builder.Services.AddDbContext<TimeTrackingDbContext>(options =>
    options.UseSqlServer(connectionString));

builder.Services.AddControllers(); // Add Controllers

// Add Blazor services
// builder.Services.AddRazorPages();
// builder.Services.AddServerSideBlazor();

// Build
var app = builder.Build();

app.MapControllers();
// app.MapBlazorHub();
// app.MapFallbackToPage("/_Host");

app.Run();