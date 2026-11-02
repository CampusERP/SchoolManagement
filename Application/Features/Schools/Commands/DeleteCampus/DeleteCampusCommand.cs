using Application.Common.Behaviors;
using Application.Common.Interfaces;

namespace Application.Features.Schools.Commands.DeleteCampus;

public record DeleteCampusCommand(Guid SchoolId, Guid CampusId)
    : ICommand, IBaseCommand, ITenantScopedRequest;
