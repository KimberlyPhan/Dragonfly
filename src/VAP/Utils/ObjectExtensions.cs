using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace Utils
{
    public static class ObjectExtensions
    {
        public static string ToJsonForLogging(this object source)
        {
            try
            {
                if (source == null)
                {
                    return string.Empty;
                }

                return JsonConvert.SerializeObject(source, Formatting.Indented);
            }
            catch (Exception)
            {
                return string.Empty;
            }
        }

        public static string ListToString<T>(this List<T> list)
        {
            var stringBuilder = new StringBuilder();
            foreach (var item in list)
            {
                stringBuilder.AppendLine(item.ToString());
            }

            return stringBuilder.ToString();
        }
    }
}
