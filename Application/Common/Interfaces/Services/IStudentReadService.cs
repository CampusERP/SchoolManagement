using Application.Common.Models;
using Application.Features.People.Queries.GetStudents;
using Application.Features.People.Queries.StudentDetails;

namespace Application.Common.Interfaces.Services;

public interface IStudentReadService
{
    Task<PagedResult<StudentListDto>> GetStudentsAsync(
        Guid schoolId,
        string? searchTerm,
        Guid? gradeLevelId,
        Guid? classRoomId,
        PaginationParams pagination,
        CancellationToken cancellationToken);

    Task<StudentDetailDto?> GetStudentDetailsAsync(Guid schoolId, Guid studentId, CancellationToken cancellationToken);
    Task<StudentDetailDto?> GetMyProfileAsync(Guid schoolId, Guid userId, CancellationToken cancellationToken);
}