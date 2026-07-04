using Application.Common.Behaviors;

namespace Application.Features.Schools.Commands.CreateSchool;

// ── Command ──────────────────────────────────────────────────────────
public record CreateSchoolCommand(string Name, string SubdomainCode) : ICommand<Guid>, IBaseCommand;