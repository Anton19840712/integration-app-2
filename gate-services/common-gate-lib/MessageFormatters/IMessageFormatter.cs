﻿using System.Text.Json;

namespace CommonGateLib.MessageFormatters
{
	public interface IMessageFormatter
	{
		string DecodeUnicodeEscape(string input);
		string FormatJson(string json);
		void WriteFormattedJson(JsonElement element, Utf8JsonWriter writer);
	}
}
