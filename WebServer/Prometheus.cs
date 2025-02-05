// Prometheus.cs
using MinerPulse.Models;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;

namespace MinerPulse
{
	/// <summary>
	/// Defines and manages Prometheus metrics for the MinerStack application.
	/// </summary>
	public static class PrometheusMetrics
	{
		/// <summary>
		/// Sanitizes and formats numeric values for Prometheus metrics.		
		/// </summary>
		/// <param name="value">The value to sanitize.</param>
		/// <returns>A sanitized double value.</returns>
		private static double SanitizeNumericValue(object value)
		{
			if (value == null)
				return 0.0;

			string stringValue = value.ToString()
				?.Replace("\r", "")
				?.Replace("\n", "")
				?.Trim() ?? "0";

			if (double.TryParse(
				stringValue,
				NumberStyles.Float,
				CultureInfo.InvariantCulture,
				out double result))
			{
				return Math.Round(result, 2);
			}

			return 0.0;
		}

		/// <summary>
		/// Sanitizes metric types by removing any carriage return or newline characters.
		/// </summary>
		/// <param name="metricType">The metric type to sanitize.</param>
		/// <returns>A sanitized metric type string.</returns>
		private static string SanitizeMetricType(string metricType)
		{
			return metricType.Replace("\r", "").Replace("\n", "").Trim();
		}

		/// <summary>
		/// Generates the Prometheus-formatted metrics string based on the latest data from Globals.MinerData.
		/// </summary>
		/// <returns>A string containing all metrics in Prometheus exposition format.</returns>
		public static string GetMetrics()
		{
			var sb = new StringBuilder();

			// Comprehensive metric headers
			var metrics = new Dictionary<string, string>
	{
		{ "miner_mhs_average", "gauge" },
		{ "miner_temperature_celsius", "gauge" },
		{ "miner_device_temperature_celsius", "gauge" },
		{ "miner_psu_temperature_celsius", "gauge" },
		{ "miner_errors_total", "counter" },
		{ "miner_fan_speed_in_rpm", "gauge" },
		{ "miner_fan_speed_out_rpm", "gauge" },
		{ "miner_power_consumption_watts", "gauge" },
		{ "miner_power_rate_percent", "gauge" },
		{ "miner_pool_rejected_percentage", "gauge" },
		{ "miner_pool_stale_percentage", "gauge" },
		{ "miner_uptime_seconds", "counter" },
		{ "miner_target_frequency_hz", "gauge" },
		{ "miner_target_mhs", "gauge" },
		{ "miner_environment_temperature_celsius", "gauge" },
		{ "miner_power_mode", "gauge" },
		{ "miner_factory_ghs", "gauge" },
		{ "miner_power_limit_watts", "gauge" },
		{ "miner_chip_temperature_min_celsius", "gauge" },
		{ "miner_chip_temperature_max_celsius", "gauge" },
		{ "miner_chip_temperature_avg_celsius", "gauge" },
		{ "miner_btminer_fast_boot_status", "gauge" },
		{ "miner_upfreq_complete_count", "counter" },
		{ "miner_effective_chips_count", "gauge" },
		{ "miner_chip_frequency_hz", "gauge" },
		{ "miner_chip_voltage_difference_voltage", "gauge" },
		{ "miner_hs_rt", "gauge" },
		{ "miner_accepted_total", "counter" },
		{ "miner_rejected_total", "counter" }
	};

			// Append metric headers
			foreach (var metric in metrics)
			{
				sb.Append($"# HELP {metric.Key} {metric.Key.Replace("_", " ")}\n");
				sb.Append($"# TYPE {metric.Key} {SanitizeMetricType(metric.Value)}\n");
			}

			foreach (var minerEntry in Globals.MinerData)
			{
				int minerId = minerEntry.Key;
				MegaObject megaObject = minerEntry.Value;

				var summary = megaObject.Summary?.Summaries?.FirstOrDefault();
				if (summary == null) continue;

				// Retrieve VIN (Voltage In) from PSU if available
				double voltageIn = 0;
				if (double.TryParse(megaObject.PSU?.Msg?.Vin, NumberStyles.Float, CultureInfo.InvariantCulture, out double parsedVoltageIn))
				{
					voltageIn = parsedVoltageIn;
				}

				// Miner MHS Average
				sb.Append($"miner_mhs_average{{miner_id=\"{minerId}\"}} {SanitizeNumericValue(summary.MhsAverage).ToString("F2", CultureInfo.InvariantCulture)}\n");

				// Temperature
				sb.Append($"miner_temperature_celsius{{miner_id=\"{minerId}\"}} {SanitizeNumericValue(summary.Temperature).ToString("F2", CultureInfo.InvariantCulture)}\n");

				// Device Temperatures and Chip Frequencies
				if (megaObject.Devices?.Devices != null)
				{
					foreach (var device in megaObject.Devices.Devices)
					{
						string deviceSlot = device.Slot.ToString();

						sb.Append($"miner_device_temperature_celsius{{miner_id=\"{minerId}\",device_slot=\"{deviceSlot}\"}} {SanitizeNumericValue(device.Temperature).ToString("F2", CultureInfo.InvariantCulture)}\n");
						sb.Append($"miner_chip_frequency_hz{{miner_id=\"{minerId}\",device_slot=\"{deviceSlot}\"}} {SanitizeNumericValue(device.ChipFrequency).ToString("F2", CultureInfo.InvariantCulture)}\n");
						sb.Append($"miner_chip_voltage_difference_voltage{{miner_id=\"{minerId}\",device_slot=\"{deviceSlot}\"}} {SanitizeNumericValue(device.ChipVolDiff).ToString("F2", CultureInfo.InvariantCulture)}\n");
						sb.Append($"miner_effective_chips_count{{miner_id=\"{minerId}\",device_slot=\"{deviceSlot}\"}} {SanitizeNumericValue(device.EffectiveChips)}\n");
					}
				}

				// PSU Temperature
				sb.Append($"miner_psu_temperature_celsius{{miner_id=\"{minerId}\"}} {SanitizeNumericValue(megaObject.PSU?.Msg?.Temp0).ToString("F2", CultureInfo.InvariantCulture)}\n");

				// Error Status
				sb.Append($"miner_errors_total{{miner_id=\"{minerId}\"}} {(megaObject.ErrorCode?.Status == "OK" ? 0 : 1)}\n");

				// Fan Speeds
				sb.Append($"miner_fan_speed_in_rpm{{miner_id=\"{minerId}\"}} {SanitizeNumericValue(summary.FanSpeedIn)}\n");
				sb.Append($"miner_fan_speed_out_rpm{{miner_id=\"{minerId}\"}} {SanitizeNumericValue(summary.FanSpeedOut)}\n");

				// Power Metrics
				sb.Append($"miner_power_consumption_watts{{miner_id=\"{minerId}\"}} {SanitizeNumericValue(summary.Power)}\n");
				sb.Append($"miner_power_rate_percent{{miner_id=\"{minerId}\"}} {SanitizeNumericValue(summary.PowerRate).ToString("F2", CultureInfo.InvariantCulture)}\n");
				sb.Append($"miner_power_limit_watts{{miner_id=\"{minerId}\"}} {SanitizeNumericValue(summary.PowerLimit)}\n");

				// Pool Metrics
				sb.Append($"miner_pool_rejected_percentage{{miner_id=\"{minerId}\"}} {SanitizeNumericValue(summary.PoolRejectedPercentage).ToString("F2", CultureInfo.InvariantCulture)}\n");
				sb.Append($"miner_pool_stale_percentage{{miner_id=\"{minerId}\"}} {SanitizeNumericValue(summary.PoolStalePercentage).ToString("F2", CultureInfo.InvariantCulture)}\n");

				// Uptime and Frequency
				sb.Append($"miner_uptime_seconds{{miner_id=\"{minerId}\"}} {SanitizeNumericValue(summary.Uptime)}\n");
				sb.Append($"miner_target_frequency_hz{{miner_id=\"{minerId}\"}} {SanitizeNumericValue(summary.TargetFreq)}\n");
				sb.Append($"miner_target_mhs{{miner_id=\"{minerId}\"}} {SanitizeNumericValue(summary.TargetMhs).ToString("F2", CultureInfo.InvariantCulture)}\n");

				// Environmental Metrics
				sb.Append($"miner_environment_temperature_celsius{{miner_id=\"{minerId}\"}} {SanitizeNumericValue(summary.EnvTemp).ToString("F2", CultureInfo.InvariantCulture)}\n");

				// Power Mode (binary)
				int powerMode = summary.PowerMode?.Equals("Enabled", StringComparison.OrdinalIgnoreCase) == true ? 1 : 0;
				sb.Append($"miner_power_mode{{miner_id=\"{minerId}\"}} {powerMode}\n");

				// Other Metrics
				sb.Append($"miner_factory_ghs{{miner_id=\"{minerId}\"}} {SanitizeNumericValue(summary.FactoryGhs)}\n");
				sb.Append($"miner_chip_temperature_min_celsius{{miner_id=\"{minerId}\"}} {SanitizeNumericValue(summary.ChipTempMin).ToString("F2", CultureInfo.InvariantCulture)}\n");
				sb.Append($"miner_chip_temperature_max_celsius{{miner_id=\"{minerId}\"}} {SanitizeNumericValue(summary.ChipTempMax).ToString("F2", CultureInfo.InvariantCulture)}\n");
				sb.Append($"miner_chip_temperature_avg_celsius{{miner_id=\"{minerId}\"}} {SanitizeNumericValue(summary.ChipTempAvg).ToString("F2", CultureInfo.InvariantCulture)}\n");

				// Btminer Fast Boot Status (binary)
				int btminerFastBoot = summary.BtminerFastBoot?.Equals("Enabled", StringComparison.OrdinalIgnoreCase) == true ? 1 : 0;
				sb.Append($"miner_btminer_fast_boot_status{{miner_id=\"{minerId}\"}} {btminerFastBoot}\n");

				// Upfreq Complete Count
				sb.Append($"miner_upfreq_complete_count{{miner_id=\"{minerId}\"}} {SanitizeNumericValue(summary.UpfreqComplete)}\n");

				// Hash Rate and Shares
				sb.Append($"miner_hs_rt{{miner_id=\"{minerId}\"}} {SanitizeNumericValue(summary.HsRt).ToString("F2", CultureInfo.InvariantCulture)}\n");
				sb.Append($"miner_accepted_total{{miner_id=\"{minerId}\"}} {SanitizeNumericValue(summary.Accepted)}\n");
				sb.Append($"miner_rejected_total{{miner_id=\"{minerId}\"}} {SanitizeNumericValue(summary.Rejected)}\n");

				// Miner VIN
				if (voltageIn != 0)
				{
					sb.Append($"miner_vin{{miner_id=\"{minerId}\"}} {SanitizeNumericValue(voltageIn)}\n");
				}
			}

			// No carriage return
			string metricsOutput = sb.ToString().Replace("\r", "");

			return metricsOutput;
		}
	}
}
