using System;
using Replica.App.Extensions;
using Replica.Core.Commands;
using Replica.Core.Contexts;

namespace Replica.App.Permissions
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
    public class DisallowAttribute : Attribute, IRestriction
    {
        bool IRestriction.Check(Context ctx) => ctx.GetUser().IsSuperUser;
    }
}