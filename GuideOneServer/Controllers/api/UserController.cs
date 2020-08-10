using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using GuideOneServer.DataBase;
using GuideOneServer.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json.Linq;

namespace GuideOneServer.Controllers.api
{
	[Route("api/[controller]")]
	[ApiController]
	public class UserController : G1ControllerBaseController
	{
		public UserController(IOptions<Config> c) : base(c) { }

		[Authorize]
		[HttpGet]
		public async Task<JsonResult> Get()
		{
			User reqUser;
			using (var db = new UserDB())
				reqUser = await db.Get(UserR, ((JObject)HttpContext.Items["JSON"]).GetValue("userId").ToObject<int>());
			var response = new JsonResult(new
			{
				user = reqUser
			});
			HttpContext.Items["Response"] = response;

			return response;
		}

		[Authorize]
		[HttpDelete]
		[Route("logout")]
		public async Task<JsonResult> Logout(string code)
		{
			var rez = false;
			using (var db = new UserDB())
				rez = await db.Logout(UserR);
			var resp = new JsonResult(new { Success = rez });
			HttpContext.Items["Response"] = resp;

			return resp;
		}

		[Authorize]
		[HttpPost]
		[Route("confirm")]
		public async Task<JsonResult> Confirm(string code)
		{
			code = ((JObject)HttpContext.Items["JSON"]).GetValue("Code").ToString();
			var rez = false;
			using (var db = new UserDB())
				rez = await db.Confirm(UserR, code);
			var resp = new JsonResult(new
			{
				IsConfirmed = rez,
				User = rez ? UserR : null
			});
			//HttpContext.Items["Response"] = resp;

			return resp;
		}

		// POST: api/User
		//[RequireHttps]
		[HttpPost]
		public async Task Post()
		{
			var user = (User)this.HttpContext.Items[typeof(User).Name];
			if (user.Phone == null || user.PublicKey == null)
			{
				HttpContext.Response.StatusCode = 406;
				await HttpContext.Response.WriteAsync("Недостаточно данных для регистрации!");
				;
			}
			var identity = await GetIdentity(user);
			var now = DateTime.UtcNow;
			// создаем JWT-токен
			
			var jwt = new JwtSecurityToken(
					issuer: AuthOptions.ISSUER,
					audience: AuthOptions.AUDIENCE,
					notBefore: now,
					claims: identity.Claims,
					expires: now.Add(TimeSpan.FromMinutes(AuthOptions.LIFETIME)),
					signingCredentials: new SigningCredentials(AuthOptions.GetSymmetricSecurityKey(), SecurityAlgorithms.HmacSha256));
			user.Token = new JwtSecurityTokenHandler().WriteToken(jwt);
			
			var rand = new Random();
			string code = rand.Next(0, 10).ToString() + rand.Next(0, 10).ToString() + rand.Next(0, 10).ToString()
					+ rand.Next(0, 10).ToString() + rand.Next(0, 10).ToString() + rand.Next(0, 10).ToString();
			using (var db = new UserDB())
				await db.Register(user, code);
			var response = new
			{
				access_token = user.Token,
				userId = user.Id,
				authId = user.AuthId
			};
			
			HttpContext.Items["Response"] =  new JsonResult(response);
		}

		[NonAction]
		private async Task<ClaimsIdentity> GetIdentity(User user)
		{
			using (var db = new UserDB()) 
				await db.GetRole(user);
			var claims = new List<Claim>
				{
					new Claim(ClaimsIdentity.DefaultNameClaimType, user.Phone),
					new Claim(ClaimsIdentity.DefaultRoleClaimType, user.Role)
				};
			ClaimsIdentity claimsIdentity =
			new ClaimsIdentity(claims, "Token", ClaimsIdentity.DefaultNameClaimType,
				ClaimsIdentity.DefaultRoleClaimType);
			return claimsIdentity;
		}

		// PUT: api/User/5
		[Authorize]
		[HttpPut]
		public async Task<JsonResult> Put()
		{
			using (var db = new UserDB())
				await db.Edit(UserR);
			var resp = new JsonResult(new
			{
				User = UserR
			});
			HttpContext.Items["Response"] = resp;

			return resp;
		}

		// DELETE: api/ApiWithActions/5
		[Authorize]
		[HttpDelete]
		public async Task<JsonResult> Delete()
		{
			var rez = false;
			using (var db = new UserDB())
				rez = await db.Delete(UserR);
			var resp = new JsonResult(new
			{
				IsConfirmed = rez
			}) ;
			HttpContext.Items["Response"] = resp;

			return resp;
		}
	}
}
