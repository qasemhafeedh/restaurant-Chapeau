using restaurant_Chapeau.Repositories;
using restaurant_Chapeau.Repositaries;
using restaurant_Chapeau.Services;
using restaurant_Chapeau.Services.Interfaces;


var builder = WebApplication.CreateBuilder(args);

// ✅ Add MVC support
builder.Services.AddControllersWithViews();

// ✅ Add session support
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

// ✅ Register Repositories and Services
builder.Services.AddScoped<IOrderRepository, OrderRepository>();
builder.Services.AddScoped<IOrderService, OrderService>();

builder.Services.AddScoped<IAuthService, AuthService>(); // ✅ Required for AuthController

builder.Services.AddScoped<IStockRepository, StockRepository>(); // ✅  stock repository.  this connects the interface to the DB logic
builder.Services.AddScoped<IStockService, StockService>(); // ✅  stock service.  t

builder.Services.AddScoped<IMenuRepository, MenuRepository>();
builder.Services.AddScoped<IMenuService, MenuService>();

var app = builder.Build();

// ✅ Error handling
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

// ✅ Middleware setup
app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseSession();         // 👈 Session before auth
app.UseAuthorization();

// ✅ Route config
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Auth}/{action=Login}/{id?}");

app.Run();
