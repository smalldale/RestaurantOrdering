using System.Security.Claims;
using System.Text;
using FluentValidation;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using RestaurantOrdering.API.Data;
using RestaurantOrdering.API.Data.Seeders;
using RestaurantOrdering.API.DTOs.Auth;
using RestaurantOrdering.API.DTOs.Category;
using RestaurantOrdering.API.DTOs.MenuItem;
using RestaurantOrdering.API.DTOs.Order;
using RestaurantOrdering.API.Models;
using RestaurantOrdering.API.Repositories;
using RestaurantOrdering.API.Repositories.Interfaces;
using RestaurantOrdering.API.Services;
using RestaurantOrdering.API.Services.Interfaces;
using RestaurantOrdering.API.Validators.Auth;
using RestaurantOrdering.API.Validators.Category;
using RestaurantOrdering.API.Validators.MenuItem;
using RestaurantOrdering.API.Validators.Order;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();

// Database
builder.Services.AddDbContext<RestaurantDbContext>(options =>
    options.UseSqlite(
        builder.Configuration.GetConnectionString("DefaultConnection")));

// Add Identity
builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
    {
        // Password requirements
        options.Password.RequiredLength = 8;
        options.Password.RequireDigit = true;
        options.Password.RequireLowercase = true;
        options.Password.RequireUppercase = true;
        options.Password.RequireNonAlphanumeric = false;

        // User settings
        options.User.RequireUniqueEmail = true;

        // Account lockout
        options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(15);
        options.Lockout.MaxFailedAccessAttempts = 5;
        options.Lockout.AllowedForNewUsers = true;

        // No email confirmation flow implemented yet
        options.SignIn.RequireConfirmedAccount = false;
    })
    .AddEntityFrameworkStores<RestaurantDbContext>()
    .AddDefaultTokenProviders();

// Add JWT Authentication
var jwtSettings = builder.Configuration.GetSection("Jwt");

var secretKey = jwtSettings["SecretKey"]
    ?? throw new InvalidOperationException("JWT SecretKey is missing.");

// Keep the key as a plain string in configuration (no generated key).
// TokenService must derive the signing key from the same UTF-8 bytes.
var keyBytes = Encoding.UTF8.GetBytes(secretKey);

if (keyBytes.Length < 32)
{
    throw new InvalidOperationException(
        "JWT SecretKey must be at least 32 characters.");
}

var signingKey = new SymmetricSecurityKey(keyBytes);

builder.Services
    .AddAuthentication(options =>
    {
        options.DefaultAuthenticateScheme =
            JwtBearerDefaults.AuthenticationScheme;

        options.DefaultChallengeScheme =
            JwtBearerDefaults.AuthenticationScheme;
    })
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = jwtSettings["Issuer"],

            ValidateAudience = true,
            ValidAudience = jwtSettings["Audience"],

            ValidateIssuerSigningKey = true,
            IssuerSigningKey = signingKey,

            ValidateLifetime = true,
            ClockSkew = TimeSpan.FromSeconds(30),

            NameClaimType = ClaimTypes.Name,
            RoleClaimType = ClaimTypes.Role
        };
    });

builder.Services.AddAuthorization();

// Add Repositories
// Restaurant
builder.Services.AddScoped<ICategoryRepository, CategoryRepository>();
builder.Services.AddScoped<IMenuItemRepository, MenuItemRepository>();
builder.Services.AddScoped<IOrderRepository, OrderRepository>();
// Authentication
builder.Services.AddScoped<IRefreshTokenRepository, RefreshTokenRepository>();

// Add FluentValidation
// Category
builder.Services.AddScoped<IValidator<CategoryQueryDto>, CategoryQueryValidator>();
builder.Services.AddScoped<IValidator<CreateCategoryDto>, CreateCategoryValidator>();
builder.Services.AddScoped<IValidator<UpdateCategoryDto>, UpdateCategoryValidator>();
// MenuItem
builder.Services.AddScoped<IValidator<MenuItemQueryDto>, MenuItemQueryValidator>();
builder.Services.AddScoped<IValidator<CreateMenuItemDto>, CreateMenuItemValidator>();
builder.Services.AddScoped<IValidator<UpdateMenuItemDto>, UpdateMenuItemValidator>();
// Order
builder.Services.AddScoped<IValidator<CreateOrderDto>, CreateOrderValidator>();
builder.Services.AddScoped<IValidator<UpdateOrderCustomerNameDto>, UpdateOrderCustomerNameValidator>();
builder.Services.AddScoped<IValidator<AddOrderItemDto>, AddOrderItemValidator>();
builder.Services.AddScoped<IValidator<CheckoutOrderDto>, CheckoutOrderValidator>();
builder.Services.AddScoped<IValidator<UpdateOrderStatusDto>, UpdateOrderStatusValidator>();
builder.Services.AddScoped<IValidator<UpdateOrderItemQuantityDto>, UpdateOrderItemQuantityValidator>();
// Login and Refresh Token
builder.Services.AddScoped<IValidator<LoginDto>, LoginValidator>();
builder.Services.AddScoped<IValidator<RefreshTokenDto>, RefreshTokenValidator>();

// Add AutoMapper
builder.Services.AddAutoMapper(typeof(Program));

// Add Services
builder.Services.AddScoped<ICategoryService, CategoryService>();
builder.Services.AddScoped<IMenuService, MenuService>();
builder.Services.AddScoped<IOrderService, OrderService>();
builder.Services.AddScoped<ITokenService, TokenService>();
builder.Services.AddScoped<IAuthService, AuthService>();

// Add controllers
builder.Services.AddControllers();

// Add API documentation
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "RestaurantOrdering API",
        Version = "v1",
        Description = """
            RESTful API for managing restaurant operations.

            Features:
            - Category management
            - Menu item management
            - Order management
            - JWT authentication
            - Role-based authorization (Admin and Staff)
            - Refresh token rotation
            """
    });

    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Enter your JWT access token."
    });

    options.AddSecurityRequirement(new OpenApiSecurityRequirement
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
            Array.Empty<string>()
        }
    });
});

// Add CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", builder =>
    {
        builder.AllowAnyOrigin()
               .AllowAnyMethod()
               .AllowAnyHeader();
    });
});

var app = builder.Build();

// Seed data
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var context = services.GetRequiredService<RestaurantDbContext>();

    // Uncomment on a fresh machine (or when migrations change) to create/update
    // the SQLite schema before seeding. Keep commented when using an existing
    // restaurant.db or after running "dotnet ef database update".
    // await context.Database.MigrateAsync();

    // Seed roles, admin, and staff users
    await IdentitySeeder.EnsureSeededAsync(services);

    // Seed Category and MenuItem
    await RestaurantDataSeeder.EnsureSeededAsync(context);
}


// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint(
            "/swagger/v1/swagger.json",
            "RestaurantOrdering API v1");

        options.RoutePrefix = "swagger";
    });
}

app.UseHttpsRedirection();

app.UseCors("AllowAll");

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
