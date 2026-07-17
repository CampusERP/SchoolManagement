using Application.Common.Interfaces.Services;
using Application.Common.Models;
using MediatR;

namespace Application.Features.People.Queries.GetTeachers;

public class GetTeachersQueryHandler
    : IRequestHandler<GetTeachersQuery, Result<PagedResult<TeacherListDto>>>
{
    private readonly ITeacherReadService _teacherReadService;

    public GetTeachersQueryHandler(ITeacherReadService teacherReadService)
    {
        _teacherReadService = teacherReadService;
    }

    public async Task<Result<PagedResult<TeacherListDto>>> Handle(
        GetTeachersQuery request, CancellationToken ct)
    {
        var pagination = request.Pagination ?? new PaginationParams();
        var teachers = await _teacherReadService.GetTeachersAsync(request.SchoolId, request.SearchTerm, pagination, ct);

        return Result.Success(teachers);
    }
}
