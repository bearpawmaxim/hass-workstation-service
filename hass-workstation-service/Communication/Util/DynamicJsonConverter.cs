﻿using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Text.Json;
using System.Text.Json.Serialization;

#pragma warning disable CS8632 // The annotation for nullable reference types should only be used in code within a '#nullable' annotations context.
namespace hass_workstation_service.Communication.Util
{
    /// <summary>
    ///     Temp Dynamic Converter
    ///     by:tchivs@live.cn
    /// </summary>
    public class DynamicJsonConverter : JsonConverter<dynamic>
	{
		public override dynamic Read(ref Utf8JsonReader reader,
			Type typeToConvert,
			JsonSerializerOptions options) {
			if (reader.TokenType == JsonTokenType.True) return true;

			if (reader.TokenType == JsonTokenType.False) return false;

			if (reader.TokenType == JsonTokenType.Number) {
				if (reader.TryGetInt64(out var l)) return l;

				return reader.GetDouble();
			}

			if (reader.TokenType == JsonTokenType.String) {
				if (reader.TryGetDateTime(out var datetime)) return datetime;

				return reader.GetString();
			}

			if (reader.TokenType == JsonTokenType.StartObject) {
				using var documentV = JsonDocument.ParseValue(ref reader);
				return ReadObject(documentV.RootElement);
			}

			// Use JsonElement as fallback.
			// Newtonsoft uses JArray or JObject.
			var document = JsonDocument.ParseValue(ref reader);
			return document.RootElement.Clone();
		}

		private object ReadObject(JsonElement jsonElement) {
			IDictionary<string, object> expandoObject = new ExpandoObject();
			foreach (var obj in jsonElement.EnumerateObject()) {
				var k = obj.Name;
				var value = ReadValue(obj.Value);
				expandoObject[k] = value;
			}

			return expandoObject;
		}

		private object? ReadValue(JsonElement jsonElement) {
			object? result = null;
			switch (jsonElement.ValueKind) {
				case JsonValueKind.Object:
					result = ReadObject(jsonElement);
					break;
				case JsonValueKind.Array:
					result = ReadList(jsonElement);
					break;
				case JsonValueKind.String:
					//TODO: Missing Datetime&Bytes Convert
					result = jsonElement.GetString();
					break;
				case JsonValueKind.Number:
					//TODO: more num type
					result = 0;
					if (jsonElement.TryGetInt64(out var l)) result = l;
					break;
				case JsonValueKind.True:
					result = true;
					break;
				case JsonValueKind.False:
					result = false;
					break;
				case JsonValueKind.Undefined:
				case JsonValueKind.Null:
					result = null;
					break;
				default:
					throw new ArgumentOutOfRangeException();
			}

			return result;
		}

		private object? ReadList(JsonElement jsonElement) {
			IList<object?> list = new List<object?>();
			foreach (var item in jsonElement.EnumerateArray()) list.Add(ReadValue(item));
			return list.Count == 0 ? null : list;
		}

		public override void Write(Utf8JsonWriter writer,
			object value,
			JsonSerializerOptions options) {
			// writer.WriteStringValue(value.ToString());
		}
	}
}
#pragma warning restore CS8632 // The annotation for nullable reference types should only be used in code within a '#nullable' annotations context.