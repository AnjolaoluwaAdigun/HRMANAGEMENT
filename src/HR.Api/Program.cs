using System.Text;
using HR.Application.Interfaces;
using HR.Application.UseCases;
using HR.Infrastructure.Persistence;
using HR.Infrastructure.Security;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;

var builder = WebApplication.CreateBuilder(args);

// --- Database ---
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// --- Infrastructure services ---
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
builder.Services.AddScoped<IPasswordHasher, PasswordHasher>();
builder.Services.AddScoped<ITokenService, TokenService>();

// --- Application use cases ---
builder.Services.AddScoped<LoginUseCase>();
builder.Services.AddScoped<OnboardEmployeeUseCase>();
builder.Services.AddScoped<SubmitLeaveRequestUseCase>();
builder.Services.AddScoped<ApproveLeaveRequestUseCase>();
builder.Services.AddScoped<RejectLeaveRequestUseCase>();
builder.Services.AddScoped<CancelLeaveRequestUseCase>();
builder.Services.AddScoped<GetLeaveBalanceUseCase>();
builder.Services.AddScoped<GetTeamLeaveCalendarUseCase>();
builder.Services.AddScoped<AllocateYearlyBalancesUseCase>();
builder.Services.AddScoped<CreateLeaveTypeUseCase>();
builder.Services.AddScoped<GetLeaveTypesUseCase>();
builder.Services.AddScoped<GetLeaveRequestHistoryUseCase>();
// --- Controllers ---
builder.Services.AddControllers();

// --- JWT Authentication ---
var jwtSettings = builder.Configuration.GetSection("Jwt");
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
        ValidateIssuerSigningKey = true,
        ValidIssuer = jwtSettings["Issuer"],
        ValidAudience = jwtSettings["Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings["Key"]!))
    };
});

builder.Services.AddAuthorization();

// --- Swagger, grouped by role per NFR-Usability ---
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo { Title = "HR Management Platform API", Version = "v1" });

    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme. Example: \"Bearer {token}\"",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.Http,
        Scheme = "Bearer"
    });

    options.AddSecurityRequirement(document => new OpenApiSecurityRequirement
    {
        [new OpenApiSecuritySchemeReference("Bearer", document)] = new  List<string>()
    });
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();