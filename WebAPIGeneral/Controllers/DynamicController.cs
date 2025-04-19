using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Web.Http;

namespace WebAPIGeneral.Controllers
{
    public class DynamicController : ApiController
    {
        private readonly string _connectionString = "Data Source=LMESQLAP300.LME.FLEXTRONICS.COM;Integrated Security=False;User Id=2024_MaterialsWS;Password=e#_GfB$7;MultipleActiveResultSets=True";

        // POST: api/dynamic
        [AcceptVerbs("POST")]
        [HttpPost]
        [Route("api/dynamic")]
        public IHttpActionResult Post([FromBody] Dictionary<string, object> inputParams)
        {
            try
            {
                // Obtener el nombre del procedimiento almacenado del JSON
                if (inputParams.TryGetValue("@procedureName", out var procedureName))
                {
                    var result = ExecuteStoredProcedure(procedureName.ToString(), inputParams);
                    return Ok(result);
                }
                return BadRequest("El parámetro '@procedureName' no se encuentra en el JSON.");
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }

        private List<Dictionary<string, object>> ExecuteStoredProcedure(string storedProcedureName, Dictionary<string, object> parameters)
        {
            var results = new List<Dictionary<string, object>>();
            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                SqlCommand command = new SqlCommand(storedProcedureName, connection)
                {
                    CommandType = CommandType.StoredProcedure
                };

                foreach (var param in parameters)
                {
                    command.Parameters.AddWithValue(param.Key, param.Value?.ToString());
                }

                connection.Open();

                using (SqlDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        var row = new Dictionary<string, object>();
                        for (int i = 0; i < reader.FieldCount; i++)
                        {
                            row[reader.GetName(i)] = reader.GetValue(i);
                        }
                        results.Add(row);
                    }
                }
            }
            return results;
        }
    }
}
