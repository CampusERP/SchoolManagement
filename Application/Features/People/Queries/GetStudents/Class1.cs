using Application.Common.Interfaces.Repositories;
using Application.Common.Models;
using MediatR;

namespace Application.Features.People.Queries.GetStudents;

public record GetStudentsDto(
    Guid Id,
    string StudentCode,
    string FirstName,
    string LastName,
    DateTime DateOfBirth,
    string ClassRoomName,
    string Status
);

public record GetStudentsQuery(
    Guid SchoolId,
    Guid AcademicYearId,
    PaginationParams Pagination
) : IRequest<PagedResult<GetStudentsDto>>;

public class GetStudentsQueryHandler
    : IRequestHandler<GetStudentsQuery, PagedResult<GetStudentsDto>>
{
    private readonly IStudentRepository _studentRepository;

    public GetStudentsQueryHandler(IStudentRepository studentRepository) =>
        _studentRepository = studentRepository;

    public Task<PagedResult<GetStudentsDto>> Handle(GetStudentsQuery request, CancellationToken ct) =>
        _studentRepository.GetStudentsAsync(request.SchoolId, request.AcademicYearId, request.Pagination, ct);
}