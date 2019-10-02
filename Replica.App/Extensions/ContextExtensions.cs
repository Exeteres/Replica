using System.Linq;
using Replica.App.Models;
using Replica.Core.Contexts;

namespace Replica.App.Extensions
{
    public static class ContextExtensions
    {
        public static User GetUser(this Context context)
        {
            var db = new RepoContext();
            return db.Users.FirstOrDefault(x => x.Username == context.Message.Sender.Username.ToLower());
        }

        public static Chat GetChat(this Context context, bool populate = false)
        {
            var db = new RepoContext();
            return db.Chats.FirstOrDefault(x => x.ChatId == context.Message.Chat.Id);
        }

        public static ChatMember GetMember(this Context context, User user = null, Chat chat = null)
        {
            user ??= context.GetUser();
            chat ??= context.GetChat();
            if (user == null || chat == null) return null;
            var db = new RepoContext();
            return db.ChatMembers.FirstOrDefault(x => x.ChatId == chat.Id && x.UserId == user.Id);
        }
    }
}