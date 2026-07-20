using Api.Middleware;
using Application;
using Application.Common.Models;
using Infrastructure;
using Infrastructure.Persistence.Seed;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.OpenApi;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;
using Scalar.AspNetCore;
using System.Text;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);

builder.Services.AddControllers().AddJsonOptions(options =>
{
    options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
});
builder.Services.AddEndpointsApiExplorer();

builder.Services.AddOpenApi(options =>
{
    options.AddDocumentTransformer((document, context, cancellationToken) =>
    {
        document.Info = new OpenApiInfo
        {
            Title = "School Management API",
            Version = "v1",
            Description = "Multi-tenant School ERP"
        };
        document.Components ??= new OpenApiComponents();
        document.Components.SecuritySchemes ??= new Dictionary<string, IOpenApiSecurityScheme>();
        document.Components.SecuritySchemes["Bearer"] = new OpenApiSecurityScheme
        {
            Type = SecuritySchemeType.Http,
            Scheme = "Bearer",
            BearerFormat = "JWT",
            In = ParameterLocation.Header,
            Description = "Enter: Bearer {your_token}"
        };
        document.AddComponent("Bearer", document.Components.SecuritySchemes["Bearer"]);
        document.Security =
        [
            new OpenApiSecurityRequirement
            {
                [new OpenApiSecuritySchemeReference("Bearer", document)] = []
            }
        ];
        return Task.CompletedTask;
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

builder.Services
    .AddAuthentication(options =>
    {
        options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        options.DefaultForbidScheme = JwtBearerDefaults.AuthenticationScheme;
    })
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
    "GradeLevel.Read", "GradeLevel.Create", "GradeLevel.Update", "Room.Read", "Room.Create", "Room.Update",
    "EducationStage.Read", "EducationStage.Create", "EducationStage.Update", "EducationStage.Delete",
    "School.Read", "School.Dashboard", "School.Create", "School.Update",
    "Platform.Analytics", "Student.Read", "Student.Create", "Student.Update",
    "Teacher.Read", "Teacher.Create", "Teacher.Update",
    "Parent.Read", "Parent.Create", "Parent.Update",
    "Profile.Read", "Children.Read", "MyClasses.Read",
    "Enrollment.Create", "Schedule.Create",
    "School.Manage", "Assignment.Create", "Assignment.Submit", "Assignment.Read", "Assignment.ReadOwn",
    "Attendance.Record", "Attendance.ReadOwn", "Attendance.ReadChild", "Schedule.Read", "Grade.Enter",
    "Billing.Read", "Billing.Manage", "Exam.Read", "Exam.Create", "Exam.Manage",
    "Notification.Read", "Notification.Send"
};

builder.Services.AddAuthorization(options =>
{
    foreach (var policyName in permissionPolicies)
    {
        var permission = policyName.ToLowerInvariant();

        options.AddPolicy(policyName, policy => policy.RequireAssertion(context =>
            context.User.HasClaim("is_platform_admin", "true") ||
            context.User.HasClaim("permission", permission)));
    }
});

var app = builder.Build();

await DataSeeder.SeedAsync(app.Services);

app.UseMiddleware<ExceptionHandlingMiddleware>();

app.MapOpenApi();
app.MapScalarApiReference();

    app.UseHttpsRedirection();
    app.UseCors(app.Environment.IsDevelopment() ? "Development" : "Frontend");
    app.UseAuthentication();
    app.UseAuthorization();
    app.MapControllers();

app.Run();
