using Application.Common.Models;
using MediatR;

namespace Application.Features.SchoolRegistrationRequests.Queries.GetSchoolRegistrationRequestById;

public record GetSchoolRegistrationRequestByIdQuery(Guid Id)
    : IRequest<Result<SchoolRegistrationRequestDto>>;
