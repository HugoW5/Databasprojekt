using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Databasprojekt
{
	public class Utility
	{
		public static bool RecordExists(SqlConnection conn, SqlTransaction transaction, string tableName, string columnName, int value)
		{
			string query = $"SELECT COUNT(*) FROM {tableName} WHERE {columnName} = @Value";

			using (SqlCommand cmd = new SqlCommand(query, conn, transaction))
			{
				cmd.Parameters.AddWithValue("@Value", value);
				int count = (int)cmd.ExecuteScalar();
				return count > 0;
			}
		}
	}
}
