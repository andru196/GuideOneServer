using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GuideOneServer.Models;
using Newtonsoft.Json.Linq;
using System.IO;

namespace GuideOneServer.Middleware
{
	public class JsonParseMiddleware
	{
		private RequestDelegate next;

		public JsonParseMiddleware(RequestDelegate nxt)
		{
			next = nxt;
		}

		public async Task InvokeAsync(HttpContext context)
		{
			try
			{
				var request = context.Request;
				int length = request.ContentLength.HasValue ? (int) request.ContentLength : 0;
				if (length < 7)
					throw new Exception("Сообщение имеет не достаточный размер");
				if (length > 1024 * 1024 * 5)
					throw new Exception("Сообщение слишком большое");
				var bytes = new byte[length];
				context.Items["Content"] = bytes;
				var count = 0;
				while (count < length)
				{
					count += await request.Body.ReadAsync(bytes, count, length - count);
				}
				using (var stream = new System.IO.StreamReader(new MemoryStream(bytes)))
				{
					using (var jstream = new JsonTextReader(stream))
					{
						var jobj = await JObject.LoadAsync(jstream);
						var user = (User)jobj.GetValue("User").ToObject(typeof(User));
						jobj.Remove("User");
						if (user == null || (user.Id < 1 && string.IsNullOrEmpty(user.Phone)))
							throw new Exception("Не указан пользователь");
						context.Items[typeof(User).Name] = user;
						context.Items["JSON"] = jobj;
						await request.Body.DisposeAsync();
					}
				}
				await next.Invoke(context);
			}
			catch (Exception ex)
			{
				context.Response.StatusCode = 418;
				await context.Response.WriteAsync($"I’m a teapot... Not, it's you\n{ex.Message}");
			}
		}
	}
}
