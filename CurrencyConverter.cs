using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Microsoft.Data.Sqlite;
using System.Net.Http;
using ConsoleApp5;
using System.Globalization;

namespace ConsoleApp5
{
    internal static class CurrencyConverter
    {
        public static readonly List<string> Currencies = new List<string>
        {
            "RUB",
            "USD",
            "EUR",
            "GBP",
            "JPY",
            "BYN",
            "PLN",
            "CNY",
            "TRY",
            "KZT"
        };
       
        private static readonly string apiKey = "182e065a094f208e4790ad5b"; // Ключ API
        private static readonly string apiUrl = "https://v6.exchangerate-api.com/v6/{0}/latest/{1}"; // API для получения курсов валют
        
        private const string connectionString = $"Data Source=ExchangeRates.db"; // Строка подключения к БД
        private static readonly HttpClient _httpClient = new HttpClient();

        public static string dbQuery = "INSERT INTO ExchangeRates (FromCurrency, ToCurrency, Rate) VALUES "; // Заготовка для запроса на добавление всех валют в БД
        public static string dbQueryText; 


        public static async Task GetExchangeRates()
        {
            foreach (string currrency in Currencies)
            {
                string url = string.Format(apiUrl, apiKey, currrency); // Подстановка в ссылку API-ключа и необходимой валюты
                HttpResponseMessage response = await _httpClient.GetAsync(url);
                response.EnsureSuccessStatusCode();

                string content = await response.Content.ReadAsStringAsync();
                var rates = JsonConvert.DeserializeObject<ExchangeRatesResponse>(content);


                foreach (var rate in rates.Rates)
                {
                    if (Currencies.Contains(rate.Key))
                    {
                        string insert = $"('{currrency}', '{rate.Key}', {rate.Value.ToString(CultureInfo.InvariantCulture)}),";
                        dbQuery += insert;
                    }
                    dbQueryText = dbQuery.Substring(0,dbQuery.Length - 1) + ';'; // Удаляю запятую в конце и заменяю её на ';'
                }
            }
        }

        public static async Task ClearDB()
        {
            using (var connection = new SqliteConnection(connectionString))
            {
                connection.Open();

                string query = "DELETE FROM ExchangeRates";
                using (SqliteCommand command = new SqliteCommand(query, connection))
                {
                    command.ExecuteNonQuery();
                }
            }
        }

        public static async Task UpdateDBAsync()
        {
            await GetExchangeRates(); // Создание INSERT-запроса
            await ClearDB(); // Очистка БД от устаревших курсов валют

            using (SqliteConnection connection = new SqliteConnection(connectionString))
            {
                connection.Open();

                using (SqliteCommand command = new SqliteCommand(dbQueryText, connection))
                {
                    command.ExecuteNonQuery();
                    Console.WriteLine("success db");
                }
            }
        }
 
        public static async Task<decimal?> GetRate(string fromCurrency, string toCurrency)
        {
            using (SqliteConnection connection = new SqliteConnection(connectionString))
            {
                connection.Open();

                string query = $"SELECT Rate FROM ExchangeRates WHERE FromCurrency = '{fromCurrency}' and ToCurrency = '{toCurrency}'";

                using (SqliteCommand command = new SqliteCommand(query, connection))
                {
                    using (SqliteDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            return Convert.ToDecimal(reader[0]);
                        }
                    }
                }
            }
            return 0;
        }
    }

}
