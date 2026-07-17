using Application.Common.Models;
using Application.Features.Schools.Queries.GetAllSchools;
using Application.Features.Schools.Queries.GetPlatformAnalytics;
using Application.Features.Schools.Queries.GetSchoolById;
using Application.Features.Schools.Queries.GetSchoolDashboard;

namespace Application.Common.Interfaces.Services;

public interface ISchoolReadService
{
    Task<PagedResult<SchoolListDto>> GetSchoolsAsync(PaginationParams pagination, CancellationToken ct = default);
    Task<SchoolDetailDto?> GetSchoolByIdAsync(Guid schoolId, CancellationToken ct = default);
    Task<SchoolDashboardDto?> GetSchoolDashboardAsync(Guid schoolId, CancellationToken ct = default);
    Task<PlatformAnalyticsDto> GetPlatformAnalyticsAsync(CancellationToken ct = default);
}
