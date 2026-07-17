using Application.Common.Interfaces;

namespace Application.Common.Services;

public class PermissionProvider : IPermissionProvider
{
    public List<string> GetPlatformAdminPermissions() =>
        new() { "school.create", "school.manage", "platform.analytics", "subscription.manage" };

    public List<string> GetPermissionsForRole(string role) => role switch
    {
        "SchoolAdmin" => new()
        {
            "school.read", "school.update", "school.dashboard",
            "teacher.create", "teacher.read", "teacher.update",
            "student.create", "student.read", "student.update", "enrollment.create",
            "parent.create", "parent.read", "parent.update",
            "classroom.read", "classroom.create", "classroom.update",
            "academicyear.read", "academicyear.create", "academicyear.update",
            "gradelevel.read", "gradelevel.update", "room.read", "room.update",
            "schedule.create",
            "attendance.read", "report.read"
        },
        "Teacher" => new()
        {
            "attendance.record", "grade.enter", "assignment.create",
            "classroom.read", "schedule.read", "myclassesread"
        },
        "Student" => new()
        {
            "grade.read.own", "attendance.read.own",
            "schedule.read", "assignment.read", "profileread"
        },
        "Parent" => new()
        {
            "grade.read.child", "attendance.read.child", "notification.read", "childrenread"
        },
        _ => new()
    };
}
