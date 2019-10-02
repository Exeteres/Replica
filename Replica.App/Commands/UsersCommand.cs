
using Replica.App.Extensions;
using Replica.App.Models;
using Replica.App.Permissions;
using Replica.Core.Commands;
using Replica.Core.Entity;

namespace Replica.App.Commands
{
    [Command("users", "u")]
    public class UsersCommand : CommandBase
    {
        // public void Info(User user = null)
        //     => this.EnterView("user", user ?? Context.GetUser());

        public OutMessage Mute(Chat chat = null)
        {
            var me = Context.GetMember(null, chat);
            var localizer = Context.GetLocalizer();

            if (me.Mute > MuteState.None)
                return OutMessage.FromCode(localizer["YouAlreadyMuted"]);
            var db = new RepoContext();
            me.Mute = MuteState.Muted;
            db.Update(me);
            db.SaveChanges();
            return OutMessage.FromCode(localizer["YouMuted"]);
        }

        public OutMessage Unmute(Chat chat = null)
        {
            var me = Context.GetMember(null, chat);
            var localizer = Context.GetLocalizer();

            if (me.Mute == MuteState.Blocked)
                return OutMessage.FromCode(localizer["CantUnmute"]);

            if (me.Mute == MuteState.None)
                return OutMessage.FromCode(localizer["NotMuted"]);

            var db = new RepoContext();
            me.Mute = MuteState.None;
            db.Update(me);
            db.SaveChanges();
            return OutMessage.FromCode(localizer["YouUnmuted"]);
        }

        public OutMessage Mute([Protect(UserScope.Moderator)] Chat chat, User user)
        {
            var localizer = Context.GetLocalizer();
            var target = Context.GetMember(user, chat);

            if (target == null)
                return OutMessage.FromCode(localizer["TargetNotFound"]);

            var db = new RepoContext();
            target.Mute = MuteState.Blocked;
            db.Update(target);
            db.SaveChanges();
            return OutMessage.FromCode(localizer["UserMuted"]);
        }

        public OutMessage Unmute([Protect(UserScope.Moderator)] Chat chat, User user)
        {
            var localizer = Context.GetLocalizer();
            var target = Context.GetMember(user, chat);

            if (target == null)
                return OutMessage.FromCode(localizer["TargetNotFound"]);

            var db = new RepoContext();
            target.Mute = MuteState.None;
            db.Update(target);
            db.SaveChanges();
            return OutMessage.FromCode(localizer["UserUnmuted"]);
        }

        public OutMessage Scope([Protect(UserScope.Admin)] Chat chat, User user, UserScope scope)
        {
            var me = Context.GetMember(null, chat);
            var target = Context.GetMember(user, chat);
            var localizer = Context.GetLocalizer();
            if (target == null)
                return OutMessage.FromCode(localizer["TargetNotFound"]);
            if (!Context.GetUser().IsSuperUser
                && (me.Scope < target.Scope
                || (me.Scope != UserScope.Owner
                && target.Scope >= UserScope.Admin
                && scope >= UserScope.Admin)))
                return OutMessage.FromCode(localizer["AccessDenied"]);
            var db = new RepoContext();
            target.Scope = scope;
            db.Update(target);
            db.SaveChanges();
            return OutMessage.FromCode(localizer["UserScopeUpdated"]);
        }
    }
}