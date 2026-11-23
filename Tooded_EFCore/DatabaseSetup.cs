using Microsoft.Data.SqlClient;
using System.Text.RegularExpressions;

namespace Tooded_EFCore
{
    internal static class DatabaseSetup
    {
        public static void Initialize()
        {
            using var connection = new SqlConnection(
                @"Server=(localdb)\MSSQLLocalDB;Database=master;Trusted_Connection=True;Connect Timeout=30;");

            connection.Open();

            // Use the existing database when it is already present.
            using (var check = connection.CreateCommand())
            {
                check.CommandText = "SELECT DB_ID(N'Tooded_DB');";

                if (check.ExecuteScalar() is int)
                    return;
            }

            // Create the database from the script on first startup.
            string path = Path.Combine(
                AppContext.BaseDirectory, "Database.sql");

            string sql = File.ReadAllText(path);

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