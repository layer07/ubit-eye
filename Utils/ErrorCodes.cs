﻿namespace MinerPulse { 

public static partial class Helpers
{
public static Dictionary<string, (string Meaning, string Solution)> errorCodes = new Dictionary<string, (string Meaning, string Solution)>
{
	{ "110", ("Inlet fan detection speed error", "Check whether the fan connection is normal, or replace the power supply, or replace the fan") },
	{ "140", ("Fan speed is too high", "Please check the environment temperature") },
	{ "200", ("Power detection error, no PSU found", "Check the power output wiring, update the latest firmware or replace the PSU") },
	{ "201", ("PSU is not compatible with the profile", "Replace the correct PSU") },
	{ "202", ("PSU output voltage error", "Upgrade the latest firmware or check the PSU") },
	{ "203", ("Power protection", "Please check the ambient temp") },
	{ "205", ("Power current error", "Check the PSU in the grid") },
	{ "206", ("Power input voltage low", "Improve the PSU conditions and input voltage") },
	{ "208", ("PSU changes too much", "Replace the power supplyWhatsminer PSU List") },
	{ "210", ("Power error status", "Check power failure code") },
	{ "211", ("Power output current deviation is too large", "Replace the power supply") },
	{ "212", ("Power output voltage margin is insufficient", "Contact after-sales") },
	{ "213", ("Power input voltage and current do not match the power", "Replace the power supply") },
	{ "214", ("Power pin did not change", "Contact after-sales") },
	{ "216", ("Power remained unchanged for a long time", "Replace the power supply") },
	{ "218", ("Power input voltage is lower than 230V for high power mode", "Increase input voltage, replace PSU") },
	{ "219", ("Power input current is incorrect", "Replace the power supply") },
	{ "233", ("Over temperature protection of power output", "Please check the environment temperature") },
	{ "236", ("Overcurrent protection of power output", "Please check the environment temperature and the copper row screw") },
	{ "239", ("Overvoltage protection of power output", "Inspection of power supply in power grid") },
	{ "241", ("Power output current imbalance", "Replace the power") },
	{ "243", ("Over temperature protection for power input", "Please check the environment temperature") },
	{ "248", ("Overvoltage protection for power input", "Inspection of power supply in power grid") },
	{ "253", ("Power fan error", "Replace the power supply") },
	{ "255", ("Protection of over power output", "Please check the environment temperature") },
	{ "257", ("Input over current protection of power supply primary side", "Try to power off and restart, no effect to replace the power supply") },
	{ "258", ("Power input three-phase voltage imbalance warning", "Inspection of power supply in power grid") },
	{ "263", ("Power communication warning", "Check whether the screws of the control board are locked") },
	{ "267", ("Power watchdog protection", "Contact the technician in time") },
	{ "268", ("Power output over-current protection", "Check the ambient temperature, check the copper bar screw") },
	{ "269", ("Power input over-current protection", "Improve power supply conditions and input voltage") },
	{ "270", ("Power input over-voltage protection", "Inspection of input voltage in power grid") },
	{ "272", ("Warning of excessive power output of power supply", "Please check the environment temperature") },
	{ "274", ("Power fan warning", "Check if the power fan is blocked and may need to be replaced") },
	{ "275", ("Power over temperature warning", "Please check the environment temperature") },
	{ "28X-29X", ("Power custom register error", "Contact after-sales") },
	{ "30X", ("SMX temperature sensor detection error", "Check the connection of the hashboard") },
	{ "32X", ("SMX temperature reading error", "Check whether the control board screw is locked properly, check the connection board and the arrangement contact") },
	{ "326", ("Liquid cooling temperature sensor communication error", "Replace the power supply") },
	{ "35X", ("SMX temperature protecting", "Please check the environment temperature") },
	{ "360", ("The temperature of the hash board is overheating", "Check the environment temperature") },
	{ "370", ("The environment temperature fluctuates too much", "Check the environment temperature, or check the environment wind direction and wind speed") },
	{ "41X", ("SMX detect eeprom error", "Check adapter board and wiring contact") },
	{ "42X", ("SMX parser eeprom error", "Upgrade firmwareWhatsminer Firmware Download Link") },
	{ "45X", ("SMX eeprom xfer error", "Check adapter board and wiring contact. Upgrade firmware") },
	{ "500", ("No software configuration is added for the model", "Upgrade firmware") },
	{ "51X", ("SMX miner type error", "The version and type of hashboard are inconsistent, replace the correct hashboard") },
	{ "52X", ("SMX bin type error", "The chip type of the hashrate board is inconsistent, replace the correct hashrate board") },
	{ "53X", ("SMX not found", "Check the connection and arrangement of the adapter board, or replace the control board, check whether the hash board connector is empty welded") },
	{ "54X", ("SMX reading chip id error", "Check adapter board and wiring contact, Clean the dust on the hashboard") },
	{ "55X", ("SMX have bad chips", "Contact after-sales") },
	{ "56X", ("SMX loss balance", "Plug in the adapter plate, and then screw in the power connection hashboard again") },
	{ "57X", ("SMX xfer error chip", "Contact after-sales") },
	{ "58X", ("SMX reset error", "If the hashrate is abnormal, please consult after-sales") },
	{ "59X", ("SMX frequency too low", "Contact after-sales") },
	{ "600", ("Environment temperature is high", "Check the environment temperature") },
	{ "610", ("If the ambient temperature is too high in high performance mode, return to normal mode", "Check the ambient temperature, high performance mode needs to be controlled below 30°C") },
	{ "620", ("Liquid cooling liquid temperature protection", "Check liquid temperature") },
	{ "701", ("Control board no support chip", "Upgrade the corresponding type of firmware") },
	{ "702", ("Control board version unknown", "Contact after-sales") },
	{ "710", ("Control board rebooted as exception", "Updating the latest firmware. Check whether the control board screw is locked properly") },
	{ "714", ("The network connection is seriously unstable", "Check the network cable connection or replace the control board") },
	{ "72X", ("SMX serial port communication error", "Upgrade firmware or replace control board") },
	{ "800", ("Cgminer checksum error", "Re-upgrade firmware") },
	{ "810", ("Air to liquid PCBSN does not match", "Re-upgrade the liquid cooling firmware") },
	{ "820", ("Air to liquid or Adjust frequency and PSUSN does not match", "Re-upgrade the liquid cooling firmware or Adjust frequency") },
	{ "901", ("Power rate error", "Check whether the miner has been modified") },
	{ "90XX", ("The process exited abnormally", "Upgrade firmware. If it doesn’t work, consult aftersales") },
	{ "2000", ("No pool information configured", "Check pool configuration") },
	{ "2010", ("All pools are disable", "Check the network or mining pools configure") },
	{ "2030", ("Mining pool rejection rate is too high", "Check the network or mining pool setting, and the mining setting of the cryptocurrency") },
	{ "2040", ("The pool does not support the asicboost mode", "Check pool configuration") },
	{ "2050", ("Failed to switch to new pool", "Check the network or pools configure") },
	{ "2310", ("Hash rate is too low", "Check input voltage, network environment, and ambient temperature") },
	{ "501X", ("SMX chip voltage too low", "Contact after-sales") },
	{ "502X", ("Chip voltage change", "Replace the power supply") },
	{ "503X", ("The maximum and minimum temperature difference of the SMX chip is too large", "Check the heat dissipation of the miner") },
	{ "505X", ("SMX chip temperature protection", "Please check the environment temperature") },
	{ "507X", ("SMX liquid velocity is abnormal", "Check if the liquid flow is normal") },
	{ "508X", ("SMX chip temperature calibration failed", "Reset") },
	{ "5090", ("Chip temperature calibration abnormality", "Try shutting down for a while and then restarting, or restoring factory settings, or checking the heat dissipation of the hash board") },
	{ "511X", ("SMX Frequency Up Timeout", "Reboot") },
	{ "8010", ("Frequency is not up to standard", "Upgrade the latest software or reset") },
	{ "8400", ("Wrong software version (older or not the official version)", "Upgrade to the correct software version") },
	{ "8410", ("Software version error (M2x miner with M3x firmware, or M3x with M2x firmware)", "Upgrade to the correct firmware version") },
	{ "8700", ("Miner and PSU model do not match", "Replace with correct PSU") },
	{ "52XBBB", ("SMX chip BBB error nonce", "Contact after-sales") },
	{ "54XBBB", ("SMX chip temp protected", "Restart. If it doesn't work, consult after-sales") },
	{ "55XBBB", ("SMX chip BBB is reset", "Replace the power supply. If it doesn't work, consult after-sales") },
	{ "100000", ("Security library error", "Upgrade the latest firmware or burn the card") },
	{ "0x0001", ("Input voltage is too low, need improvement", "Check the power supply") },
	{ "0x0002", ("Temperature sampling over temperature protection of power radiator", "Power on again after 10 minutes of power failure. If it occurs again, replace the power supply") },
	{ "0x0020", ("Output undervoltage", "Check the power supply") },
	{ "0x0040", ("Output over current (continuous load 320A for more than 2S)", "Tighten the copper bar screw again") },
	{ "0x0080", ("Primary side over current", "Power on again after 10 minutes of power failure. If it occurs again, replace the power supply") },
	{ "0x0100", ("Single circuit overcurrent (protection point 120a)", "Check the PSU") },
	{ "0x0800", ("Fan failure", "Replace the PSU") },
	{ "0x1000", ("Output over current (continuous load of 310A for more than 5min)", "Check the PSU") },
};

		public static (string Meaning, string Solution) GetErrorCodeDetails(string errorCode)
		{
			if (errorCodes.TryGetValue(errorCode, out var details))
			{
				return details;
			}
			return ("Unknown", "Unknown"); // Default for unknown error
		}


	}

}