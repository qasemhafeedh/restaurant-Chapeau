using restaurant_Chapeau.Repositaries;
using restaurant_Chapeau.Repositories;
using restaurant_Chapeau.Services;
using restaurant_Chapeau.Services.Interfaces;

var builder = WebApplication.CreateBuilder(args);

// Read connection string from configuration
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

builder.Services.AddControllersWithViews();

// Session
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

// Repositories and Services
builder.Services.AddScoped<IOrderRepository, OrderRepository>();
builder.Services.AddScoped<IOrderService, OrderService>();
builder.Services.AddScoped<IKitchenBarService, KitchenBarService>();


builder.Services.AddScoped<ITableRepository, TableRepository>();
builder.Services.AddScoped<ITableService, TableService>();

builder.Services.AddScoped<IMenuItemRepository, MenuItemRepository>();
builder.Services.AddScoped<IMenuItemService, MenuItemService>();


builder.Services.AddScoped<IMenuManagementRepository, MenuManagementRepository>();
builder.Services.AddScoped<IMenuManagementService, MenuManagementService>();

builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IUserService, UserService>();

builder.Services.AddScoped<IInvoiceRepository, InvoiceRepository>();
builder.Services.AddScoped<IInvoiceService, InvoiceService>();

// ✅ Auth dependencies
builder.Services.AddScoped<IAuthRepository, AuthRepository>();
builder.Services.AddScoped<IAuthService, AuthService>();

// ✅ Register PaymentRepository with connection string
builder.Services.AddScoped<IPaymentRepository>(provider =>
    new PaymentRepository(connectionString));
builder.Services.AddScoped<IPaymentService, PaymentService>();

var app = builder.Build();

// Error handling
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

// Middleware pipeline
app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseSession();
app.UseAuthentication(); // Optional for future use
app.UseAuthorization();

// Default route
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Auth}/{action=Login}/{id?}");

app.Run();
