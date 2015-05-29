﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OneDrive
{
    internal static class ArrayExtensionMethods
    {
        public static string ComponentsJoinedByString(this IEnumerable<string> source, string separator, int startIndex = 0)
        {
            StringBuilder sb = new StringBuilder();
            int index = 0;
            foreach (var component in source)
            {
                if (index < startIndex)
                {
                    index++;
                    continue;
                }

                if (sb.Length > 0)
                    sb.Append(separator);
                sb.Append(component);

                index++;
            }
            return sb.ToString();
        }
    }
}
