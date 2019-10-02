using Replica.App.Extensions;
using Replica.App.Models;
using Replica.Core.Commands;
using Replica.Core.Contexts;

namespace Replica.App.Converters
{
    public class RouteConverter : ConverterBase<Route>
    {
        public override string TryConvert(Context context, string origin, out Route result)
        {
            var localizer = context.GetLocalizer();
            if (int.TryParse(origin, out var id))
            {
                var db = new RepoContext();
                result = db.Routes.Find(id);
                if (result == null)
                    return localizer["RouteNotFound"];
                var user = context.GetUser();
                return null;
            }
            result = null;
            return localizer["ChatIdExpected"];
        }
    }
}