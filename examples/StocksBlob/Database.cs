using SQLite.Net;
using SQLite.Net.Platform.Win32;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StocksBlob
{
    public class Database : SQLiteConnection
    {
        public Database(string path)
            : base(new SQLitePlatformWin32(), path)
        {
            DropTable<Stock>();
            DropTable<Valuation>();
            CreateTable<Stock>();
            CreateTable<Valuation>();
        }
        public IEnumerable<Valuation> QueryValuations(Stock stock)
        {
            return stock.Prices;
        }
        public Valuation QueryLatestValuation(Stock stock)
        {
            return stock.Prices.OrderByDescending(x => x.Time).Take(1).FirstOrDefault();
        }

        public Stock QueryStock(string stockSymbol)
        {
            return (from s in Table<Stock>()
                    where s.Symbol == stockSymbol
                    select s).FirstOrDefault();
        }

        public IEnumerable<Stock> QueryAllStocks()
        {
            return from s in Table<Stock>()
                   orderby s.Symbol
                   select s;
        }

        public void UpdateStock(string stockSymbol)
        {
            //
            // Ensure that there is a valid Stock in the DB
            //
            var stock = QueryStock(stockSymbol);
            if (stock == null)
            {
                stock = new Stock { Symbol = stockSymbol, Prices = new List<Valuation>() };
                Insert(stock);
            }

            //
            // When was it last valued?
            //
            var latest = QueryLatestValuation(stock);
            var latestDate = latest != null ? latest.Time : new DateTime(1950, 1, 1);

            //
            // Get the latest valuations
            //
            try
            {
                var newVals = YahooScraper.GetValuations(stock, latestDate + TimeSpan.FromHours(23), DateTime.Now);
                //stock.Prices.Clear();
                stock.Prices = newVals.ToList();
                this.Update(stock);
                //InsertAll (newVals);
            }
            catch (System.Net.WebException ex)
            {
                Console.WriteLine(ex);
            }
        }
    }
}
