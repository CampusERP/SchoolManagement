using FluentValidation;

namespace Application.Features.Notifications.Commands;

public class SendNotificationCommandValidator : AbstractValidator<SendNotificationCommand>
{
    public SendNotificationCommandValidator()
    {
        RuleFor(x => x.SchoolId).NotEmpty();
        RuleFor(x => x.Subject).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Body).NotEmpty().MaximumLength(2000);
        RuleFor(x => x.TargetUserId)
            .NotEmpty().When(x => x.Scope == NotificationScope.Individual)
            .WithMessage("TargetUserId is required for Individual scope.");
        RuleFor(x => x.TargetClassRoom)
            .NotEmpty()
            .When(x => x.Scope == NotificationScope.ClassRoomParents ||
                       x.Scope == NotificationScope.ClassRoomStudents)
            .WithMessage("TargetClassRoom is required for classroom scopes.");
        RuleFor(x => x.TargetGrade)
            .NotEmpty().When(x => x.Scope == NotificationScope.GradeLevelParents)
            .WithMessage("TargetGrade is required for GradeLevel scope.");
    }
}
