using Application.Common.Exceptions;
using Application.Common.Interfaces.Repositories;
using Application.Common.Models;
using MediatR;

namespace Application.Features.People.Commands.StudentActivation;

public class WithdrawStudentCommandHandler : IRequestHandler<WithdrawStudentCommand, Result>
{
    private readonly IStudentEnrollmentRepository _enrollments;
    public WithdrawStudentCommandHandler(IStudentEnrollmentRepository enrollments)
        => _enrollments = enrollments;

    public async Task<Result> Handle(WithdrawStudentCommand request, CancellationToken ct)
    {
        var enrollment = await _enrollments.GetByIdAsync(request.StudentEnrollmentId, ct);
        if (enrollment is null)
            throw new NotFoundException("StudentEnrollment", request.StudentEnrollmentId);
        try { enrollment.Withdraw(); return Result.Success(); }
        catch (Domain.Exceptions.DomainException ex) { return Result.Failure(ex.Message); }
    }
}