using System.Collections.Generic;
using System.Linq;

namespace MobileApp.Extensions
{
    public static class DictionaryExtension
    {
        public static string ToJsonString(this Dictionary<string, string> dictionary, bool valueAsNumber = false)
        {
            string s = "";
            s = "{";
            int i = 0;

            foreach (var d in dictionary)
            {
                if (!valueAsNumber)
                {
                    s += "\"" + d.Key + "\":\"" + d.Value + "\"";
                }
                else
                {
                    s += "\"" + d.Key + "\":" + d.Value;
                }
                i++;

                if (i != dictionary.Count)
                {
                    s += ",";
                }
            }
            s += "}";
            return s;
        }
    }
}