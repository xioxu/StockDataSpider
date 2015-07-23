using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using Newtonsoft.Json;

namespace StockSpider
{
    class FromSohu
    {
        public void Execute()
        {
            var baseUrl = "http://q.stock.sohu.com/hisHq?code=cn_{0}&start=19910101&end=20150424&stat=1&order=D&period=d&callback=historySearchHandler&rt=jsonp&r=0.4286239536013454&0.14818861591629684";

            var sql = "select * from StockItem a where  not exists (select 1 from [DayData] where StockCode = a.Code and Area = a.Area )";

            using (var db = new StockContext1())
            {

                //   db.Configuration.AutoDetectChangesEnabled = false;
                // db.Configuration.ValidateOnSaveEnabled = false;

                var stocks = db.ExecuteStoreQuery<StockItem>(sql).ToList();
                WebClient client = new WebClient();
                var jsonpFuncLen = "historySearchHandler(".Length;

                foreach (var stock in stocks)
                {
                    Console.WriteLine("Downloading " + stock.Code + "." + stock.Area);

                    try
                    {
                        var jsonpContent = client.DownloadString(string.Format(baseUrl, stock.Code));
                        Console.WriteLine("Download done.");

                        if (string.IsNullOrEmpty(jsonpContent))
                        {
                            continue;
                        }

                        var jsonStr = jsonpContent.Substring(jsonpFuncLen, jsonpContent.Length - jsonpFuncLen - 2);

                        var jsonObj = JsonConvert.DeserializeObject<SohuStockInfo[]>(jsonStr);

                        if (jsonObj[0].status != 0)
                        {
                            continue;
                        }

                        Console.WriteLine("Begin commit");
                        foreach (var array in jsonObj[0].hq)
                        {

                            var stockData = new DayData()
                            {
                                Date = DateTime.Parse(array[0]),
                                Open = decimal.Parse(array[1]),
                                High = decimal.Parse(array[6]),
                                Low = decimal.Parse(array[5]),
                                Close = decimal.Parse(array[2]),
                                Volume = decimal.Parse(array[7]),
                                //AdjClose = decimal.Parse(array[6]),
                                CreateAt = DateTime.Now,
                                Area = stock.Area,
                                StockCode = stock.Code
                            };

                            db.DayData.AddObject(stockData);

                        }

                        db.SaveChanges();
                        Console.WriteLine("Commit done");
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex);
                    }


                }

            }
        }
    }
}
