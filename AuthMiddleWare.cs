using System.Data;
using System.Security.Cryptography;
using System.Data.SqlClient;
namespace Uspevaemost_client
{
    public class AuthMiddleWare
    {
      
  
        private string _createToken(string name,string con)
        {
            using var conn = new SqlConnection(con);
            using var cmd = new SqlCommand("publicbase.dbo.CreateUserToken", conn)
            {
                CommandType = CommandType.StoredProcedure
            };

            cmd.Parameters.AddWithValue("@Login", name);

            var outputToken = new SqlParameter("@Token", SqlDbType.NVarChar, 512)
            {
                Direction = ParameterDirection.Output
            };

            cmd.Parameters.Add(outputToken);

            conn.Open();
            cmd.ExecuteNonQuery();

            return outputToken.Value?.ToString();
        }
        public string createToken(string name,string con)
        {
            string token = _createToken(name,con);
            if (token != "") return token;
            return "";
        }
    }
}
