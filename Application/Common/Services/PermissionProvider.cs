using Application.Common.Interfaces;

namespace Application.Common.Services;

public class PermissionProvider : IPermissionProvider
{
    public List<string> GetPlatformAdminPermissions() =>
        new() { "school.create", "school.manage", "subscription.manage" };

    public List<string> GetPermissionsForRole(string role) => role switch
    {
        "SchoolAdmin" => new()
        {
            "school.read", "teacher.create", "teacher.read",
            "student.create", "student.read", "enrollment.create",
            "classroom.create", "academicyear.create", "schedule.create",
            "attendance.read", "report.read"
        },
        "Teacher" => new()
        {
            "attendance.record", "grade.enter", "assignment.create",
            "classroom.read", "schedule.read"
        },
        "Student" => new()
        {
            "grade.read.own", "attendance.read.own",
            "schedule.read", "assignment.read"
        },
        "Parent" => new()
        {
            "grade.read.child", "attendance.read.child", "notification.read"
        },
        _ => new()
    };
}