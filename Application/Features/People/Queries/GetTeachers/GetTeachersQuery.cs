using Application.Common.Interfaces.Repositories;
using Application.Common.Models;
using MediatR;

namespace Application.Features.People.Queries.GetTeachers;

public record GetTeachersDto(
    Guid Id,
    string EmployeeCode,
    string FirstName,
    string LastName,
    string EmploymentStatus
);

public record GetTeachersQuery(
    Guid SchoolId,
    PaginationParams Pagination
) : IRequest<PagedResult<GetTeachersDto>>;

public class GetTeachersQueryHandler
    : IRequestHandler<GetTeachersQuery, PagedResult<GetTeachersDto>>
{
    private readonly ITeacherRepository _teacherRepository;

    public GetTeachersQueryHandler(ITeacherRepository teacherRepository) =>
        _teacherRepository = teacherRepository;

    public Task<PagedResult<GetTeachersDto>> Handle(GetTeachersQuery request, CancellationToken ct) =>
        _teacherRepository.GetTeachersAsync(request.SchoolId, request.Pagination, ct);
}
