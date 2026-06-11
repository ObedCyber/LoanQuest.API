using System.Reflection;
using System.Text;
using LoanRequestApplication.Interfaces.Repositories;
using LoanRequestApplication.Interfaces.Services;
using LoanRequestApplication.Profiles;
using LoanRequestInfrastructure.Persistence;
using LoanRequestInfrastructure.Persistence.Repositories;
using LoanRequestInfrastructure.Services.Applicants;
using LoanRequestInfrastructure.Services.Auth;
using LoanRequestInfrastructure.Services.BVN_NIN;
using LoanRequestInfrastructure.Services.Files;
using LoanRequestInfrastructure.Services.Loans;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString));
builder.Services.AddIdentityCore<IdentityUser>(options =>
{
    options.Password.RequireDigit = false;
    options.Password.RequireLowercase = false;
    options.Password.RequireUppercase = false;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequiredLength = 6;
})
.AddRoles<IdentityRole>()
.AddEntityFrameworkStores<ApplicationDbContext>()
.AddDefaultTokenProviders();

builder.Services.AddAutoMapper(cfg =>
{
    cfg.AddMaps(typeof(ApplicantProfile).Assembly);
    cfg.AddMaps(typeof(EmploymentProfile).Assembly);
});


// Add services to the container.
builder.Services.AddScoped<ITokenService, TokenService>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IRefreshTokensRepository, RefreshTokensRepository>();

builder.Services.AddHttpClient<IBVN_NINService, BVN_NINService>();
builder.Services.AddScoped<IEmploymentService, EmploymentService>();
//builder.Services.AddScoped<IBVN_NINService, BVN_NINService>();
builder.Services.AddScoped<IApplicantService, ApplicantService>();
builder.Services.AddScoped<IApplicantRepository, ApplicantRepository>();
builder.Services.AddScoped<IApplicantEmploymentRepository, ApplicantEmploymentRepository>();
builder.Services.AddScoped<IApplicantFinancialsRepository, ApplicantFinancialsRepository>();
builder.Services.AddScoped<IEligibilityChecksRepository, EligibilityChecksRepository>();
builder.Services.AddScoped<IEligibilityEngine, EligibilityEngine>();
builder.Services.AddScoped<ILoanProductService, LoanProductService>();
builder.Services.AddScoped<ILoanApplicationRepository, LoanApplicationRepository>();
builder.Services.AddScoped<ILoanApplicationService, LoanApplicationService>();
builder.Services.AddScoped<IDocumentRequirementRepository, DocumentRequirementRepository>();
builder.Services.AddScoped<IDocumentChecklistService, DocumentChecklistService>();
builder.Services.AddScoped<ILoanDocumentService, LoanDocumentService>();
builder.Services.AddScoped<IFileUploadService, FileUploadService>();
builder.Services.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));
builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();

builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo { Title = "Loan Request API", Version = "v1" });

    // Define the Security Scheme
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "JWT Authorization header using the Bearer scheme. \r\n\r\n Enter 'Bearer' [space] and then your token in the text input below.\r\n\r\nExample: \"Bearer 1safsfsdfdfd\""
    });

    // 2. SAFE DEFENSIVE XML LOADING BLOCK
    var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);

    // Check if the file actually exists before telling Swagger to look for it!
    if (File.Exists(xmlPath))
    {
        options.IncludeXmlComments(xmlPath);
    }

    // Make Swagger use that scheme globally
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
// Add JWt Service
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ClockSkew = TimeSpan.Zero,
        ValidateIssuerSigningKey = true,
        ValidIssuer = builder.Configuration["Jwt:Issuer"],
        ValidAudience = builder.Configuration["Jwt:Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]))
    };
});
builder.Services.AddHttpContextAccessor();

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowReactApp", policy =>
    {
        policy.WithOrigins("http://localhost:5173", "http://localhost:3000") // Put your frontend URLs here
              .AllowAnyHeader()                                            // Allows headers like Authorization (JWT tokens) or Content-Type
              .AllowAnyMethod()                                            // Allows GET, POST, PUT, DELETE, etc.
              .AllowCredentials();                                         // Crucial if passing HTTP-Only cookies or Refresh Tokens
    });
});


var app = builder.Build();
app.UseDeveloperExceptionPage();
// Configure the HTTP request pipeline.


app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "Loan Request API v1");
    c.RoutePrefix = string.Empty;
});


app.UseHttpsRedirection();
app.UseCors("AllowReactApp");
app.UseAuthentication();
app.UseAuthorization();


app.MapControllers();


// Automatically apply pending migrations on application startup
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var context = services.GetRequiredService<ApplicationDbContext>();

        // Check if there are any migrations that haven't been applied yet
        if ((await context.Database.GetPendingMigrationsAsync()).Any())
        {
            await context.Database.MigrateAsync();
            Console.WriteLine("Database migrations applied successfully in the cloud!");
        }
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "An error occurred while migrating the database on startup.");
        // Optional: Fail the startup if the database is out of sync
        throw;
    }
}


app.Run();
