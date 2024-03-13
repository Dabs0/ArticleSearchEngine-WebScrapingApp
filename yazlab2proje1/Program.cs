using Business.Abstract;
using Business.Concrete;
using Core.Repository.Abstract;
using Core.Settings;
using DataAccess.Abstarct;
using DataAccess.Concrete;
using DataAccess.Repository;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

// Configure MongoDB settings
builder.Services.Configure<MongoSettings>(options =>
{
    options.ConnectionString = builder.Configuration.GetSection("MongoConnection:ConnectionString").Value;
    options.Database = builder.Configuration.GetSection("MongoConnection:Database").Value;
});

builder.Services.AddScoped(typeof(IRepository<>), typeof(MongoRepositoryBase<>));
builder.Services.AddScoped<IAkademikYayinDataAccess, AkademikYayinDataAccess>();
builder.Services.AddScoped<IAkademikYayinService, AkademikYayinManager>();

var app = builder.Build(); // Hizmetleri ekledikten sonra Build() yöntemini çaðýrýn.

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

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
