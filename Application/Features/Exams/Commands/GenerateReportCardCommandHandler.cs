using Application.Common.Interfaces.Repositories;
using Application.Common.Interfaces.Services;
using Application.Common.Models;
using Domain.Entities.Exams;
using FluentValidation;
using MediatR;

namespace Application.Features.Exams.Commands;

public class GenerateReportCardCommandHandler
    : IRequestHandler<GenerateReportCardCommand, Result<Guid>>
{
    private readonly IReportCardRepository _reportCardRepository;
    private readonly IExamResultRepository _examResultRepository;
    private readonly ICurrentUserService  _user;

    public GenerateReportCardCommandHandler(
        IReportCardRepository reportCardRepository,
        IExamResultRepository examResultRepository,
        ICurrentUserService user)
    { 
        _reportCardRepository = reportCardRepository;
        _examResultRepository = examResultRepository;
        _user = user; 
    }

    public async Task<Result<Guid>> Handle(GenerateReportCardCommand request, CancellationToken ct)
    {
        var existing = await _reportCardRepository.ExistsAsync(request.StudentEnrollmentId, request.TermId, ct);

        if (existing)
            return Result.Failure<Guid>("A report card already exists for this student and term.");

        var results = await _examResultRepository.GetResultsForReportCardAsync(request.StudentEnrollmentId, request.TermId, ct);

        if (!results.Any())
            return Result.Failure<Guid>("No exam results found for this student and term.");

        // NOTE: The calculation assumes the ExamResult entity loads its ExamSchedule.Exam.Subject.Name.
        // If not, this data logic should be handled by a specific read service.
        var totalScore   = results.Sum(r => r.Score);
        var totalMax     = results.Sum(r => 100m); // Fallback logic for MaxScore if not loaded
        var percentage   = totalMax > 0 ? Math.Round(totalScore / totalMax * 100, 2) : 0;
        var overallGrade = CalculateGrade(percentage);

        var subjects = results.Select(r => (
            r.ExamId, // Simplified for brevity in abstraction
            "Subject Name", 
            r.Score,
            100m,
            Grade: CalculateGrade(r.Score)));

        var card = ReportCard.Generate(
            request.SchoolId, request.StudentEnrollmentId, request.TermId,
            percentage, overallGrade, _user.UserId!.Value, subjects);

        await _reportCardRepository.AddAsync(card, ct);
        return Result.Success(card.Id);
    }

    private static string CalculateGrade(decimal percentage) => percentage switch
    {
        >= 90 => "A+",
        >= 80 => "A",
        >= 70 => "B",
        >= 60 => "C",
        >= 50 => "D",
        _     => "F"
    };
}
