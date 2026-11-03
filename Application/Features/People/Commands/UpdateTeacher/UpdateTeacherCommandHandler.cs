using Application.Common.Interfaces.Repositories;
using Application.Common.Models;
using MediatR;

namespace Application.Features.People.Commands.UpdateTeacher;

public class UpdateTeacherCommandHandler : IRequestHandler<UpdateTeacherCommand, Result>
{
    private readonly ITeacherRepository _teachers;
    private readonly ITeacherCampusRepository _teacherCampuses;
    private readonly ISchoolRepository _schools;

    public UpdateTeacherCommandHandler(ITeacherRepository teachers, ITeacherCampusRepository teacherCampuses, ISchoolRepository schools)
    {
        _teachers = teachers;
        _teacherCampuses = teacherCampuses;
        _schools = schools;
    }

    public async Task<Result> Handle(UpdateTeacherCommand request, CancellationToken ct)
    {
        var teacher = await _teachers.GetByIdAsync(request.TeacherId, ct);
        if (teacher is null || teacher.SchoolId != request.SchoolId)
            return Result.Failure("Teacher not found.");

        var campuses = await _schools.GetCampusesAsync(request.SchoolId, ct);
        if (campuses.Count > 0 && (request.CampusIds is null || request.CampusIds.Count == 0))
            return Result.Failure("Select at least one campus for this teacher.");
        foreach (var campusId in request.CampusIds?.Distinct() ?? Enumerable.Empty<Guid>())
            if (await _schools.GetCampusAsync(request.SchoolId, campusId, ct) is null)
                return Result.Failure("One or more selected campuses were not found in this school.");

        if (await _teachers.ExistsAsync(request.SchoolId, request.EmployeeCode, request.TeacherId, ct))
            return Result.Failure($"A teacher with employee code '{request.EmployeeCode}' already exists in this school.");

        try
        {
            teacher.Update(request.EmployeeCode, request.FirstName, request.LastName, request.EmploymentStatus);
            if (request.CampusIds is not null)
                await _teacherCampuses.SetCampusIdsAsync(request.SchoolId, teacher.Id, request.CampusIds, ct);
        }
        catch (ArgumentException ex)
        {
            return Result.Failure(ex.Message);
        }

        return Result.Success();
    }
}
