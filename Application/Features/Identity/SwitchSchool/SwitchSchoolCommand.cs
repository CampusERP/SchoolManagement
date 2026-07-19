using Application.Common.Models;
using MediatR;

namespace Application.Features.Identity.SwitchSchool;

public record SwitchSchoolCommand(Guid TargetSchoolId)
    : ICommand<SwitchSchoolResponse>;
