using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Data.Sqlite;

namespace SlowVMusic.Model
{
    class DataAccess
    {
        public static void InitializeDatabase()
        {
            using (SqliteConnection db =
                new SqliteConnection("Filename=slowvmusic.db"))
            {
                db.Open();

                String tableCommand = "CREATE TABLE IF NOT " +
                    "EXISTS Song (id INTEGER PRIMARY KEY, " +
                    "name NVARCHAR(191) NULL, " +
                    "description NVARCHAR(2048), " +
                    "singer NVARCHAR(191) NULL, " +
                    "author NVARCHAR(191) NULL, " +
                    "thumbnail NVARCHAR(2048)," +
                    "link NVARCHAR(515) NULL)";

                SqliteCommand createTable = new SqliteCommand(tableCommand, db);

                createTable.ExecuteReader();
            }

            using (SqliteConnection db =
                new SqliteConnection("Filename=slowvmusic.db"))
            {
                db.Open();

                String tableCommand = "CREATE TABLE IF NOT " +
                    "EXISTS ViewSong (" +
                    "idSong INTEGER PRIMARY KEY, view INTEGER DEFAULT 0 NOT NULL,created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP," +
                    "updated_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP)";

                SqliteCommand createTable = new SqliteCommand(tableCommand, db);

                createTable.ExecuteReader();
            }
        }
    }
}
