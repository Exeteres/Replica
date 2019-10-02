using System.Collections.Generic;
using System.Linq;
using Replica.Core.Commands;
using Replica.Core.Entity;
using Replica.Core.Entity.Attachments;
using Replica.Core.Localization;
using Replica.Core.Messages;

namespace Replica.App.Logic
{
    public static class InfoRenderer
    {
        private static string RenderAttachment(object attachment)
        {
            switch (attachment)
            {
                case Photo photo:
                    return $"Sizes:\n  {string.Join("\n  ", photo.Sizes.Select(x => $"- {x.Width}, {x.Height}; {x.FileSize}"))}\n";
                case Document doc:
                    return $"Name: {doc.FileName}\nSize: {doc.FileSize}";
                case Sticker sticker:
                    return $"Size: {sticker.FileSize}\nWidth: {sticker.Width}\nHeight: {sticker.Height}\nEmoji: {sticker.Emoji}";
                case Voice voice:
                    return $"Duration: {voice.Duration}\nSize: {voice.FileSize}";
            }
            return $"Unsupported";
        }

        public static string RenderMessage(InMessage message, int depth = 0)
        {
            var builder = new MessageBuilder()
                .AddText($"== User ==")
                .AddText($"Id: {message.Sender.Id}")
                .AddText($"IsBot: {message.Sender.IsBot}")
                .AddText($"Username: {message.Sender.Username}")
                .AddText($"FirstName: {message.Sender.FirstName}");
            if (message.Sender.LastName != null)
                builder.AddText($"LastName: {message.Sender.LastName}");
            builder.AddText($"Language: {message.Sender.Language}");
            var offset = string.Join("", Enumerable.Repeat("    ", depth));
            if (message.Reply != null)
                builder.AddText($"== Reply ==\n" + RenderMessage(message.Reply, depth + 1));
            for (var i = 0; i < message.Forwarded.Length; i++)
                builder.AddText($"== Forwarded #{i} ==\n" + RenderMessage(message.Forwarded[i], depth + 1));
            for (var i = 0; i < message.Attachments.Length; i++)
            {
                var attachment = message.Attachments[i];
                var aset = string.Join("", Enumerable.Repeat("    ", depth == 0 ? 1 : depth));
                builder.AddText($"== Attachment #{i} : {attachment.GetType().Name} ==\n{aset}{RenderAttachment(attachment).Replace("\n", "\n" + aset)}");
            }
            return offset + builder.Build().Text.Replace("\n", "\n" + offset);
        }
    }
}