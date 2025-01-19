// Currency Converter Logic

internal class CurrencyConverter
{
    private static readonly HttpClient _httpClient = new HttpClient();
    private static readonly MemoryCache _cache = new MemoryCache("CurrencyCache");

    private static readonly string apiKey = "1b29b58f1aa826b79ead3bb5";
    private static readonly string apiUrl = "https://v6.exchangerate-api.com/v6/{0}/latest/{1}"; // API для получения курсов валют

    public async Task<decimal?> GetConversionRateAsync(string fromCurrency, string toCurrency)
    {
        string cacheKey = $"conversion_rate_{fromCurrency}_to_{toCurrency}";

        if (_cache.Contains(cacheKey))
        {
            Console.WriteLine("Data received from cache.");
            return (decimal?)_cache.Get(cacheKey);
        }


        Console.WriteLine("Data received from API.");

        string url = string.Format(apiUrl, apiKey, fromCurrency);
        Console.WriteLine(url);
        var response = await _httpClient.GetStringAsync(url);
        var data = JsonConvert.DeserializeObject<ApiResponse>(response);

        if (data?.ConversionRates != null && data.ConversionRates.ContainsKey(toCurrency))
        {
            var rate = data.ConversionRates[toCurrency];

            _cache.Add(cacheKey, rate, DateTime.Now.AddHours(1));
            Console.WriteLine("Cache added");
            return rate;
        }

        return null;
    }
}

internal class Program
{
    static async Task Main(string[] args)
    {
        var converter = new CurrencyConverter();

        Console.Write("Initial currency (USD): ");
        string fromCurrency = Console.ReadLine().ToUpper();
        Console.Write("Final currency (EUR): ");
        string toCurrency = Console.ReadLine().ToUpper();
        Console.Write("Amount to convert: ");
        decimal amount = Convert.ToDecimal(Console.ReadLine());

        decimal? rate = await converter.GetConversionRateAsync(fromCurrency, toCurrency);
        decimal? result = amount * rate;

        Console.WriteLine(result);


        decimal? rate1 = await converter.GetConversionRateAsync(fromCurrency, toCurrency);
        decimal? result1 = (amount* 2) * rate;

        Console.WriteLine(result1);

        Console.ReadKey();
    }
}

internal class ApiResponse
{
    public bool Success { get; set; }
    public string BaseCode { get; set; }

    [JsonProperty("conversion_rates")]
    public Dictionary<string, decimal> ConversionRates { get; set; }
}