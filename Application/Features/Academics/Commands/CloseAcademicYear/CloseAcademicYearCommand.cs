using Application.Common.Behaviors;
using Application.Common.Interfaces;
using Application.Common.Models;
using MediatR;

namespace Application.Features.Academics.Commands.CloseAcademicYear;

public record CloseAcademicYearCommand(Guid SchoolId, Guid AcademicYearId)
    : ICommand, ITenantScopedRequest;
