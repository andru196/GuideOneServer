using GuideOneServer.Models;
using NetTopologySuite;
using NetTopologySuite.Geometries;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;

namespace GuideOneServer.DataBase
{
	public static class RecordDb
	{
		public static string connectionString;

		public async static Task Create(Record record)
		{
			using (SqlConnection con = new SqlConnection(connectionString))
			{
				SqlCommand cmd = new SqlCommand("CreateRecord", con);
				cmd.CommandType = CommandType.StoredProcedure;
				cmd.Parameters.AddWithValue("@name", record.Name);
				cmd.Parameters.AddWithValue("@path", record.Path);
				cmd.Parameters.AddWithValue("@duaration", record.Duaration);
				cmd.Parameters.AddWithValue("@userId", record.User.Id);
				cmd.Parameters.AddWithValue("@isPub", record.IsPublic);
				cmd.Parameters.AddWithValue("@isAnon", record.IsAnon);
				cmd.Parameters.AddWithValue("@isPaid", record.IsPaid);
				cmd.Parameters.AddWithValue("@valTime", record.ValidatyTime);
				cmd.Parameters.AddWithValue("@area", record.Area);

				cmd.Parameters.AddWithValue("@PhotoPath", record.PhotoPath);
				cmd.Parameters.AddWithValue("@canSee", record.UserWhoCanSee);
				cmd.Parameters.AddWithValue("@lang", record.Language);
				cmd.Parameters.AddWithValue("@latitude", record.Point.Coordinate.X);
				cmd.Parameters.AddWithValue("@longitude", record.Point.Coordinate.Y);

				await con.OpenAsync();
				SqlDataReader rdr = cmd.ExecuteReader();
				if (rdr.Read())
					record.Id = (uint)Convert.ToInt64(rdr["Id"].ToString());
				await con.CloseAsync();
			}
		}

		public async static Task<bool> Edit(Record record)
		{
			using (SqlConnection con = new SqlConnection(connectionString))
			{
				SqlCommand cmd = new SqlCommand("AlterRecord", con);
				cmd.CommandType = CommandType.StoredProcedure;
				cmd.Parameters.AddWithValue("@recordId", record.Id);
				cmd.Parameters.AddWithValue("@userId", record.User.Id);
				cmd.Parameters.AddWithValue("@name", record.Name);
				cmd.Parameters.AddWithValue("@isPublic", record.IsPublic);
				cmd.Parameters.AddWithValue("@isAnon", record.IsAnon);
				cmd.Parameters.AddWithValue("@isPaid", record.IsPaid);
				cmd.Parameters.AddWithValue("@validityTime", record.ValidatyTime);
				cmd.Parameters.AddWithValue("@area", record.Area);
				cmd.Parameters.AddWithValue("@PhotoPath", record.PhotoPath);
				
				
				await con.OpenAsync();
				var rez = await cmd.ExecuteNonQueryAsync();
				await con.CloseAsync();
				return rez > 0;
			}
		}
		
		public static async Task GetPublic(Record record, uint userId)
		{
			await Get(record, userId, "GetRecordPublic");
		}

		public static async Task GetPaid(Record record, uint userId)
		{
			await Get(record, userId, "GetRecordPaid");
		}

		async static Task Get(Record record, uint userId, string sproc)
		{
			using (SqlConnection con = new SqlConnection(connectionString))
			{
				SqlCommand cmd = new SqlCommand(sproc, con);
				cmd.CommandType = CommandType.StoredProcedure;
				cmd.Parameters.AddWithValue("@recordId", record.Id);
				cmd.Parameters.AddWithValue("@userId", userId);

				await con.OpenAsync();
				SqlDataReader rdr = cmd.ExecuteReader();
				if (rdr.Read())
				{
					record.Id = (uint)Convert.ToInt64(rdr["Id"].ToString());
					record.Path = rdr["Path"].ToString();
					record.Duaration = Convert.ToInt32(rdr["Id"].ToString());
					record.DateTime = DateTime.Parse(rdr["Id"].ToString());
					record.User = new User() { Id = (uint)Convert.ToInt64(rdr["UserId"].ToString()) };
					record.ValidatyTime = DateTime.Parse(rdr["ValidatyTime"].ToString());
					record.PhotoPath = rdr["Id"].ToString();
					record.Language = rdr["Id"].ToString();
					record.Point = NtsGeometryServices.Instance.CreateGeometryFactory(srid: 4326).CreatePoint(new Coordinate(
						Convert.ToDouble(rdr["Latitude"].ToString()),
						Convert.ToDouble(rdr["Longitude"].ToString())));
				}
				await con.CloseAsync();
			}
		}
		public async static Task<bool> Delete(Record record)
		{
			using (SqlConnection con = new SqlConnection(connectionString))
			{
				SqlCommand cmd = new SqlCommand("DeleteRecord", con);
				cmd.CommandType = CommandType.StoredProcedure;
				cmd.Parameters.AddWithValue("@recordId", record.Id);
				cmd.Parameters.AddWithValue("@userId", record.User.Id);


				await con.OpenAsync();
				var rez = await cmd.ExecuteNonQueryAsync();
				await con.CloseAsync();
				return rez > 0;
			}
		}
	}
}
