using Npgsql;
using System.Threading.Tasks;
using Xunit;

namespace Bar.Tests
{
    public class DbConnectionTests
    {
        [Fact]
        public async Task TestRelationalConnection()
        {
            using (var connection = new NpgsqlConnection(SliceFixture.RelationalDbConnectionString))
            {
                await connection.OpenAsync();
            };
        }
    }
}
