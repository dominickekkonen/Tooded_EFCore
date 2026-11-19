using Microsoft.Data.SqlClient;
using System.Text.RegularExpressions;

namespace Tooded_EFCore
{
    internal static class DatabaseSetup
    {
        public static void Initialize()
        {
            string path = Path.Combine(
                AppContext.BaseDirectory, "Database.sql");

            string sql = File.ReadAllText(path);

            // Connect to master because Tooded_DB might not exist yet.
            using var connection = new SqlConnection(
                @"Server=(localdb)\MSSQLLocalDB;Database=master;Trusted_Connection=True;Connect Timeout=30;");

            connection.Open(); // Starts the installed LocalDB instance.

            // SqlCommand cannot execute GO, so run each SQL block separately.
            foreach (string batch in Regex.Split(
                sql,
                @"^\s*GO\s*$",
                RegexOptions.Multiline | RegexOptions.IgnoreCase))
            {
                if (string.IsNullOrWhiteSpace(batch))
                    continue;

                using var command = connection.CreateCommand();
                command.CommandText = batch;
                command.CommandTimeout = 120;
                command.ExecuteNonQuery();
            }
        }
    }
}