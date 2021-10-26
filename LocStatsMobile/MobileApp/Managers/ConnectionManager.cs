using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace MobileApp.Managers
{
    public static class ConnectionManager
    {
        private const string URL = "https://localhost:44343/";

        public static async Task<(bool success, string token, string errorMessage)> Register(string email, string username, string password)
        {
            var httpClientHandler = new HttpClientHandler();

            httpClientHandler.ServerCertificateCustomValidationCallback =
            (message, cert, chain, errors) => { return true; };

            using (var client = new HttpClient(httpClientHandler))
            {
                client.BaseAddress = new Uri(URL + "api/auth/register");


                string jsonClientString = "{\"email\":\"" + email + "\",\"userName\":\"" + username + "\",\"password\":\"" + password + "\"}";

                var content = new StringContent(jsonClientString, Encoding.UTF8, "application/json");
                content.Headers.ContentType = new MediaTypeHeaderValue("application/json");

                var response = await client.PostAsync(URL, content);


                if (response != null)
                {
                    var jsonString = await response.Content.ReadAsStringAsync();

                    dynamic responseJson = JsonConvert.DeserializeObject(jsonString);

                    if (response.IsSuccessStatusCode)
                    {
                        return (response.IsSuccessStatusCode, responseJson.token, "");
                    }
                    else
                    {
                        return (false, null, responseJson.errors.ToString());
                    }

                }
                else
                {
                    return (false, null, "No response from server");
                }
            }

        }

    }
}