using System;
using System.Collections.Generic;
using Replica.App.Extensions;
using Replica.App.Logic;
using Replica.Core.Commands;
using Replica.Core.Entity;

namespace Replica.App.Commands
{
    [Command("help")]
    public class HelpCommand : CommandBase
    {
        private static readonly Dictionary<int, DateTime> _debounce = new Dictionary<int, DateTime>();

        public OutMessage Default()
        {
            var key = Context.GetChat().Id;
            var now = DateTime.Now;
            if (_debounce.TryGetValue(key, out var value) && now - value < TimeSpan.FromSeconds(60))
                return null;
            _debounce[key] = now;

            var localizer = Context.GetLocalizer();
            var scope = Context.GetMember().Scope;
            return OutMessage.FromCode(HelpMessage.Instance.Resolve(localizer, scope));
        }
    }
}