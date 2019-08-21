using System.Data;
using System.Data.SqlClient;
using System.Threading.Tasks;
using TodoApi.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;

namespace TodoApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ConfigController : ControllerBase
    {
        private IConfiguration _configuration;

        private static Config _config;

        public ConfigController(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        // GET: api/Config
        [HttpGet]
        public async Task<Config> Get()
        {
            if (_config == null)
            {
                var config = new Config();
                using (var connection = new SqlConnection(_configuration.GetConnectionString("DefaultConnection")))
                {
                    await connection.OpenAsync();
                    var command = new SqlCommand(DtAccess.BuildStoredProcedureName("GetConfig"), connection);
                    command.CommandType = CommandType.StoredProcedure;
                    var reader = await command.ExecuteReaderAsync();
                    if (reader.Read())
                    {
                        config.NbMailItems = int.Parse(reader["NbMailItems"].ToString());
                        config.EventListTracked = reader["EventListTracked"].ToString();
                        config.PathToStoreSignatureImg = reader["PathToStoreSignatureImg"].ToString();
                        config.TTNumberOfDays = int.Parse(reader["TTNumberOfDays"].ToString());
					}
                }

                _config = config;
            }

            Request.HttpContext.Response.Headers.Add("Cache-Control", "no-cache");
            Request.HttpContext.Response.Headers.Add("Pragma", "no-cache");

            return _config;
        }

        // PUT: api/Config
        [HttpPut]
        public async Task<IActionResult> Put([FromBody] Config config)
        {
            if (_config == null)
            {
                await Get();
            }

            if (!ModelState.IsValid)
            {
                return BadRequest();
            }

            using (var connection = new SqlConnection(_configuration.GetConnectionString("DefaultConnection")))
            {
                await connection.OpenAsync();

                var command = new SqlCommand(DtAccess.BuildStoredProcedureName("SetConfig"), connection);
                command.CommandType = CommandType.StoredProcedure;
                command.Parameters.AddWithValue("@NbMailItems", config.NbMailItems);
                command.Parameters.AddWithValue("@EventListTracked", config.EventListTracked);
                command.Parameters.AddWithValue("@PathToStoreSignatureImg", config.PathToStoreSignatureImg);
                command.Parameters.AddWithValue("@TTNumberOfDays", config.TTNumberOfDays);

				await command.ExecuteNonQueryAsync();

                _config = config;
            }

            return NoContent();
        }
    }
}
