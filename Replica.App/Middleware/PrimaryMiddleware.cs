using System;
using System.Linq;
using Replica.App.Extensions;
using Replica.App.Logic;
using Replica.Core.Handlers;
using Serilog;

namespace Replica.App.Middleware
{
    public class PrimaryMiddleware : HandlerBase
    {
        protected override void TakeOver()
        {
            if (Program.Options.Public)
            {
                base.TakeOver();
                return;
            }

            var user = Context.GetUser();
            // Разрешить доступ только участникам, состоящим хотя бы в одном известном чате
            if (user == null || (!user.IsSuperUser && !user.Chats.Any())) return;

            base.TakeOver();
        }
    }
}
