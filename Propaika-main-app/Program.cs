using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.FileProviders;
using Propaika_main_app.Data;
using Propaika_main_app.Services;

var builder = WebApplication.CreateBuilder(args);



var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString)));


builder.Services.AddDefaultIdentity<IdentityUser>(options => {
    options.SignIn.RequireConfirmedAccount = false;
    options.User.RequireUniqueEmail = false;
    options.Password.RequireDigit = false;          
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequireUppercase = false;
    options.Password.RequiredLength = 6;
    options.User.AllowedUserNameCharacters =
    "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789-._@+"; //
})
.AddEntityFrameworkStores<ApplicationDbContext>();

builder.Services.AddRazorPages(options =>
{
    options.Conventions.AuthorizeFolder("/Admin");
});


builder.Services.AddSingleton<TelegramQueue>();
builder.Services.AddHostedService<TelegramBotWorker>();



var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
var staticPath = Path.Combine(builder.Environment.ContentRootPath, "wwwroot");
Console.WriteLine($"Static files path: {staticPath}"); // ƒл€ отладки в логах

app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new PhysicalFileProvider(staticPath),
    RequestPath = ""
});
app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();


app.MapRazorPages();
/*
using (var scope = app.Services.CreateScope())
{
    var userManager = scope.ServiceProvider.GetRequiredService<UserManager<IdentityUser>>();
    var login = "flildman@yandex.ru"; // »спользуем как логин
    var password = "Admin123";

    if (await userManager.FindByNameAsync(login) == null) // »щем по UserName
    {
        var user = new IdentityUser
        {
            UserName = login,
            Email = login + "@example.com",
            EmailConfirmed = true
        };
        await userManager.CreateAsync(user, password);
    }
}*/

app.Run();
