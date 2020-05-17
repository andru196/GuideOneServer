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

namespace GuideOneServer.Controllers.api
{
	[Route("api/[controller]")]
	[ApiController]
	public class RecordController : G1ControllerBaseController
	{
	   public RecordController(IOptions<Config> c) : base(c){ }
		// GET: api/Record
		

		[HttpGet]
		[Route("listen")]
		public async Task<FileResult> Get()
		{
			var record = ((JObject)HttpContext.Items["JSON"]).GetValue("Record").ToObject<Record>();
			record.Path = null;
			if (record.IsPublic)
				await RecordDb.GetPublic(record, UserR.Id.Value);
			else
				await RecordDb.GetPaid(record, UserR.Id.Value);
			if (record.Path != null)
			{
				FileStream fs = new FileStream(record.Path, FileMode.Open);
				return File(fs, "application/mp3", record.Name + ".mp3");
			}
			return null;
		}

		// POST: api/Record
		[HttpPost]
		public async Task<JsonResult> Post()
		{
			var record = ((JObject)HttpContext.Items["JSON"]).GetValue("Record").ToObject<Record>();
			record.User = UserR;
			if (record.Path == null || record.Name == null || record.Point == null ||
					(!record.IsPaid && !record.IsAnon && !record.IsPublic) || record.Language == null)
				return null;
			var dirPath = Path.Combine(_config.AutioDirectory, $"userPart{UserR.Id / 1000 + 1}", $"user{UserR.Id}", "audio");
			if (!Directory.Exists(dirPath))
				Directory.CreateDirectory(dirPath);
			var bytes = Convert.FromBase64String(record.Path);
			record.Path = Path.Combine(dirPath ,$"{UserR.Id}_{DateTime.UtcNow.ToLongDateString()}.wav");
			try
			{
				using (var file = System.IO.File.Create(record.Path))
				{
					await file.WriteAsync(bytes);
				}
				if (record.PhotoPath != null)
				{
					dirPath = Path.Combine(_config.PhotoDirectory, $"userPart{UserR.Id / 1000 + 1}", $"user{UserR.Id}", "photo");
					if (!Directory.Exists(dirPath))
						Directory.CreateDirectory(dirPath);
					bytes = Convert.FromBase64String(record.PhotoPath);
					record.PhotoPath = Path.Combine(dirPath, $"{UserR.Id}_{DateTime.UtcNow.ToLongDateString()}.jpg");
					using (var file = System.IO.File.Create(record.PhotoPath))
					{
						await file.WriteAsync(bytes);
					}
				}
				bytes = null;
				await RecordDb.Create(record);
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
			{
				var res = new { Record = record };
				return new JsonResult(res);
			}
			else 
				return new JsonResult(new { Status = "Error"});

		}

		// PUT: api/Record/5
		[HttpPut]
		public async Task<JsonResult> Put()
		{
			var record = ((JObject)HttpContext.Items["JSON"]).GetValue("Record").ToObject<Record>();
			var rez = await RecordDb.Edit(record);
			var resp = new { Success = rez };
			return new JsonResult(resp);
		}

		[HttpDelete]
		public async Task<JsonResult> Delete()
		{
			var record = ((JObject)HttpContext.Items["JSON"]).GetValue("Record").ToObject<Record>();
			var rez = await RecordDb.Delete(record);
			var resp = new { Success = rez };
			return new JsonResult(resp);
		}
	}
}
