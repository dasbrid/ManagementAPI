using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TodoApi.Models;
using Microsoft.Extensions.Configuration;
using System.Data.SqlClient;
using System.Data;
using System;

namespace TodoApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TokensController : ControllerBase
    {
        private IConfiguration _configuration;

        public TokensController(IConfiguration configuration)
        {
            _configuration = configuration;
        }
        
        // GET: api/Tokens
        [HttpGet("{lang}")]
        public IEnumerable<Token> GetTokens([FromRoute]string lang)
		{
			var tokens = new List<Token>();
			using (var connection = new SqlConnection(_configuration.GetConnectionString("DefaultConnection")))
			{
				connection.Open();
				var command = new SqlCommand(DtAccess.BuildStoredProcedureName("GetTokens"), connection);
				command.CommandType = CommandType.StoredProcedure;
				command.Parameters.AddWithValue("@Lang", lang);
				var reader = command.ExecuteReader();
				var tokenId = 0;
				Token token = null;
				while (reader.Read())
				{
					if (tokenId != (int)reader["TokenId"])
					{
						tokenId = (int)reader["TokenId"];
						if (token != null)
						{
							tokens.Add(token);
						}

						// ReSharper disable once UseObjectOrCollectionInitializer
						token = new Token();
						token.Id = (int)reader["TokenId"];
						token.TokenName = reader["TokenName"].ToString();
						token.Operations = new List<Operation>();
						token.TokenString = new Guid(reader["TokenString"].ToString());
					}
					
					if (reader["OperationId"] != DBNull.Value)
					{
						// ReSharper disable once UseObjectOrCollectionInitializer
						var operation = new Operation();
						operation.Id = (int)reader["OperationId"];
						if (reader["OperationNameTranslated"] != DBNull.Value)
						{
							operation.Name = reader["OperationNameTranslated"].ToString();
						}
						else
						{
							operation.Name = reader["OperationName"].ToString();
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

						token?.Operations.Add(operation);
					}
					
				}
				if (token != null)
					tokens.Add(token);
			}

			Request.HttpContext.Response.Headers.Add("Cache-Control", "no-cache");
			//Request.HttpContext.Response.Headers.Add("Access-Control-Allow-Origin", "*");
			Request.HttpContext.Response.Headers.Add("Pragma", "no-cache");

			return tokens;
		}

        // GET: api/Tokens/5
		[HttpGet("{lang}/{id}")]
		public IActionResult GetToken([FromRoute]string lang, [FromRoute] int id)
		{
			Token token = null;
			using (var connection = new SqlConnection(_configuration.GetConnectionString("DefaultConnection")))
			{
				connection.Open();
				var command = new SqlCommand(DtAccess.BuildStoredProcedureName("GetToken"), connection);
				command.CommandType = CommandType.StoredProcedure;
				command.Parameters.AddWithValue("@Id", id);
				command.Parameters.AddWithValue("@Lang", lang);
				var reader = command.ExecuteReader();
				while (reader.Read())
				{
					if (token == null)
					{
						// ReSharper disable once UseObjectOrCollectionInitializer
						token = new Token();
						token.Id = (int)reader["TokenId"];
						token.TokenName = reader["TokenName"].ToString();
						token.Operations = new List<Operation>();
						token.TokenString = new Guid(reader["TokenString"].ToString());
					}

					if (reader["OperationId"] != DBNull.Value)
					{
						// ReSharper disable once UseObjectOrCollectionInitializer
						var operation = new Operation();
						operation.Id = (int)reader["OperationId"];
						if (reader["OperationNameTranslated"] != DBNull.Value)
						{
							operation.Name = reader["OperationNameTranslated"].ToString();
						}
						else
						{
							operation.Name = reader["OperationName"].ToString();
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

						token?.Operations.Add(operation);
					}
				}
			}

			Request.HttpContext.Response.Headers.Add("Cache-Control", "no-cache");
			//Request.HttpContext.Response.Headers.Add("Access-Control-Allow-Origin", "*");
			Request.HttpContext.Response.Headers.Add("Pragma", "no-cache");

			return Ok(token);
		}

        // PUT: api/Tokens/5
		[HttpPut("{id}")]
		public IActionResult PutToken([FromRoute] int id, [FromBody] Token token)
		{
			if (!ModelState.IsValid)
			{
				return BadRequest(ModelState);
			}

			if (id != token.Id)
			{
				return BadRequest();
			}

			var operationsIds = string.Join(",", token.Operations.Select(o => o.Id));

			using (var connection = new SqlConnection(_configuration.GetConnectionString("DefaultConnection")))
			{
				connection.Open();
				var command = new SqlCommand(DtAccess.BuildStoredProcedureName("UpdateToken"), connection);
				command.CommandType = CommandType.StoredProcedure;
				command.Parameters.AddWithValue("@Id", token.Id);
				command.Parameters.AddWithValue("@Name", token.TokenName);
				command.Parameters.AddWithValue("@OperationsIds", operationsIds);
				
				command.ExecuteNonQuery();
			}
			return NoContent();
		}

		// POST: api/Tokens
		[HttpPost]
		public IActionResult PostToken([FromBody] Token token)
		{
			if (!ModelState.IsValid)
			{
				return BadRequest(ModelState);
			}

			var operationsIds = string.Join(",", token.Operations.Select(o => o.Id));

			using (var connection = new SqlConnection(_configuration.GetConnectionString("DefaultConnection")))
			{
				connection.Open();
				var command = new SqlCommand(DtAccess.BuildStoredProcedureName("InsertToken"), connection);
				command.CommandType = CommandType.StoredProcedure;
				command.Parameters.AddWithValue("@Name", token.TokenName);
				command.Parameters.AddWithValue("@OperationsIds", operationsIds);

				command.ExecuteNonQuery();
			}

			return Ok();
		}

		// DELETE: api/Tokens/5
		[HttpDelete("{id}")]
		public IActionResult DeleteToken([FromRoute] int id)
		{
			if (!ModelState.IsValid)
			{
				return BadRequest(ModelState);
			}

			var exist = TokenExist(id);
			if (!exist)
			{
				return NotFound();
			}

			using (var connection = new SqlConnection(_configuration.GetConnectionString("DefaultConnection")))
			{
				connection.Open();
				var command = new SqlCommand(DtAccess.BuildStoredProcedureName("DeleteToken"), connection);
				command.CommandType = CommandType.StoredProcedure;
				command.Parameters.AddWithValue("@Id", id);

				command.ExecuteNonQuery();
			}

			return Ok();
		}

        private bool TokenExist(int id)
		{
			using (var connection = new SqlConnection(_configuration.GetConnectionString("DefaultConnection")))
			{
				connection.Open();
				var command = new SqlCommand(DtAccess.BuildStoredProcedureName("GetToken"), connection);
				command.CommandType = CommandType.StoredProcedure;
				command.Parameters.AddWithValue("@Id", id);

				var reader = command.ExecuteReader();

				return reader.HasRows;
			}
		}
    }
}