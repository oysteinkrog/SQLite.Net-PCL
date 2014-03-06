using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StocksBlob
{
    public class Valuation
    {
        public DateTime Time { get; set; }
        public decimal Price { get; set; }

        public override string ToString()
        {
            return string.Format("{0:MMM dd yy}    {1:C}", Time, Price);
        }
    }
}
