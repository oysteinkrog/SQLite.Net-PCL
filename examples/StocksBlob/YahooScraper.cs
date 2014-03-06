using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StocksBlob
{
    public static class YahooScraper
    {
        public static IEnumerable<Valuation> GetValuations(Stock stock, DateTime start, DateTime end)
        {
            var t = "http://ichart.finance.yahoo.com/table.csv?s={0}&d={1}&e={2}&f={3}&g=d&a={4}&b={5}&c={6}&ignore=.csv";
            var url = string.Format(t, stock.Symbol, end.Month - 1, end.Day, end.Year, start.Month - 1, start.Day, start.Year);
            Console.WriteLine("GET {0}", url);
            var req = System.Net.WebRequest.Create(url);
            using (var resp = new System.IO.StreamReader(req.GetResponse().GetResponseStream()))
            {
                var first = true;
                var dateCol = 0;
                var priceCol = 6;
                for (var line = resp.ReadLine(); line != null; line = resp.ReadLine())
                {
                    var parts = line.Split(',');
                    if (first)
                    {
                        dateCol = Array.IndexOf(parts, "Date");
                        priceCol = Array.IndexOf(parts, "Adj Close");
                        first = false;
                    }
                    else
                    {
                        yield return new Valuation
                        {
                            Price = decimal.Parse(parts[priceCol], CultureInfo.InvariantCulture),
                            Time = DateTime.Parse(parts[dateCol])
                        };
                    }
                }
            }
        }
    }
}
