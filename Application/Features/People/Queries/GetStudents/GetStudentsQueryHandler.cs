using Application.Common.Interfaces.Services;
using Application.Common.Models;
using MediatR;

namespace Application.Features.People.Queries.GetStudents;


public class GetStudentsQueryHandler
    : IRequestHandler<GetStudentsQuery, Result<PagedResult<StudentListDto>>>
{
    private readonly IStudentReadService _studentReadService;

    public GetStudentsQueryHandler(IStudentReadService studentReadService)
    {
        _studentReadService = studentReadService;
    }

    public async Task<Result<PagedResult<StudentListDto>>> Handle(
        GetStudentsQuery request, CancellationToken ct)
    {
        var pagination = request.Pagination ?? new PaginationParams();

        var students = await _studentReadService.GetStudentsAsync(
            request.SchoolId,
            request.SearchTerm,
            request.GradeLevelId,
            request.ClassRoomId,
            pagination,
            ct);

        return Result.Success(students);
    }
}