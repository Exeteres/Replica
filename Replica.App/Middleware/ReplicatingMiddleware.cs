using System;
using Replica.Core.Handlers;
using Replica.Core.Messages;
using Replica.Core.Extensions;
using Replica.App.Extensions;
using Replica.Core.Entity;
using Replica.App.Models;
using Serilog;
using System.Text.RegularExpressions;

namespace Replica.App.Middleware
{
    public class ReplicatingMiddleware : HandlerBase
    {
        private string RenderName(UserInfo user)
            => user.FirstName + (!string.IsNullOrEmpty(user.LastName) ? " " + user.LastName : "");

        protected override async void TakeOver()
        {
            if (Program.Options.Skip && Message.Date < Program.StartTime)
                return;

            if (!string.IsNullOrEmpty(Message.Text) && Message.Text.StartsWith("/"))
            {
                base.TakeOver();
                return;
            }

            if (string.IsNullOrEmpty(Message.Text) && Message.Attachments.Length == 0 && Message.Forwarded.Length == 0 && Message.Reply == null)
            {
                Log.Verbose("Can't reply message");
                return;
            }

            var chat = Context.GetChat();
            var member = Context.GetMember(null, chat);

            if (chat == null)
            {
                Log.Verbose("Chat is null");
                return;
            }

            if (member == null)
            {
                Log.Verbose("Member is null");
                return;
            }

            if (member.Mute > MuteState.None)
            {
                Log.Verbose("User {memberId} with username {username} mutted, skipping", member.Id, member.User.Username);
                return;
            }

            var dests = chat.GetDestinations();
            foreach (var dest in dests)
            {
                var controller = Program.Core.ResolveController(dest.Controller);
                var name = RenderName(Message.Sender);

                var profileUrl = Controller.Name switch
                {
                    "vk" => $"https://vk.com/id{Context.Message.Sender.Id}",
                    "tg" => $"https://t.me/{Context.Message.Sender.Id}",
                    "dc" => "https://discordapp.com/",
                    _ => throw new Exception("в дурку его")
                };

                var header = controller.Name switch
                {
                    "tg" => $"<a href=\"{profileUrl}\">{name}</a>",
                    "vk" => $"[club182157612|{name}]",
                    _ => ""
                };

                var message = Message;

                if (Controller.Name == "dc" && !string.IsNullOrEmpty(message.Text))
                {
                    var pattern = @"<@!?(\d*)>";
                    var options = RegexOptions.Multiline;
                    foreach (Match m in Regex.Matches(message.Text, pattern, options))
                    {
                        if (m.Groups.Count < 2 && !m.Groups[1].Success) continue;
                        var id = long.Parse(m.Groups[1].Value);
                        var user = await Controller.GetUserInfo(id);
                        message.Text = message.Text.Replace(m.Value, "@" + user.Username);
                    }
                }

                if (Controller.Name == "tg" && Message.Forwarded.Length > 0)
                {
                    var sender = Message.Forwarded[0].Sender;
                    var a = sender.Title == null ? "от" : "из";
                    if (controller.Name == "vk")
                        header += $" ⤷ Переслано {a} [club182157612|{sender.Title ?? RenderName(sender)}]\n";
                    message = Message.Forwarded[0];
                }
                else if (controller.Name != "dc") header += "\n";

                var builder = new MessageBuilder()
                    .Replicate(message)
                    .Set(MessageFlags.AllowHTMl)
                    .SetText(header);
                if (!string.IsNullOrEmpty(message.Text))
                    builder.AddText(controller.Name == "tg" ? message.Text.EscapeHTML() : message.Text);
                var outMessage = builder.Build();
                if (controller.Name == "dc")
                {
                    // Мегакостыль
                    var info = await Controller.GetUserInfo(Context.Message.Sender.Id);
                    outMessage.AuthorIcon = info.AvatarUrl;

                    outMessage.AuthorName = name;
                    outMessage.AuthorUrl = profileUrl;

                    if (Controller.Name == "tg" && Message.Forwarded.Length > 0)
                    {
                        var sender = Message.Forwarded[0].Sender;
                        var a = sender.Title == null ? "от" : "из";
                        outMessage.Footer = $" Переслано {a} {sender.Title ?? RenderName(sender)}\n";
                    }
                }
                await controller.SendMessage(dest.ChatId, outMessage);
                Log.Verbose("Successfully replicated to {destId} of type {destType}", dest.Id, dest.Controller);
            }
        }
    }
}
