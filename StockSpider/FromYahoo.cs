using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;

namespace StockSpider
{
    class FromYahoo
    {
        public void Execute()
        {
            var baseUrl = "http://ichart.finance.yahoo.com/table.csv?ignore=.csv&s=";

            var sql = "select * from StockItem a where  not exists (select 1 from [DayData] where StockCode = a.Code and Area = a.Area )";

            using (var db = new StockContext1())
            {
                var stocks = db.ExecuteStoreQuery<StockItem>(sql);
                WebClient client = new WebClient();

                foreach (var stock in stocks)
                {

                    Console.WriteLine("Downloading " + stock.Code + "." + stock.Area);
                    try
                    {
                        var csvContent = client.DownloadString(baseUrl + stock.Code + "." + stock.Area);
                        Console.WriteLine("Download done.");

                        if (string.IsNullOrEmpty(csvContent))
                        {
                            continue;
                        }

                        var index = 0;
                        foreach (var line in csvContent.Split('\r', '\n'))
                        {
                            if (index++ != 0 && !string.IsNullOrEmpty(line))
                            {
                                var array = line.Split(',');

                                var stockData = new DayData()
                                {
                                    Date = DateTime.Parse(array[0]),
                                    Open = decimal.Parse(array[1]),
                                    High = decimal.Parse(array[2]),
                                    Low = decimal.Parse(array[3]),
                                    Close = decimal.Parse(array[4]),
                                    Volume = decimal.Parse(array[5]),
                                    AdjClose = decimal.Parse(array[6]),
                                    CreateAt = DateTime.Now,
                                    Area = stock.Area,
                                    StockCode = stock.Code
                                };

                                db.DayData.AddObject(stockData);
                            }
                        }


                        db.SaveChanges();

                        //var objectStateEntries = db
                        //     .ObjectStateManager
                        //     .GetObjectStateEntries(EntityState.Added);

                        //foreach (var objectStateEntry in objectStateEntries)
                        //{
                        //    db.Detach(objectStateEntry);
                        //}

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
