using MediatR;
using Application.Common.Interfaces;
using Application.Common.Models;
using Application.Common.Interfaces.Repositories;
using Application.Common.Exceptions;
using Domain.Entities.People;

namespace Application.Features.People.Commands.LinkStudentGuardian;

public class LinkStudentGuardianCommandHandler : IRequestHandler<LinkStudentGuardianCommand, Result<Guid>>
{
    private readonly IStudentRepository _students;
    private readonly IParentRepository _parents;
    private readonly IStudentGuardianRepository _guardians;
    private readonly ITenantContext _tenant;

    public LinkStudentGuardianCommandHandler(
        IStudentRepository students,
        IParentRepository parents,
        IStudentGuardianRepository guardians,
        ITenantContext tenant)
    {
        _students = students;
        _parents = parents;
        _guardians = guardians;
        _tenant = tenant;
    }

    public async Task<Result<Guid>> Handle(LinkStudentGuardianCommand request, CancellationToken ct)
    {
        var schoolId = _tenant.SchoolId
            ?? throw new UnauthorizedAccessException("No school context found.");

        var student = await _students.GetByIdAsync(request.StudentId, ct);
        if (student is null)
            return Result.Failure<Guid>($"Student with ID '{request.StudentId}' was not found.");

        var parent = await _parents.GetByIdAsync(request.ParentId, ct);
        if (parent is null)
            return Result.Failure<Guid>($"Parent with ID '{request.ParentId}' was not found.");

        var guardian = StudentGuardian.Create(
            schoolId,
            request.StudentId,
            request.ParentId,
            request.RelationshipType,
            request.IsPrimaryContact,
            request.CanViewGrades,
            request.CanViewBilling);

        await _guardians.AddAsync(guardian, ct);

        return Result.Success(guardian.Id);
    }
}
