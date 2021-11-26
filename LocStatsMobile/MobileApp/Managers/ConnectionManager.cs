
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using MobileApp.Extensions;
using MobileApp.Database;

namespace MobileApp.Managers
{
    public static class ConnectionManager
    {
        //private const string URL = "https://192.168.0.217:45455/"; //download VS extension: Conveyor by Keyoti, paste here address from field 'Remote URL'

        private const string URL = "https://locstats.azurewebsites.net/";

        private static string _currentToken;
        private static string _currentRefreshToken;

        private static bool _authorizationAttempt = false;

        public static string CurrentUsername { get; private set; }

        public static async Task<(bool success, string errorMessage)> Register(string email, string username, string password)
        {
            return await PostRequestLogInRegister(new Dictionary<string, string>()
                {
                    { "email", email },
                    { "userName", username },
                    { "password", password }

                }.ToJsonString(), "api/Auth/Register", username);
        }

        public static async Task<(bool success, string errorMessage)> LogIn (string username, string password)
        {
            return await PostRequestLogInRegister(new Dictionary<string, string>()
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
                locationsDictionary.Add("longitude", l.Longitude.ToString());

                s += locationsDictionary.ToJsonString(true);
                i++;

                if (i != locations.Count())
                {
                    s += ",";
                }
            }
            s += "]";
            return await PostRequestData(s, "/api/GPSData/SendMultiple");
        }

        public static async Task<(bool success, Dictionary<string, double> values, string errors)> GetTimeStats(DateTime dateFrom, DateTime dateTo)
        {
            var result = await GetRequest("/api/Stats/Time/" + dateFrom.ToString("yyyy'-'MM'-'dd") + "/" + dateTo.ToString("yyyy'-'MM'-'dd"));

            Dictionary<string, double> data = new Dictionary<string, double>();

            try
            {

                if (result.jsonResponse != null)
                    foreach (var element in result.jsonResponse)
                    {
                        string date = element.date;
                        int value = element.value;
                        data.Add(date, value);
                    }

                return (result.success, data, result.errors);

            }
            catch (Microsoft.CSharp.RuntimeBinder.RuntimeBinderException e)
            {
                return (false, null, e.Message);
            }
        }

        public static async Task<(bool success, Dictionary<string, double> values, string errors)> GetDistanceStats(DateTime dateFrom, DateTime dateTo)
        {
            var result = await GetRequest("/api/Stats/Distance/" + dateFrom.ToString("yyyy'-'MM'-'dd") + "/" + dateTo.ToString("yyyy'-'MM'-'dd"));

            Dictionary<string, double> data = new Dictionary<string, double>();

            try
            {

                if (result.jsonResponse != null)
                    foreach (var element in result.jsonResponse)
                    {
                        string date = element.date;
                        int value = element.value;
                        data.Add(date, value);
                    }

                return (result.success, data, result.errors);

            }
            catch (Microsoft.CSharp.RuntimeBinder.RuntimeBinderException e)
            {
                return (false, null, e.Message);
            }
        }

        public static async Task<(bool success, IEnumerable<PolyLinesModel> polyLines, string errors)> GetGPSData(DateTime dateFrom, DateTime dateTo)
        {
            var result = await GetRequest("/api/GpsData/" + dateFrom.ToString("yyyy'-'MM'-'dd") + "/" + dateTo.ToString("yyyy'-'MM'-'dd"));

            List<PolyLinesModel> polyLines = new List<PolyLinesModel>();

            try
            {
                if (result.jsonResponse != null)
                    foreach (var element in result.jsonResponse)
                    {
                        double latitude = element.latitude;
                        double longitude = element.longitude;
                        long timestamp = element.timestamp;
                        polyLines.Add(new PolyLinesModel
                        {
                            Latitude = latitude,
                            Longitude = longitude,
                            Timestamp = timestamp
                        });

                    }

                return (result.success, polyLines, result.errors);
            }
            catch (Microsoft.CSharp.RuntimeBinder.RuntimeBinderException e)
            {
                return (false, null, e.Message);
            }
        }

        private static async Task<(bool success, dynamic jsonResponse, string errors)> GetRequest (string endpoint)
        {
            var httpClientHandler = new HttpClientHandler();

            httpClientHandler.ServerCertificateCustomValidationCallback =
            (message, cert, chain, errors) => { return true; };

            using (var client = new HttpClient(httpClientHandler))
            {
                try
                {
                    client.BaseAddress = new Uri(URL);
                    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _currentToken);

                    client.Timeout = TimeSpan.FromMilliseconds(30000);
                    var response = await client.GetAsync(endpoint);

                    if (response != null)
                    {
                        var responeJsonString = await response.Content.ReadAsStringAsync();

                        dynamic responseJson = JsonConvert.DeserializeObject(responeJsonString);

                        if (response.IsSuccessStatusCode)
                        {
                            return (true, responseJson,  null);
                        }

                        if (response.StatusCode != System.Net.HttpStatusCode.Unauthorized || _authorizationAttempt)
                        {
                            return (false, null, responseJson.errors.ToString().Trim(new char[] { ']', '[', '\n' }));
                        }

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
                            return await GetRequest(endpoint);
                        }

                        _authorizationAttempt = false;
                        return (false, null, refreshTokensResult.error);
                    }
                    return (false, null, "No response from server");
                }
                catch (HttpRequestException e)
                {
                    return (false, null, e.Message);
                }
                catch (Microsoft.CSharp.RuntimeBinder.RuntimeBinderException e)
                {
                    return (false, null, e.Message);
                }
            }
        }



        private static async Task<(bool success, string errorMessage)> PostRequestLogInRegister (string jsonString, string endpoint, string username)
        {
            var httpClientHandler = new HttpClientHandler();

            httpClientHandler.ServerCertificateCustomValidationCallback =
            (message, cert, chain, errors) => true;

            using (var client = new HttpClient(httpClientHandler))
            {
                try
                {
                    client.BaseAddress = new Uri(URL);

                    var content = new StringContent(jsonString, Encoding.UTF8, "application/json");
                    content.Headers.ContentType = new MediaTypeHeaderValue("application/json");

                    client.Timeout = TimeSpan.FromMilliseconds(30000);

                    var response = await client.PostAsync(endpoint, content);

                    if (response == null)
                    {
                        return (false, "No response from server");
                    }

                    var responeJsonString = await response.Content.ReadAsStringAsync();

                    dynamic responseJson = JsonConvert.DeserializeObject(responeJsonString);

                    if (response.IsSuccessStatusCode)
                    {
                        _currentToken = responseJson.token.ToString();
                        _currentRefreshToken = responseJson.refreshToken.ToString();

                       
                        CurrentUsername = username;
                        return (true, "");
                    }
                    return (false, responseJson.errors.ToString().Trim(new char[] { ']', '[', '\n' }));
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

        private static async Task<(bool success, string errorMessage)> PostRequestData(string jsonString, string endpoint)
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

                        if (response.StatusCode != System.Net.HttpStatusCode.Unauthorized || _authorizationAttempt)
                        {
                            return (false, responseJson.errors.ToString().Trim(new char[] {']', '[', '\n'}));
                        }

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
                            return await PostRequestData(jsonString, endpoint);
                        }

                        _authorizationAttempt = false;
                        return (false, refreshTokensResult.error);
                    }
                    return (false, "No response from server");
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
            (message, cert, chain, errors) => true;

            using (var client = new HttpClient(httpClientHandler))
            {
                try
                {
                    client.BaseAddress = new Uri(URL);
                    var content = new StringContent(jsonString, Encoding.UTF8, "application/json");
                    content.Headers.ContentType = new MediaTypeHeaderValue("application/json");

                    client.Timeout = TimeSpan.FromMilliseconds(30000);

                    var response = await client.PostAsync(endpoint, content);

                    if (response == null)
                    {
                        return (false, null, null, "No response from server");
                    }

                    var responseJsonString = await response.Content.ReadAsStringAsync();
                    dynamic responseJson = JsonConvert.DeserializeObject(responseJsonString);

                    if (response.IsSuccessStatusCode)
                    {
                        return (true, responseJson.token.ToString(), responseJson.refreshToken.ToString(), "Success");
                    }
                    return (false, null, null, responseJson.errors.ToString().Trim(new char[] { ']', '[', '\n' }));
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