using Application.Common.Interfaces.Services;
using Application.Common.Models;
using MediatR;

namespace Application.Features.People.Queries.StudentDetails;

public class GetStudentByIdQueryHandler
    : IRequestHandler<GetStudentByIdQuery, Result<StudentDetailDto>>
{
    private readonly IStudentReadService _studentReadService;
    public GetStudentByIdQueryHandler(IStudentReadService studentReadService) => _studentReadService = studentReadService;

    public async Task<Result<StudentDetailDto>> Handle(
        GetStudentByIdQuery request, CancellationToken ct)
    {
        var student = await _studentReadService.GetStudentDetailsAsync(request.SchoolId, request.StudentId, ct);

        if (student is null) return Result.Failure<StudentDetailDto>("Student not found.");
        return Result.Success(student);
    }
}