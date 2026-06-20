using CSharpFunctionalExtensions;
using Domain.InterFaces;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using System.Globalization;
using System.Text;

namespace Infrastructure.Services
{
    public sealed class DatabaseBackupService : IDatabaseBackupService
    {
        private const string BackupHeaderMarker = "NAQLAH_FULL_DATABASE_BACKUP";
        private const string TableBeginPrefix = "-- BEGIN TABLE:";
        private const string TableEndPrefix = "-- END TABLE:";


        private readonly string _connectionString;

        public DatabaseBackupService(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection")
                ?? throw new InvalidOperationException("DefaultConnection is not configured.");
        }

        public async Task<Result<MemoryStream>> CreateFullBackupSqlAsync(
            IProgress<(int completed, int total, string currentItem)>? progress = null,
            CancellationToken cancellationToken = default)
        {
            try
            {
                await using var connection = new SqlConnection(_connectionString);
                await connection.OpenAsync(cancellationToken);

                var databaseName = connection.Database;
                var tables = await GetTablesAsync(connection, cancellationToken);
                var orderedTables = OrderTablesByForeignKeys(tables, await GetForeignKeyEdgesAsync(connection, cancellationToken));
                var total = orderedTables.Count;

                var stream = new MemoryStream();
                await using (var writer = new StreamWriter(stream, new UTF8Encoding(false), 1024 * 64, leaveOpen: true))
                {
                    await writer.WriteLineAsync("/*");
                    await writer.WriteLineAsync(BackupHeaderMarker);
                    await writer.WriteLineAsync("Version: 2");
                    await writer.WriteLineAsync($"GeneratedUtc: {DateTime.UtcNow:O}");
                    await writer.WriteLineAsync($"Database: {databaseName}");
                    await writer.WriteLineAsync("RestoreMode: MERGE_MISSING_ONLY");
                    await writer.WriteLineAsync("*/");

                    var completed = 0;
                    foreach (var table in orderedTables)
                    {
                        cancellationToken.ThrowIfCancellationRequested();
                        progress?.Report((completed, total, $"{table.Schema}.{table.Name}"));

                        try
                        {
                            await AppendTableBackupAsync(connection, table, writer, cancellationToken);
                        }
                        catch (Exception ex)
                        {
                            await writer.WriteLineAsync($"-- BACKUP ERROR FOR {table.Schema}.{table.Name}: {ex.Message.Replace("\r", " ").Replace("\n", " ")}");
                            await writer.WriteLineAsync("GO");
                        }

                        completed++;
                        progress?.Report((completed, total, $"{table.Schema}.{table.Name}"));
                    }

                    await writer.FlushAsync();
                }

                stream.Position = 0;
                return Result.Success(stream);
            }
            catch (Exception ex)
            {
                return Result.Failure<MemoryStream>($"DatabaseBackupFailed:{ex.Message}");
            }
        }

        public async Task<Result<DatabaseRestoreSummary>> RestoreMergeFromSqlAsync(
            Stream sqlStream,
            IProgress<(int completed, int total, string currentItem)>? progress = null,
            CancellationToken cancellationToken = default)
        {
            if (sqlStream == null || !sqlStream.CanRead)
                return Result.Failure<DatabaseRestoreSummary>("BackupFileRequired");

            try
            {
                using var reader = new StreamReader(sqlStream, Encoding.UTF8, detectEncodingFromByteOrderMarks: true, leaveOpen: true);
                var sql = await reader.ReadToEndAsync(cancellationToken);

                if (string.IsNullOrWhiteSpace(sql) || !sql.Contains(BackupHeaderMarker, StringComparison.Ordinal))
                    return Result.Failure<DatabaseRestoreSummary>("InvalidBackupFile");

                var tableSections = ParseTableSections(sql);
                if (tableSections.Count == 0)
                    return Result.Failure<DatabaseRestoreSummary>("InvalidBackupFile");

                var totalTables = tableSections.Count;
                var rowsInserted = 0;
                var rowsSkipped = 0;
                var tablesChanged = 0;
                var batchesExecuted = 0;
                var tablesProcessed = 0;

                await using var connection = new SqlConnection(_connectionString);
                await connection.OpenAsync(cancellationToken);

                await ExecuteNonQueryAsync(connection, "SET NOCOUNT OFF;", cancellationToken);

                var completed = 0;
                foreach (var section in tableSections)
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    progress?.Report((completed, totalTables, section.TableName));

                    var tableInserted = 0;

                    foreach (var batch in section.Batches)
                    {
                        var trimmed = batch.Trim();
                        if (string.IsNullOrWhiteSpace(trimmed))
                            continue;

                        if (trimmed.StartsWith("--", StringComparison.Ordinal))
                            continue;

                        batchesExecuted++;

                        if (trimmed.StartsWith("MERGE", StringComparison.OrdinalIgnoreCase))
                        {
                            var affected = await ExecuteNonQueryAsync(connection, trimmed, cancellationToken);
                            if (affected > 0)
                            {
                                rowsInserted += affected;
                                tableInserted += affected;
                            }
                            else
                            {
                                rowsSkipped++;
                            }

                            continue;
                        }

                        await ExecuteNonQueryAsync(connection, trimmed, cancellationToken);
                    }

                    tablesProcessed++;
                    if (tableInserted > 0)
                        tablesChanged++;

                    completed++;
                    progress?.Report((completed, totalTables, section.TableName));
                }

                return Result.Success(new DatabaseRestoreSummary
                {
                    TotalTables = totalTables,
                    TablesProcessed = tablesProcessed,
                    TablesChanged = tablesChanged,
                    RowsInserted = rowsInserted,
                    RowsSkipped = rowsSkipped,
                    BatchesExecuted = batchesExecuted
                });
            }
            catch (SqlException ex)
            {
                return Result.Failure<DatabaseRestoreSummary>($"DatabaseRestoreFailed:{ex.Message}");
            }
            catch (Exception)
            {
                return Result.Failure<DatabaseRestoreSummary>("DatabaseRestoreFailed");
            }
        }

        private static async Task<int> ExecuteNonQueryAsync(SqlConnection connection, string sql, CancellationToken cancellationToken)
        {
            await using var command = new SqlCommand(sql, connection) { CommandTimeout = 0 };
            return await command.ExecuteNonQueryAsync(cancellationToken);
        }

        private static List<TableSection> ParseTableSections(string sql)
        {
            var sections = new List<TableSection>();
            var lines = sql.Replace("\r\n", "\n").Split('\n');
            TableSection? current = null;
            var batchBuilder = new StringBuilder();

            void FlushBatch()
            {
                if (current == null || batchBuilder.Length == 0)
                    return;

                var batch = batchBuilder.ToString().Trim();
                batchBuilder.Clear();
                if (!string.IsNullOrWhiteSpace(batch))
                    current.Batches.Add(batch);
            }

            foreach (var rawLine in lines)
            {
                var line = rawLine;
                var trimmed = line.Trim();

                if (trimmed.StartsWith(TableBeginPrefix, StringComparison.OrdinalIgnoreCase))
                {
                    FlushBatch();
                    current = new TableSection
                    {
                        TableName = trimmed[TableBeginPrefix.Length..].Trim()
                    };
                    sections.Add(current);
                    continue;
                }

                if (trimmed.StartsWith(TableEndPrefix, StringComparison.OrdinalIgnoreCase))
                {
                    FlushBatch();
                    current = null;
                    continue;
                }

                if (current == null)
                    continue;

                if (trimmed.Equals("GO", StringComparison.OrdinalIgnoreCase))
                {
                    FlushBatch();
                    continue;
                }

                batchBuilder.AppendLine(line);
            }

            FlushBatch();
            return sections;
        }

        private static async Task AppendTableBackupAsync(
            SqlConnection connection,
            TableInfo table,
            TextWriter writer,
            CancellationToken cancellationToken)
        {
            var columns = await GetColumnsAsync(connection, table, cancellationToken);
            if (columns.Count == 0)
                return;

            var pkColumns = columns.Where(c => c.IsPrimaryKey).Select(c => c.Name).ToList();
            if (pkColumns.Count == 0)
                pkColumns = new List<string> { columns[0].Name };

            var hasIdentity = columns.Any(c => c.IsIdentity);
            var qualifiedName = table.QualifiedName;
            var columnList = string.Join(", ", columns.Select(c => $"[{c.Name}]"));
            var selectSql = $"SELECT {columnList} FROM {qualifiedName}";

            await writer.WriteLineAsync($"{TableBeginPrefix} {table.Schema}.{table.Name}");
            await writer.WriteLineAsync("GO");

            if (hasIdentity)
            {
                await writer.WriteLineAsync($"SET IDENTITY_INSERT {qualifiedName} ON;");
                await writer.WriteLineAsync("GO");
            }

            await using var command = new SqlCommand(selectSql, connection) { CommandTimeout = 0 };
            await using var reader = await command.ExecuteReaderAsync(cancellationToken);

            var rowCount = 0;
            while (await reader.ReadAsync(cancellationToken))
            {
                var sourceColumns = new List<string>();
                var sourceValues = new List<string>();
                var onConditions = new List<string>();

                for (var i = 0; i < columns.Count; i++)
                {
                    var column = columns[i];
                    var value = reader.IsDBNull(i) ? null : reader.GetValue(i);
                    var formatted = FormatSqlValue(value, column.DataType);
                    sourceColumns.Add($"[{column.Name}]");
                    sourceValues.Add($"{formatted} AS [{column.Name}]");

                    if (pkColumns.Contains(column.Name))
                        onConditions.Add($"t.[{column.Name}] = s.[{column.Name}]");
                }

                await writer.WriteLineAsync($"MERGE {qualifiedName} AS t");
                await writer.WriteLineAsync($"USING (SELECT {string.Join(", ", sourceValues)}) AS s");
                await writer.WriteLineAsync($"ON {string.Join(" AND ", onConditions)}");
                await writer.WriteLineAsync($"WHEN NOT MATCHED BY TARGET THEN INSERT ({columnList}) VALUES ({string.Join(", ", sourceColumns.Select(c => $"s.{c}"))});");
                await writer.WriteLineAsync("GO");
                rowCount++;
            }

            if (hasIdentity && rowCount > 0)
            {
                await writer.WriteLineAsync($"SET IDENTITY_INSERT {qualifiedName} OFF;");
                await writer.WriteLineAsync("GO");
            }

            await writer.WriteLineAsync($"{TableEndPrefix} {table.Schema}.{table.Name} ({rowCount} rows)");
            await writer.WriteLineAsync("GO");
        }

        private static async Task<List<TableInfo>> GetTablesAsync(SqlConnection connection, CancellationToken cancellationToken)
        {
            const string sql = """
                SELECT s.name AS SchemaName, t.name AS TableName
                FROM sys.tables t
                INNER JOIN sys.schemas s ON s.schema_id = t.schema_id
                WHERE t.is_ms_shipped = 0
                  AND t.temporal_type = 0
                  AND s.name NOT IN ('HangFire', 'hangfire')
                ORDER BY s.name, t.name
                """;

            var tables = new List<TableInfo>();
            await using var command = new SqlCommand(sql, connection);
            await using var reader = await command.ExecuteReaderAsync(cancellationToken);
            while (await reader.ReadAsync(cancellationToken))
            {
                tables.Add(new TableInfo(reader.GetString(0), reader.GetString(1)));
            }

            return tables;
        }

        private static async Task<List<(TableInfo Parent, TableInfo Referenced)>> GetForeignKeyEdgesAsync(
            SqlConnection connection,
            CancellationToken cancellationToken)
        {
            const string sql = """
                SELECT
                    OBJECT_SCHEMA_NAME(fk.parent_object_id) AS ParentSchema,
                    OBJECT_NAME(fk.parent_object_id) AS ParentTable,
                    OBJECT_SCHEMA_NAME(fk.referenced_object_id) AS ReferencedSchema,
                    OBJECT_NAME(fk.referenced_object_id) AS ReferencedTable
                FROM sys.foreign_keys fk
                """;

            var edges = new List<(TableInfo Parent, TableInfo Referenced)>();
            await using var command = new SqlCommand(sql, connection);
            await using var reader = await command.ExecuteReaderAsync(cancellationToken);
            while (await reader.ReadAsync(cancellationToken))
            {
                edges.Add((
                    new TableInfo(reader.GetString(0), reader.GetString(1)),
                    new TableInfo(reader.GetString(2), reader.GetString(3))));
            }

            return edges;
        }

        private static List<TableInfo> OrderTablesByForeignKeys(
            IReadOnlyList<TableInfo> tables,
            IReadOnlyList<(TableInfo Parent, TableInfo Referenced)> edges)
        {
            var tableSet = tables.ToHashSet();
            var inDegree = tables.ToDictionary(t => t, _ => 0);
            var adjacency = tables.ToDictionary(t => t, _ => new List<TableInfo>());

            foreach (var (parent, referenced) in edges)
            {
                if (!tableSet.Contains(parent) || !tableSet.Contains(referenced) || parent.Equals(referenced))
                    continue;

                adjacency[referenced].Add(parent);
                inDegree[parent]++;
            }

            var queue = new Queue<TableInfo>(inDegree.Where(x => x.Value == 0).Select(x => x.Key));
            var ordered = new List<TableInfo>();

            while (queue.Count > 0)
            {
                var current = queue.Dequeue();
                ordered.Add(current);

                foreach (var child in adjacency[current])
                {
                    inDegree[child]--;
                    if (inDegree[child] == 0)
                        queue.Enqueue(child);
                }
            }

            foreach (var table in tables)
            {
                if (!ordered.Contains(table))
                    ordered.Add(table);
            }

            return ordered;
        }

        private static async Task<List<ColumnInfo>> GetColumnsAsync(
            SqlConnection connection,
            TableInfo table,
            CancellationToken cancellationToken)
        {
            const string sql = """
                SELECT
                    c.name,
                    t.name AS data_type,
                    c.is_nullable,
                    c.is_identity,
                    CASE WHEN pk.column_id IS NOT NULL THEN 1 ELSE 0 END AS is_primary_key
                FROM sys.columns c
                INNER JOIN sys.types t ON c.user_type_id = t.user_type_id
                INNER JOIN sys.tables tb ON tb.object_id = c.object_id
                INNER JOIN sys.schemas s ON s.schema_id = tb.schema_id
                LEFT JOIN (
                    SELECT ic.object_id, ic.column_id
                    FROM sys.indexes i
                    INNER JOIN sys.index_columns ic ON ic.object_id = i.object_id AND ic.index_id = i.index_id
                    WHERE i.is_primary_key = 1
                ) pk ON pk.object_id = c.object_id AND pk.column_id = c.column_id
                WHERE s.name = @schema
                  AND tb.name = @table
                  AND c.is_computed = 0
                  AND t.name NOT IN ('timestamp', 'rowversion')
                ORDER BY c.column_id
                """;

            var columns = new List<ColumnInfo>();
            await using var command = new SqlCommand(sql, connection);
            command.Parameters.AddWithValue("@schema", table.Schema);
            command.Parameters.AddWithValue("@table", table.Name);

            await using var reader = await command.ExecuteReaderAsync(cancellationToken);
            while (await reader.ReadAsync(cancellationToken))
            {
                columns.Add(new ColumnInfo(
                    reader.GetString(0),
                    reader.GetString(1),
                    reader.GetBoolean(2),
                    reader.GetBoolean(3),
                    reader.GetInt32(4) == 1));
            }

            return columns;
        }

        private static string FormatSqlValue(object? value, string dataType)
        {
            if (value == null || value is DBNull)
                return "NULL";

            switch (value)
            {
                case bool boolValue:
                    return boolValue ? "1" : "0";
                case byte[] bytes:
                    return "0x" + Convert.ToHexString(bytes);
                case Guid guid:
                    return $"N'{guid}'";
                case DateTime dateTime:
                    return $"CONVERT(datetime2, '{dateTime:yyyy-MM-dd HH:mm:ss.fff}', 121)";
                case DateTimeOffset dateTimeOffset:
                    return $"CONVERT(datetimeoffset, '{dateTimeOffset:yyyy-MM-dd HH:mm:ss.fff zzz}', 127)";
                case string text:
                    return "N'" + text.Replace("'", "''") + "'";
                case char character:
                    return "N'" + character.ToString().Replace("'", "''") + "'";
                case float or double or decimal or int or long or short or byte:
                    return Convert.ToString(value, CultureInfo.InvariantCulture) ?? "NULL";
                default:
                    if (value is IFormattable formattable && dataType is not ("nvarchar" or "varchar" or "nchar" or "char" or "text" or "ntext"))
                        return formattable.ToString(null, CultureInfo.InvariantCulture) ?? "NULL";

                    return "N'" + (value.ToString()?.Replace("'", "''") ?? string.Empty) + "'";
            }
        }

        private sealed class TableSection
        {
            public string TableName { get; init; } = string.Empty;
            public List<string> Batches { get; } = new();
        }

        private sealed record TableInfo(string Schema, string Name)
        {
            public string QualifiedName => $"[{Schema}].[{Name}]";
        }

        private sealed record ColumnInfo(string Name, string DataType, bool IsNullable, bool IsIdentity, bool IsPrimaryKey);
    }
}
