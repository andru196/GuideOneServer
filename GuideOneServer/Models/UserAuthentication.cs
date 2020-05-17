using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GuideOneServer.Models
{
	public class UserAuthentication
	{
		public long UserId { get; set; }
		public string Token { get; set; }
		public string PublicKey { get; set; }
		public bool IsConfitm { get; set; }
	}
}
