using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DfmExtractor.Extensions
{
    public static class AppSettingsExtensions
    {
        public static T GetSetting<T>(this NameValueCollection source, string key, T defaultValue = default(T)) where T : IConvertible
        {
            var value = source[key] ?? string.Empty;
            var result = defaultValue;

            if (!string.IsNullOrEmpty(value))
            {
                var typeDefault = default(T);

                if (typeof(T) == typeof(string))
                {
                    typeDefault = (T)(object)string.Empty;
                }

                result = (T)Convert.ChangeType(value, typeDefault.GetTypeCode());
            }

            return result;
        }
    }
}
