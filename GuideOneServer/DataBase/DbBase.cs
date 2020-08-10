using Microsoft.Data.SqlClient;
using NetTopologySuite.Geometries;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;

namespace GuideOneServer.DataBase
{
	public abstract class DbBase : IDisposable
	{
		protected static string connectionString;

		~DbBase()
		{
			Dispose();
		}

		public void Dispose()
		{
			if (Opened)
			{
				_connection.Close();
				_opened = false;
			}
		}
		public static string ConnectionString { 
			get
			{
				return connectionString;
			} 
			set
			{
				if (string.IsNullOrEmpty(connectionString))
					connectionString = value;
			}
		}

		ActionMethod ExecuteMethod;

		SqlConnection _connection;

		bool _opened;

		public bool Opened 
		{ 
			get
			{
				return _opened;
			}
		}

		public void Close()
		{
			if (Opened)
			{
				_connection.Close();
				_opened = false;
			}
		}

		protected async Task<SqlDataReader> ExecuteProcedureReaderAsync(string procName, params (string, object)[] prms)
		{
			ExecuteMethod = ActionMethod.ExecuteReader;
			var rez = (SqlDataReader) await ExecuteProcedureWithActionMethodAsync(procName, prms);
			return rez;
		}

		protected async Task<int> ExecuteProcedureNonQueryAsync(string procName, params (string, object)[] prms)
		{
			ExecuteMethod = ActionMethod.ExecuteNonQuery;
			var rez = (int)await ExecuteProcedureWithActionMethodAsync(procName, prms);
			return rez;
		}

		private async Task<object> ExecuteProcedureWithActionMethodAsync(string procName, params (string, object)[] prms)
		{
			_connection = new SqlConnection(connectionString);
			using var command = _connection.CreateCommand();
			command.CommandType = CommandType.StoredProcedure;
			command.CommandText = procName;
			foreach (var obj in prms)
				command.Parameters.AddWithValue(obj.Item1, obj.Item2);
			if (!_opened)
			{
				_connection.Open();
				_opened = true;
			}
			var rez = ExecuteMethod switch
			{
				ActionMethod.ExecuteNonQuery => (object) await command.ExecuteNonQueryAsync(),
				ActionMethod.ExecuteReader => (object) await command.ExecuteReaderAsync(),
				_ => throw new NotImplementedException()
			};
			//_connection.Close();
			return rez;
		}

		enum ActionMethod
		{
			ExecuteReader,
			ExecuteNonQuery
		}
	}
}
