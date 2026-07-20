using Application.Features.Academics.Commands.CloseAcademicYear;
using Application.Features.Academics.Commands.UpdateAcademicYear;
using Application.Features.Academics.Commands.UpdateClassRoom;
using Application.Features.Academics.Commands.UpdateGradeLevel;
using Application.Features.Academics.Commands.UpdateRoom;
using Application.Features.Billing.Commands;
using Application.Features.Exams.Commands;
using Application.Features.Notifications.Commands;
using Application.Features.People.Commands.StudentActivation;
using Application.Features.People.Commands.TerminateTeacher;
using Application.Features.Schools.Commands.SchoolActivation;
using FluentValidation;

namespace Application.Common.Validation;

public sealed class CloseAcademicYearCommandValidator : AbstractValidator<CloseAcademicYearCommand>
{
    public CloseAcademicYearCommandValidator() => RuleFor(x => x.AcademicYearId).NotEmpty();
}

public sealed class UpdateAcademicYearCommandValidator : AbstractValidator<UpdateAcademicYearCommand>
{
    public UpdateAcademicYearCommandValidator()
    {
        RuleFor(x => x.AcademicYearId).NotEmpty(); RuleFor(x => x.Name).NotEmpty().MaximumLength(100);
        RuleFor(x => x.EndDate).GreaterThan(x => x.StartDate);
    }
}

public sealed class UpdateClassRoomCommandValidator : AbstractValidator<UpdateClassRoomCommand>
{
    public UpdateClassRoomCommandValidator() { RuleFor(x => x.ClassRoomId).NotEmpty(); RuleFor(x => x.Name).NotEmpty().MaximumLength(100); }
}

public sealed class UpdateGradeLevelCommandValidator : AbstractValidator<UpdateGradeLevelCommand>
{
    public UpdateGradeLevelCommandValidator() { RuleFor(x => x.GradeLevelId).NotEmpty(); RuleFor(x => x.Name).NotEmpty().MaximumLength(100); RuleFor(x => x.Sequence).GreaterThanOrEqualTo(0); }
}

public sealed class UpdateRoomCommandValidator : AbstractValidator<UpdateRoomCommand>
{
    public UpdateRoomCommandValidator() { RuleFor(x => x.RoomId).NotEmpty(); RuleFor(x => x.Name).NotEmpty().MaximumLength(100); RuleFor(x => x.Capacity).GreaterThan(0); }
}

public sealed class AssignSubscriptionCommandValidator : AbstractValidator<AssignSubscriptionCommand>
{
    public AssignSubscriptionCommandValidator() { RuleFor(x => x.SchoolId).NotEmpty(); RuleFor(x => x.SubscriptionPlanId).NotEmpty(); RuleFor(x => x.EndDate).GreaterThan(x => x.StartDate); }
}

public sealed class CancelSubscriptionCommandValidator : AbstractValidator<CancelSubscriptionCommand>
{
    public CancelSubscriptionCommandValidator() => RuleFor(x => x.SchoolId).NotEmpty();
}

public sealed class GenerateInvoiceCommandValidator : AbstractValidator<GenerateInvoiceCommand>
{
    public GenerateInvoiceCommandValidator() { RuleFor(x => x.SchoolId).NotEmpty(); RuleFor(x => x.Amount).GreaterThan(0); RuleFor(x => x.DueDate).GreaterThan(DateTime.UtcNow); }
}

public sealed class SuspendSubscriptionCommandValidator : AbstractValidator<SuspendSubscriptionCommand>
{
    public SuspendSubscriptionCommandValidator() => RuleFor(x => x.SchoolId).NotEmpty();
}

public sealed class UpgradeSubscriptionCommandValidator : AbstractValidator<UpgradeSubscriptionCommand>
{
    public UpgradeSubscriptionCommandValidator() { RuleFor(x => x.SchoolId).NotEmpty(); RuleFor(x => x.NewPlanId).NotEmpty(); }
}

public sealed class GenerateReportCardCommandValidator : AbstractValidator<GenerateReportCardCommand>
{
    public GenerateReportCardCommandValidator() { RuleFor(x => x.SchoolId).NotEmpty(); RuleFor(x => x.StudentEnrollmentId).NotEmpty(); RuleFor(x => x.TermId).NotEmpty(); }
}

public sealed class LockExamCommandValidator : AbstractValidator<LockExamCommand>
{
    public LockExamCommandValidator() { RuleFor(x => x.SchoolId).NotEmpty(); RuleFor(x => x.ExamId).NotEmpty(); }
}

public sealed class MarkAllNotificationsReadCommandValidator : AbstractValidator<MarkAllNotificationsReadCommand>
{
    public MarkAllNotificationsReadCommandValidator() => RuleFor(x => x.SchoolId).NotEmpty();
}

public sealed class MarkNotificationReadCommandValidator : AbstractValidator<MarkNotificationReadCommand>
{
    public MarkNotificationReadCommandValidator() { RuleFor(x => x.SchoolId).NotEmpty(); RuleFor(x => x.NotificationId).NotEmpty(); }
}

public sealed class WithdrawStudentCommandValidator : AbstractValidator<WithdrawStudentCommand>
{
    public WithdrawStudentCommandValidator() { RuleFor(x => x.SchoolId).NotEmpty(); RuleFor(x => x.StudentEnrollmentId).NotEmpty(); }
}

public sealed class TerminateTeacherCommandValidator : AbstractValidator<TerminateTeacherCommand>
{
    public TerminateTeacherCommandValidator() { RuleFor(x => x.SchoolId).NotEmpty(); RuleFor(x => x.TeacherId).NotEmpty(); }
}

public sealed class SuspendSchoolCommandValidator : AbstractValidator<SuspendSchoolCommand>
{
    public SuspendSchoolCommandValidator() => RuleFor(x => x.SchoolId).NotEmpty();
}

public sealed class ReactivateSchoolCommandValidator : AbstractValidator<ReactivateSchoolCommand>
{
    public ReactivateSchoolCommandValidator() => RuleFor(x => x.SchoolId).NotEmpty();
}
