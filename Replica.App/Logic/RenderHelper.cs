using System.Collections.Generic;
using System.Linq;

namespace Replica.App.Logic
{
    public static class RenderHelper
    {
        public static string RenderList(IEnumerable<string> values, string empty)
        {
            if (!values.Any()) return ": " + empty + "\n";
            return ":\n- " + string.Join("\n- ", values) + "\n";
        }
    }
}