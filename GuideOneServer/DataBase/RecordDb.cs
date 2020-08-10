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
	public  class RecordDb : DbBase
	{

		public async Task Create(Record record)
		{
			var prms = new (string, object)[]
			{
				("@name", record.Name),
				("@path", record.Path),
				("@duaration", record.Duaration),
				("@userId", (long)record.User.Id),
				("@isPub", record.IsPublic),
				("@isAnon", record.IsAnon),
				("@isPaid", record.IsPaid),
				("@valTime", record.ValidatyTime),
				("@area", record.Area),

				("@PhotoPath", record.PhotoPath),
				("@canSee", record.UserWhoCanSee),
				("@lang", record.Language),
				("@latitude", record.Point.Coordinate.X),
				("@longitude", record.Point.Coordinate.Y)
			};
			var rdr = await ExecuteProcedureReaderAsync("CreateRecord", prms);
			if (rdr.Read())
				record.Id = (uint)Convert.ToInt64(rdr["Id"].ToString());
		}

		public async Task<bool> Edit(Record record)
		{
			var prms = new (string, object)[]
			{
				("@recordId", record.Id),
				("@userId", record.User.Id),
				("@name", record.Name),
				("@isPublic", record.IsPublic),
				("@isAnon", record.IsAnon),
				("@isPaid", record.IsPaid),
				("@validityTime", record.ValidatyTime),
				("@area", record.Area),
				("@PhotoPath", record.PhotoPath)
			};
			var rez = await ExecuteProcedureNonQueryAsync("AlterRecord", prms);
			return rez > 0;
		}
		
		public async Task GetPublic(Record record, uint userId)
		{
			await Get(record, userId, "GetRecordPublic");
		}

		public async Task GetPaid(Record record, uint userId)
		{
			await Get(record, userId, "GetRecordPaid");
		}

		async Task Get(Record record, uint userId, string sproc)
		{
		var prms = new (string, object)[]
		{
			("@recordId", (long)record.Id),
			("@userId", (long)userId)
		};
				
		var rdr = await ExecuteProcedureReaderAsync(sproc, prms);
			if (rdr.Read())
			{
				//record.Id = (uint)Convert.ToInt64(rdr["Id"].ToString());
				record.Path = rdr["Path"].ToString();
				record.Duaration = Convert.ToInt32(rdr["Duration"].ToString());
				record.Description = rdr["Description"].ToString();
				record.DateTime = DateTime.Parse(rdr["DateTime"].ToString());
				record.User = new User() { Id = (uint)Convert.ToInt64(rdr["UserId"].ToString()) };
				//record.ValidatyTime = DateTime.Parse(rdr["ValidatyTime"].ToString());
				record.PhotoPath = rdr["PhotoPath"]?.ToString();
				record.Language = rdr["Language"].ToString();
				record.Point = NtsGeometryServices.Instance.CreateGeometryFactory(srid: 4326).CreatePoint(new Coordinate(
					Convert.ToDouble(rdr["Latitude"].ToString()),
					Convert.ToDouble(rdr["Longitude"].ToString())));
				}
			}

		public async Task<List<Record>> GetList(MapSpan ms, uint userId)
		{
			try
			{
				var prms = new (string, object)[]
				{
					("@latitude", ms.Latitude),
					("@longitude", ms.Longitude),
					("@radius", ms.Radius),
					("@userId", (long)userId)
				};
				var rdr = await ExecuteProcedureReaderAsync("GetRecordsForMap", prms);
				var lst = new List<Record>();
				while (rdr.Read())
					lst.Add(new Record
					{
						Id = (uint)Convert.ToInt64(rdr["Id"].ToString()),
						Name = rdr["Name"].ToString(),
						Language = rdr["Language"].ToString(),
						Point = NtsGeometryServices.Instance.CreateGeometryFactory(srid: 4326).CreatePoint(new Coordinate(
							rdr.GetDouble("Latitude"), rdr.GetDouble("Longitude"))),
						//ValidatyTime = rdr.GetDateTime("ValidatyTime"),
						//Raiting = rdr.GetDouble("Raiting")

					});
				return lst;
			}
			catch (Exception ex)
			{
				return null;
			}
		}
		public async Task<bool> Delete(Record record)
		{
			var prms = new (string, object)[]
			{
				("@recordId", record.Id),
				("@userId", record.User.Id)
			};
			var rez = await ExecuteProcedureNonQueryAsync("DeleteRecord", prms);
			return rez > 0;
		}
	}
}
