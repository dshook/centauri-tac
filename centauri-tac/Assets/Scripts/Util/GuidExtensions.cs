using System;

namespace ctac
{
    public static class GuidExtensions
    {
        public static string ToShort(this Guid g)
        {
            var str = g.ToString();
            return str.Substring(0, str.IndexOf('-'));
        }
    }
}

