using Application.Common.Models;
using MediatR;

namespace Application.Features.Identity.SwitchSchool;

public record GetUserMembershipsQuery : IRequest<Result<List<UserMembershipDto>>>;
