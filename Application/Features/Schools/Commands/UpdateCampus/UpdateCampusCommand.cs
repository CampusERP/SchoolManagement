using Application.Common.Behaviors;
using Application.Common.Interfaces;

namespace Application.Features.Schools.Commands.UpdateCampus;

public record UpdateCampusCommand(Guid SchoolId, Guid CampusId, string Name, string Address)
    : ICommand, IBaseCommand, ITenantScopedRequest;
