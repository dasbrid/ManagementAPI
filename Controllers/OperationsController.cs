using System;
using System.Collections.Generic;
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
    public class OperationsController : ControllerBase
	{
	    private IConfiguration _configuration;

	    public OperationsController(IConfiguration configuration)
	    {
	        _configuration = configuration;
        }

        // GET: api/Operations
        [HttpGet("{lang}")]
        public async Task<IEnumerable<Operation>> GetOperations([FromRoute]string lang)
        {
            var operations = new List<Operation>();
            using (var connection = new SqlConnection(_configuration.GetConnectionString("DefaultConnection")))
            {
                await connection.OpenAsync();
                var command = new SqlCommand(DtAccess.BuildStoredProcedureName("GetOperations"), connection);
                command.CommandType = CommandType.StoredProcedure;
                command.Parameters.AddWithValue("@Lang", lang);
                var reader = await command.ExecuteReaderAsync();
                while (reader.Read())
                {
                    // ReSharper disable once UseObjectOrCollectionInitializer
                    var operation = new Operation();
                    operation.Id = (int)reader["Id"];
                    if (reader["OperationNameTranslated"] != DBNull.Value)
                    {
                        operation.Name = reader["OperationNameTranslated"].ToString();
                    }
                    else
                    {
                        operation.Name = reader["Name"].ToString();
                    }

                    if (reader["DescriptionTranslated"] != DBNull.Value)
                    {
                        operation.Description = reader["DescriptionTranslated"].ToString();
                    }
                    else
                    {
                        operation.Description = reader["Description"].ToString();
                    }
                    operation.Signature = reader["Signature"].ToString();

                    operations.Add(operation);
                }
            }

            Request.HttpContext.Response.Headers.Add("Cache-Control", "no-cache");
            Request.HttpContext.Response.Headers.Add("Pragma", "no-cache");

            return operations;
        }
    }
}