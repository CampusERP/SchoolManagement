using Application.Common.Interfaces;

namespace Application.Common.Services;

public class PermissionProvider : IPermissionProvider
{
    public List<string> GetPlatformAdminPermissions() =>
        new()
        {
            "school.create", "school.read", "school.update",
            "school.manage", "platform.analytics", "subscription.manage"
        };

    public List<string> GetPermissionsForRole(string role) => role switch
    {
        "SchoolAdmin" => new()
        {
            "school.read",
            "school.update",
            "school.dashboard",

            "teacher.create",
            "teacher.read",
            "teacher.update",

            "student.create",
            "student.read",
            "student.update",

            "enrollment.create",

            "parent.create",
            "parent.read",
            "parent.update",

            "classroom.create",
            "classroom.read",
            "classroom.update",

            "academicyear.create",
            "academicyear.read",
            "academicyear.update",

            "gradelevel.create",
            "gradelevel.read",
            "gradelevel.update",

            "room.create",
            "room.read",
            "room.update",

            "schedule.create",

            "attendance.read",

            "report.read",

            "subject.manage",

            "exam.create",

            "notification.send",
            "notification.read",
            "educationstage.read",
            "educationstage.create",
            "educationstage.update",
            "educationstage.delete",

            "profile.read",
            "myclasses.read",
        },

        "Teacher" => new()
        {
            "attendance.record",

            "grade.enter",

            "assignment.create",
            "assignment.grade",

            "classroom.read",

            "academicyear.read",

            "room.read",

            "student.read",

            "schedule.read",

            "exam.record",
            "exam.read",
            "exam.create",
            "exam.manage",

            "assignment.read",

            "myclasses.read",

            "profile.read",
            "notification.read"
        },

        "Student" => new()
        {
            "attendance.read.own",

            "grade.read.own",

            "schedule.read",

            "assignment.read",
            "assignment.submit",

            "exam.read.own",

            "reportcard.read.own",

            "profile.read",

            "myclasses.read",
            "exam.read",
            "notification.read"
        },

        "Parent" => new()
        {
            "attendance.read.child",

            "grade.read.child",

            "notification.read",

            "assignment.read.child",

            "exam.read.child",

            "reportcard.read.child",

            "schedule.read.child",

            "children.read"
        },
        _ => new()
    };
}
