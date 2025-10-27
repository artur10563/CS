using Grpc.Core;
using LR4_gRPC_Server;
using System.Collections.Concurrent;

namespace LR4_gRPC_Chat_Server.Services;

public class ChatServiceImpl : ChatService.ChatServiceBase
{
    private static readonly ConcurrentDictionary<int, Room> _rooms = new();
    private static readonly ConcurrentDictionary<int, ConcurrentDictionary<string, IServerStreamWriter<ChatMessage>>> _roomStreams = new();
    private static int _roomCounter = 0;

    public override Task<RoomCreatedResponse> CreateNewRoom(Room request, ServerCallContext context)
    {
        var room = new Room
        {
            Id = Interlocked.Increment(ref _roomCounter),
            Name = request.Name
        };

        _rooms[room.Id] = room;
        _roomStreams.TryAdd(room.Id, new ConcurrentDictionary<string, IServerStreamWriter<ChatMessage>>());

        Console.WriteLine($"[Room Created] #{room.Id} {room.Name}");
        return Task.FromResult(new RoomCreatedResponse { Room = room });
    }

    public override async Task JoinRoom(JoinRoomRequest request, IServerStreamWriter<ChatMessage> responseStream, ServerCallContext context)
    {
        if (!_roomStreams.TryGetValue(request.RoomId, out var users))
            throw new RpcException(new Status(StatusCode.NotFound, "Room not found"));

        users[request.UserName] = responseStream;

        Console.WriteLine($"[Join] {request.UserName} joined room #{request.RoomId}");

        // Notify everyone
        await BroadcastAsync(request.RoomId, new ChatMessage
        {
            RoomId = request.RoomId,
            UserName = "System",
            Message = $"{request.UserName} joined the room.",
            Timestamp = DateTime.UtcNow.ToString("O")
        });

        try
        {
            while (!context.CancellationToken.IsCancellationRequested)
                await Task.Delay(1000, context.CancellationToken);
        }
        catch (TaskCanceledException)
        {
            // Expected on disconnect
        }
        finally
        {
            users.TryRemove(request.UserName, out _);
            Console.WriteLine($"[Disconnect] {request.UserName} disconnected from room #{request.RoomId}");
        }
    }

    public override async Task<Empty> LeaveRoom(LeaveRoomRequest request, ServerCallContext context)
    {
        if (_roomStreams.TryGetValue(request.RoomId, out var users))
        {
            if (users.TryRemove(request.UserName, out _))
            {
                Console.WriteLine($"[Leave] {request.UserName} left room #{request.RoomId}");
                await BroadcastAsync(request.RoomId, new ChatMessage
                {
                    RoomId = request.RoomId,
                    UserName = "System",
                    Message = $"{request.UserName} left the room.",
                    Timestamp = DateTime.UtcNow.ToString("O")
                });
            }
        }

        return new Empty();
    }

    public override async Task<Empty> SendMessage(MessageRequest request, ServerCallContext context)
    {
        var msg = new ChatMessage
        {
            RoomId = request.RoomId,
            UserName = request.UserName,
            Message = request.Message,
            Timestamp = DateTime.UtcNow.ToString("O")
        };

        await BroadcastAsync(request.RoomId, msg);
        return new Empty();
    }

    private static async Task BroadcastAsync(int roomId, ChatMessage msg)
    {
        if (!_roomStreams.TryGetValue(roomId, out var users))
            return;

        foreach (var (user, stream) in users)
        {
            try
            {
                await stream.WriteAsync(msg);
            }
            catch
            {
                users.TryRemove(user, out _);
            }
        }
    }
}
