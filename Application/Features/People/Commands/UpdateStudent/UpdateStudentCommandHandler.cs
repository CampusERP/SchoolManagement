using Application.Common.Interfaces.Repositories;
using Application.Common.Models;
using MediatR;

namespace Application.Features.People.Commands.UpdateStudent;

public class UpdateStudentCommandHandler : IRequestHandler<UpdateStudentCommand, Result>
{
    private readonly IStudentRepository _students;
    private readonly ISchoolRepository _schools;

    public UpdateStudentCommandHandler(IStudentRepository students, ISchoolRepository schools)
    {
        _students = students;
        _schools = schools;
    }

    public async Task<Result> Handle(UpdateStudentCommand request, CancellationToken ct)
    {
        var student = await _students.GetByIdAsync(request.StudentId, ct);
        if (student is null || student.SchoolId != request.SchoolId)
            return Result.Failure("Student not found.");
        var campuses = await _schools.GetCampusesAsync(request.SchoolId, ct);
        if (campuses.Count > 0 && !request.CampusId.HasValue)
            return Result.Failure("Select a campus for this student.");
        if (request.CampusId.HasValue && await _schools.GetCampusAsync(request.SchoolId, request.CampusId.Value, ct) is null)
            return Result.Failure("The selected campus was not found in this school.");

        var codeInUse = await _students.ExistsAsync(
            request.SchoolId, request.StudentCode, request.StudentId, ct);
        if (codeInUse)
            return Result.Failure($"A student with code '{request.StudentCode}' already exists in this school.");

        try
        {
            student.Update(request.StudentCode, request.FirstName, request.LastName, request.DateOfBirth,
                request.NationalId, request.SecondName, request.ThirdName,
                request.NationalityCountry, request.ResidenceCountry, request.CampusId);
        }
        catch (ArgumentException ex)
        {
            return Result.Failure(ex.Message);
        }

        return Result.Success();
    }
}
