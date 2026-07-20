using Application.Common.Models;
using MediatR;

namespace Application.Features.Schools.Commands.SchoolActivation;

public record SuspendSchoolCommand(Guid SchoolId) : ICommand;
