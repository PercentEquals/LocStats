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
using MobileApp.Extensions;

namespace MobileApp.Managers
{
    public static class ConnectionManager
    {
        private const string URL = "https://192.168.0.217:45455/"; //download VS extension: Conveyor by Keyoti, paste here address from field 'Remote URL'

        //private const string URL = "https://locstats.azurewebsites.net/";

        private static string _currentToken;

        public static async Task<(bool success, string errorMessage)> Register(string email, string username, string password)
        {
            return await PostRequest(new Dictionary<string, string>()
                {
                    { "email", email },
                    { "userName", username },
                    { "password", password }

                }.ToJsonString(), "api/Auth/Register");
              
        }

        public static async Task<(bool success, string errorMessage)> LogIn (string username, string password)
        {
            return await PostRequest(new Dictionary<string, string>()
                { 
                    { "userName", username },
                    { "password", password }

                }.ToJsonString(), "api/Auth/Login");
        }

        private static async Task<(bool success, string errorMessage)> PostRequest (string jsonString, string endpoint)
        {
            var httpClientHandler = new HttpClientHandler();

            httpClientHandler.ServerCertificateCustomValidationCallback =
            (message, cert, chain, errors) => { return true; };

            using (var client = new HttpClient(httpClientHandler))
            {
                try
                {
                    client.BaseAddress = new Uri(URL);

                    var content = new StringContent(jsonString, Encoding.UTF8, "application/json");
                    content.Headers.ContentType = new MediaTypeHeaderValue("application/json");

                    client.Timeout = TimeSpan.FromMilliseconds(30000);

                    var response = await client.PostAsync(endpoint, content);


                    if (response != null)
                    {
                        var responeJsonString = await response.Content.ReadAsStringAsync();

                        dynamic responseJson = JsonConvert.DeserializeObject(responeJsonString);

                        if (response.IsSuccessStatusCode)
                        {
                            _currentToken = responseJson.token.ToString();

                            return (true, "");
                        }
                        else
                        {
                            return (false, responseJson.errors.ToString().Trim(new char[] { ']', '[', '\n' }));
                        }

                    }
                    else
                    {
                        return (false, "No response from server");
                    }
                }
                catch (HttpRequestException e)
                {
                    return (false, e.Message);
                }
                catch (Microsoft.CSharp.RuntimeBinder.RuntimeBinderException e)
                {
                    return (false, e.Message);
                }
            }
        }

    }
}