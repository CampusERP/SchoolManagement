using Application.Common.Interfaces.Repositories;
using Application.Common.Models;
using MediatR;

namespace Application.Features.Schools.Commands.UpdateSchool;

public class UpdateSchoolCommandHandler : IRequestHandler<UpdateSchoolCommand, Result>
{
    private readonly ISchoolRepository _schools;

    public UpdateSchoolCommandHandler(ISchoolRepository schools)
    {
        _schools = schools;
    }

    public async Task<Result> Handle(UpdateSchoolCommand request, CancellationToken ct)
    {
        var school = await _schools.GetByIdAsync(request.SchoolId, ct);
        if (school is null) return Result.Failure("School not found.");

        try
        {
            school.UpdateInfo(request.Name);
        }
        catch (Exception ex)
        {
            return Result.Failure(ex.Message);
        }

        return Result.Success();
    }
}
