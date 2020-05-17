using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GuideOneServer.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace GuideOneServer.Controllers.api
{
	//[NonController]
	public abstract class G1ControllerBaseController : ControllerBase
	{
		private User _user;
		protected User UserR
		{
			get{
				if (_user == null)
					_user = (User)HttpContext.Items[typeof(User).Name];
				return _user;
			}
		}
		protected Config _config;

		public G1ControllerBaseController(IOptions<Config> settings)
		{
			_config = settings.Value;
		}

	}
}