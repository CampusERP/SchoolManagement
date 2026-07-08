using Application.Common.Behaviors;

namespace Application.Features.Academics.CreateRoom;

public record CreateRoomCommand(
    string Name,
    int Capacity) : ICommand<Guid>, IBaseCommand;
