using Application.Common.Models;
using MediatR;

namespace Application.Features.Identity.Login;

public record LoginCommand(string Email, string Password, Guid? SchoolId = null) : ICommand<LoginResponse>;
