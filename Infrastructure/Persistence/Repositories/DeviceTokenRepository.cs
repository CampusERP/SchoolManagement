using Application.Common.Interfaces.Repositories;
using Domain.Entities.Notifications;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence.Repositories;

public class DeviceTokenRepository : IDeviceTokenRepository
{
    private readonly ApplicationDbContext _db;
    public DeviceTokenRepository(ApplicationDbContext db) => _db = db;

    public async Task<DeviceToken?> GetByTokenAsync(Guid userId, string token, CancellationToken ct = default) =>
        await _db.DeviceTokens.FirstOrDefaultAsync(t => t.ApplicationUserId == userId && t.Token == token, ct);

    public void Add(DeviceToken deviceToken) => _db.DeviceTokens.Add(deviceToken);
}
