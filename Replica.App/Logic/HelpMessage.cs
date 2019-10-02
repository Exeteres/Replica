using System.Collections.Generic;
using System.Linq;
using Replica.App.Extensions;
using Replica.App.Models;
using Replica.App.Permissions;
using Replica.Core.Commands;
using Replica.Core.Localization;

namespace Replica.App.Logic
{
    public class HelpMessage
    {
        private Dictionary<(string, UserScope), string> _cache = new Dictionary<(string, UserScope), string>();
        private IEnumerable<CommandInfo> _commands;

        public static HelpMessage Instance { get; private set; }
        public static void Init(IEnumerable<CommandInfo> commands)
            => Instance = new HelpMessage { _commands = commands };

        public string Resolve(Localizer localizer, UserScope scope)
            => _cache.TryGetValue((localizer.Language, scope), out var message)
                ? message : _cache[(localizer.Language, scope)] = Render(localizer, scope);

        private string Render(Localizer localizer, UserScope uscope)
        {
            var result = localizer["AvailableCommands"] + ":\n";
            var overloads = new Dictionary<string, int>();
            foreach (var info in _commands)
            {
                if (info.Command == "test" || info.Command == "sudo") continue;
                foreach (var action in info.Actions)
                {
                    var scope = (action.Restriction as AllowAttribute)?.Scope;
                    if (scope > uscope) continue;
                    var restrictions = action.GetRestrictions();
                    if (restrictions.Any())
                    {
                        scope = action.GetRestrictions()
                            .Where(x => x is ProtectAttribute)
                            .Cast<ProtectAttribute>()
                            .OrderBy(x => x.Scope)
                            .First().Scope;
                        if (scope > uscope) continue;
                    }
                    var overkey = info.Command + "_" + action.Name;
                    var name = action.Name;
                    if (overloads.TryGetValue(overkey, out var count))
                    {
                        name += count;
                        overloads[overkey]++;
                    }
                    overloads[overkey] = 1;
                    result += $"# {localizer[info.Command.FirstCharToUpper() + "." + name]}\n";
                    result += action.Usage + "\n";
                }
                result += "\n";
            }
            return result;
        }
    }
}