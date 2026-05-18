using Microsoft.EntityFrameworkCore;
using QLDH.Authorization;
using QLDH.Data;
using QLDH.Models;
using QLDH.Services;
using QLDH.Services.Encryption;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

// Session
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

// Đọc kiểu CSDL từ appsettings.json
var dbType = builder.Configuration["DatabaseType"];

// Tự động chọn database
if (dbType == "Oracle")
{
    builder.Services.AddDbContext<ApplicationDbContext>(options =>
        options.UseOracle(builder.Configuration.GetConnectionString("OracleDb")));
    Console.WriteLine("✅ Using Oracle Database");

    builder.Services.AddDbContext<QLDHContext>(options =>
        options.UseOracle(builder.Configuration.GetConnectionString("OracleDb")));

    builder.Services.AddDbContext<AppDbContext>(options =>
        options.UseOracle(builder.Configuration.GetConnectionString("OracleDb")));
}

// ========== ĐĂNG KÝ SERVICES ==========

// Authentication Service
builder.Services.AddSingleton<QLDH.Services.OracleAuthService>();
builder.Services.AddSingleton<CryptoService>();

// Oracle Security Service (VPD, FGA, DB-level Encryption)
builder.Services.AddSingleton<OracleSecurityService>();

// NhanVien Decryption Service (giải mã dữ liệu Oracle)
builder.Services.AddSingleton<NhanVienDecryptionService>();

// Encryption Services
builder.Services.AddSingleton<AesEncryptionService>(sp =>
{
    var config = sp.GetRequiredService<IConfiguration>();
    return new AesEncryptionService(config);
});

builder.Services.AddSingleton<RsaEncryptionService>(sp =>
{
    var config = sp.GetRequiredService<IConfiguration>();
    return new RsaEncryptionService(config);
});

builder.Services.AddSingleton<HybridEncryptionService>(sp =>
{
    var rsaService = sp.GetRequiredService<RsaEncryptionService>();
    return new HybridEncryptionService(rsaService);
});

builder.Services.AddSingleton<CustomerEncryptionService>(sp =>
{
    var aes = sp.GetRequiredService<AesEncryptionService>();
    var rsa = sp.GetRequiredService<RsaEncryptionService>();
    var hybrid = sp.GetRequiredService<HybridEncryptionService>();
    return new CustomerEncryptionService(aes, rsa, hybrid);
});

// Authorization Service
builder.Services.AddScoped<AuthorizationService>();

// HttpContext Accessor (cần cho Authorization)
builder.Services.AddHttpContextAccessor();

var app = builder.Build();


if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseSession();

app.UseAuthorization();

// Default route
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Database}/{action=Index}/{id?}");

app.Run();
