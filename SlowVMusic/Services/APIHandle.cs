using System;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using SlowVMusic.Entity;

namespace SlowVMusic.Services
{
    class APIHandle
    {
        private static String API_REGISTER = "https://2-dot-backup-server-002.appspot.com/_api/v2/members";
        private static String API_LOGIN = "https://2-dot-backup-server-002.appspot.com/_api/v2/members/authentication";
        public async static Task<HttpResponseMessage> Sign_Up(Member member)
        {
            HttpClient httpClient = new HttpClient();
            var content = new StringContent(JsonConvert.SerializeObject(member), System.Text.Encoding.UTF8, "application/json");
            var response = httpClient.PostAsync(API_REGISTER, content);
            return response.Result;
        }
        public static Task<HttpResponseMessage> Sign_In(string email, string password)
        {
            Dictionary<String, String> memberLogin = new Dictionary<string, string>();
            memberLogin.Add("email", "xuanhung2401@gmail.com");
            memberLogin.Add("password", "123");
            HttpClient httpClient = new HttpClient();
            var content = new StringContent(JsonConvert.SerializeObject(memberLogin), System.Text.Encoding.UTF8, "application/json");

            Debug.WriteLine(httpClient.PostAsync(API_LOGIN, content).Result.Content);
            return null;
        }
    }
}
