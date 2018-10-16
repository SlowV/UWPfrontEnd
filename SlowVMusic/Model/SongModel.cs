using Newtonsoft.Json;
using SlowVMusic.Entity;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace SlowVMusic.Model
{
    class SongModel
    {
        private static String API_CREATE_SONG = "http://2-dot-backup-server-002.appspot.com/_api/v2/songs";
        public async static Task<string> Create_Song(Song song)
        {
            HttpClient httpClient = new HttpClient();
            StringContent content = new StringContent(JsonConvert.SerializeObject(song), System.Text.Encoding.UTF8, "application/json");
            var response = httpClient.PostAsync(API_CREATE_SONG, content);
            var contents = await response.Result.Content.ReadAsStringAsync();
            Debug.WriteLine(contents + "content bai hat");
            return contents;
        }
    }
}
