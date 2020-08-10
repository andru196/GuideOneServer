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
	public class UserDB : DbBase
	{

		public async Task GetAuth(User user)
		{
			SqlDataReader rdr = await ExecuteProcedureReaderAsync("GetUserAuth", ("@userId", (long)user.Id),
				("@idAuth", (long)user.AuthId));
			if (rdr.Read())
			{
				user.Token = rdr["Token"].ToString();
				user.PublicKey = rdr["PublicKey"].ToString();
				user.IsConfirmed = Convert.ToBoolean(rdr["IsConfirmed"]?.ToString());
			}
			Close();
		}

		public async Task GetRole(User user)
		{
			var prms = new (string, object)[] { ("@userId", user.Id), ("@phone", user.Phone) };
			SqlDataReader rdr = await ExecuteProcedureReaderAsync("GetUserRole", prms);
			if (rdr.Read())
				user.Role = rdr["Role"].ToString();
			Close();
		}

		public async  Task Register(User user, string code)
		{
			var prms = new (string, object)[]
			{
				("@phone", user.Phone),
				("@publicKey", user.PublicKey),
				("@token", user.Token),
				("@code", code),
				("@role", user.Role)
			};
			SqlDataReader rdr = await ExecuteProcedureReaderAsync("CreateUser", prms);
			if (rdr.Read())
			{
				user.Id = (uint)Convert.ToInt64(rdr["UserId"].ToString());
				user.AuthId = (uint)Convert.ToInt64(rdr["AuthId"].ToString());
			}
			Close();
		}

		public async Task<bool> Confirm(User user, string code)
		{
			bool rez = false;
			var prms = new (string, object)[]
			{
				("@userId", (long)user.Id),
				("@authId", (long)user.AuthId),
				("@code", code)
			};
			SqlDataReader rdr = await ExecuteProcedureReaderAsync("ConfirmUser", prms);
			if (rdr.Read())
			{
				rez = Convert.ToBoolean(rdr["IsConfirmed"].ToString());
				user.Name = rdr["Name"].ToString();
				user.SecondName = rdr["SecondName"].ToString();
				user.CompanyId = GetCompanyId(rdr);
				user.HourPrice = GetHourPrice(rdr);
			}
			Close();
			return rez;
		}

		public async  Task<bool> Logout(User user)
		{
			var rez = false;
			var prms = new (string, object)[]
			{
				("@userId", (long)user.Id),
				("@authId", (long)user.AuthId)
			};
			var rdr = await ExecuteProcedureReaderAsync("DeleteUserAuth", prms);
			if (rdr.Read())
				rez = Convert.ToBoolean(Convert.ToInt16(rdr["IsConfirmed"].ToString()));
			Close();
			return rez;
		}

		public async Task<bool> Delete(User user)
		{
			var rez = false;
			var rdr = await ExecuteProcedureReaderAsync("DeleteUser", ("@userId", user.Id));
			if (rdr.Read())
				rez = Convert.ToBoolean(Convert.ToInt16(rdr["IsConfirmed"].ToString()));
			Close();
			return rez;
		}

		public async Task Edit(User user)
		{

			var prms = new (string, object)[]
			{
				("@userId", (long)user.Id),
				("@name", user.Name),
				("@secondName", user.SecondName),
				("@companyId", user.CompanyId),
				("@hourPrice", user.HourPrice)
			};
			var rdr = await ExecuteProcedureReaderAsync("AlterUser", prms);
			if (rdr.Read())
			{
				user.Name = rdr["Name"].ToString();
				user.SecondName = rdr["SecondName"].ToString();
				user.CompanyId = GetCompanyId(rdr);
				user.HourPrice = GetHourPrice(rdr);
			}
			Close();
		}

		public async Task<User> Get(User user, int reqUserId)
		{
			var requestedUSer = (User)null;
			var prms = new (string, object)[]
			{   ("@userId", (long)user.Id),
				("@reqUserId", reqUserId)
			};
			var rdr = await ExecuteProcedureReaderAsync("GetUser", prms);
			if (rdr.Read())
			{
				requestedUSer = new User();
				requestedUSer.Name = rdr["Name"].ToString();
				requestedUSer.SecondName = rdr["SecondName"].ToString();
				requestedUSer.CompanyId = GetCompanyId(rdr);
				requestedUSer.HourPrice = GetHourPrice(rdr);
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
