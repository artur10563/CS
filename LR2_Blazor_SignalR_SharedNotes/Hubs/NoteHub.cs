using Microsoft.AspNetCore.SignalR;

namespace LR2_Blazor_SignalR_SharedNotes.Hubs
{
    public class Note
    {
        public int Id { get; set; }
        public string Description { get; set; }
        public User Author { get; set; }
    }

    public class User
    {
        public Guid Guid { get; set; } = Guid.NewGuid();
        public string ConnectionId { get; set; }
        public string Name { get; set; }
    }

    public class NoteHub : Hub
    {
        private static readonly List<User> _users = new();
        private static readonly List<Note> _notes = new();
        private static int _noteCounter = 1;

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            var user = _users.FirstOrDefault(u => u.ConnectionId == Context.ConnectionId);
            if (user != null)
            {
                _users.Remove(user);
                await Clients.All.SendAsync("UserLeft", user.Name);
            }

            await base.OnDisconnectedAsync(exception);
        }

        public async Task Join(string userName)
        {
            if (string.IsNullOrWhiteSpace(userName)) return;

            var user = new User
            {
                ConnectionId = Context.ConnectionId,
                Name = userName
            };

            _users.Add(user);

            await Clients.Caller.SendAsync("LoadNotes", _notes);
            await Clients.All.SendAsync("UserJoined", user.Name);
        }

        public async Task CreateNote(string description)
        {
            var user = _users.FirstOrDefault(x => x.ConnectionId == Context.ConnectionId);
            if (user is null || string.IsNullOrWhiteSpace(description)) return;

            var note = new Note
            {
                Id = _noteCounter++,
                Author = user,
                Description = description
            };

            _notes.Add(note);
            await Clients.All.SendAsync("NewNoteCreated", note);
        }
    }
}
