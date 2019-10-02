using Replica.App.Logic;
using Replica.Core.Commands;
using Replica.Core.Entity;
using Replica.Core.Messages;

namespace Replica.App.Commands
{
    [Command("test", "t")]
    public class TestCommand : CommandBase
    {
        public MessageBuilder Info()
        {
            var builder = new MessageBuilder()
                .Set(MessageFlags.Code)
                .AddText(InfoRenderer.RenderMessage(Message));
            foreach (var attachment in Message.Attachments)
                builder.AddAttachment(attachment);
            return builder;
        }

        public void Delete()
        {
            if (Message.Reply == null)
                return;
            Controller.DeleteMessage(Message.Reply.Chat.Id, Message.Reply.Id);
        }
    }
}
