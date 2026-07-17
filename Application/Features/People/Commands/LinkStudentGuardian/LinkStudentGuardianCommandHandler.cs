using MediatR;
using Application.Common.Models;
using Application.Common.Interfaces.Repositories;
using Domain.Entities.People;

namespace Application.Features.People.Commands.LinkStudentGuardian;

public class LinkStudentGuardianCommandHandler : IRequestHandler<LinkStudentGuardianCommand, Result<Guid>>
{
    private readonly IStudentRepository _students;
    private readonly IParentRepository _parents;
    private readonly IStudentGuardianRepository _guardians;

    public LinkStudentGuardianCommandHandler(
        IStudentRepository students,
        IParentRepository parents,
        IStudentGuardianRepository guardians)
    {
        _students = students;
        _parents = parents;
        _guardians = guardians;
    }

    public async Task<Result<Guid>> Handle(LinkStudentGuardianCommand request, CancellationToken ct)
    {
        var student = await _students.GetByIdAsync(request.StudentId, ct);
        if (student is null)
            return Result.Failure<Guid>($"Student with ID '{request.StudentId}' was not found.");

        var parent = await _parents.GetByIdAsync(request.ParentId, ct);
        if (parent is null)
            return Result.Failure<Guid>($"Parent with ID '{request.ParentId}' was not found.");

        var exists = await _guardians.ExistsAsync(request.StudentId, request.ParentId, ct);
        if (exists)
            return Result.Failure<Guid>($"Guardian with ID '{request.ParentId}' is already linked to student with ID '{request.StudentId}'.");

        var guardian = StudentGuardian.Create(
            request.SchoolId,
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
