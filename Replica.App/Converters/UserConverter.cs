using System.Linq;
using Replica.App.Models;
using Replica.Core.Commands;
using Replica.Core.Contexts;

namespace Replica.App.Converters
{
    public class UserConverter : ConverterBase<User>
    {
        public override string TryConvert(Context context, string origin, out User user)
        {
            var db = new RepoContext();
            var localizer = context.GetLocalizer();
            if (!int.TryParse(origin, out var id))
            {
                user = db.Users.FirstOrDefault(x => x.Username == origin);
                if (user == null)
                    return localizer["UserNotFound"];
                return null;
            }
            user = db.Users.FirstOrDefault(x => x.Id == id || x.Accounts.Exists(y => y.UserId == id));
            if (user == null)
                return localizer["UserNotFound"];
            return null;
        }
    }
}