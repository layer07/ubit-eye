using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;
using MessagePack;

namespace MinerPulse.Models
{
	/// <summary>
	/// A simple JsonConverter that converts a JSON value to a string, handling both string and numeric types.
	/// </summary>
	public class StringOrNumberJsonConverter : JsonConverter<string>
	{
		public override string Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
		{
			switch (reader.TokenType)
			{
				case JsonTokenType.String:
					return reader.GetString();
				case JsonTokenType.Number:
					if (reader.TryGetInt64(out long longValue))
						return longValue.ToString();
					if (reader.TryGetDouble(out double doubleValue))
						return doubleValue.ToString();
					return reader.GetDecimal().ToString();
				case JsonTokenType.True:
				case JsonTokenType.False:
					return reader.GetBoolean().ToString();
				case JsonTokenType.Null:
					return null;
				default:
					throw new JsonException($"Unexpected token parsing string. Token: {reader.TokenType}");
			}
		}

		public override void Write(Utf8JsonWriter writer, string value, JsonSerializerOptions options)
		{
			writer.WriteStringValue(value);
		}
	}

	/// <summary>
	/// Represents a status message in the response
	/// </summary>
	[MessagePackObject]
	public class MinerStatus
	{
		[Key("STATUS")]
		[JsonPropertyName("STATUS")]
		public string Status { get; set; }

		[Key("Msg")]
		[JsonPropertyName("Msg")]
		public string Message { get; set; }
	}

	/// <summary>
	/// Represents the summary response from a miner
	/// </summary>
	[MessagePackObject]
	public class SummaryResponse
	{
		[Key("STATUS")]
		[JsonPropertyName("STATUS")]
		public List<MinerStatus> Statuses { get; set; }

		[Key("SUMMARY")]
		[JsonPropertyName("SUMMARY")]
		public List<SummaryData> Summaries { get; set; }

		[Key("id")]
		[JsonPropertyName("id")]
		public int Id { get; set; }
	}

	/// <summary>
	/// Represents detailed summary data from a miner
	/// </summary>
	[MessagePackObject]
	public class SummaryData
	{
		[Key("Elapsed")]
		[JsonPropertyName("Elapsed")]
		public int Elapsed { get; set; }

		[Key("MHS av")]
		[JsonPropertyName("MHS av")]
		public double MhsAverage { get; set; }

		[Key("MHS 5s")]
		[JsonPropertyName("MHS 5s")]
		public double Mhs5Seconds { get; set; }

		[Key("MHS 1m")]
		[JsonPropertyName("MHS 1m")]
		public double Mhs1Minute { get; set; }

		[Key("MHS 5m")]
		[JsonPropertyName("MHS 5m")]
		public double Mhs5Minutes { get; set; }

		[Key("MHS 15m")]
		[JsonPropertyName("MHS 15m")]
		public double Mhs15Minutes { get; set; }

		[Key("HS RT")]
		[JsonPropertyName("HS RT")]
		public double HsRt { get; set; }

		[Key("Accepted")]
		[JsonPropertyName("Accepted")]
		public int Accepted { get; set; }

		[Key("Rejected")]
		[JsonPropertyName("Rejected")]
		public int Rejected { get; set; }

		[Key("Total MH")]
		[JsonPropertyName("Total MH")]
		public double TotalMh { get; set; }

		[Key("Temperature")]
		[JsonPropertyName("Temperature")]
		public double Temperature { get; set; }

		[Key("freq_avg")]
		[JsonPropertyName("freq_avg")]
		public int FreqAverage { get; set; }

		[Key("Fan Speed In")]
		[JsonPropertyName("Fan Speed In")]
		public int FanSpeedIn { get; set; }

		[Key("Fan Speed Out")]
		[JsonPropertyName("Fan Speed Out")]
		public int FanSpeedOut { get; set; }

		[Key("Power")]
		[JsonPropertyName("Power")]
		public int Power { get; set; }

		[Key("Power Rate")]
		[JsonPropertyName("Power Rate")]
		public double PowerRate { get; set; }

		[Key("Pool Rejected%")]
		[JsonPropertyName("Pool Rejected%")]
		public double PoolRejectedPercentage { get; set; }

		[Key("Pool Stale%")]
		[JsonPropertyName("Pool Stale%")]
		public double PoolStalePercentage { get; set; }

		[Key("Uptime")]
		[JsonPropertyName("Uptime")]
		public int Uptime { get; set; }

		[Key("Target Freq")]
		[JsonPropertyName("Target Freq")]
		public int TargetFreq { get; set; }

		[Key("Target MHS")]
		[JsonPropertyName("Target MHS")]
		public double TargetMhs { get; set; }

		[Key("Env Temp")]
		[JsonPropertyName("Env Temp")]
		public double EnvTemp { get; set; }

		[Key("Power Mode")]
		[JsonPropertyName("Power Mode")]
		public string PowerMode { get; set; }

		[Key("Factory GHS")]
		[JsonPropertyName("Factory GHS")]
		public int FactoryGhs { get; set; }

		[Key("Power Limit")]
		[JsonPropertyName("Power Limit")]
		public int PowerLimit { get; set; }

		[Key("Chip Temp Min")]
		[JsonPropertyName("Chip Temp Min")]
		public double ChipTempMin { get; set; }

		[Key("Chip Temp Max")]
		[JsonPropertyName("Chip Temp Max")]
		public double ChipTempMax { get; set; }

		[Key("Chip Temp Avg")]
		[JsonPropertyName("Chip Temp Avg")]
		public double ChipTempAvg { get; set; }

		[Key("Btminer Fast Boot")]
		[JsonPropertyName("Btminer Fast Boot")]
		public string BtminerFastBoot { get; set; }

		[Key("Upfreq Complete")]
		[JsonPropertyName("Upfreq Complete")]
		public int UpfreqComplete { get; set; }
	}

	/// <summary>
	/// Represents the devices response from a miner
	/// </summary>
	[MessagePackObject]
	public class DevicesResponse
	{
		[Key("STATUS")]
		[JsonPropertyName("STATUS")]
		public List<MinerStatus> Statuses { get; set; }

		[Key("DEVS")]
		[JsonPropertyName("DEVS")]
		public List<DeviceData> Devices { get; set; }

		[Key("id")]
		[JsonPropertyName("id")]
		public int Id { get; set; }
	}

	/// <summary>
	/// Represents detailed device data from a miner
	/// </summary>
	[MessagePackObject]
	public class DeviceData
	{
		[Key("Slot")]
		[JsonPropertyName("Slot")]
		public int Slot { get; set; }

		[Key("Enabled")]
		[JsonPropertyName("Enabled")]
		public string Enabled { get; set; }

		[Key("Status")]
		[JsonPropertyName("Status")]
		public string Status { get; set; }

		// Continue with other properties matching the JSON structure...
		[Key("Temperature")]
		[JsonPropertyName("Temperature")]
		public double Temperature { get; set; }

		[Key("Chip Frequency")]
		[JsonPropertyName("Chip Frequency")]
		public int ChipFrequency { get; set; }

		[Key("MHS av")]
		[JsonPropertyName("MHS av")]
		public double MhsAverage { get; set; }

		[Key("MHS 5s")]
		[JsonPropertyName("MHS 5s")]
		public double Mhs5Seconds { get; set; }

		[Key("MHS 1m")]
		[JsonPropertyName("MHS 1m")]
		public double Mhs1Minute { get; set; }

		[Key("MHS 5m")]
		[JsonPropertyName("MHS 5m")]
		public double Mhs5Minutes { get; set; }

		[Key("MHS 15m")]
		[JsonPropertyName("MHS 15m")]
		public double Mhs15Minutes { get; set; }

		[Key("HS RT")]
		[JsonPropertyName("HS RT")]
		public double HsRt { get; set; }

		[Key("Factory GHS")]
		[JsonPropertyName("Factory GHS")]
		public int FactoryGhs { get; set; }

		[Key("Upfreq Complete")]
		[JsonPropertyName("Upfreq Complete")]
		public int UpfreqComplete { get; set; }

		[Key("Effective Chips")]
		[JsonPropertyName("Effective Chips")]
		public int EffectiveChips { get; set; }

		[Key("PCB SN")]
		[JsonPropertyName("PCB SN")]
		public string PcbSn { get; set; }

		[Key("Chip Data")]
		[JsonPropertyName("Chip Data")]
		public string ChipData { get; set; }

		[Key("Chip Temp Min")]
		[JsonPropertyName("Chip Temp Min")]
		public double ChipTempMin { get; set; }

		[Key("Chip Temp Max")]
		[JsonPropertyName("Chip Temp Max")]
		public double ChipTempMax { get; set; }

		[Key("Chip Temp Avg")]
		[JsonPropertyName("Chip Temp Avg")]
		public double ChipTempAvg { get; set; }

		[Key("chip_vol_diff")]
		[JsonPropertyName("chip_vol_diff")]
		public int ChipVolDiff { get; set; }
	}

	/// <summary>
	/// Represents the PSU response from a miner
	/// </summary>
	[MessagePackObject]
	public class PsuResponse
	{
		[Key("STATUS")]
		[JsonPropertyName("STATUS")]
		public string Status { get; set; }

		[Key("When")]
		[JsonPropertyName("When")]
		public long When { get; set; }

		[Key("Code")]
		[JsonPropertyName("Code")]
		[JsonConverter(typeof(StringOrNumberJsonConverter))]
		public string Code { get; set; }

		[Key("Msg")]
		[JsonPropertyName("Msg")]
		public PsuMsg Msg { get; set; }

		[Key("Description")]
		[JsonPropertyName("Description")]
		public string Description { get; set; }
	}

	/// <summary>
	/// Represents the PSU message details
	/// </summary>
	[MessagePackObject]
	public class PsuMsg
	{
		[Key("name")]
		[JsonPropertyName("name")]
		public string Name { get; set; }

		[Key("hw_version")]
		[JsonPropertyName("hw_version")]
		public string HwVersion { get; set; }

		[Key("sw_version")]
		[JsonPropertyName("sw_version")]
		public string SwVersion { get; set; }

		[Key("model")]
		[JsonPropertyName("model")]
		public string Model { get; set; }

		[Key("enable")]
		[JsonPropertyName("enable")]
		public string Enable { get; set; }

		[Key("iin")]
		[JsonPropertyName("iin")]
		public string Iin { get; set; }

		[Key("vin")]
		[JsonPropertyName("vin")]
		public string Vin { get; set; }

		[Key("pin")]
		[JsonPropertyName("pin")]
		public string Pin { get; set; }

		[Key("fan_speed")]
		[JsonPropertyName("fan_speed")]
		public string FanSpeed { get; set; }

		[Key("serial_no")]
		[JsonPropertyName("serial_no")]
		public string SerialNo { get; set; }

		[Key("vendor")]
		[JsonPropertyName("vendor")]
		public string Vendor { get; set; }

		[Key("temp0")]
		[JsonPropertyName("temp0")]
		public string Temp0 { get; set; }
	}

	/// <summary>
	/// Represents the error code response from a miner
	/// </summary>
	[MessagePackObject]
	public class ErrorCodeResponse
	{
		[Key("STATUS")]
		[JsonPropertyName("STATUS")]
		public string Status { get; set; }

		[Key("When")]
		[JsonPropertyName("When")]
		public long When { get; set; }

		[Key("Code")]
		[JsonPropertyName("Code")]
		[JsonConverter(typeof(StringOrNumberJsonConverter))]
		public string Code { get; set; }

		[Key("Msg")]
		[JsonPropertyName("Msg")]
		public ErrorCodeMsg Msg { get; set; }

		[Key("Description")]
		[JsonPropertyName("Description")]
		public string Description { get; set; }
	}

	/// <summary>
	/// Represents the error code message details
	/// </summary>
	[MessagePackObject]
	public class ErrorCodeMsg
	{
		[Key("error_code")]
		[JsonPropertyName("error_code")]
		public List<Dictionary<string, string>> ErrorCodes { get; set; }
	}

	/// <summary>
	/// Represents a complete miner object containing all response types
	/// </summary>
	[MessagePackObject]
	public class MegaObject
	{
		[Key("summary")]
		[JsonPropertyName("summary")]
		public SummaryResponse Summary { get; set; }

		[Key("edevs")]
		[JsonPropertyName("edevs")]
		public DevicesResponse Devices { get; set; }

		[Key("get_psu")]
		[JsonPropertyName("get_psu")]
		public PsuResponse PSU { get; set; }

		[Key("get_error_code")]
		[JsonPropertyName("get_error_code")]
		public ErrorCodeResponse ErrorCode { get; set; }

		[IgnoreMember]
		[JsonIgnore]
		public string ErrorMeaning { get; set; }

		[IgnoreMember]
		[JsonIgnore]
		public string ErrorSolution { get; set; }		
	}
}
