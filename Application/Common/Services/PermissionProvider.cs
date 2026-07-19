using Application.Common.Interfaces;

namespace Application.Common.Services;

public class PermissionProvider : IPermissionProvider
{
    public List<string> GetPlatformAdminPermissions() =>
        new()
        {
            "school.create", "school.read", "school.update", "school.dashboard",
            "school.manage", "platform.analytics", "subscription.manage",
            "academicyear.read", "academicyear.create", "academicyear.update",
            "classroom.read", "classroom.create", "classroom.update",
            "gradelevel.read", "gradelevel.update",
            "room.read", "room.update",
            "teacher.create", "teacher.read", "teacher.update",
            "student.create", "student.read", "student.update",
            "parent.create", "parent.read", "parent.update",
            "enrollment.create", "schedule.create", "schedule.read",
            "attendance.read", "attendance.record", "attendance.read.own", "attendance.read.child",
            "grade.enter", "grade.read.own", "grade.read.child",
            "assignment.create", "assignment.read",
            "report.read",
            "profile.read", "profileread",
            "children.read", "childrenread",
            "myclasses.read", "myclassesread",
            "notification.read"
        };

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
            "educationstage.read", "educationstage.create", "educationstage.update", "educationstage.delete",
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
