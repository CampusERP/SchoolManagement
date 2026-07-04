using MediatR;
using Application.Common.Models;
using Application.Common.Interfaces.Repositories;
using Domain.Entities.Tenancy;

namespace Application.Features.Schools.Commands.CreateSchool;

// ── Handler ──────────────────────────────────────────────────────────
public class CreateSchoolCommandHandler : IRequestHandler<CreateSchoolCommand, Result<Guid>>
{
    private readonly ISchoolRepository _schools;

    public CreateSchoolCommandHandler(ISchoolRepository schools)
    {
        _schools = schools;
    }

    public async Task<Result<Guid>> Handle(CreateSchoolCommand request, CancellationToken ct)
    {
        var existing = await _schools.GetBySubdomainAsync(request.SubdomainCode, ct);
        if (existing is not null)
            return Result.Failure<Guid>($"Subdomain '{request.SubdomainCode}' is already taken.");

        var school = School.Create(request.Name, request.SubdomainCode);
        await _schools.AddAsync(school, ct);

        return Result.Success(school.Id);
    }
}