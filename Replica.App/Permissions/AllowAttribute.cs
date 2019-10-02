using System;
using Replica.App.Extensions;
using Replica.App.Models;
using Replica.Core.Commands;
using Replica.Core.Contexts;

namespace Replica.App.Permissions
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
    public class AllowAttribute : Attribute, IRestriction
    {
        internal UserScope Scope { get; private set; }

        public AllowAttribute(UserScope scope)
        {
            Scope = scope;
        }

        bool IRestriction.Check(Context ctx)
        {
            var user = ctx.GetUser();
            if (user.IsSuperUser) return true;

            var chat = ctx.GetChat();
            var position = user.Chats.Find(x => x.Id.Equals(chat?.Id));
            if (position == null) return false;
            return position.Scope >= Scope;
        }
    }
}