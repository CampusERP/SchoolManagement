using Api.Middleware;
using Application;
using Application.Common.Models;
using Infrastructure;
using Infrastructure.Persistence.Seed;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "School Management API",
        Version = "v1",
        Description = "Multi-tenant School ERP"
    });
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Enter: Bearer {your_token}"
    });
    options.AddSecurityRequirement(document =>
        new OpenApiSecurityRequirement
        {
            [
                new OpenApiSecuritySchemeReference("Bearer", document)
            ] = new List<string>()
        });
});

var frontendOrigins = builder.Configuration
    .GetSection("Cors:AllowedOrigins")
    .Get<string[]>()
    ?.Where(origin =>
        Uri.TryCreate(origin, UriKind.Absolute, out var uri) &&
        (uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps))
    .Distinct(StringComparer.OrdinalIgnoreCase)
    .ToArray() ?? [];

builder.Services.AddCors(options =>
{
    options.AddPolicy("Development", policy =>
        policy.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod());

    options.AddPolicy("Frontend", policy =>
    {
        if (frontendOrigins.Length > 0)
            policy.WithOrigins(frontendOrigins).AllowAnyHeader().AllowAnyMethod();
    });
});

var jwtSettings = builder.Configuration.GetSection("Jwt").Get<JwtSettings>()
    ?? throw new InvalidOperationException("Jwt settings not configured.");

if (string.IsNullOrWhiteSpace(jwtSettings.Secret) || Encoding.UTF8.GetByteCount(jwtSettings.Secret) < 32)
    throw new InvalidOperationException("Jwt:Secret must be set and at least 256 bits (32 bytes).");

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.MapInboundClaims = false;
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtSettings.Issuer,
            ValidAudience = jwtSettings.Audience,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings.Secret)),
            ClockSkew = TimeSpan.FromSeconds(30)
        };
    });

var permissionPolicies = new[]
{
    "AcademicYear.Read", "AcademicYear.Create", "AcademicYear.Update",
    "ClassRoom.Read", "ClassRoom.Create", "ClassRoom.Update",
    "GradeLevel.Read", "GradeLevel.Update", "Room.Read", "Room.Update",
    "School.Read", "School.Dashboard", "School.Create", "School.Update",
    "Platform.Analytics", "Student.Read", "Student.Create", "Student.Update",
    "Teacher.Read", "Teacher.Create", "Teacher.Update",
    "Parent.Read", "Parent.Create", "Parent.Update",
    "Profile.Read", "Children.Read", "MyClasses.Read",
    "Enrollment.Create", "Schedule.Create"
};

builder.Services.AddAuthorization(options =>
{
    foreach (var policyName in permissionPolicies)
    {
        var permission = policyName == "Platform.Analytics"
            ? "platform.analytics"
            : policyName.Replace(".", string.Empty).ToLowerInvariant();

        options.AddPolicy(policyName, policy => policy.RequireAssertion(context =>
            context.User.HasClaim("is_platform_admin", "true") ||
            context.User.HasClaim("permission", permission)));
    }
});

var app = builder.Build();

await DataSeeder.SeedAsync(app.Services);

app.UseMiddleware<ExceptionHandlingMiddleware>();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "School Management v1"));
}

app.UseHttpsRedirection();
app.UseCors(app.Environment.IsDevelopment() ? "Development" : "Frontend");
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();
