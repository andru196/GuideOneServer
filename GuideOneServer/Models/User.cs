using System.Text.Json;
using System;
using System.Collections.Generic;
using System.Linq;
//using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace GuideOneServer.Models
{
	//[JsonConverter]
	public class User
	{
		public uint? AuthId { get; set; }
		public uint? Id { get; set; }
		[JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
		public string Phone { get; set; }
		[JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
		public string Name { get; set; }
		[JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
		public string SecondName { get; set; }
		[JsonProperty(NullValueHandling = NullValueHandling.Ignore)]

		public double? HourPrice { get; set; }
		[JsonIgnore]
		public string Role { get; set; }
		[JsonIgnore]
		public string Token { get; set; }
		[JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
		public string PublicKey { get; set; }
		[JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
		public uint? CompanyId { get; set; }
		[JsonIgnore]
		public bool IsConfirmed { get; set; }
	}
}
