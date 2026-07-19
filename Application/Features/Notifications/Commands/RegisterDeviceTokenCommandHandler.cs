using Application.Common.Interfaces.Repositories;
using Application.Common.Interfaces.Services;
using Application.Common.Models;
using Domain.Entities.Notifications;
using MediatR;

namespace Application.Features.Notifications.Commands;

public class RegisterDeviceTokenCommandHandler
    : IRequestHandler<RegisterDeviceTokenCommand, Result>
{
    private readonly IDeviceTokenRepository _repository;
    private readonly ICurrentUserService  _user;

    public RegisterDeviceTokenCommandHandler(IDeviceTokenRepository repository, ICurrentUserService user)
    { _repository = repository; _user = user; }

    public async Task<Result> Handle(RegisterDeviceTokenCommand request, CancellationToken ct)
    {
        var userId = _user.UserId;
        if (userId is null) return Result.Failure("Not authenticated.");

        var existing = await _repository.GetByTokenAsync(userId.Value, request.Token, ct);

        if (existing is not null)
        {
            existing.RefreshLastSeen();
            return Result.Success();
        }

        var token = DeviceToken.Create(userId.Value, request.Platform, request.Token);
        _repository.Add(token);
        return Result.Success();
    }
}
