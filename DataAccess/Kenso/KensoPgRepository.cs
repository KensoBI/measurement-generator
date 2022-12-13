using Models;
using Npgsql;
using PostgreSQLCopyHelper;

namespace DataAccess.Kenso
{
    public class KensoPgRepository : IRepository
    {
        public KensoPgRepository(string connectionString)
        {
            ConnectionString = connectionString;
        }

        private string ConnectionString { get; }

        public PostgreSQLCopyHelper<Measurement> CopyHelper =
            new PostgreSQLCopyHelper<Measurement>("public", "measurement")
                .MapBigInt("characteristic_id", x => x.CharacteristicId)
                .MapNumeric("value", x => (decimal)x.Value)
                .MapTimeStampTz("time", x => x.Time);
        public async Task<IList<Characteristic>> GetCharacteristics(int[] partIds)
        {
            var characteristics = new List<Characteristic>();
            var sql = "SELECT characteristic.id, part_id, feature_id, nominal, usl, lsl FROM characteristic INNER JOIN feature on feature.id = characteristic.feature_id";

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
                var characteristic = new Characteristic();
                characteristic.Id = reader.GetInt32(0);
                characteristic.PartId = reader.GetInt32(1);
                characteristic.FeatureId = reader.GetInt32(2);
                characteristic.Nominal = reader.GetDouble(3);

                if (reader.IsDBNull(4))
                {
                    characteristic.Usl = 1;
                }
                else
                {
                    characteristic.Usl = reader.GetDouble(4);
                }

                if (reader.IsDBNull(5))
                {
                    characteristic.Lsl = -1;
                }
                else
                {
                    characteristic.Lsl = reader.GetDouble(5); 
                }

                characteristics.Add(characteristic);
            }
            return characteristics;
        }

        public async Task Save(IList<Measurement> measurements)
        {
            await using var conn = new NpgsqlConnection(ConnectionString);
            await conn.OpenAsync();
            await CopyHelper.SaveAllAsync(conn, measurements);
        }
    }
}
