using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using MongoDB.Bson;

namespace StockSpider
{
    public abstract class BaseSpider : IDataConverter
    {
        private WebClient webClient = new WebClient();
        private int maxTryTimes = 10;
        private Regex numberReg = new Regex(@"[\d|,|\.]+");

        protected BsonValue getCellData(string str)
        {
            if (str.Trim() == "--")
            {
                return new BsonDouble(0);
            }
            DateTime converted = new DateTime();

            if (DateTime.TryParse(str, out converted))
            {
                return new BsonDateTime(converted);
            }

            var numbermch = numberReg.Match(str);

            if (numbermch.Success)
            {
                double cvtDb = 0;
                if (Double.TryParse(numbermch.Value, out cvtDb))
                {
                    return new BsonDouble(cvtDb);
                }
            }

            throw new Exception("不支持的值：" + str);
        }

        protected string downloadHtml(string url)
        {
            for (int i = 0; i < maxTryTimes; i++)
            {
                try
                {
                    var htmlContent = webClient.DownloadString(url);
                    return htmlContent;
                }
                catch (Exception ex)
                {

                    Console.WriteLine("连接服务器出错，暂停下载进程20s");
                    Thread.Sleep(20 * 1000);
                    Console.WriteLine("开始重试");
                }
            }

            throw new Exception("服务器连接不上，请检查网络," + url);
        }

        public abstract IList<MongoDB.Bson.BsonDocument> GetData(string area, string stockCode);
    }
}
