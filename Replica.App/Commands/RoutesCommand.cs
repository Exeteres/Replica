using System.Linq;
using Replica.App.Models;
using Replica.Core.Commands;
using Replica.Core.Messages;
using Replica.Core.Commands.Validation;
using Replica.Core.Entity;
using Replica.App.Permissions;
using Replica.App.Extensions;
using Microsoft.EntityFrameworkCore;

namespace Replica.App.Commands
{
    [Command("routes", "r")]
    public class RoutesCommand : CommandBase
    {
        public MessageBuilder Default()
        {
            var localizer = Context.GetLocalizer();
            var user = Context.GetUser();

            return new MessageBuilder()
                .Set(MessageFlags.Code)
                .AddText(localizer["Routes"]
                    + (user.Routes.Any() ? ":\n" + string.Join("\n", user.Routes) : " " + localizer["NotFound"]));
        }

        public MessageBuilder Sync([MinLength(2)] [Protect(UserScope.Admin)] params Chat[] chats)
        {
            var db = new RepoContext();

            var user = Context.GetUser();
            var (created, route) = Route.Sync(user, chats.Distinct().ToArray());
            var localizer = Context.GetLocalizer();

            return new MessageBuilder()
                .Set(MessageFlags.Code)
                .AddText(created ? localizer["RouteInitialized"] : localizer["RouteUpdated"])
                .AddText(route.ToString());
        }

        public MessageBuilder Exclude(Route route, [MinLength(1)] [Protect(UserScope.Admin)] params Chat[] chats)
        {
            var info = route.ToString();
            var localizer = Context.GetLocalizer();

            if (route.Exclude(chats))
            {
                return new MessageBuilder()
                    .Set(MessageFlags.Code)
                    .AddText(localizer["RouteDestroyed"])
                    .AddText(info);
            }
            return new MessageBuilder()
                .Set(MessageFlags.Code)
                .AddText(localizer["RouteUpdated"])
                .AddText(route.ToString());
        }

        public MessageBuilder Desync([Protect(UserScope.Owner)] Route route)
        {
            var db = new RepoContext();
            var localizer = Context.GetLocalizer();
            var routeS = route.ToString();
            db.Remove(route);
            db.SaveChanges();
            return new MessageBuilder()
                .Set(MessageFlags.Code)
                .AddText(localizer["RouteDestroyed"])
                .AddText(routeS);
        }
    }
}