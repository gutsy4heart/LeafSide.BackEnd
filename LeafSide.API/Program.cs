using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Microsoft.AspNetCore.Identity;
using LeafSide.Infrastructure.Identity;
var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
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

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

// // Seed roles and admin user on startup
// using (var scope = app.Services.CreateScope())
// {
//     var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole<Guid>>>();
//     var userManager = scope.ServiceProvider.GetRequiredService<UserManager<AppUser>>();

//     var roles = new[] { "Admin", "User" };
//     foreach (var role in roles)
//     {
//         if (!await roleManager.RoleExistsAsync(role))
//         {
//             await roleManager.CreateAsync(new IdentityRole<Guid>(role));
//         }
//     }

//     var adminEmail = "admin@leafside.local";
//     var admin = await userManager.FindByEmailAsync(adminEmail);
//     if (admin is null)
//     {
//         admin = new AppUser { Id = Guid.NewGuid(), UserName = adminEmail, Email = adminEmail, EmailConfirmed = true };
//         await userManager.CreateAsync(admin, "Admin12345!");
//         await userManager.AddToRoleAsync(admin, "Admin");
//     }
// }

app.Run();
