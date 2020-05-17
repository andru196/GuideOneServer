using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GuideOneServer.Controllers
{
	public class AuthOptions
	{
		public const string ISSUER = "GuideOneServer";
		public const string AUDIENCE = "GuideOneServer";
		const string KEY = "777666GuideOneServerKEY666asdaslksjflksdjfj123kjbkdqkjw__23423rfs1234";
		public const int LIFETIME = 262980;
		public static SymmetricSecurityKey GetSymmetricSecurityKey()
		{
			return new SymmetricSecurityKey(Encoding.ASCII.GetBytes(KEY));
		}

	}
}
