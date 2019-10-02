using Microsoft.EntityFrameworkCore;
using Replica.Core.Entity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Replica.App.Models
{
    public class Chat : Model
    {
        public long ChatId { get; set; }
        public string Controller { get; set; }

        public string FormattedId => Id.ToString("D2");

        public virtual List<ChatMember> Members { get; set; } = new List<ChatMember>();
        public virtual List<ChatRoute> Routes { get; set; } = new List<ChatRoute>();
        public string Language { get; set; }
        public bool Notifications { get; set; }

        public Task<ChatInfo> GetInfo()
            => Program.Core
                .ResolveController(Controller)
                .GetChatInfo(ChatId);

        public User GetOwner()
            => Members.FirstOrDefault(x => x.Scope == UserScope.Owner)?.User;

        public static Chat Resolve(string controller, long chatId)
        {
            var db = new RepoContext();
            return db.Chats.FirstOrDefault(x => x.Controller == controller && x.ChatId == chatId);
        }

        public static void EnsureContained(string controller, ChatInfo chat, User user)
        {
            var db = new RepoContext();
            var existed = Resolve(controller, chat.Id);
            if (existed == null) return;
            if (!existed.Members.Any(x => x.UserId == user.Id))
            {
                user.AssignPermissions(existed, UserScope.None);
                db.Update(user);
                db.SaveChanges();
            }
        }

        public IEnumerable<Chat> GetDestinations()
        {
            var db = new RepoContext();
            var routes = db.Routes.Where(x => x.Chats.Any(x => x.ChatId == Id));
            var temp = routes
                .Select(x => x.Chats.AsEnumerable());
            if (temp.Count() < 1) return new Chat[0];
            var chats = temp.ToList()
                .Aggregate((a, b) => a.Concat(b))
                .ToList();
            chats.Distinct();
            chats.RemoveAll(x => x.ChatId == Id);
            return chats.Select(x => x.Chat);
        }

        #region Equality
        public override int GetHashCode()
        {
            if (Id != 0) return Id.GetHashCode();
            return HashCode.Combine(Controller, ChatId);
        }

        public override bool Equals(object obj)
        {
            if (obj is Chat other)
            {
                if (Id != 0 && other.Id != 0)
                    return Id == other.Id;
                return other.Controller == Controller && other.ChatId == ChatId;
            }
            return false;
        }
        #endregion
    }
}