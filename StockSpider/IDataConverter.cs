using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MongoDB.Bson;

namespace StockSpider
{
    public interface IDataConverter
    {
        IList<BsonDocument> GetData(string area, string stockCode);
    }
}
