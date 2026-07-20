using Application.Common.Behaviors;
using Application.Common.Interfaces;
using MediatR;

namespace Application.Features.Identity.ForgotPassword;

public record ForgotPasswordCommand(string Email)
    : ICommand, IBaseCommand;
