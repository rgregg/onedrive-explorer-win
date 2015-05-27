using System;
using Newtonsoft.Json;
using System.Net;
using System.Linq;
using System.Collections.Generic;

namespace OneDrive
{
    internal static class GenericExtensionMethods
    {
        public static int OccurrencesOfCharacter(this string input, char lookFor)
        {
            int count = 0;
            foreach (char c in input)
                if (c == lookFor) count++;
            return count;
        }

        public static bool ValidFilename(this string filename)
        {
            char[] restrictedCharacters = { '\\', '/', ':', '*', '?', '<', '>', '|' };

            if (filename.IndexOfAny(restrictedCharacters) != -1)
            {
                return false;
            }

            return true;
        }

        public static T Copy<T>(this T sourceItem) where T : ODDataModel
        {
            string serialized = JsonConvert.SerializeObject(sourceItem);
            return JsonConvert.DeserializeObject<T>(serialized);
        }

        public static bool IsSuccess(this HttpStatusCode code)
        {
            int statusCode = (int)code;
            if (statusCode >= 200 && statusCode < 300)
                return true;
            else
                return false;
        }

        /// <summary>
        /// Ensures that a path component has a leading path seperator "foo/bar" => "/foo/bar"
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public static string EnsureLeadingPathSeperator(this string path)
        {
            if (path.StartsWith("/"))
                return path;
            else
                return "/" + path;
        }

        public static void AppendLine(this System.Text.StringBuilder sb, string format, params object[] parameters)
        {
            sb.AppendLine(string.Format(format, parameters));
        }

        public static string GetToNextSeparator(this string input, string seperator, out string remainingString)
        {
            var index = input.IndexOf(seperator);
            if (index == -1)
            {
                remainingString = null;
                return input;
            }

            var result = input.Substring(0, index);
            if (input.Length > index + 1)
                remainingString = input.Substring(index + 1);
            else
                remainingString = null;
            
            return result;
        }

        public static string[] SplitAndTrim(this string input, char character)
        {
            string[] values = input.Split(character);
            return (from v in values select v.Trim()).ToArray();
        }
    }
}
