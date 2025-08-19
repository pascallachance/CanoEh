using System.Security.Claims;
using System.Text;
using Domain.Services.Implementations;
using Domain.Services.Interfaces;
using Infrastructure.Configuration;
using Infrastructure.Repositories.Implementations;
using Infrastructure.Repositories.Interfaces;
using Infrastructure.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;

internal class Program
{
    private static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        var jwtSettings = builder.Configuration.GetSection("JwtSettings");
        var secretKey = Encoding.UTF8.GetBytes(jwtSettings["Secret"]);

        builder.Services.Configure<EmailSettings>(builder.Configuration.GetSection("EmailSettings"));
        builder.Services
            .AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(jwtOptions =>
            {
                jwtOptions.Authority = jwtSettings["Issuer"];
                jwtOptions.Audience = jwtSettings["Audience"];
                jwtOptions.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = jwtSettings["Issuer"],
                    ValidAudience = jwtSettings["Audience"],
                    IssuerSigningKey = new SymmetricSecurityKey(secretKey),
                    NameClaimType = ClaimTypes.NameIdentifier,
                };
            });

        // Add CORS policy for Store app
        builder.Services.AddCors(options =>
        {
            options.AddDefaultPolicy(policy =>
                policy.WithOrigins("https://localhost:64941") // Frontend dev server
                      .AllowAnyHeader()
                      .AllowAnyMethod()
                      .AllowCredentials());
        });

        // Add CORS policy for Seller app
        builder.Services.AddCors(options =>
        {
            options.AddPolicy("AllowFrontend", policy =>
            {
                policy.WithOrigins("https://localhost:62209") // Frontend dev server
                      .AllowAnyHeader()
                      .AllowAnyMethod()
                      .AllowCredentials();
            });
        });

        // Register services
        builder.Services.AddScoped<IUserService, UserService>();
        builder.Services.AddScoped<ILoginService, LoginService>();
        builder.Services.AddScoped<ISessionService, SessionService>();
        builder.Services.AddScoped<IEmailService, EmailService>();
        builder.Services.AddScoped<IItemService, ItemService>();
        builder.Services.AddScoped<ICategoryService, CategoryService>();
        builder.Services.AddScoped<ICompanyService, CompanyService>();
        builder.Services.AddScoped<IAddressService, AddressService>();
        builder.Services.AddScoped<IPaymentMethodService, PaymentMethodService>();
        builder.Services.AddScoped<IOrderService>(provider =>
        {
            var config = provider.GetRequiredService<IConfiguration>();
            var connectionString = config.GetConnectionString("DefaultConnection");
            var orderRepository = provider.GetRequiredService<IOrderRepository>();
            var orderItemRepository = provider.GetRequiredService<IOrderItemRepository>();
            var orderAddressRepository = provider.GetRequiredService<IOrderAddressRepository>();
            var orderPaymentRepository = provider.GetRequiredService<IOrderPaymentRepository>();
            var orderStatusRepository = provider.GetRequiredService<IOrderStatusRepository>();
            var itemRepository = provider.GetRequiredService<IItemRepository>();
            var userRepository = provider.GetRequiredService<IUserRepository>();
            var taxRatesService = provider.GetRequiredService<ITaxRatesService>();
            return new OrderService(orderRepository, orderItemRepository, orderAddressRepository, 
                                  orderPaymentRepository, orderStatusRepository, itemRepository, 
                                  userRepository, taxRatesService, connectionString);
        });
        builder.Services.AddScoped<ITaxRatesService, TaxRatesService>();

        // Register Repositories
        builder.Services.AddScoped<IUserRepository>(provider =>
        {
            var config = provider.GetRequiredService<IConfiguration>();
            var connectionString = config.GetConnectionString("DefaultConnection");
            return new UserRepository(connectionString);
        });
        
        builder.Services.AddScoped<ISessionRepository>(provider =>
        {
            var config = provider.GetRequiredService<IConfiguration>();
            var connectionString = config.GetConnectionString("DefaultConnection");
            return new SessionRepository(connectionString);
        });

        builder.Services.AddScoped<IItemRepository>(provider =>
        {
            var config = provider.GetRequiredService<IConfiguration>();
            var connectionString = config.GetConnectionString("DefaultConnection");
            return new ItemRepository(connectionString);
        });

        builder.Services.AddScoped<ICategoryRepository>(provider =>
        {
            var config = provider.GetRequiredService<IConfiguration>();
            var connectionString = config.GetConnectionString("DefaultConnection");
            return new CategoryRepository(connectionString);
        });

        builder.Services.AddScoped<ICompanyRepository>(provider =>
        {
            var config = provider.GetRequiredService<IConfiguration>();
            var connectionString = config.GetConnectionString("DefaultConnection");
            return new CompanyRepository(connectionString);
        });

        builder.Services.AddScoped<IAddressRepository>(provider =>
        {
            var config = provider.GetRequiredService<IConfiguration>();
            var connectionString = config.GetConnectionString("DefaultConnection");
            return new AddressRepository(connectionString);
        });

        builder.Services.AddScoped<IPaymentMethodRepository>(provider =>
        {
            var config = provider.GetRequiredService<IConfiguration>();
            var connectionString = config.GetConnectionString("DefaultConnection");
            return new PaymentMethodRepository(connectionString);
        });

        builder.Services.AddScoped<ITaxRateRepository>(provider =>
        {
            var config = provider.GetRequiredService<IConfiguration>();
            var connectionString = config.GetConnectionString("DefaultConnection");
            return new TaxRateRepository(connectionString);
        });

        builder.Services.AddScoped<IItemAttributeRepository>(provider =>
        {
            var config = provider.GetRequiredService<IConfiguration>();
            var connectionString = config.GetConnectionString("DefaultConnection");
            return new ItemAttributeRepository(connectionString);
        });

        builder.Services.AddScoped<IItemVariantAttributeRepository>(provider =>
        {
            var config = provider.GetRequiredService<IConfiguration>();
            var connectionString = config.GetConnectionString("DefaultConnection");
            return new ItemVariantAttributeRepository(connectionString);
        });

        builder.Services.AddScoped<IOrderRepository>(provider =>
        {
            var config = provider.GetRequiredService<IConfiguration>();
            var connectionString = config.GetConnectionString("DefaultConnection");
            return new OrderRepository(connectionString);
        });

        builder.Services.AddScoped<IOrderItemRepository>(provider =>
        {
            var config = provider.GetRequiredService<IConfiguration>();
            var connectionString = config.GetConnectionString("DefaultConnection");
            return new OrderItemRepository(connectionString);
        });

        builder.Services.AddScoped<IOrderAddressRepository>(provider =>
        {
            var config = provider.GetRequiredService<IConfiguration>();
            var connectionString = config.GetConnectionString("DefaultConnection");
            return new OrderAddressRepository(connectionString);
        });

        builder.Services.AddScoped<IOrderPaymentRepository>(provider =>
        {
            var config = provider.GetRequiredService<IConfiguration>();
            var connectionString = config.GetConnectionString("DefaultConnection");
            return new OrderPaymentRepository(connectionString);
        });

        builder.Services.AddScoped<IOrderStatusRepository>(provider =>
        {
            var config = provider.GetRequiredService<IConfiguration>();
            var connectionString = config.GetConnectionString("DefaultConnection");
            return new OrderStatusRepository(connectionString);
        });

        builder.Services.AddScoped<IOrderItemStatusRepository>(provider =>
        {
            var config = provider.GetRequiredService<IConfiguration>();
            var connectionString = config.GetConnectionString("DefaultConnection");
            return new OrderItemStatusRepository(connectionString);
        });

        // Add services to the container.
        builder.Services.AddControllers();
        builder.Services.AddControllersWithViews();
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen(options =>
        {
            // Add JWT Bearer definition
            options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
            {
                Description = "JWT Authorization header using the Bearer scheme. Example: \"Bearer {token}\"",
                Name = "Authorization",
                In = ParameterLocation.Header,
                Type = SecuritySchemeType.Http,
                Scheme = "Bearer"
            });

            // Add global security requirement
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

        var app = builder.Build();

        // Configure the HTTP request pipeline.
        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger(options => { }); // Explicitly specify the overload to resolve ambiguity
            app.UseSwaggerUI();
        }

        app.UseHttpsRedirection();
         
        app.UseAuthentication();
        app.UseAuthorization();

        app.UseCors("AllowFrontend");

        app.MapControllers();
        app.MapDefaultControllerRoute();

        app.Run();
    }
}