// SPDX-License-Identifier: GPL-2.0-or-later
/*
 * ConfigParser.cs
 * 
 * Version: @(#)ConfigParser.cs 0.0.03 15/01/2025
 *
 * Description: Quick hack for reading a config file.
 *
 * Author: D. Leatti (Forbannet)
 * URL: https://kernelriot.com
 * Github: /layer07
 *
 *        ██▓    ▄▄▄     ▓██   ██▓▓█████  ██▀███  
 *       ▓██▒   ▒████▄    ▒██  ██▒▓█   ▀ ▓██ ▒ ██▒
 *       ▒██░   ▒██  ▀█▄   ▒██ ██░▒███   ▓██ ░▄█ ▒
 *       ▒██░   ░██▄▄▄▄██  ░ ▐██▓░▒▓█ ▄ ▒██▀▀█▄  
 *       ░██████▒▓█   ▓██▒ ░ ██▒▓░░▒████▒░██▓ ▒██▒
 *       ░ ▒░▓  ░▒▒   ▓▒█░  ██▒▒▒ ░░ ▒░ ░░ ▒▓ ░▒▓░
 *       ░ ░ ▒  ░ ▒   ▒▒ ░▓██ ░▒░  ░ ░  ░  ░▒ ░ ▒░
 *         ░ ░    ░   ▒   ▒ ▒ ░░     ░     ░░   ░ 
 *           ░  ░     ░  ░░ ░        ░  ░   ░     
 */

using System;
using System.Collections.Generic;
using System.Dynamic;
using System.IO;
using System.Net;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using System.Linq;
using System.Drawing; 


namespace MinerPulse
{
	/// <summary>
	/// Parses and manages application configuration from a fixed or specified file location.
	/// Supports dynamic access to configuration values with a 1337 twist.
	/// </summary>
	public static class ConfigParser
	{
		// Internal storage for parsed configuration data
		private static readonly Dictionary<string, Dictionary<string, object>> config_data = new(StringComparer.OrdinalIgnoreCase);
		private static bool debug_mode = false;

		// Dynamic property to access configuration values
		public static dynamic Config { get; private set; }

		/// <summary>
		/// Loads and parses the configuration file from the current directory. Enables debug mode if specified.
		/// </summary>
		/// <param name="enable_debug">Flag to enable debug mode.</param>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void load_config(bool enable_debug = false) =>
			load_config(Path.Combine(get_application_base_path(), "app.conf"), enable_debug);

		/// <summary>
		/// Loads and parses the configuration file from the specified path. Enables debug mode if specified.
		/// </summary>
		/// <param name="config_path">Path to the configuration file.</param>
		/// <param name="enable_debug">Flag to enable debug mode.</param>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void load_config(string config_path, bool enable_debug = false)
		{
			if (!File.Exists(config_path))
			{
				log_error($"Configuration file not found at {config_path}. Using default settings.");
				return;
			}

			log_info($"Loading configuration file from {config_path}.");
			string config_content = File.ReadAllText(config_path).Replace("${currfolder}/", get_application_base_path());
			log_debug("Configuration content loaded successfully.");

			// Regex patterns
			var section_regex = new Regex(@"\[(?<section>[^\]]+)\]\s*(?<content>.*?)(?=\n\[[^\]]+\]|\z)", RegexOptions.Singleline);
			var key_value_regex = new Regex(@"(?<key>\w+)\s*=\s*""?(?<value>[^""\n\r]*)""?", RegexOptions.Multiline);

			foreach (Match section_match in section_regex.Matches(config_content))
			{
				string section_name = section_match.Groups["section"].Value.Trim();
				string content = section_match.Groups["content"].Value.Trim();

				log_debug($"Processing section: {section_name}");

				var key_values = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);

				foreach (Match pair in key_value_regex.Matches(content))
				{
					string key = pair.Groups["key"].Value.Trim();
					string value_str = pair.Groups["value"].Value.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
					object parsed_value = parse_value_dynamically(value_str);
					key_values[key] = parsed_value;
					log_debug($"Found key-value pair: {key} = {parsed_value} (Type: {parsed_value.GetType()})");
				}

				config_data[section_name] = key_values;
				log_info($"Section '{section_name}' parsed with {key_values.Count} key-value pair(s).");
			}
						
			Config = ConvertToExpando(config_data);

			if (enable_debug)
			{
				log_info("Debug mode enabled via parameter.");
				log_info("Debug mode is active. Echoing all configuration values:");
				echo_config_values();
			}

			log_info("Configuration file parsed successfully.");
		}

		/// <summary>
		/// Converts a nested dictionary to an ExpandoObject for dynamic access.
		/// </summary>
		/// <param name="dict">The dictionary to convert.</param>
		/// <returns>An ExpandoObject representing the dictionary.</returns>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private static ExpandoObject ConvertToExpando(Dictionary<string, Dictionary<string, object>> dict)
		{
			var expando = new ExpandoObject() as IDictionary<string, object>;

			foreach (var section in dict)
			{
				var sectionExpando = new ExpandoObject() as IDictionary<string, object>;

				foreach (var kvp in section.Value)
				{
					sectionExpando[kvp.Key] = kvp.Value;
				}

				expando[section.Key] = sectionExpando;
			}

			return (ExpandoObject)expando;
		}

		/// <summary>
		/// Parses a string value into its appropriate type.
		/// </summary>
		/// <param name="value_str">The string value to parse.</param>
		/// <returns>Parsed object.</returns>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private static object parse_value_dynamically(string value_str) =>
			bool.TryParse(value_str, out bool b) ? (object)b :
			Guid.TryParse(value_str, out Guid g) ? (object)g :
			int.TryParse(value_str, out int i) ? (object)i :
			(object)value_str;

		/// <summary>
		/// Retrieves a string value from the configuration.
		/// </summary>
		/// <param name="section">Section name.</param>
		/// <param name="key">Key within the section.</param>
		/// <param name="default_value">Default value if key not found.</param>
		/// <returns>String value.</returns>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static string parse_as_string(string section, string key, string default_value = null) =>
			config_data.ContainsKey(section) && config_data[section].ContainsKey(key)
				? config_data[section][key]?.ToString()
				: default_value;

		/// <summary>
		/// Retrieves and normalizes a path from the configuration.
		/// </summary>
		/// <param name="section">Section name.</param>
		/// <param name="key">Key within the section.</param>
		/// <param name="default_path">Default path if key not found.</param>
		/// <returns>Normalized path.</returns>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static string parse_as_path(string section, string key, string default_path = null) =>
			!string.IsNullOrWhiteSpace(parse_as_string(section, key, default_path))
				? TryGetFullPath(parse_as_string(section, key), default_path)
				: default_path;

		/// <summary>
		/// Retrieves an integer value from the configuration.
		/// </summary>
		/// <param name="section">Section name.</param>
		/// <param name="key">Key within the section.</param>
		/// <param name="default_value">Default integer if parsing fails.</param>
		/// <returns>Integer value.</returns>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static int parse_as_int(string section, string key, int default_value = 0) =>
			config_data.ContainsKey(section) && config_data[section].ContainsKey(key)
				? (config_data[section][key] is int val ? val :
				   (config_data[section][key] is string s && int.TryParse(s, out int parsed) ? parsed : default_value))
				: default_value;

		/// <summary>
		/// Retrieves a boolean value from the configuration.
		/// </summary>
		/// <param name="section">Section name.</param>
		/// <param name="key">Key within the section.</param>
		/// <param name="default_value">Default boolean if parsing fails.</param>
		/// <returns>Boolean value.</returns>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool parse_as_bool(string section, string key, bool default_value = false) =>
			config_data.ContainsKey(section) && config_data[section].ContainsKey(key)
				? (config_data[section][key] is bool val ? val :
				   (config_data[section][key] is string s && bool.TryParse(s, out bool parsed) ? parsed : default_value))
				: default_value;

		/// <summary>
		/// Retrieves a GUID value from the configuration.
		/// </summary>
		/// <param name="section">Section name.</param>
		/// <param name="key">Key within the section.</param>
		/// <param name="default_value">Default GUID if parsing fails.</param>
		/// <returns>GUID value.</returns>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static Guid parse_as_guid(string section, string key, Guid default_value = default) =>
			config_data.ContainsKey(section) && config_data[section].ContainsKey(key)
				? (config_data[section][key] is Guid val ? val :
				   (config_data[section][key] is string s && Guid.TryParse(s, out Guid parsed) ? parsed : default_value))
				: default_value;

		/// <summary>
		/// Retrieves a list of strings from the configuration, split by a delimiter.
		/// </summary>
		/// <param name="section">Section name.</param>
		/// <param name="key">Key within the section.</param>
		/// <param name="delimiter">Delimiter character.</param>
		/// <param name="default_list">Default list if key not found.</param>
		/// <returns>List of strings.</returns>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static List<string> parse_as_list(string section, string key, char delimiter = ',', List<string> default_list = null) =>
			parse_as_string(section, key)?.Split(delimiter, StringSplitOptions.RemoveEmptyEntries).Select(s => s.Trim()).ToList() ?? default_list ?? new();

		/// <summary>
		/// Retrieves a value from the configuration using a custom parser.
		/// </summary>
		/// <typeparam name="T">Type to parse the value into.</typeparam>
		/// <param name="section">Section name.</param>
		/// <param name="key">Key within the section.</param>
		/// <param name="parser">Function to parse the string value.</param>
		/// <param name="default_value">Default value if parsing fails.</param>
		/// <returns>Parsed value of type T.</returns>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static T parse_value<T>(string section, string key, Func<string, T> parser, T default_value = default) =>
			parse_as_string(section, key) is string value_str
				? TryParse(parser, value_str, default_value)
				: default_value;

		/// <summary>
		/// Displays all configuration entries with their types.
		/// </summary>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private static void echo_config_values()
		{
			Console.ForegroundColor = ConsoleColor.DarkGray;
			Console.WriteLine("=== CONFIGURATION VALUES ===");

			foreach (var section in config_data)
			{
				Console.ForegroundColor = ConsoleColor.Yellow;
				Console.Write("[");
				Console.ForegroundColor = ConsoleColor.Green;
				Console.Write(section.Key);
				Console.ForegroundColor = ConsoleColor.Yellow;
				Console.WriteLine("]");

				foreach (var kvp in section.Value)
				{
					Console.ForegroundColor = ConsoleColor.DarkGray;
					Console.Write("[");
					Console.ForegroundColor = ConsoleColor.Blue;
					Console.Write(GetTypeLabel(kvp.Value.GetType()));
					Console.ForegroundColor = ConsoleColor.DarkGray;
					Console.Write("] ");

					Console.ForegroundColor = ConsoleColor.White;
					Console.Write(kvp.Key);
					Console.ForegroundColor = ConsoleColor.DarkGray;
					Console.Write(" = ");
					Console.ForegroundColor = ConsoleColor.Gray;
					Console.WriteLine(kvp.Value);
				}
				Console.WriteLine();
			}

			Console.ForegroundColor = ConsoleColor.DarkGray;
			Console.WriteLine("=============================");
			Console.ResetColor();
		}

		/// <summary>
		/// Maps a Type to its corresponding label for display purposes.
		/// </summary>
		/// <param name="type">The Type to map.</param>
		/// <returns>String label representing the Type.</returns>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private static string GetTypeLabel(Type type) =>
			type switch
			{
				Type t when t == typeof(bool) => "BOOL",
				Type t when t == typeof(Guid) => "GUID",
				Type t when t == typeof(int) => "INT",
				Type t when t == typeof(string) => "STRING",
				Type t when t == typeof(double) => "DOUBLE",
				_ => type.Name.ToUpper()
			};

		/// <summary>
		/// Logs informational messages.
		/// </summary>
		/// <param name="message">Message to log.</param>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private static void log_info(string message)
		{
			Console.ForegroundColor = ConsoleColor.Cyan;
			Console.WriteLine($"[INFO] {message}");
			Console.ResetColor();
		}

		/// <summary>
		/// Logs error messages.
		/// </summary>
		/// <param name="message">Error message to log.</param>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private static void log_error(string message)
		{
			Console.ForegroundColor = ConsoleColor.Red;
			Console.WriteLine($"[ERROR] {message}");
			Console.ResetColor();
		}

		/// <summary>
		/// Logs debug messages if debug mode is active.
		/// </summary>
		/// <param name="message">Debug message to log.</param>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private static void log_debug(string message)
		{
			if (debug_mode)
			{
				Console.ForegroundColor = ConsoleColor.Magenta;
				Console.WriteLine($"[DEBUG] {message}");
				Console.ResetColor();
			}
		}

		/// <summary>
		/// Gets the application's base directory.
		/// </summary>
		/// <returns>Base directory path.</returns>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private static string get_application_base_path() => AppDomain.CurrentDomain.BaseDirectory;

		/// <summary>
		/// Retrieves a full path or returns the default path.
		/// </summary>
		/// <param name="path">Path string.</param>
		/// <param name="default_path">Default path if invalid.</param>
		/// <returns>Full path.</returns>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private static string TryGetFullPath(string path, string default_path) =>
			Path.GetFullPath(path, get_application_base_path()) ?? default_path;

		/// <summary>
		/// Tries to parse a value using a provided parser function.
		/// </summary>
		/// <typeparam name="T">Type to parse into.</typeparam>
		/// <param name="parser">Parser function.</param>
		/// <param name="value">Value string.</param>
		/// <param name="default_value">Default value if parsing fails.</param>
		/// <returns>Parsed value or default.</returns>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private static T TryParse<T>(Func<string, T> parser, string value, T default_value) =>
			parser != null ? (parser.Invoke(value) ?? default_value) : default_value;

		/// <summary>
		/// Retrieves all configuration data.
		/// </summary>
		/// <returns>Read-only dictionary of configurations.</returns>
		public static IReadOnlyDictionary<string, Dictionary<string, object>> get_all_config() =>
			new Dictionary<string, Dictionary<string, object>>(config_data, StringComparer.OrdinalIgnoreCase);
	}
}
