using Domain.Entities.Academics;

namespace Application.Common.Interfaces.Repositories;

public interface ISubjectRepository
{
    Task<bool> ExistsAsync(string code, CancellationToken ct);
    void Add(Subject subject);
}
