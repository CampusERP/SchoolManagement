using Application.Common.Behaviors;
using Domain.Enums;

namespace Application.Features.People.Commands.LinkStudentGuardian;

public record LinkStudentGuardianCommand(
    Guid StudentId,
    Guid ParentId,
    GuardianRelationshipType RelationshipType,
    bool IsPrimaryContact = false,
    bool CanViewGrades = true,
    bool CanViewBilling = false) : ICommand<Guid>, IBaseCommand;
