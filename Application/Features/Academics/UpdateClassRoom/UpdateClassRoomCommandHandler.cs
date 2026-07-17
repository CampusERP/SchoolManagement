using Application.Common.Interfaces.Repositories;
using Application.Common.Models;
using MediatR;

namespace Application.Features.Academics.UpdateClassRoom;

public class UpdateClassRoomCommandHandler : IRequestHandler<UpdateClassRoomCommand, Result>
{
    private readonly IClassRoomRepository _classRooms;

    public UpdateClassRoomCommandHandler(IClassRoomRepository classRooms)
    {
        _classRooms = classRooms;
    }

    public async Task<Result> Handle(UpdateClassRoomCommand request, CancellationToken ct)
    {
        var classRoom = await _classRooms.GetByIdAsync(request.ClassRoomId, ct);
        if (classRoom is null) return Result.Failure("Classroom not found.");

        try
        {
            classRoom.Update(request.Name);
        }
        catch (Exception ex)
        {
            return Result.Failure(ex.Message);
        }

        return Result.Success();
    }
}
