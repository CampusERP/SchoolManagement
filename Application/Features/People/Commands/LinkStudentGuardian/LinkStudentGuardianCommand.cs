using Application.Common.Behaviors;
using Application.Common.Interfaces;
using Domain.Enums;

namespace Application.Features.People.Commands.LinkStudentGuardian;

public record LinkStudentGuardianCommand(
    Guid SchoolId,
    Guid StudentId,
    Guid ParentId,
    GuardianRelationshipType RelationshipType,
    bool IsPrimaryContact = false,
    bool CanViewGrades = true,
    bool CanViewBilling = false) : ICommand<Guid>, IBaseCommand, ITenantScopedRequest;
