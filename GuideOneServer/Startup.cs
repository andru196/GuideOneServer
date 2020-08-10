using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GuideOneServer.Controllers;
using GuideOneServer.Middleware;
using GuideOneServer.Moddleware;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using Microsoft.EntityFrameworkCore;
//using GuideOneServer.DBContextModel;
using GuideOneServer.DataBase;
using Microsoft.IdentityModel.Logging;
using Microsoft.AspNetCore.Mvc.NewtonsoftJson;

namespace GuideOneServer
{
	public class Startup
	{
		public Startup(IConfiguration configuration)
		{
			Configuration = configuration;
		}

		public IConfiguration Configuration { get; }

		// This method gets called by the runtime. Use this method to add services to the container.
		public void ConfigureServices(IServiceCollection services)
		{
			string connection = Configuration.GetConnectionString("TestConnection");
			DbBase.ConnectionString = connection;

			services.Configure<Config>(options => Configuration.GetSection("Config").Bind(options));

			//Токены/
			services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
				.AddJwtBearer(options =>
				{
					options.RequireHttpsMetadata = false;
					options.TokenValidationParameters = new TokenValidationParameters
					{
						ValidateIssuer = true,
						ValidIssuer = AuthOptions.ISSUER,
						ValidateAudience = true,
						ValidAudience = AuthOptions.AUDIENCE,
						IssuerSigningKey = AuthOptions.GetSymmetricSecurityKey(),
						ValidateIssuerSigningKey = true
					};
				});
			//services.AddControllersWithViews();
			services.AddControllers().AddNewtonsoftJson();
		}

		// This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
		public void Configure(IApplicationBuilder app, IWebHostEnvironment env, ILoggerFactory loggerFactory)
		{
			//Коневеер
			app.UseMiddleware<JsonParseMiddleware>();
			app.UseMiddleware<PreAuthMiddleWare>();
			app.UseMiddleware<RSAEncoder>();

			if (env.IsDevelopment())
			{
				IdentityModelEventSource.ShowPII = true;
				app.UseDeveloperExceptionPage();
			}

			//app.UseHttpsRedirection();

			app.UseRouting();

			//авторизация
			app.UseAuthentication();
			app.UseAuthorization();

			app.UseEndpoints(endpoints =>
			{
				endpoints.MapControllers();
			});
		}
	}
}
