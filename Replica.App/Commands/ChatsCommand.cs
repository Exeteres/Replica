using System.Threading.Tasks;
using Replica.App.Models;
using Replica.Core.Commands;
using Replica.Core.Entity;
using Replica.Core.Extensions;
using Replica.App.Permissions;
using Replica.App.Extensions;
using System.Linq;
using Replica.App.Logic;

namespace Replica.App.Commands
{
    [Command("chats", "c")]
    public class ChatsCommand : CommandBase
    {
        public async Task<OutMessage> Default()
        {
            var localizer = Context.GetLocalizer();
            var user = Context.GetUser();

            var response = localizer["Chats"] + (user.Chats.Any() ? ":\n" : " " + localizer["NotFound"]);
            foreach (var position in user.Chats)
            {
                var chat = position.Chat;
                response += $"[{chat.FormattedId}] [{chat.Controller}] [{position.Scope}] {(await chat.GetInfo()).Title}\n";
            }
            return OutMessage.FromCode(response);
        }

        public async Task<OutMessage> Init()
        {
            var localizer = Context.GetLocalizer();
            var chat = Context.GetChat();
            var db = new RepoContext();

            if (chat != null)
                return OutMessage.FromCode(localizer["ChatAlreadyInitialized"]);

            chat = new Chat
            {
                Controller = Controller.Name,
                ChatId = Message.Chat.Id,
                Language = Message.Sender.Language,
                Notifications = true
            };
            db.Update(chat);
            db.SaveChanges();

            var user = Context.GetUser();

            var result = $"{localizer["ChatInitialized"]}";

            var info = await Controller.GetChatInfo(chat.ChatId);
            if (info.Owner == 0)
            {
                user.AssignPermissions(chat, UserScope.Owner);
                result += $"\n{user.Username} {localizer["AssignedAsOwner"]}\n{localizer["UnableToAssignOwner"]}";
                return OutMessage.FromCode(result);
            }

            var uinfo = await Controller.GetUserInfo(info.Owner);
            var owner = User.EnsureCreated(Controller.Name, uinfo);

            owner.AssignPermissions(chat, UserScope.Owner);

            if (owner.Id == user.Id)
            {
                result += $"\n{user.Username} {localizer["AssignedAsOwner"]}";
                return OutMessage.FromCode(result);
            }

            user.AssignPermissions(chat, UserScope.Admin);

            result += $"\n{user.Username} {localizer["AssignedAsAdmin"]}\n{owner.Username} {localizer["AssignedAsOwner"]}";
            return OutMessage.FromCode(result);
        }

        public OutMessage Destroy([Protect(UserScope.Owner)] Chat chat = null)
        {
            var localizer = Context.GetLocalizer();
            var db = new RepoContext();

            chat ??= Context.GetChat();
            if (chat == null) return OutMessage.FromCode(localizer["ChatNotFound"]);
            var routes = chat.Routes.Select(x => x.Route).ToList();
            db.Remove(chat);
            db.SaveChanges();
            routes.ForEach(x => x.DeleteIfNotUsed());
            return OutMessage.FromCode(localizer["ChatDestroyed"]);
        }

        public OutMessage Notifications([Protect(UserScope.Admin)] Chat chat, bool value)
        {
            var db = new RepoContext();
            var localizer = Context.GetLocalizer();

            chat.Notifications = value;
            db.Update(chat);
            db.SaveChanges();

            return OutMessage.FromCode(value ? localizer["NotificationsEnabled"] : localizer["NotificationsDisabled"]);
        }

        public async Task<OutMessage> Info([Protect(UserScope.None)] Chat chat = null)
        {
            chat ??= Context.GetChat();
            var localizer = Context.GetLocalizer();

            if (chat == null)
                return OutMessage.FromCode(localizer["ChatNotFound"]);

            var owner = chat.GetOwner();
            var members = chat.Members;

            var (admins, moderators) = members.SelectTuple(
                x => x.Scope == UserScope.Admin ? x.User : null,
                x => x.Scope == UserScope.Moderator ? x.User : null);

            var response = $"[{chat.Controller}] {(await chat.GetInfo()).Title}\n";
            response += localizer["Notifications"] + ": " + (chat.Notifications ? localizer["Enabled"] : localizer["Disabled"]) + "\n";
            response += localizer["Owner"] + ": " + (owner != null ? owner.Username : localizer["NotAssigned"]) + "\n";

            response += localizer["Admins"];
            response += RenderHelper.RenderList(admins
                .Select(x => x.Username), localizer["NotAssignedPlural"]);

            response += localizer["Moderators"];
            response += RenderHelper.RenderList(moderators
                .Select(x => x.Username), localizer["NotAssignedPlural"]);

            response += localizer["Members"];
            response += RenderHelper.RenderList(members
                .Select(x => x.User.Username), localizer["NotAssignedPlural"]);

            return OutMessage.FromCode(response);
        }
    }
}