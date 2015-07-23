using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace StockSpider
{
    public interface IDataFill
    {
        void Fill(string area , String stockCode);
    }
}
