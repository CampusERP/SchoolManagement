using Domain.Entities.Notifications;

namespace Application.Common.Interfaces.Repositories;

public interface IDeviceTokenRepository
{
    Task<DeviceToken?> GetByTokenAsync(Guid userId, string token, CancellationToken ct = default);
    void Add(DeviceToken deviceToken);
}
