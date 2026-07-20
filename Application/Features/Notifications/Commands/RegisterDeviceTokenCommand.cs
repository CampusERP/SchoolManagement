using Application.Common.Models;
using MediatR;

namespace Application.Features.Notifications.Commands;

public record RegisterDeviceTokenCommand(string Platform, string Token)
    : ICommand;
