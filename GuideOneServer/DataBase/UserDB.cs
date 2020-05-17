using GuideOneServer.Models;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Protocols;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Numerics;
using System.Threading.Tasks;

namespace GuideOneServer.DataBase
{
	public static class UserDB
	{
		public static string connectionString;

		public async static Task GetAuth(User user)
		{

			using (SqlConnection con = new SqlConnection(connectionString))
			{
				SqlCommand cmd = new SqlCommand("GetUserAuth", con);
				cmd.CommandType = CommandType.StoredProcedure;
				cmd.Parameters.AddWithValue("@userId", (long)user.Id);
				cmd.Parameters.AddWithValue("@idAuth", (long)user.AuthId);
				//https://social.technet.microsoft.com/wiki/contents/articles/51324.asp-net-core-2-0-crud-operation-with-ado-net.aspx

				await con.OpenAsync();
				SqlDataReader rdr = cmd.ExecuteReader();
				if (rdr.Read())
				{
					user.Token = rdr["Token"].ToString();
					user.PublicKey = rdr["PublicKey"].ToString();
					user.IsConfirmed = Convert.ToBoolean(rdr["IsConfirmed"]?.ToString());
				}
				await con.CloseAsync();
			}
		}

		public async static Task GetRole(User user)
		{
			using (SqlConnection con = new SqlConnection(connectionString))
			{
				SqlCommand cmd = new SqlCommand("GetUserRole", con);
				cmd.CommandType = CommandType.StoredProcedure;
				cmd.Parameters.AddWithValue("@userId", user.Id);
				cmd.Parameters.AddWithValue("@phone", user.Phone);
				//https://social.technet.microsoft.com/wiki/contents/articles/51324.asp-net-core-2-0-crud-operation-with-ado-net.aspx

				await con.OpenAsync();
				SqlDataReader rdr = cmd.ExecuteReader();
				if (rdr.Read())
					user.Role = rdr["Role"].ToString();
				await con.CloseAsync();
			}
		}

		public async static Task Register(User user, string code)
		{
			using (SqlConnection con = new SqlConnection(connectionString))
			{
				SqlCommand cmd = new SqlCommand("CreateUser", con);
				cmd.CommandType = CommandType.StoredProcedure;
				cmd.Parameters.AddWithValue("@phone", user.Phone);
				cmd.Parameters.AddWithValue("@publicKey", user.PublicKey);
				cmd.Parameters.AddWithValue("@token", user.Token);
				cmd.Parameters.AddWithValue("@code", code);
				cmd.Parameters.AddWithValue("@role", user.Role);
				//https://social.technet.microsoft.com/wiki/contents/articles/51324.asp-net-core-2-0-crud-operation-with-ado-net.aspx

				await con.OpenAsync();
				SqlDataReader rdr = cmd.ExecuteReader();
				if (rdr.Read())
				{
					user.Id = (uint)Convert.ToInt64(rdr["UserId"].ToString());
					user.AuthId = (uint)Convert.ToInt64(rdr["AuthId"].ToString());
				}
				await con.CloseAsync();
			}
		}

		public async static Task<bool> Confirm(User user, string code)
		{
			bool rez = false;
			using (SqlConnection con = new SqlConnection(connectionString))
			{
				SqlCommand cmd = new SqlCommand("ConfirmUser", con);
				cmd.CommandType = CommandType.StoredProcedure;
				cmd.Parameters.AddWithValue("@userId", (long)user.Id);
				cmd.Parameters.AddWithValue("@authId", (long)user.AuthId);
				cmd.Parameters.AddWithValue("@code", code);
				//https://social.technet.microsoft.com/wiki/contents/articles/51324.asp-net-core-2-0-crud-operation-with-ado-net.aspx

				await con.OpenAsync();
				SqlDataReader rdr = cmd.ExecuteReader();
				if (rdr.Read())
				{
					rez = Convert.ToBoolean(rdr["IsConfirmed"].ToString());
					user.Name = rdr["Name"].ToString();
					user.SecondName = rdr["SecondName"].ToString();
					user.CompanyId = GetCompanyId(rdr);
					user.HourPrice = GetHourPrice(rdr);
				}
				await con.CloseAsync();
			}
			return rez;
		}

		public async static Task<bool> Logout(User user)
		{
			bool rez = false;
			using (SqlConnection con = new SqlConnection(connectionString))
			{
				SqlCommand cmd = new SqlCommand("DeleteUserAuth", con);
				cmd.CommandType = CommandType.StoredProcedure;
				cmd.Parameters.AddWithValue("@userId", (long)user.Id);
				cmd.Parameters.AddWithValue("@authId", (long)user.AuthId);
				//https://social.technet.microsoft.com/wiki/contents/articles/51324.asp-net-core-2-0-crud-operation-with-ado-net.aspx
				await con.OpenAsync();
				SqlDataReader rdr = cmd.ExecuteReader();
				if (rdr.Read())
					rez = Convert.ToBoolean(Convert.ToInt16(rdr["IsConfirmed"].ToString()));
				await con.CloseAsync();
			}
			return rez;
		}

		public async static Task<bool> Delete(User user)
		{
			bool rez = false;
			using (SqlConnection con = new SqlConnection(connectionString))
			{
				SqlCommand cmd = new SqlCommand("DeleteUser", con);
				cmd.CommandType = CommandType.StoredProcedure;
				cmd.Parameters.AddWithValue("@userId", user.Id);
				//https://social.technet.microsoft.com/wiki/contents/articles/51324.asp-net-core-2-0-crud-operation-with-ado-net.aspx

				await con.OpenAsync();
				SqlDataReader rdr = cmd.ExecuteReader();
				if (rdr.Read())
					rez = Convert.ToBoolean(Convert.ToInt16(rdr["IsConfirmed"].ToString()));
				await con.CloseAsync();
			}
			return rez;
		}

		public async static Task Edit(User user)
		{
			using (SqlConnection con = new SqlConnection(connectionString))
			{
				SqlCommand cmd = new SqlCommand("AlterUser", con);
				cmd.CommandType = CommandType.StoredProcedure;
				cmd.Parameters.AddWithValue("@userId", (long)user.Id);
				cmd.Parameters.AddWithValue("@name", user.Name);
				cmd.Parameters.AddWithValue("@secondName", user.SecondName);
				cmd.Parameters.AddWithValue("@companyId", user.CompanyId);
				cmd.Parameters.AddWithValue("@hourPrice", user.HourPrice);
				//https://social.technet.microsoft.com/wiki/contents/articles/51324.asp-net-core-2-0-crud-operation-with-ado-net.aspx

				await con.OpenAsync();
				SqlDataReader rdr = cmd.ExecuteReader();
				if (rdr.Read())
				{
					user.Name = rdr["Name"].ToString();
					user.SecondName = rdr["SecondName"].ToString();
					user.CompanyId = GetCompanyId(rdr);
					user.HourPrice = GetHourPrice(rdr);
				}
				await con.CloseAsync();
			}
		}

		public async static Task<User> Get(User user, int reqUserId)
		{
			User requestedUSer = null;
			using (SqlConnection con = new SqlConnection(connectionString))
			{
				SqlCommand cmd = new SqlCommand("GetUser", con);
				cmd.CommandType = CommandType.StoredProcedure;
				cmd.Parameters.AddWithValue("@userId", (long)user.Id);
				cmd.Parameters.AddWithValue("@reqUserId", reqUserId);
				//https://social.technet.microsoft.com/wiki/contents/articles/51324.asp-net-core-2-0-crud-operation-with-ado-net.aspx

				await con.OpenAsync();
				SqlDataReader rdr = cmd.ExecuteReader();
				if (rdr.Read())
				{
					requestedUSer = new User();
					requestedUSer.Name = rdr["Name"].ToString();
					requestedUSer.SecondName = rdr["SecondName"].ToString();
					requestedUSer.CompanyId = GetCompanyId(rdr);
					requestedUSer.HourPrice = GetHourPrice(rdr);
				}
				await con.CloseAsync();
			}
			return requestedUSer;
		}

		private static double? GetHourPrice(SqlDataReader reader)
		{
			return string.IsNullOrEmpty(reader["HourPrice"].ToString()) ? null : (double?)Convert.ToDouble(reader["HourPrice"].ToString());
		}
		private static uint? GetCompanyId(SqlDataReader reader)
		{
			return string.IsNullOrEmpty(reader["CompanyId"].ToString()) ? null : (uint?)Convert.ToUInt32(reader["CompanyId"].ToString());
		}
	}
}
