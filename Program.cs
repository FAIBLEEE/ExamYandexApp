using Microsoft.EntityFrameworkCore;
using ExamYandexApp.Data;
using ExamYandexApp.Services;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddSingleton<IObjectStorageService, YandexObjectStorageService>();
// Add services to the container.
builder.Services.AddControllersWithViews();

// Configure Entity Framework - PostgreSQL
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
if (string.IsNullOrEmpty(connectionString))
{
    // Для контейнера используем переменные окружения
    var host = builder.Configuration["DB_HOST"] ?? "rc1b-tptaie72mntboqdv.mdb.yandexcloud.net";
    var port = builder.Configuration["DB_PORT"] ?? "6432";
    var database = builder.Configuration["DB_NAME"] ?? "examyandexapp";
    var username = builder.Configuration["DB_USER"] ?? "user";
    var dbPassword = builder.Configuration["DB_PASSWORD"] ?? "bars1995";
    
    connectionString = $"Host={host};Port={port};Database={database};Username={username};Password={dbPassword};Ssl Mode=VerifyFull;Trust Server Certificate=true";
}

// Используем PostgreSQL
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(connectionString));

// Configure Object Storage Service
builder.Services.AddSingleton<IObjectStorageService, YandexObjectStorageService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

// Apply database creation on startup
try
{
    using var scope = app.Services.CreateScope();
    var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    
    // Простая проверка подключения и создание таблиц
    var canConnect = await dbContext.Database.CanConnectAsync();
    Console.WriteLine($"Database can connect: {canConnect}");
    
    if (canConnect)
    {
        await dbContext.Database.EnsureCreatedAsync();
        Console.WriteLine("Database tables created successfully.");
    }
}
catch (Exception ex)
{
    Console.WriteLine($"Database error: {ex.Message}");
}

try
{
    using var scope = app.Services.CreateScope();
    var storageService = scope.ServiceProvider.GetRequiredService<IObjectStorageService>();
    Console.WriteLine("Object Storage service initialized successfully.");
}
catch (Exception ex)
{
    Console.WriteLine($"Object Storage configuration error: {ex.Message}");
}

app.Run();