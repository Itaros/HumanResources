using System.Collections.Generic;
using System.Text;

namespace HumanResources
{
    public static class Diagnostic
    {
        public static string ExpandEnumerableSafelyToString(IEnumerable<object> sink)
        {
            if (sink == null)
                return "NULL";
            StringBuilder b = new StringBuilder();
            foreach (var item in sink)
            {
                if (b.Length > 0)
                    b.Append(", ");
                b.Append(item);
            }
            return $"[{b.ToString()}]";
        }
    }
}