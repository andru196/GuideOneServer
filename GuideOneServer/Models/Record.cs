using GuideOneServer.Helpers;
using NetTopologySuite.Geometries;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Spatial;
using System.Threading.Tasks;

namespace GuideOneServer.Models
{
	public class Record
	{
		public uint Id { get; set; }
		public string Name { get; set; }
		[JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
		public string Path { get; set; }
		[JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
		public string Audio { get; set; }
		public string Description { get; set; }
		public int Duaration { get; set; }
		public DateTime DateTime { get; set; }
		[JsonIgnore]
		public User User { get; set; }
		public bool IsPublic { get; set; }
		public bool IsPaid { get; set; }
		public bool IsAnon { get; set; }
		[JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
		public DateTime? ValidatyTime { get; set; }
		[JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
		public uint? Area { get; set; }
		[JsonConverter(typeof(PointJsonConverter))]
		public Point Point { get; set; }
		[JsonProperty(NullValueHandling = NullValueHandling.Ignore, PropertyName = "Photo")]
		public string PhotoPath { get; set; }
		[JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
		public string UserWhoCanSee { get; set; }
		public string Language { get; set; }
		public double? Raiting { get; set; }
	}
}
