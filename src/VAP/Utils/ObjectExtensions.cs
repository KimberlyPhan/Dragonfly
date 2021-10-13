using Newtonsoft.Json;
using System;

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
    }
}
