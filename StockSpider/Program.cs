using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Data.Entity;
using System.Globalization;
using System.Net;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using ArtOfTest.WebAii.Controls.HtmlControls;
using ArtOfTest.WebAii.Core;
using ArtOfTest.WebAii.ObjectModel;
using MongoDB.Bson;
using MongoDB.Driver;
using Newtonsoft.Json;
using TestProject1;
using EntityState = System.Data.EntityState;

namespace StockSpider
{
    class Program
    {
        static void Main(string[] args)
        {
            //由于我本地之前已经将A股所有公司代码下载下来，所以直接取的是我的数据库
            //您可以将取股票代码的这部分源码替换使用本文件中下边注释部分的源码
            //其中沪深股市代码分别在stockItems_sh.txt和stockItems_sz.txt中

            var sql = "select *  from StockItem order by code";
            var lirb = new LiRunbiao();
            var xjllb = new XianjinliuliangBiao();
            var zifzb = new ZiChanFuzhaiBiao();

              var mongoClient = new MongoClient("mongodb://localhost:27017");
            var database = mongoClient.GetServer().GetDatabase("stockData");

            var lirunbiao = database.GetCollection<BsonDocument>("lirunbiao");
            var xianjinliuliangbiao = database.GetCollection<BsonDocument>("xianjinliuliangbiao");
            var zichanfuzhaibiao = database.GetCollection<BsonDocument>("zichanfuzhaibiao");

            bool started = false;

            using (var db = new StockContext1())
            {
                var stocks = db.ExecuteStoreQuery<StockItem>(sql).ToList();
 
                try
                {
                    foreach (var stock in stocks)
                    {
                        var docs2 = xjllb.GetData(stock.Area, stock.Code);
                        var docs1 = lirb.GetData(stock.Area, stock.Code);
                        var docs3 = zifzb.GetData(stock.Area, stock.Code);

                        if (docs1.Count > 0)
                        {
                            lirunbiao.InsertBatch(docs1);    
                        }
                        

                        if (docs2.Count > 0)
                        {
                            xianjinliuliangbiao.InsertBatch(docs2);    
                        }

                        if (docs3.Count > 0)
                        {
                            zichanfuzhaibiao.InsertBatch(docs3);
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex);
                    throw;
                }
            }


            Console.WriteLine("Done");
            Console.Read();
        
       
            return;
            var ash = File.ReadAllText(@"c:\a.txt");

            Regex regex = new Regex("\\<li.*href=\"(.*)\">(.*)\\</.*/li");
            WebClient client = new WebClient();

            foreach (Match match in regex.Matches(ash))
            {
                var href = match.Groups[1].Value;
                var html = client.DownloadString(href);

                var table = new DomFinder(html).Find.ByExpression<HtmlTable>("id=wahaha", "|", "tagIndex=table:0");

                if (table != null)
                {
                    Console.WriteLine("Found it");
                }
            }

            Console.WriteLine("Done");
            Console.Read();
        }
    }
}
