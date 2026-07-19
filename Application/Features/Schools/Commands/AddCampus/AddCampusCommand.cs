using Application.Common.Behaviors;
using Application.Common.Interfaces;
using Application.Common.Models;
using MediatR;

namespace Application.Features.Schools.Commands.AddCampus;

public record AddCampusCommand(Guid SchoolId, string Name, string Address)
    : ICommand<Guid>, ITenantScopedRequest;
