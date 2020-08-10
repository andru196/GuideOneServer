using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
//using System.Text.Json.Serialization;
//using System.Text.Json;
using Newtonsoft.Json;
using NetTopologySuite.Geometries;
using NetTopologySuite;

namespace GuideOneServer.Helpers
{
	public class PointJsonConverter : JsonConverter<Point>
	{
		public override Point ReadJson(JsonReader reader, Type objectType, Point existingValue, bool hasExistingValue, JsonSerializer serializer)
		{
		
			double lat = 0, lon = 0;
			try
			{
				var readed = reader.Read();
				for (var i = 0; i < 6 && readed && (lat == 0 || lon == 0) && reader.TokenType != JsonToken.EndObject; i++)
					if (reader.TokenType == JsonToken.PropertyName)
						switch (reader.Value)
						{
							case "Latitude":
								lat = reader.ReadAsDouble().GetValueOrDefault();
								break;
							case "Longitude":
								lon = reader.ReadAsDouble().GetValueOrDefault();
								break;
						}
					else
						readed = reader.Read();

			}
			catch (Exception ex)
			{

			}
			if (lat == 0 || lon == 0)
				return null;
			return NtsGeometryServices.Instance.CreateGeometryFactory(srid: 4326).CreatePoint(new Coordinate(
				lat,
				lon
				));
		}


		public override void WriteJson(JsonWriter writer, Point point, JsonSerializer serializer)
		{
			writer.WriteStartObject();
			writer.WritePropertyName("Latitude");
			writer.WriteValue(point.Coordinate.X);
			writer.WritePropertyName("Longitude");
			writer.WriteValue(point.Coordinate.Y); ;
			writer.WriteEndObject();
		}
	}
}
