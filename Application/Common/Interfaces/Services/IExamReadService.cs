using Application.Common.Models;
using Application.Features.Exams.Queries;

namespace Application.Common.Interfaces.Services;

public interface IExamReadService
{
    Task<PagedResult<ExamListDto>> GetExamsAsync(Guid schoolId, Guid termId, PaginationParams p, CancellationToken ct = default);
    Task<PagedResult<ReportCardDto>> GetReportCardsAsync(Guid studentEnrollmentId, PaginationParams p, CancellationToken ct = default);
    Task<PagedResult<ExamResultDto>> GetStudentExamResultsAsync(Guid studentEnrollmentId, Guid? termId, PaginationParams p, CancellationToken ct = default);
    Task<List<ClassExamResultDto>> GetClassExamResultsAsync(Guid examScheduleId, CancellationToken ct = default);
    Task<List<ExamScheduleDto>> GetExamSchedulesAsync(Guid examId, CancellationToken ct = default);
}
