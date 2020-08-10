using GuideOneServer.DataBase;
using GuideOneServer.Models;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
//using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Pipelines;
using System.Linq;
using System.Security.Cryptography;
using System.Security.Policy;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using JsonSerializer = System.Text.Json.JsonSerializer;

namespace GuideOneServer.Moddleware
{
	public class PreAuthMiddleWare
	{
		private RequestDelegate next;

		public PreAuthMiddleWare(RequestDelegate nxt)
		{
			next = nxt;
		}

		public async Task InvokeAsync(HttpContext context)
		{
			try
			{
				
				var hash = context.Request.Headers["hash"];
				var jobj = (JObject)context.Items["JSON"];
				if (jobj == null)
					return;
				var dt = (DateTime?)jobj.GetValue("CrDt")?.ToObject(typeof(DateTime));//TODO: Заменить время на 5 минут
				jobj.Remove("CrDt");
				if (dt == null || (DateTime.UtcNow - dt.Value).TotalMinutes > 50000 || dt > DateTime.Now)
					throw new Exception("Валидация по времени создания не пройдена");
				var user = (User)context.Items[typeof(User).Name];
				if (user.AuthId != null)
				{
					using (var db = new UserDB())
						await db.GetAuth(user);
					var tokenBytes = Encoding.ASCII.GetBytes(user.Token);
					var bytes = (byte[])context.Items["Content"];
					var bytesToHash = new byte[tokenBytes.Length + bytes.Length];
					Array.Copy(bytes, bytesToHash, bytes.Length);
					Array.Copy(tokenBytes, 0, bytesToHash, bytes.Length, tokenBytes.Length);//Рассмотреть альтернативный способ hex прелбразования хеша 
					var contentHash = BitConverter.ToString(MD5.Create().ComputeHash(bytesToHash)).Replace("-", ""); //Видел есть быстрее
					if (hash != contentHash)
						throw new Exception("Хеши не совпадают");
					if (user.IsConfirmed || context.Request.Path.Value.ToLower().EndsWith(@"user/confirm"))
					{
						context.Request.Headers["Authorization"] = "Bearer " + user.Token;
						user.Token = null;
					}
					context.Items.Remove("Content");
				}
				else if (!context.Request.Path.Value.ToLower().EndsWith(@"user") || context.Request.Method != "POST")
					throw new Exception("Не верный адрес и глагол запроса");
			}
			catch (Exception ex)
			{
				context.Response.StatusCode = 409;
				await context.Response.WriteAsync($"Auth Error\n{ex.Message}");
				return;
			}
			await next.Invoke(context);
		}
	}
}
