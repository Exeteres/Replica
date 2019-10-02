using Replica.App.Models;
using Replica.Core.Handlers;

namespace Replica.App.Middleware
{
    public class UsersMiddleware : HandlerBase
    {
        protected override void TakeOver()
        {
            var user = User.EnsureCreated(Controller.Name, Message.Sender);
            Chat.EnsureContained(Controller.Name, Message.Chat, user);
            base.TakeOver();
        }
    }
}