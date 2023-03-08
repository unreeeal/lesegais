using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Lesegais
{
    public class Downloader
    {
        public static string MakePostRequest(string url, string data)
        {


            string res = null;
            using (WebClient wb = new WebClient())
            {

                wb.Proxy = null;
                wb.Headers.Add("user-agent", "Mozilla/5.0 (Windows NT 6.1; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/84.0.4147.135 Safari/537.36");
                wb.Headers.Add("Accept", "text/html,application/xhtml+xml,application/xml;q=0.9,image/webp,*/*;q=0.8");
                //wb.Headers.Add("Accept-Encoding", "gzip, deflate, sdch");
                wb.Headers.Add("Accept-Language", "ru-RU,ru;q=0.8,en-US;q=0.6,en;q=0.4");
                wb.Headers.Add("Content-Type", "application/x-www-form-urlencoded;charset=UTF-8");
                wb.Encoding = Encoding.UTF8;
                for (int i = 0; i < 10; i++)
                {
                    try
                    {
                        res = wb.UploadString(url, data);

                        break;
                    }
                    catch (WebException ex)
                    {

                        Console.WriteLine(ex);
                        return null;

                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex);
                        Thread.Sleep(5000);
                    }
                }
            }
            return res;
        }

    }
}
