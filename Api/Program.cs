using Api.Middleware;
using Application;
using Application.Common.Models;
using Infrastructure;
using Infrastructure.Persistence.Seed;
using Microsoft.AspNetCore.Mvc;
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
}).ConfigureApiBehaviorOptions(options =>
{
    options.InvalidModelStateResponseFactory = context =>
    {
        var errors = context.ModelState
            .Where(e => e.Value?.Errors.Count > 0)
            .ToDictionary(
                e => e.Key,
                e => e.Value!.Errors.Select(x => x.ErrorMessage).ToArray());
        return new BadRequestObjectResult(new { status = 400, message = "Model validation failed.", errors });
    };
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

// Permission strings must exactly match the claim values stored in JWT by PermissionProvider.
// These are already lowercase with dots (e.g. "attendance.read.own") so we use them directly.
var permissionPolicies = new Dictionary<string, string>
{
    // Academics
    ["AcademicYear.Read"]       = "academicyear.read",
    ["AcademicYear.Create"]     = "academicyear.create",
    ["AcademicYear.Update"]     = "academicyear.update",
    ["ClassRoom.Read"]          = "classroom.read",
    ["ClassRoom.Create"]        = "classroom.create",
    ["ClassRoom.Update"]        = "classroom.update",
    ["GradeLevel.Read"]         = "gradelevel.read",
    ["GradeLevel.Create"]       = "gradelevel.create",
    ["GradeLevel.Update"]       = "gradelevel.update",
    ["Room.Read"]               = "room.read",
    ["Room.Create"]             = "room.create",
    ["Room.Update"]             = "room.update",
    ["EducationStage.Read"]     = "educationstage.read",
    ["EducationStage.Create"]   = "educationstage.create",
    ["EducationStage.Update"]   = "educationstage.update",
    ["EducationStage.Delete"]   = "educationstage.delete",
    // School / Platform
    ["School.Read"]             = "school.read",
    ["School.Dashboard"]        = "school.dashboard",
    ["School.Create"]           = "school.create",
    ["School.Update"]           = "school.update",
    ["School.Manage"]           = "school.manage",
    ["Platform.Analytics"]      = "platform.analytics",
    // People
    ["Student.Read"]            = "student.read",
    ["Student.Create"]          = "student.create",
    ["Student.Update"]          = "student.update",
    ["Teacher.Read"]            = "teacher.read",
    ["Teacher.Create"]          = "teacher.create",
    ["Teacher.Update"]          = "teacher.update",
    ["Parent.Read"]             = "parent.read",
    ["Parent.Create"]           = "parent.create",
    ["Parent.Update"]           = "parent.update",
    // Portal / Personal
    ["Profile.Read"]            = "profile.read",
    ["Children.Read"]           = "children.read",
    ["MyClasses.Read"]          = "myclasses.read",
    // Enrollment & Schedule
    ["Enrollment.Create"]       = "enrollment.create",
    ["Schedule.Create"]         = "schedule.create",
    ["Schedule.Read"]           = "schedule.read",
    // Assignments
    ["Assignment.Create"]       = "assignment.create",
    ["Assignment.Read"]         = "assignment.read",
    ["Assignment.ReadOwn"]      = "assignment.read",       // students see their own via same claim
    ["Assignment.Submit"]       = "assignment.submit",
    // Attendance  — NOTE: claim uses dots, not camelCase
    ["Attendance.Record"]       = "attendance.record",
    ["Attendance.ReadOwn"]      = "attendance.read.own",
    ["Attendance.ReadChild"]    = "attendance.read.child",
    // Grades
    ["Grade.Enter"]             = "grade.enter",
    // Billing
    ["Billing.Read"]            = "billing.read",
    ["Billing.Manage"]          = "billing.manage",
    // Exams
    ["Exam.Read"]               = "exam.read",
    ["Exam.Create"]             = "exam.create",
    ["Exam.Manage"]             = "exam.manage",
    // Notifications
    ["Notification.Read"]       = "notification.read",
    ["Notification.Send"]       = "notification.send",
};

builder.Services.AddAuthorization(options =>
{
    foreach (var (policyName, claimValue) in permissionPolicies)
    {
        var claim = claimValue; // capture for closure

        options.AddPolicy(policyName, policy => policy.RequireAssertion(context =>
            context.User.HasClaim("is_platform_admin", "true") ||
            context.User.HasClaim("permission", claim)));
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
