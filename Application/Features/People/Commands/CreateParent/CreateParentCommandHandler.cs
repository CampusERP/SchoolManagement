using Application.Common.Interfaces;
using Application.Common.Interfaces.Repositories;
using Application.Common.Interfaces.Services;
using Application.Common.Messages;
using Application.Common.Models;
using Domain.Entities.People;
using Domain.Entities.Tenancy;
using MediatR;
using System.Security.Principal;

namespace Application.Features.People.Commands.CreateParent;

public class CreateParentCommandHandler : IRequestHandler<CreateParentCommand, Result<Guid>>
{
    private readonly IParentRepository _parents;
    private readonly ISchoolRepository _schools;
    private readonly IIdentityService _identityService;
    private readonly IOutboxService _outbox;
    private readonly IEmailNotifier _emailNotifier;

    public CreateParentCommandHandler(
        IParentRepository parents,
        ISchoolRepository schools,
        IIdentityService identityService,
        IOutboxService outbox,
        IEmailNotifier emailNotifier)
    {
        _parents = parents;
        _schools = schools;
        _identityService = identityService;
        _outbox = outbox;
        _emailNotifier = emailNotifier;
    }

    public async Task<Result<Guid>> Handle(CreateParentCommand request, CancellationToken ct)
    {
        var userResult = await _identityService.CreateUserAsync(request.Email, request.Password, ct);
        if (!userResult.IsSuccess)
            return Result.Failure<Guid>(userResult.Error!);

        var userId = userResult.Value;

        try
        {
            await _identityService.AddToRoleAsync(userId, "Parent", ct);

            var parent = Parent.Create(request.SchoolId, userId,
                request.FirstName, request.LastName);

            await _parents.AddAsync(parent, ct);

            _outbox.Publish(new LinkParentLoginMessage(parent.Id, userId));

            var school = await _schools.GetByIdAsync(request.SchoolId, ct);
            await _emailNotifier.SendAsync(
                userId,
                EmailTemplate.Welcome,
                new Dictionary<string, string?>
                {
                    ["Name"] = $"{request.FirstName} {request.LastName}".Trim(),
                    ["Role"] = "Parent",
                    ["SchoolName"] = school?.Name ?? "your school",
                    ["Email"] = request.Email,
                    ["Password"] = request.Password,
                    ["LoginUrl"] = $"{_emailNotifier.AppBaseUrl}/auth/login"
                },
                ct);

            return Result.Success(parent.Id);
        }
        catch
        {
            await _identityService.DeleteUserAsync(userId, ct);
            throw;
        }
    }
}