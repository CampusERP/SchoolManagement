using Application.Common.Interfaces.Services;
using Application.Common.Models;
using MediatR;

namespace Application.Features.Assignments.Queries.GetClassAssignments;

public class GetStudentAssignmentsQueryHandler
    : IRequestHandler<GetStudentAssignmentsQuery, Result<PagedResult<StudentAssignmentDto>>>
{
    private readonly IAssignmentReadService _readService;

    public GetStudentAssignmentsQueryHandler(IAssignmentReadService readService) => _readService = readService;

    public async Task<Result<PagedResult<StudentAssignmentDto>>> Handle(
        GetStudentAssignmentsQuery request, CancellationToken ct)
    {
        var p = request.Pagination ?? new PaginationParams();
        var result = await _readService.GetStudentAssignmentsAsync(request.StudentEnrollmentId, p, ct);
        return Result.Success(result);
    }
}
