using Application.Common.Behaviors;
using Application.Common.Interfaces;

namespace Application.Features.Schools.Commands.UpdateSchool;

public record UpdateSchoolCommand(
    Guid SchoolId,
    string Name) : ICommand, IBaseCommand;
