using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Runtime.Serialization.Json;
using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations.Schema;

namespace Digiscape.Services
{
    public static class StravaHelper
    {
        private static readonly HttpClient _client = new HttpClient();

        public static async Task<GetTokenResult> GetToken(string clientId, string secret, string code, string grant = "authorization_code")
        {
            var url = $"https://www.strava.com/oauth/token?client_id={clientId}&client_secret={secret}&code={code}&grant_type={grant}";
            var response = await _client.PostAsync(url, null);
            var json = await response.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<GetTokenResult>(json);
        }

        public static async Task<RefreshTokenResult> RefreshToken(string clientId, string secret, string token, string grant = "refresh_token")
        {
            var url = $"https://www.strava.com/oauth/token?client_id={clientId}&client_secret={secret}&refresh_token={token}&grant_type={grant}";
            var response = await _client.PostAsync(url, null);
            var json = await response.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<RefreshTokenResult>(json);
        }

        public static async Task<Athlete> GetAthlete(string token)
        {
            var url = $"https://www.strava.com/api/v3/athlete";
            var request = new HttpRequestMessage(HttpMethod.Get, url);
            request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
            var response = await _client.SendAsync(request);
            if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
            {
                throw new Exception("Access denied");
            }
            else
            {
                var json = await response.Content.ReadAsStringAsync();
                return JsonConvert.DeserializeObject<Athlete>(json);
            }
        }

        public static async Task<List<Activity>> GetActivities(string token, long from, long to)
        {
            var url = $"https://www.strava.com/api/v3/athlete/activities?after={from}&before={to}";
            var request = new HttpRequestMessage(HttpMethod.Get, url);
            request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
            var response = await _client.SendAsync(request);
            if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
            {
                throw new Exception("Access denied");
            }
            else if (response.StatusCode == System.Net.HttpStatusCode.BadRequest)
            {
                throw new Exception("Bad Request");
            }
            else
            {
                var json = await response.Content.ReadAsStringAsync();
                return JsonConvert.DeserializeObject<List<Activity>>(json);
            }
        }

        public static DateTime FromUnixTimeStamp(this long seconds)
        {
            return epoch.AddSeconds(seconds);
        }
        public static long ToUnixTimeStamp(this DateTime date)
        {
            TimeSpan diff = date.ToUniversalTime() - epoch;
            return (long)Math.Floor(diff.TotalSeconds);
        }
        private static readonly DateTime epoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
    }

    public class GetTokenResult
    {
        [JsonProperty("token_type")]
        public string TokenType { get; set; }
        [JsonProperty("access_token")]
        public string AccessToken { get; set; }
        [JsonProperty("refresh_token")]
        public string RefreshToken { get; set; }
        [JsonProperty("expires_at")]
        public long ExpiresAt { get; set; }
        [JsonProperty("state")]
        public string State { get; set; }
    }

    public class RefreshTokenResult
    {
        [JsonProperty("token_type")]
        public string TokenType { get; set; }
        [JsonProperty("access_token")]
        public string AccessToken { get; set; }
        [JsonProperty("refresh_token")]
        public string RefreshToken { get; set; }
        [JsonProperty("expires_at")]
        public long ExpiresAt { get; set; }
    }

    public class Athlete
    {
        [JsonProperty("id")]
        public long Id { get; set; }
        [JsonProperty("firstname")]
        public string FirstName { get; set; }
        [JsonProperty("lastname")]
        public string LastName { get; set; }
        [JsonProperty("profile_medium")]
        public string Profile { get; set; }
    }

    public class Activity
    {
        [JsonProperty("id")]
        public long Id { get; set; }
        [JsonProperty("athlete")]
        [NotMapped]
        public Meta Athlete { get; set; } = new Meta();
        [JsonProperty("type")]
        public string Type { get; set; }
        [JsonProperty("distance")]
        public double Distance { get; set; }
        [JsonProperty("elapsed_time")]
        public double ElapsedTime { get; set; }
        [JsonProperty("start_date")]
        [NotMapped]
        public DateTime StartDateStrava { get; set; }
        public long StartDate
        {
            get
            {
                return StartDateStrava.ToUnixTimeStamp();
            }
            set
            {
                StartDateStrava = value.FromUnixTimeStamp();
            }
        }
        public long UserId
        {
            get
            {
                return Athlete.Id;
            }
            set
            {
                Athlete.Id = value;
            }
        }
    }

    public class Meta
    {
        [JsonProperty("id")]
        public long Id { get; set; }
    }
}
