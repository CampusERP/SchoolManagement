using Application.Common.Behaviors;
using Application.Common.Interfaces;
using MediatR;

namespace Application.Features.Identity.ResetPassword;

public record ResetPasswordCommand(string Email, string Token, string NewPassword)
    : ICommand, IBaseCommand;
