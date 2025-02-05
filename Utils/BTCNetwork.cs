using MessagePack;
using System.Text.Json;

[MessagePackObject(true)]
public class NetworkOverview
{
	public string schema { get; set; }
	public NetworkData data { get; set; }
}

[MessagePackObject(true)]
public class NetworkData
{
	public DateTime timestamp { get; set; }
	public double marketcap { get; set; }
	public double hashpriceUsd { get; set; }
	public long networkHashrate7d { get; set; }
	public long networkDiff { get; set; }
	public double coinbaseRewards24h { get; set; }
	public double feesBlocks24h { get; set; }
	public double txRateAvg7d { get; set; }
	public DateTime nextHalvingDate { get; set; }
	public int nextHalvingCount { get; set; }
	public double estDiffAdj { get; set; }
	public int avgBlockTime { get; set; }
	public int blocksToAdj { get; set; }
	public DateTime estDiffAdjDate { get; set; }
	public decimal BTCValueNow { get; set; }
	public double USDBRL { get; set; }
	public double USDEUR { get; set; }
	public double USDCNY { get; set; }
	public double ETHValueNow { get; set; }

}

public static class GlobalData
{
	public static NetworkOverview NetworkOverviewGlobal;
}

public static class NetworkDataFetcher
{
	private static readonly HttpClient httpClient = new HttpClient();

	public static void FetchNetworkData()
	{
		try
		{

			string url = "https://data.hashrateindex.com/hi-api/hashrateindex/network/overview";
			var response = httpClient.GetAsync(url).Result;
			if (!response.IsSuccessStatusCode)
				throw new Exception("Hashrate Index API request failed.");

			var content = response.Content.ReadAsStringAsync().Result;

			var networkOverview = JsonSerializer.Deserialize<NetworkOverview>(content);

			if (networkOverview == null)
				throw new Exception("Failed to deserialize Hashrate Index API response.");

			string btcPriceUrl = "https://api.coindesk.com/v1/bpi/currentprice.json";
			var btcResponse = httpClient.GetAsync(btcPriceUrl).Result;
			if (btcResponse.IsSuccessStatusCode)
			{
				var btcContent = btcResponse.Content.ReadAsStringAsync().Result;
				using (JsonDocument doc = JsonDocument.Parse(btcContent))
				{
					var root = doc.RootElement;
					var rateString = root.GetProperty("bpi")
										 .GetProperty("USD")
										 .GetProperty("rate")
										 .GetString();

					rateString = rateString.Replace(",", "");

					if (decimal.TryParse(rateString, out decimal btcValueNow))
					{

						networkOverview.data.BTCValueNow = btcValueNow;
					}
				}
			}

			string usdBrlUrl = "https://economia.awesomeapi.com.br/last/USD-BRL";
			var usdBrlResponse = httpClient.GetAsync(usdBrlUrl).Result;
			if (usdBrlResponse.IsSuccessStatusCode)
			{
				var usdBrlContent = usdBrlResponse.Content.ReadAsStringAsync().Result;
				using (JsonDocument doc = JsonDocument.Parse(usdBrlContent))
				{
					var root = doc.RootElement.GetProperty("USDBRL");
					var askString = root.GetProperty("ask").GetString();

					if (double.TryParse(askString, out double usdBrl))
					{
						networkOverview.data.USDBRL = usdBrl;
					}
				}
			}

			string usdEurUrl = "https://economia.awesomeapi.com.br/last/USD-EUR";
			var usdEurResponse = httpClient.GetAsync(usdEurUrl).Result;
			if (usdEurResponse.IsSuccessStatusCode)
			{
				var usdEurContent = usdEurResponse.Content.ReadAsStringAsync().Result;
				using (JsonDocument doc = JsonDocument.Parse(usdEurContent))
				{
					var root = doc.RootElement.GetProperty("USDEUR");
					var askString = root.GetProperty("ask").GetString();

					if (double.TryParse(askString, out double usdEur))
					{
						networkOverview.data.USDEUR = usdEur;
					}
				}
			}

			string usdCnyUrl = "https://economia.awesomeapi.com.br/last/USD-CNY";
			var usdCnyResponse = httpClient.GetAsync(usdCnyUrl).Result;
			if (usdCnyResponse.IsSuccessStatusCode)
			{
				var usdCnyContent = usdCnyResponse.Content.ReadAsStringAsync().Result;
				using (JsonDocument doc = JsonDocument.Parse(usdCnyContent))
				{
					var root = doc.RootElement.GetProperty("USDCNY");
					var askString = root.GetProperty("ask").GetString();

					if (double.TryParse(askString, out double usdCny))
					{
						networkOverview.data.USDCNY = usdCny;
					}
				}
			}

			string ethPriceUrl = "https://min-api.cryptocompare.com/data/price?fsym=ETH&tsyms=USD";
			var ethPriceResponse = httpClient.GetAsync(ethPriceUrl).Result;

			if (ethPriceResponse.IsSuccessStatusCode)
			{
				var ethPriceContent = ethPriceResponse.Content.ReadAsStringAsync().Result;
				using (JsonDocument doc = JsonDocument.Parse(ethPriceContent))
				{
					if (doc.RootElement.TryGetProperty("USD", out JsonElement usdElement) &&
						usdElement.TryGetDouble(out double ethPrice))
					{
						networkOverview.data.ETHValueNow = ethPrice;
					}
				}
			}
			else
			{
				Console.WriteLine($"Failed to fetch ETH price. Status Code: {ethPriceResponse.StatusCode}");
			}

			GlobalData.NetworkOverviewGlobal = networkOverview;
		}
		catch
		{

			var emptyOverview = new NetworkOverview
			{
				schema = string.Empty,
				data = new NetworkData()
			};
			GlobalData.NetworkOverviewGlobal = emptyOverview;
		}
	}
}