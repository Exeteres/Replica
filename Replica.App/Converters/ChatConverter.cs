using System.Linq;
using Replica.App.Extensions;
using Replica.App.Models;
using Replica.Core.Commands;
using Replica.Core.Contexts;

namespace Replica.App.Converters
{
    public class ChatConverter : ConverterBase<Chat>
    {
        public override string TryConvert(Context context, string origin, out Chat result)
        {
            if (origin == "this" || origin == "here")
            {
                result = context.GetChat();
                return null;
            }
            var localizer = context.GetLocalizer();
            if (int.TryParse(origin, out var id))
            {
                var db = new RepoContext();
                result = db.Chats.Find(id);
                if (result == null)
                    return localizer["ChatNotFound"];
                return null;
            }
            result = null;
            return localizer["ChatIdExpected"];
        }
    }
}