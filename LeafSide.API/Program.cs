using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using LeafSide.Infrastructure.Identity;
using Microsoft.OpenApi.Models;
 
var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddControllers()
    .ConfigureApiBehaviorOptions(options =>
    {
        options.SuppressInferBindingSourcesForParameters = false;
    });

// Configure routing to be case-insensitive for mobile compatibility
builder.Services.Configure<Microsoft.AspNetCore.Routing.RouteOptions>(options =>
{
    options.LowercaseUrls = false;
    options.LowercaseQueryStrings = false;
});
// CORS
builder.Services.AddCors(options =>
{
    // Policy for web frontend
    options.AddPolicy("FrontendPolicy", policy =>
        policy.WithOrigins("http://localhost:3000", "https://localhost:3000")
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials());
    
    // Policy for mobile apps (allows all origins for mobile development)
    options.AddPolicy("MobilePolicy", policy =>
        policy.AllowAnyOrigin()
            .AllowAnyHeader()
            .AllowAnyMethod());
});
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "LeafSide API", Version = "v1" });
    // Use fully-qualified type names to avoid schema ID collisions between
    // different classes with the same name (e.g., Admin.BookResponse vs Books.BookResponse)
    c.CustomSchemaIds(type => type.FullName!.Replace("+", "."));
    var securityScheme = new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Enter 'Bearer {token}'"
    };
    c.AddSecurityDefinition("Bearer", securityScheme);
    var securityRequirement = new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            new List<string>()
        }
    };
    c.AddSecurityRequirement(securityRequirement);
});

// Currency support removed
builder.Services.AddAuthorization(options =>
{
    options.FallbackPolicy = new AuthorizationPolicyBuilder()
        .RequireAuthenticatedUser()
        .Build();
});

// DbContext and DI registrations
builder.Services.AddDbContext<LeafSide.Infrastructure.Data.AppDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddIdentity<AppUser, IdentityRole<Guid>>()
    .AddEntityFrameworkStores<LeafSide.Infrastructure.Data.AppDbContext>()
    .AddDefaultTokenProviders();

var jwtSection = builder.Configuration.GetSection("Jwt");
var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSection["Key"]!));

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
}).AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = jwtSection["Issuer"],
        ValidAudience = jwtSection["Audience"],
        IssuerSigningKey = key
    };
});

builder.Services.AddScoped<LeafSide.Domain.Repositories.IBookRepository, LeafSide.Infrastructure.Data.Repostitory.Concrete.BookRepository>();
builder.Services.AddScoped<LeafSide.Application.Services.Abstract.IBookService, LeafSide.Application.Services.BookServices>();
builder.Services.AddScoped<LeafSide.Domain.Services.IJwtTokenService, LeafSide.Infrastructure.Services.JwtTokenService>();
builder.Services.AddScoped<LeafSide.Domain.Repositories.ICartRepository, LeafSide.Infrastructure.Data.Repostitory.Concrete.CartRepository>();
builder.Services.AddScoped<LeafSide.Application.Services.Abstract.ICartService, LeafSide.Application.Services.CartService>();
builder.Services.AddScoped<LeafSide.Domain.Repositories.IOrderRepository, LeafSide.Infrastructure.Data.Repostitory.Concrete.OrderRepository>();
builder.Services.AddScoped<LeafSide.Application.Services.Abstract.IOrderService, LeafSide.Application.Services.OrderService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// Avoid forcing HTTPS locally to prevent self-signed cert issues with Node fetch
if (!app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}

app.UseRouting();
// Use MobilePolicy for mobile apps (allows all origins), FrontendPolicy for web
// MobilePolicy is more permissive for development with mobile devices
app.UseCors("FrontendPolicy");
app.UseCors("MobilePolicy");
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

// Apply pending EF Core migrations at startup
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<LeafSide.Infrastructure.Data.AppDbContext>();
    try
    {
        db.Database.Migrate();
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Database migration failed: {ex.Message}");
    }
}

// Seed roles and admin user on startup
using (var scope = app.Services.CreateScope())
{
    var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole<Guid>>>();
    var userManager = scope.ServiceProvider.GetRequiredService<UserManager<AppUser>>();

    var roles = new[] { "Admin", "User" };
    foreach (var role in roles)
    {
        if (!await roleManager.RoleExistsAsync(role))
        {
            await roleManager.CreateAsync(new IdentityRole<Guid>(role));
        }
    }

    var adminEmail = "admin@leafside.local";
    var admin = await userManager.FindByEmailAsync(adminEmail);
    if (admin is null)
    {
        admin = new AppUser { Id = Guid.NewGuid(), UserName = adminEmail, Email = adminEmail, EmailConfirmed = true };
        await userManager.CreateAsync(admin, "Admin12345!");
        await userManager.AddToRoleAsync(admin, "Admin");
    }
    else
    {
        // Убеждаемся, что существующий админ имеет роль Admin
        var adminRoles = await userManager.GetRolesAsync(admin);
        if (!adminRoles.Contains("Admin"))
        {
            await userManager.AddToRoleAsync(admin, "Admin");
        }
    }
}

app.Run();
