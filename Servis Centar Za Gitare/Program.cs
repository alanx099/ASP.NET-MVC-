using Servis_Centar_Za_Gitare.Data.Interfaces;
using Servis_Centar_Za_Gitare.Data.Repositories;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews();
builder.Services.AddSingleton<ICustomerRepository, CustomerRepository>();
builder.Services.AddSingleton<IGuitarRepository, GuitarRepository>();
builder.Services.AddSingleton<IRepairRepository, RepairRepository>();
builder.Services.AddSingleton<ITechnicianRepository, TechnicianRepository>();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
