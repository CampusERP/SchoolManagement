using Application.Common.Interfaces.Services;
using Application.Common.Models;
using MediatR;

namespace Application.Features.People.Queries.GetTeacherById;

public class GetTeacherByIdQueryHandler
    : IRequestHandler<GetTeacherByIdQuery, Result<TeacherDetailDto>>
{
    private readonly ITeacherReadService _teacherReadService;

    public GetTeacherByIdQueryHandler(ITeacherReadService teacherReadService)
    {
        _teacherReadService = teacherReadService;
    }

    public async Task<Result<TeacherDetailDto>> Handle(
        GetTeacherByIdQuery request, CancellationToken ct)
    {
        var teacher = await _teacherReadService.GetTeacherByIdAsync(request.SchoolId, request.TeacherId, ct);

        if (teacher is null) return Result.Failure<TeacherDetailDto>("Teacher not found.");
        return Result.Success(teacher);
    }
}
