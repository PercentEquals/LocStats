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
using Microsoft.AspNetCore.WebUtilities;
using MobileApp.Database;
using Android.Util;

namespace MobileApp.Managers
{
    public static class ConnectionManager
    {
        private const string URL = "https://192.168.0.217:45455/"; //download VS extension: Conveyor by Keyoti, paste here address from field 'Remote URL'

        //private const string URL = "https://locstats.azurewebsites.net/";

        private static string _currentToken;
        private static string _currentRefreshToken;

        private static bool _authorizationAttempt = false;

        public static string CurrentUsername { get; private set; }

        public static async Task<(bool success, string errorMessage)> Register(string email, string username, string password)
        {
            return await PostRequest(new Dictionary<string, string>()
                {
                    { "email", email },
                    { "userName", username },
                    { "password", password }

                }.ToJsonString(), "api/Auth/Register", username);
              
        }

        public static async Task<(bool success, string errorMessage)> LogIn (string username, string password)
        {
            return await PostRequest(new Dictionary<string, string>()
                { 
                    { "userName", username },
                    { "password", password }

                }.ToJsonString(), "api/Auth/Login", username);
        }

        public static async Task<(bool success, string errorMessage)> SendGPSData(IEnumerable<LocationModel> locations)
        {
            Dictionary<string, string> locationsDictionary = new Dictionary<string, string>();

            string s = "[";

            int i = 0;
            foreach(var l in locations)
            {
                locationsDictionary.Clear();

                locationsDictionary.Add("timestamp", l.Timestamp.ToString());
                locationsDictionary.Add("latitude", l.Latitude.ToString());
                locationsDictionary.Add("longtitude", l.Longitude.ToString());

                s += locationsDictionary.ToJsonString(true);

                i++;

                if (i != locations.Count())
                {
                    s += ",";
                }

            }

            s += "]";

            return await PostRequest(s, "/api/GPSData/SendMultiple");
                
        }

        private static async Task<(bool success, string errorMessage)> PostRequest (string jsonString, string endpoint, string username)
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
                            _currentRefreshToken = responseJson.refreshToken.ToString();

                            /*
                            byte[] data = Encoding.UTF8.GetBytes(responseJson.refreshToken.ToString());
                            string b64 = Convert.ToBase64String(data);
                            
                            byte[] byteToken = Convert.FromBase64String(b64);
                            string decodedRefreshToken = Encoding.UTF8.GetString(byteToken);
                            */


                            CurrentUsername = username;

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

        private static async Task<(bool success, string errorMessage)> PostRequest(string jsonString, string endpoint)
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
                    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _currentToken);

                    client.Timeout = TimeSpan.FromMilliseconds(30000);



                    var response = await client.PostAsync(endpoint, content);


                    if (response != null)
                    {
                        var responeJsonString = await response.Content.ReadAsStringAsync();

                        dynamic responseJson = JsonConvert.DeserializeObject(responeJsonString);

                        if (response.IsSuccessStatusCode)
                        {
                            return (true, "success!");
                        }
                        else
                        {
                            if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized && !_authorizationAttempt)
                            {
                                var refreshTokensResult = await PostRequestRefreshToken(new Dictionary<string, string>()
                                {
                                    { "token", _currentToken },
                                    { "refreshToken", _currentRefreshToken }
                                }.ToJsonString(), "/api/Auth/RefreshToken");

                                if (refreshTokensResult.success)
                                {
                                    _currentToken = responseJson.token.ToString();
                                    _currentRefreshToken = responseJson.refreshToken.ToString();

                                    _authorizationAttempt = true;

                                    return await PostRequest(jsonString, endpoint);
                                }
                                else
                                {
                                    _authorizationAttempt = false;

                                    return (false, refreshTokensResult.error);

                                }

                            }
                            else
                            {
                                return (false, responseJson.errors.ToString().Trim(new char[] { ']', '[', '\n' }));
                            }
                            
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

        private static async Task<(bool success, string token, string refreshToken, string error)>
            PostRequestRefreshToken(string jsonString, string endpoint)
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
                           
                            return (true, responseJson.token.ToString(), responseJson.refreshToken.ToString(), "Success");
                        }
                        else
                        {
                            return (false, null, null, responseJson.errors.ToString().Trim(new char[] { ']', '[', '\n' }));
                        }

                    }
                    else
                    {
                        return (false, null, null, "No response from server");
                    }
                }
                catch (HttpRequestException e)
                {
                    return (false, null, null, e.Message);
                }
                catch (Microsoft.CSharp.RuntimeBinder.RuntimeBinderException e)
                {
                    return (false, null, null, e.Message);
                }
            }
        }
    }
}