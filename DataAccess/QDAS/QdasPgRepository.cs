using Kenso.Domain;
using Models;
using Npgsql;
using PostgreSQLCopyHelper;

namespace DataAccess.QDAS
{
    public class QdasPgRepository : IRepository 
    {
        public QdasPgRepository(string connectionString)
        {
            ConnectionString = connectionString;
        }

        private string ConnectionString { get; }

        public async Task<int> GetMaxMeasurementId()
        {
            await using var conn = new NpgsqlConnection(ConnectionString);
            await conn.OpenAsync();
            await using var cmd = new NpgsqlCommand("SELECT MAX(Wvwertnr) FROM wertevar", conn);
            await using var reader = await cmd.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                if (!reader.IsDBNull(0))
                {
                    return reader.GetInt32(0);
                }
            }

            return 0;
        }
        public PostgreSQLCopyHelper<MeasurementRecord> CopyHelper =
            new PostgreSQLCopyHelper<MeasurementRecord>("public", "WERTEVAR")
                .MapSmallInt("WVUNTERS", x => (short)x.CharacteristicId)
                .MapInteger("WVWERTNR", x => x.Id)
                .MapInteger("WVTEIL", x => (int) x.PartId)
                .MapSmallInt("WVMERKMAL", x => (short)x.CharacteristicId)
                .MapTimeStampTz("WVDATZEIT", x => x.Time)
                .MapInteger("wvmaschine", (x) => 2)
                .MapDouble("WVWERT", x => x.Value);
        public async Task<MeasurementRecord[]> GetCharacteristics(long[] partIds)
        {
            var characteristics = new List<CharacteristicRecord>();
            var sql = "SELECT memerkmal as id, meteil as part_id, menennmas as nominal, meogw as usl, meugw as lsl FROM merkmal";

            if (partIds.Length > 0)
            {
                sql += " WHERE part_id in (";
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

            await using var conn = new NpgsqlConnection(ConnectionString);
            await conn.OpenAsync();
            await using var cmd = new NpgsqlCommand(sql, conn);
            await using var reader = await cmd.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                var characteristic = new CharacteristicRecord();
                characteristic.Id = reader.GetInt32(0);
                characteristic.PartId = reader.GetInt32(1);
                characteristic.Nominal = reader.GetDouble(2);

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
            return characteristics;
        }



        public async Task Save(IList<MeasurementRecord> measurements)
        {
            var measurementId = await GetMaxMeasurementId();
            foreach (var measurement in measurements)
            {
                measurement.Id = ++measurementId;
            }

            await using var conn = new NpgsqlConnection(ConnectionString);
            await conn.OpenAsync();
            await CopyHelper.SaveAllAsync(conn, measurements);
        }
    }
}
