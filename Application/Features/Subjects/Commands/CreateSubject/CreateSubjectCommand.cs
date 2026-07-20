using MediatR;
using Application.Common.Models;

namespace Application.Features.Subjects.Commands.CreateSubject;

public record CreateSubjectCommand(string Code, string Name, string? Description)
    : ICommand<Guid>;
