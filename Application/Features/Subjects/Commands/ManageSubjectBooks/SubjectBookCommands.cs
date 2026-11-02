using Application.Common.Behaviors;
using Application.Common.Interfaces;

namespace Application.Features.Subjects.Commands.ManageSubjectBooks;

public record CreateSubjectBookCommand(
    Guid SchoolId,
    Guid CurriculumSubjectId,
    string Title,
    string? Author = null,
    string? Publisher = null,
    string? Isbn = null,
    string? Edition = null,
    string? Notes = null) : ICommand<Guid>, IBaseCommand, ITenantScopedRequest;

public record UpdateSubjectBookCommand(
    Guid SchoolId,
    Guid CurriculumSubjectId,
    Guid BookId,
    string Title,
    string? Author = null,
    string? Publisher = null,
    string? Isbn = null,
    string? Edition = null,
    string? Notes = null) : ICommand, IBaseCommand, ITenantScopedRequest;

public record DeleteSubjectBookCommand(Guid SchoolId, Guid CurriculumSubjectId, Guid BookId)
    : ICommand, IBaseCommand, ITenantScopedRequest;
