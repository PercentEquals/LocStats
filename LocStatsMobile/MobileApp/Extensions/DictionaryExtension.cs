using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

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
                    s += "\"" + d.Key + "\":\"" + d.Value + "\"";
                else
                    s += "\"" + d.Key + "\":" + d.Value;

                i++;

                if (i != dictionary.Count())
                {
                    s += ",";
                }
            }

            s += "}";
            
            return s;
        }

    }
}