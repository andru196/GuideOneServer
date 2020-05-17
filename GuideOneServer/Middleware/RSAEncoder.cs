using GuideOneServer.Models;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Security.Cryptography;
using Microsoft.AspNetCore.Mvc;
using System.Text;

namespace GuideOneServer.Middleware
{
	public class RSAEncoder
	{
		private RequestDelegate next;

		public RSAEncoder(RequestDelegate nxt)
		{
			next = nxt;
		}

		public async Task InvokeAsync(HttpContext context)
		{


			var user = (User)context.Items[typeof(User).Name];

			var RSA = new RSACryptoServiceProvider();
			try
			{
				RSA.FromXmlString(user.PublicKey);
			}
			catch (Exception ex)
			{
				context.Response.Clear();
				await context.Response.WriteAsync("RSA not valid");
				return;
			}
			if (!(context.Request.Path.Value.EndsWith("api/user") && context.Request.Method == "POST"))
			user.PublicKey = null;
			await next.Invoke(context);
			var rez = (JsonResult)context.Items["Response"];
			if (rez != null)
			{
				var response = context.Response;
				var json = System.Text.Json.JsonSerializer.Serialize(rez?.Value ?? rez);
				var body = Encoding.UTF8.GetBytes(json);
				response.ContentType = "application/octet-stream";
				var partSize = (RSA.KeySize - 100) / 8;
				var keySize = RSA.KeySize / 8;
				var parts = (body.Length) / partSize + (body.Length % partSize == 0 ? 0 : 1);
				var encBytes = new byte[keySize * parts];
				var bytepart = new byte[partSize];
				try
				{
					for (var i = 0; i < parts; i++)
					{

						var lost = body.Length - partSize * i >= partSize ? partSize : body.Length - partSize * i;
						bytepart = lost == partSize ? bytepart : new byte[lost];
						Array.Copy(body, partSize * i, bytepart, 0, lost);
						var encPart = RSA.Encrypt(bytepart, false);
						Array.Copy(encPart, 0, encBytes, keySize * i, keySize);
					}
				}
				catch (Exception ex)
				{ }
				await response.Body.WriteAsync(encBytes, 0, encBytes.Length);
			}
		}
    }
}
