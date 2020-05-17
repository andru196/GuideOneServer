using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Text.Json.Serialization;
using System.Text.Json;
using NetTopologySuite.Geometries;
using NetTopologySuite;

namespace GuideOneServer.Helpers
{
	public class PointJsonConverter : JsonConverter<Point>
	{
		public override Point Read(ref Utf8JsonReader reader,
			Type typeToConvert,
			JsonSerializerOptions options)
		{
			double lat = 0, lon = 0;
			int i = 0;
			while (reader.Read() && i < 2)
			{
				i++;
				var name = reader.GetString().ToLower();
				reader.Read();
				switch (name)
				{
					case "latitude":
						lat = reader.GetDouble();
						break;
					case "longitude":
						lon = reader.GetDouble();
						break;
				}
			}
			if (lat == 0 || lon == 0)
				return null;
			return NtsGeometryServices.Instance.CreateGeometryFactory(srid: 4326).CreatePoint(new Coordinate(
				lat,
				lon
				));
		}

		public override void Write(
			Utf8JsonWriter writer, Point point,
			JsonSerializerOptions options)
		{
			writer.WriteStartObject();
			writer.WriteNumber("Latitude", point.Coordinate.X);
			writer.WriteNumber("Longitude", point.Coordinate.Y); ;
			writer.WriteEndObject();
		}
	}
}
