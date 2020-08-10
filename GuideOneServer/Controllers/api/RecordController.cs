using System;
using System.Buffers.Text;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Threading.Tasks;
using GuideOneServer.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Newtonsoft.Json.Linq;
using System.IO;
using GuideOneServer.DataBase;
using Microsoft.AspNetCore.Authorization;

namespace GuideOneServer.Controllers.api
{
	[Route("api/[controller]")]
	[ApiController]
	[Authorize]
	public class RecordController : G1ControllerBaseController
	{
	   public RecordController(IOptions<Config> c) : base(c){ }
		// GET: api/Record


		[HttpPost]
		[Route("listen")]
		public async Task<FileResult> Liseten()
		{
			var record = ((JObject)HttpContext.Items["JSON"]).GetValue("Record").ToObject<Record>();
			record.Path = null;
			using (var db = new RecordDb())
				if (record.IsPublic)
					await db.GetPublic(record, UserR.Id.Value);
				else
					await db.GetPaid(record, UserR.Id.Value);
			if (record?.Path != null)
			{
				FileStream fs = new FileStream(record.Path, FileMode.Open);
				return File(fs, "application/mp3", record.Name + ".mp3");
			}
			return null;
		}


		[HttpPost]
		[Route("description")]
		public async Task<JsonResult> Description()
		{
			var record = ((JObject)HttpContext.Items["JSON"]).GetValue("Record").ToObject<Record>();
			record.Path = null;
			try
			{
				using (var db = new RecordDb())
					if (record.IsPublic)
						await db.GetPublic(record, UserR.Id.Value);
					else
						await db.GetPaid(record, UserR.Id.Value);
				if (record?.Path != null)
				{
					record.PhotoPath = null;
					record.Path = null;
					return new JsonResult(record);
				}
			}
			catch (Exception ex) 
			{ }
				return null;
		}

		[HttpPost]
		[Route("get")]
		public async Task<JsonResult> GetList()
		{
			var mapData = ((JObject)HttpContext.Items["JSON"]).GetValue("MapSpan").ToObject<MapSpan>();
			using (var db = new RecordDb())
				return new JsonResult(await db.GetList(mapData, (uint)UserR.Id));
		}

		// POST: api/Record
		[HttpPost]
		public async Task<JsonResult> Post()
		{
			Record record = null;
			record = ((JObject)HttpContext.Items["JSON"]).GetValue("Record").ToObject<Record>();
			record.User = UserR;
			if (record.Audio == null || record.Name == null || record.Point == null )//|| (!record.IsPaid && !record.IsAnon && !record.IsPublic) || record.Language == null)
				return null;

			var dirPath = Path.Combine(_config.AutioDirectory, $"userPart{UserR.Id / 1000 + 1}", $"user{UserR.Id}", "audio");
			if (!Directory.Exists(dirPath))
				Directory.CreateDirectory(dirPath);
			var bytes = Convert.FromBase64String(record.Audio);
			record.Audio = null;
			record.Path = Path.Combine(dirPath ,$"{UserR.Id}_{DateTime.UtcNow.ToFileTime()}.wav");
			try
			{
				using (var file = System.IO.File.Create(record.Path))
					await file.WriteAsync(bytes);
				if (record.PhotoPath != null)
				{
					dirPath = Path.Combine(_config.PhotoDirectory, $"userPart{UserR.Id / 1000 + 1}", $"user{UserR.Id}", "photo");
					if (!Directory.Exists(dirPath))
						Directory.CreateDirectory(dirPath);
					bytes = Convert.FromBase64String(record.PhotoPath);
					record.PhotoPath = Path.Combine(dirPath, $"{UserR.Id}_{DateTime.UtcNow.ToFileTime()}.jpg");
					using (var file = System.IO.File.Create(record.PhotoPath))
					{
						await file.WriteAsync(bytes);
					}
				}
				bytes = null;
				using (var db = new RecordDb())
					await db.Create(record);
			}
			catch
			{
				if (System.IO.File.Exists(record.Path))
					System.IO.File.Delete(record.Path);
				if (System.IO.File.Exists(record.PhotoPath))
					System.IO.File.Delete(record.PhotoPath);
			}
			record.PhotoPath = null;
			record.Path = null;
			if (record.Id != 0)
				return new JsonResult(new { Record = record });
			else 
				return new JsonResult(new { Status = "Error"});

		}

		// PUT: api/Record/5
		[HttpPut]
		public async Task<JsonResult> Put()
		{
			var rez = false;
			var record = ((JObject)HttpContext.Items["JSON"]).GetValue("Record").ToObject<Record>();
			using (var db = new RecordDb())
				rez = await db.Edit(record);
			var resp = new { Success = rez };
			return new JsonResult(resp);
		}

		[HttpDelete]
		public async Task<JsonResult> Delete()
		{
			var rez = false;
			var record = ((JObject)HttpContext.Items["JSON"]).GetValue("Record").ToObject<Record>();
			using (var db = new RecordDb())
				rez = await db.Delete(record);
			var resp = new { Success = rez };
			return new JsonResult(resp);
		}
	}
}
