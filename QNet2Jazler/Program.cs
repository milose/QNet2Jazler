using System;
using System.Diagnostics;
using System.Linq;
using System.Data;
using System.Data.OleDb;

namespace QNet2Jazler
{
    class Program
    {
        // path to QNet files
        private const string path = @"E:\";

        // stopwatch instance
        private static Stopwatch sw = Stopwatch.StartNew();

        #region Static methods

        /// <summary>
        /// Open the OleDbConnection
        /// </summary>
        /// <param name="Mdb">Path to .mdb file.</param>
        /// <returns>New OleDbConnection instance.</returns>
        private static OleDbConnection connect(string Mdb)
        {
            string connection = @"Provider=Microsoft.Jet.OLEDB.4.0;" +
                           "Data Source=" + Mdb + ";" +
                           "Persist Security Info=True;";

            return new OleDbConnection(connection);
        }

        /// <summary>
        /// Look up artist in database.
        /// </summary>
        /// <param name="db">OleDbConnection instance.</param>
        /// <param name="AUID">AUID of artist to look-up.</param>
        /// <returns></returns>
        private static string artist(OleDbConnection db, int AUID)
        {
            string artist = string.Empty;

            using (OleDbCommand cmd = new OleDbCommand("SELECT fldName FROM snArtists WHERE AUID = " + AUID, db))
            {
                artist = cmd.ExecuteScalar().ToString();
            }

            return artist;
        }

        /// <summary>
        /// Return the AUID of artist. If no artist exists, create it.
        /// </summary>
        /// <param name="db">OleDbConnection instance.</param>
        /// <param name="Artist">Original artist.</param>
        /// <param name="Artist2">Featuring artist.</param>
        /// <param name="Feat">Combined artist name.</param>
        /// <returns>Artist AUID.</returns>
        private static int artist(OleDbConnection db, object Artist, object Artist2, object Feat)
        {
            int AUID = -1;

            string artist = Artist.ToString().Trim();
            if (Artist2.ToString().Trim().Length > 0) artist = Feat.ToString().Trim();

            // escape
            artist = artist.Replace("'", "''");

            string where = " WHERE fldName = '" + artist + "'";

            // check if artist exists
            using (OleDbCommand cmd = new OleDbCommand("SELECT COUNT(*) FROM snArtists" + where, db))
            {
                int count = (int)cmd.ExecuteScalar();

                if (count > 0)
                {
                    // get AUID
                    using (OleDbCommand get = new OleDbCommand("SELECT AUID FROM snArtists" + where, db))
                    {
                        AUID = (int)get.ExecuteScalar();
                    }
                }
                else
                {
                    // create new artist and get it's AUID
                    using (OleDbCommand set = new OleDbCommand("INSERT INTO snArtists (fldName) VALUES('" + artist + "')", db))
                    {
                        set.ExecuteNonQuery();

                        using (OleDbCommand get = new OleDbCommand("SELECT @@Identity", db))
                        {
                            AUID = (int)get.ExecuteScalar();
                        }
                    }
                }
            }

            return AUID;
        }

        /// <summary>
        /// Return the AUID of a category. If no category exists, create it.
        /// </summary>
        /// <param name="db">OleDbConnection instance.</param>
        /// <param name="Category">Category name.</param>
        /// <returns>Category AUID.</returns>
        private static int category(OleDbConnection db, object Category)
        {
            int AUID = -1;

            string category = Category.ToString().Trim().Replace("'", "''");

            if (category == string.Empty) category = "empty";


            string where = " WHERE fldMusicType = '" + category + "'";

            // provjeri da li postoji category
            using (OleDbCommand cmd = new OleDbCommand("SELECT COUNT(*) FROM snCat1" + where, db))
            {
                int count = (int)cmd.ExecuteScalar();

                if (count > 0)
                {
                    // dobij AUID
                    using (OleDbCommand get = new OleDbCommand("SELECT AUID FROM snCat1" + where, db))
                    {
                        AUID = (int)get.ExecuteScalar();
                    }
                }
                else
                {
                    // napravi AUID
                    using (OleDbCommand set = new OleDbCommand("INSERT INTO snCat1 (fldMusicType) VALUES('" + category + "')", db))
                    {
                        set.ExecuteNonQuery();

                        using (OleDbCommand get = new OleDbCommand("SELECT @@Identity", db))
                        {
                            AUID = (int)get.ExecuteScalar();
                        }
                    }
                }
            }


            return AUID;
        }

        /// <summary>
        /// Convert "hh:mm:ss.tttt" data to TimeSpan.
        /// </summary>
        /// <param name="Data">Formated time data: "hh:mm:ss.tttt".</param>
        /// <returns>TimeSpan object.</returns>
        private static TimeSpan span(object Data)
        {
            int h = 0;
            int m = 0;
            int s = 0;
            int ms = 0;

            string data = Data.ToString().Trim();

            string[] array = data.Split(new char[] { ':', '.' });

            if (array.Length == 4)
            {
                // h, m, s, ms
                h = Convert.ToInt32(array[0]);
                m = Convert.ToInt32(array[1]);
                s = Convert.ToInt32(array[2]);
                ms = Convert.ToInt32(array[3]);
            }
            else if (array.Length == 3)
            {
                // m, s, ms
                m = Convert.ToInt32(array[0]);
                s = Convert.ToInt32(array[1]);
                ms = Convert.ToInt32(array[2]);
            }

            return new TimeSpan(0, h, m, s, ms);
        }

        /// <summary>
        /// Convert "hh:mm:ss.tttt" data to seconds and milliseconds decimals.
        /// </summary>
        /// <param name="Data">Formated time data: "hh:mm:ss.tttt".</param>
        /// <returns>Decimal seconds.</returns>
        private static decimal toSeconds(object Data)
        {
            return Convert.ToDecimal(span(Data).TotalSeconds.ToString());
        }

        /// <summary>
        /// Fills the database with artists, categories and songs.
        /// </summary>
        /// <param name="db">OleDbConnection instance.</param>
        /// <param name="Artist">Artist AUID.</param>
        /// <param name="Title">Song title.</param>
        /// <param name="Category">Category AUID.</param>
        /// <param name="File">File name.</param>
        /// <param name="SongStart">Song start time.</param>
        /// <param name="SongEnd">Song end time.</param>
        /// <param name="Intro">Intro time.</param>
        /// <param name="MixIn">Mix-in time.</param>
        private static void database(OleDbConnection db, int Artist, object Title, int Category, object File, object SongStart, object SongEnd, object Intro, object MixIn)
        {
            string title = Title.ToString().Trim().Replace("'", "''");
            string file = path + File;
            string songStart = toSeconds(SongStart).ToString("0.00");
            string songEnd = toSeconds(SongEnd).ToString("0.00");
            string intro = toSeconds(Intro).ToString("0.00");
            string mixIn = toSeconds(MixIn).ToString("0.00");
            string artistName = artist(db, Artist).Replace("'", "''");

            string query = @"
                INSERT INTO snDatabase
                    (fldArtistCode, fldTitle, fldCat1a, fldFilename, fldStartPos, fldDuration, fldIntroPos, fldMixPos, fldArtistName)
                VALUES(" + Artist + ", '" + title + "', " + Category + ", '" + file + "', " + songStart + ", " + songEnd + ", " + intro + ", " + mixIn + ", '" + artistName + "')";

            using (OleDbCommand cmd = new OleDbCommand(query, db))
            {
                cmd.ExecuteNonQuery();
            }
        }

        /// <summary>
        /// Gets the qnet DataTable object.
        /// </summary>
        /// <param name="db">OleDbConnection instance.</param>
        /// <returns>DataTable with QNet data.</returns>
        private static DataTable getQnet(OleDbConnection db)
        {
            DataTable table = new DataTable();

            var cmd = new OleDbCommand("SELECT Artist, [Artist 2] AS Artist2, Artist + ' feat. ' + [Artist 2] AS Feat, Type, Title, File, [Song start] as SongStart, [Song end] AS SongEnd, Intro, [Mix in] AS MixIn FROM _qnet", db);

            OleDbDataReader reader = cmd.ExecuteReader();

            table.Load(reader);

            reader.Close();

            return table;
        }

        #endregion

        static void Main(string[] args)
        {
            if (args.Length > 0)
            {
                #region OleDbConnection db
                OleDbConnection db = connect(args[0].Trim());

                try
                {
                    db.Open();
                }
                catch(Exception ex)
                {
                    Console.WriteLine(ex.Message);
                    return;
                }
                #endregion

                #region Welcome text.

                Console.WriteLine("Opening database: " + args[0].Trim());
                Console.WriteLine(" ");
                Console.WriteLine("Working...");
                Console.WriteLine(" ");

                #endregion

                DataTable qnet = getQnet(db);

                int i = 0;
                foreach (DataRow row in qnet.Rows)
                {
                    i++;

                    database(db, artist(db, row["Artist"], row["Artist2"], row["Feat"]), row["Title"], category(db, row["Type"]), row["File"], row["SongStart"], row["SongEnd"], row["Intro"], row["MixIn"]);

                    if (i % 100 == 0) Console.Write(i + "... ");
                }

                #region Summary

                Console.WriteLine(" ");
                Console.WriteLine(" ");

                Console.WriteLine("Worked with " + qnet.Rows.Count + " rows in " + sw.Elapsed.TotalSeconds.ToString("0.00") + " seconds.");

                #endregion

            }
            else Console.WriteLine("Command: QNet2Jazler.exe file.mdb");

            #region Conclusion.

            Console.WriteLine(" ");
            Console.Write("Press any key to exit...");
            Console.ReadKey();

            #endregion
        }
    }
}
