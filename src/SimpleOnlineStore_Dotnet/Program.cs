using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using SimpleOnlineStore_Dotnet.Data;
using SimpleOnlineStore_Dotnet.Models;
using Swashbuckle.AspNetCore.Filters;

var builder = WebApplication.CreateBuilder(args);
var services = builder.Services;

services.AddControllers();
services.AddEndpointsApiExplorer();
services.AddSwaggerGen(options => {
    options.AddSecurityDefinition("oauth2", new Microsoft.OpenApi.Models.OpenApiSecurityScheme() {
        In = Microsoft.OpenApi.Models.ParameterLocation.Header,
        Name = "Authorization",
        Type = Microsoft.OpenApi.Models.SecuritySchemeType.ApiKey,
    });

    options.OperationFilter<SecurityRequirementsOperationFilter>();
});

services.AddDbContext<DataContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

services
    .AddLogging(config => {
        config.AddConsole();
        config.AddDebug();
    })
    .AddIdentity<User, IdentityRole>(options => {
        options.User.RequireUniqueEmail = true;
    })
    .AddEntityFrameworkStores<DataContext>();

services.AddAuthorization(options => {
    options.AddPolicy("RequireAdminRole", policy => policy.RequireRole(Roles.ADMIN_ROLE));
    options.AddPolicy("RequireCustomerRole", policy => policy.RequireRole(Roles.CUSTOMER_ROLE));
});
services
    .AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options => {
        options.Cookie.IsEssential = true;
        options.Cookie.HttpOnly = false;
    });

services.ConfigureApplicationCookie(options => {
    options.Events.OnRedirectToAccessDenied = context => {
        context.Response.StatusCode = 403;
        return Task.CompletedTask;
    };
    options.Events.OnRedirectToLogin = context => {
        context.Response.StatusCode = 401;
        return Task.CompletedTask;
    };
});

services.Configure<CookiePolicyOptions>(options => {
    options.MinimumSameSitePolicy = SameSiteMode.Lax;
});

var app = builder.Build();

app.UseMiddleware<GlobalRoutePrefixMiddleware>("/api/v1");

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment()) {
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UsePathBase(new PathString("/api/v1"));

app.UseCookiePolicy();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

using (var scope = app.Services.CreateScope()) {
    await Program.TryAddRoles(scope);
}

// create admin (super) user for solution
string userName = builder.Configuration["SuperUser:UserName"]
    ?? throw new Exception("Invalid AppSettings.json - SuperUser:UserName is null");
string email = builder.Configuration["SuperUser:Email"]
    ?? throw new Exception("Invalid AppSettings.json  - SuperUser:Email is null");
string password = builder.Configuration["SuperUser:Password"]
    ?? throw new Exception("Invalid AppSettings.json  - SuperUser:Password is null");
using (var scope = app.Services.CreateScope()) {
    await Program.TryAddSuperUser(scope, userName, email, password);
}

app.Run();

public partial class Program {
    private static SemaphoreSlim semaphoreSlim = new SemaphoreSlim(1);

    public static async Task TryAddSuperUser(IServiceScope scope, string userName, string email, string password) {
        await semaphoreSlim.WaitAsync();

        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<User>>();
        var dataContext = scope.ServiceProvider.GetRequiredService<DataContext>();

        var _superUser = await userManager.FindByEmailAsync(email);
        if (_superUser == null) {
            var admin = new Admin() { Creation = DateTime.UtcNow };
            admin.Creator = admin;
            var superUser = new User { UserName = userName, Email = email, UserRoleId = admin.Id };
            var superUserCreation = await userManager.CreateAsync(superUser, password);
            if (superUserCreation.Succeeded) {
                await userManager.AddToRoleAsync(superUser, Roles.ADMIN_ROLE);
                await dataContext.AddAsync(admin);
                await dataContext.SaveChangesAsync();
            } else {
                semaphoreSlim.Release();
                throw new Exception("Error creating SuperUser");
            }
        }
        semaphoreSlim.Release();
    }

    public static async Task TryAddRoles(IServiceScope scope) {
        await semaphoreSlim.WaitAsync();
        var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();

        foreach (string role in Roles.ROLES) {
            if (!await roleManager.RoleExistsAsync(role)) {
                await roleManager.CreateAsync(new IdentityRole(role));
            }
        }
        semaphoreSlim.Release();
    }
}

class GlobalRoutePrefixMiddleware {
    private readonly RequestDelegate _next;
    private readonly string _routePrefix;

    public GlobalRoutePrefixMiddleware(RequestDelegate next, string routePrefix) {
        _next = next;
        _routePrefix = routePrefix;
    }

    public async Task InvokeAsync(HttpContext context) {
        context.Request.PathBase = new PathString(_routePrefix);
        await _next(context);
    }
}