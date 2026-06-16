using Lesson3_CNLTWeb.Middleware;
using Lesson3_CNLTWeb.Data;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

var app = builder.Build();

// Initialize BookRepository with connection strings from configuration
var masterConn = builder.Configuration.GetConnectionString("MasterConnection");
var defaultConn = builder.Configuration.GetConnectionString("DefaultConnection");
BookRepository.Initialize(masterConn, defaultConn);

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRouting();

app.UseAuthorization();

app.MapStaticAssets();



app.UseMiddleware<RequestLoggingMiddleware>();

//app.UseMiddleware<CheckDBMiddleware>();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();


app.Run();
