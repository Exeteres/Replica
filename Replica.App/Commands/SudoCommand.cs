using System.Linq;
using System.Threading.Tasks;
using Replica.App.Extensions;
using Replica.App.Models;
using Replica.App.Permissions;
using Replica.Core.Commands;
using Replica.Core.Entity;
using Replica.Core.Extensions;

namespace Replica.App.Commands
{
    [Command("sudo", "s")]
    [Disallow]
    public class SudoCommand : CommandBase
    {
        public OutMessage CreateChat(string controller, long chatId, User owner = null)
        {
            var db = new RepoContext();
            if (db.Chats.Any(x => x.ChatId == chatId && x.Controller == controller))
                return OutMessage.FromCode("Already exists");
            owner ??= Context.GetUser();
            var chat = new Chat { Controller = controller, ChatId = chatId };
            var member = new ChatMember
            {
                Scope = UserScope.Owner,
                User = owner,
                Chat = chat
            };
            db.Update(member);
            db.SaveChanges();
            return OutMessage.FromCode("Chat created " + chat.Id);
        }

        public OutMessage Routes(int? page = null)
        {
            var db = new RepoContext();
            if (!page.HasValue)
            {
                var count = db.Routes.Count();
                return OutMessage.FromCode($"Found {count} routes and {(count / 20) + 1} pages");
            }
            var localizer = Context.GetLocalizer();
            var routes = db.Routes.Skip(page.Value * 20).Take(20).ToList();
            return OutMessage.FromCode($"{localizer["Routes"]} [{page}]:\n" + string.Join("\n", routes.Select(x => x.RenderInfo())));
        }

        public async Task<OutMessage> Chats(int? page = null)
        {
            var db = new RepoContext();
            if (!page.HasValue)
            {
                var count = db.Chats.Count();
                return OutMessage.FromCode($"Found {count} chats and {(count / 20) + 1} pages");
            }
            var localizer = Context.GetLocalizer();
            var chats = db.Chats.Skip(page.Value * 20).Take(20).ToList();
            return OutMessage.FromCode($"{localizer["Chats"]} [{page}]:\n" + string.Join("\n",
                await chats.SelectAsync(async x => $"[{x.FormattedId}] [{x.Controller}] {(await x.GetInfo()).Title}")));
        }
    }
}