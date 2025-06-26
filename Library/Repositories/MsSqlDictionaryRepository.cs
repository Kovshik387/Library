using System;
using System.Data;
using System.Data.SqlClient;
using Library.Repositories.Enums;
using Library.Repositories.Interfaces;
using Microsoft.Extensions.Configuration;

namespace Library.Repositories
{
    public class MsSqlDictionaryRepository : IDictionaryRepository
    {
        private readonly string _connectionString;
        private const string DbName = "MSSQL";

        public MsSqlDictionaryRepository(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString(DbName);
        }

        public DataTable GetMetaIdxTables()
        {
            using (var connection = OpenConnection())
            {
                const string sql = "SELECT * FROM METAIDX";

                using (var command = new SqlCommand(sql, connection))
                {
                    using (var adapter = new SqlDataAdapter(command))
                    {
                        var table = new DataTable();
                        adapter.Fill(table);
                        return table;
                    }
                };
            }
        }

        public void RebuildDictionary(DataTable metaIdxTables)
        {
            using (var connection = OpenConnection())
            {
                foreach (DataRow row in metaIdxTables.Rows)
                {
                    string name = row["NAME"].ToString();
                    var parts = name.Split('.');
                    if (parts.Length != 2)
                        continue;

                    string table = parts[0];
                    string term = parts[1];
                    string tableX = table + "X";
                    string tempTable = "TMP_" + Guid.NewGuid().ToString("N");

                    if (!TableExists(connection, table) || !TableExists(connection, tableX))
                    {
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine($"Пропущено: таблицы {table} или {tableX} не найдены.");
                        continue;
                    }

                    string script = @"
CREATE TABLE &TempTable (
    DOC_ID INT,
    &Term NVARCHAR(MAX));
DELETE FROM &TableX;
DELETE FROM &Table;
INSERT INTO &Table(&Term, CNT)
    SELECT &Term, COUNT(&Term)
    FROM &TempTable
    GROUP BY &Term;
INSERT INTO &TableX(IDX_ID, DOC_ID)
    SELECT t2.IDX_ID, t1.DOC_ID
    FROM &Table t2, &TempTable t1
    WHERE t1.&Term = t2.&Term;
DROP TABLE &TempTable;";

                    script = script
                        .Replace("&TempTable", tempTable)
                        .Replace("&Table", table)
                        .Replace("&TableX", tableX)
                        .Replace("&Term", term);

                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine($"Выполняется скрипт для {table} / {tableX}");
                    ExecuteScript(connection, script);
                    Console.WriteLine($@"Словарь ""{row["Caption"]}"" пересобран");
                }
            }
        }

        public DataBaseType GetProvider() => DataBaseType.MsSql;

        private SqlConnection OpenConnection()
        {
            var connection = new SqlConnection(_connectionString);
            connection.Open();
            return connection;
        }

        private static void ExecuteScript(SqlConnection connection, string script)
        {
            var commands = script.Split(';');

            foreach (var raw in commands)
            {
                var sql = raw.Trim();
                if (string.IsNullOrEmpty(sql)) continue;

                using (var command = new SqlCommand(sql, connection))
                {
                    command.ExecuteNonQuery();
                }
            }
        }

        private static bool TableExists(SqlConnection connection, string tableName)
        {
            var query = @"
SELECT COUNT(*) 
 FROM INFORMATION_SCHEMA.TABLES 
WHERE TABLE_NAME = @name";

            using (var cmd = new SqlCommand(query, connection))
            {
                cmd.Parameters.AddWithValue("@name", tableName);
                var count = (int)cmd.ExecuteScalar();
                return count > 0;
            }
        }
    }
}