using Amazon.S3;
using Hackathon_2025_ESG.Areas.Identity.Data;
using Hackathon_2025_ESG.Data;
using Hackathon_2025_ESG.Services;
using Hackathon_2025_ESG.Services.Interface;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
var builder = WebApplication.CreateBuilder(args);
var connectionString = builder.Configuration.GetConnectionString("Hackathon_2025_ESGContextConnection") ?? throw new InvalidOperationException("Connection string 'Hackathon_2025_ESGContextConnection' not found.");;

builder.Services.AddDbContext<Hackathon_2025_ESGContext>(options => options.UseSqlServer(connectionString));

builder.Services.AddDefaultIdentity<Hackathon_2025_ESGUser>(options => options.SignIn.RequireConfirmedAccount = true).AddEntityFrameworkStores<Hackathon_2025_ESGContext>();

// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddRazorPages();

builder.Services.AddHttpContextAccessor();

// Register the IAmazonS3 client
builder.Services.AddAWSService<IAmazonS3>();

// Register custom uploader service
builder.Services.AddScoped<IAwsS3UploaderService, AwsS3UploaderService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
    name: "areas",
    pattern: "{area:exists}/{controller=Home}/{action=Index}/{id?}");

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();

app.MapRazorPages();

app.Run();
