using Microsoft.Extensions.Options;
using Npgsql;
using PostgreSQLCopyHelper;
using Kenso.Data.Repository;
using Kenso.Domain;
using Models;

namespace DataAccess.Kenso
{
    public class KensoPgRepository : IRepository
    {
        private readonly string _connectionString;
        public KensoPgRepository(IOptions<DatabaseOptions> databaseOptions)
        {
            if (databaseOptions == null) throw new ArgumentNullException(nameof(databaseOptions));

            if (string.IsNullOrEmpty(databaseOptions.Value.ConnectionString))
            {
                throw new ArgumentException("Connection string not provided.");
            }

            _connectionString = databaseOptions.Value.ConnectionString;
        }

        public PostgreSQLCopyHelper<MeasurementRecord> CopyHelper =
            new PostgreSQLCopyHelper<MeasurementRecord>("public", "measurement")
                .MapBigInt("characteristic_id", x => x.CharacteristicId)
                .MapNumeric("value", x => (decimal)x.MeasurementValue)
                .MapTimeStampTz("time", x => x.MeasurementDate);
        public async Task<CharacteristicRecord[]> GetCharacteristics(long[] partIds)
        {
            var characteristics = new List<CharacteristicRecord>();
            var sql = "SELECT characteristic.id, part_id, nominal, usl, lsl FROM characteristic INNER JOIN feature on feature.id = characteristic.feature_id WHERE nominal <> 0 ";

            if (partIds.Length > 0)
            {
                sql += " AND part_id in (";
                for (var i = 0; i < partIds.Length; i++)
                {
                    sql += partIds[i];
                    if (partIds.Length != i + 1)
                    {
                        sql += ",";
                    }
                }

                sql += ")";
            }

            await using var conn = new NpgsqlConnection(_connectionString);
            await conn.OpenAsync();
            await using var cmd = new NpgsqlCommand(sql, conn);
            await using var reader = await cmd.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                var characteristic = new CharacteristicRecord();
                characteristic.Id = reader.GetInt32(0);
                characteristic.PartId = reader.GetInt32(1);
                characteristic.Nominal = reader.GetDecimal(2);

                if (reader.IsDBNull(3))
                {
                    characteristic.Usl = 1;
                }
                else
                {
                    characteristic.Usl = reader.GetDouble(3);
                }

                if (reader.IsDBNull(4))
                {
                    characteristic.Lsl = -1;
                }
                else
                {
                    characteristic.Lsl = reader.GetDouble(4); 
                }

                characteristics.Add(characteristic);
            }
            return characteristics.ToArray();
        }

        public async Task Save(IList<MeasurementRecord> measurements)
        {
            await using var conn = new NpgsqlConnection(_connectionString);
            await conn.OpenAsync();
            await CopyHelper.SaveAllAsync(conn, measurements);
        }
    }
}
