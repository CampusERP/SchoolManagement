using MediatR;
using Application.Common.Models;
using Domain.Entities.Academics;
using Application.Common.Interfaces.Repositories;

namespace Application.Features.Subjects.Commands.CreateSubject;

public class CreateSubjectCommandHandler : IRequestHandler<CreateSubjectCommand, Result<Guid>>
{
    private readonly ISubjectRepository _subjectRepository;

    public CreateSubjectCommandHandler(ISubjectRepository subjectRepository) => _subjectRepository = subjectRepository;

    public async Task<Result<Guid>> Handle(CreateSubjectCommand request, CancellationToken ct)
    {
        var exists = await _subjectRepository.ExistsAsync(request.Code.ToUpperInvariant(), ct);
        if (exists)
            return Result.Failure<Guid>($"Subject code '{request.Code}' already exists.");

        var subject = Subject.Create(request.Code, request.Name, request.Description);
        _subjectRepository.Add(subject);
        return Result.Success(subject.Id);
    }
}
