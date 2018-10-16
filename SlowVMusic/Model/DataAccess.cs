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
            //using (SqliteConnection db =
            //    new SqliteConnection("Filename=slowvmusic.db"))
            //{
            //    db.Open();

            //    String tableCommand = "CREATE TABLE IF NOT " +
            //        "EXISTS Song (id INTEGER PRIMARY KEY AUTOINCREMENT, " +
            //        "name NVARCHAR(191) NOT NULL, " +
            //        "description NVARCHAR(2048) NULL, " +
            //        "singer NVARCHAR(191) NULL, " +
            //        "author NVARCHAR(191) NULL, " +
            //        "thumbnail NVARCHAR(2048) DEFAULT https://previews.123rf.com/images/grgroup/grgroup1201/grgroup120100074/11890302-blue-music-notes-isolated-over-white-background-vector.jpg ,"+
            //        "link NVARCHAR(515) NOT NULL," + 
            //        "memberId VARCHAR(100) NOT NULL)";

            //    SqliteCommand createTable = new SqliteCommand(tableCommand, db);

            //    createTable.ExecuteReader();
            //}

            using (SqliteConnection db = new SqliteConnection("Filename=sqliteSample.db"))
            {
                db.Open();

                String tableCommand = "CREATE TABLE IF NOT " +
                        "EXISTS Song (id INTEGER PRIMARY KEY AUTOINCREMENT, " +
                        "name NVARCHAR(191) NOT NULL, " +
                        "description NVARCHAR(2048) NULL, " +
                        "singer NVARCHAR(191) NULL, " +
                        "author NVARCHAR(191) NULL, " +
                        "thumbnail NVARCHAR(2048) DEFAULT https://previews.123rf.com/images/grgroup/grgroup1201/grgroup120100074/11890302-blue-music-notes-isolated-over-white-background-vector.jpg ," +
                        "link NVARCHAR(515) NOT NULL," +
                        "memberId VARCHAR(100) NOT NULL)";

                SqliteCommand createTable = new SqliteCommand(tableCommand, db);

                createTable.ExecuteReader();
            }
        }
    }
}
