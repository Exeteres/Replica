using System;
using Replica.Core.Handlers;
using Replica.Core.Messages;
using Replica.Core.Extensions;
using Replica.App.Extensions;
using Replica.Core.Entity;
using Replica.App.Models;

namespace Replica.App.Middleware
{
    public class ReplicatingMiddleware : HandlerBase
    {
        private string RenderName(UserInfo user)
            => user.FirstName + (!string.IsNullOrEmpty(user.LastName) ? " " + user.LastName : "");

        protected override void TakeOver()
        {
            if (Program.Options.Skip && Message.Date < Program.StartTime)
                return;

            if (!string.IsNullOrEmpty(Message.Text) && Message.Text.StartsWith("/"))
            {
                base.TakeOver();
                return;
            }

            if (string.IsNullOrEmpty(Message.Text) && Message.Attachments.Length == 0 && Message.Forwarded.Length == 0 && Message.Reply == null)
                return;

            var chat = Context.GetChat();
            var member = Context.GetMember(null, chat);
            if (chat == null || member == null)
                return;

            if (member.Mute > MuteState.None)
                return;

            var dests = chat.GetDestinations();
            foreach (var dest in dests)
            {
                var controller = Program.Core.ResolveController(dest.Controller);
                var name = RenderName(Message.Sender);

                var header = controller.Name switch
                {
                    "tg" => $"<a href=\"https://vk.com/id{Context.Message.Sender.Id}\">{name}</a>",
                    "vk" => $"[club182157612|{name}]",
                    _ => throw new Exception("Ты что ебнутый")
                };

                var message = Message;

                if (Controller.Name == "tg" && Message.Forwarded.Length > 0)
                {
                    var sender = Message.Forwarded[0].Sender;
                    var a = sender.Title == null ? "от" : "из";
                    header += $" ⤷ Переслано {a} [club182157612|{sender.Title ?? RenderName(sender)}]\n";
                    message = Message.Forwarded[0];
                }
                else header += "\n";

                var builder = new MessageBuilder()
                    .Replicate(message)
                    .Set(MessageFlags.AllowHTMl)
                    .SetText(header);
                if (!string.IsNullOrEmpty(message.Text))
                    builder.AddText(controller.Name == "tg" ? message.Text.EscapeHTML() : message.Text);
                controller.SendMessage(dest.ChatId, builder.Build());
            }
        }
    }
}