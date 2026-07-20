using Application.Common.Exceptions;
using Application.Common.Interfaces.Repositories;
using Application.Common.Models;
using Domain.Entities.Tenancy;
using MediatR;

namespace Application.Features.Schools.Commands.AddCampus;

public class AddCampusCommandHandler : IRequestHandler<AddCampusCommand, Result<Guid>>
{
    private readonly ISchoolRepository _schools;
    public AddCampusCommandHandler(ISchoolRepository schools) => _schools = schools;

    public async Task<Result<Guid>> Handle(AddCampusCommand request, CancellationToken ct)
    {
        var school = await _schools.GetByIdAsync(request.SchoolId, ct);
        if (school is null) throw new NotFoundException(nameof(School), request.SchoolId);
        var campus = school.AddCampus(request.Name, request.Address);
        return Result.Success(campus.Id);
    }
}