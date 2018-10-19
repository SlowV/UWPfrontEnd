using Microsoft.Data.Sqlite;
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
        private static List<Song> listSong;
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

        public static List<Song> Search(string valueSearch)
        {
            listSong = new List<Song>();
            using (SqliteConnection db = new SqliteConnection("Filename=slowvmusic.db"))
            {
                db.Open();

                SqliteCommand insertCommand = new SqliteCommand();
                insertCommand.Connection = db;

                SqliteCommand selectCommand = new SqliteCommand
                    ("SELECT * from Song WHERE name LIKE '%" + valueSearch + "%' OR description LIKE '%" + valueSearch + "%'", db);

                SqliteDataReader query = selectCommand.ExecuteReader();
                while (query.Read())
                {
                    int id = query.GetInt32(0);
                    String name = query.GetString(1);
                    String description = query.GetString(2);
                    String singer = query.GetString(3);
                    String author = query.GetString(4);
                    String thumbnail = query.GetString(5);
                    String link = query.GetString(6);

                    listSong.Add(new Song
                    {
                        id = id,
                        name = name,
                        description = description,
                        singer = singer,
                        author = author,
                        thumbnail = thumbnail,
                        link = link
                    });
                }

                db.Close();
            }
            return listSong;
        }
    }
}
