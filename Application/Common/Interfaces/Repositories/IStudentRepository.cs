using Application.Common.Models;
using Application.Features.People.Queries.GetStudents;
using Domain.Entities.People;

namespace Application.Common.Interfaces.Repositories;

public interface IStudentRepository
{
    Task<Student?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<bool> ExistsAsync(Guid schoolId, string studentCode, CancellationToken ct = default);
    Task AddAsync(Student student, CancellationToken ct = default);

    Task<PagedResult<GetStudentsDto>> GetStudentsAsync(
        Guid schoolId,
        Guid? academicYearId,
        PaginationParams pagination,
        CancellationToken ct = default);
}