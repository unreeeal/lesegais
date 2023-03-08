using Lesegais.Domain;
using Lesegais.Domain.Models;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace Lesegais
{
    public class Crawler
    {
        //default size=20
        private const int BATCH_SIZE = 1000;
        private const int REQUEST_DELAY = 3000;
        private const string DEFAULT_URL = "https://www.lesegais.ru/open-area/graphql";
        public static void Start()
        {

            //Сначала получаем количество записей
            var postData = "{\"query\":\"query SearchReportWoodDealCount($size: Int!, $number: Int!, $filter: Filter, $orders: [Order!]) {\\n  searchReportWoodDeal(filter: $filter, pageable: {number: $number, size: $size}, orders: $orders) {\\n    total\\n    number\\n    size\\n    overallBuyerVolume\\n    overallSellerVolume\\n    __typename\\n  }\\n}\\n\",\"variables\":{\"size\":20,\"number\":0,\"filter\":null},\"operationName\":\"SearchReportWoodDealCount\"}";
            var ps = Downloader.MakePostRequest(DEFAULT_URL, postData);
            if (string.IsNullOrEmpty(ps))
            {
                Console.WriteLine("Can't get initial json, exit");

            }
            var jsonObject = JObject.Parse(ps);
            var totalRecordsString = jsonObject["data"]?["searchReportWoodDeal"]?["total"]?.ToString();
            int.TryParse(totalRecordsString, out int totalRecords);
            if (string.IsNullOrEmpty(totalRecordsString) || totalRecords == 0)
            {
                Console.WriteLine("Can't get number of pages,jsonFormat has been changed exit");
            }
            int totalPages = totalRecords / BATCH_SIZE + 1;
            // 
            var dealsDic = new Dictionary<string, LesegaisRecordModel>(totalRecords);
            //dictionary в котором будут хранится уже сушествующие продавцы и покупатели,
            //нужно чтобы было проще и быстрее сохранить все в базу.
            var buyerSellerDictionary = new Dictionary<string, BuyerSellerModel>(totalRecords);
            for (int page = 0; page < totalPages; page++)
            {

                Thread.Sleep(REQUEST_DELAY);
                Console.WriteLine("Page " + (page + 1));
                postData = $"{{\"query\":\"query SearchReportWoodDeal($size: Int!, $number: Int!, $filter: Filter, $orders: [Order!]) {{\\n  searchReportWoodDeal(filter: $filter, pageable: {{number: $number, size: $size}}, orders: $orders) {{\\n    content {{\\n      sellerName\\n      sellerInn\\n      buyerName\\n      buyerInn\\n      woodVolumeBuyer\\n      woodVolumeSeller\\n      dealDate\\n      dealNumber\\n      __typename\\n    }}\\n    __typename\\n  }}\\n}}\\n\",\"variables\":{{\"size\":{BATCH_SIZE},\"number\":{page},\"filter\":null,\"orders\":null}},\"operationName\":\"SearchReportWoodDeal\"}}";
                ps = Downloader.MakePostRequest(DEFAULT_URL, postData);
                if (string.IsNullOrEmpty(ps))
                {
                    Console.WriteLine($"Can't get page {page + 1},skipping page");
                    continue;
                }

                jsonObject = JObject.Parse(ps);
                var dataArray = (JArray)jsonObject["data"]["searchReportWoodDeal"]["content"];
                foreach (var item in dataArray)
                {


                    var declarNumber = item["dealNumber"]?.ToString();

                    //приоритет более свежим записям, если уже есть с таким номером, то пропускаем
                    if (dealsDic.ContainsKey(declarNumber))
                        continue;

                    var mod = new LesegaisRecordModel();
                    mod.DeclarationNumber = declarNumber;
                    mod.Date = item["dealDate"].ToString();
                    // в базе сохраняется как тип date, нужен строгий формат.
                    if (!Regex.IsMatch(mod.Date, @"\d{4}-\d{2}-\d{2}"))
                        mod.Date = null;
                    // в json как число, можно не проверять
                    mod.WoodFromBuyer = item["woodVolumeBuyer"]?.ToString();
                    var buyer = new BuyerSellerModel();
                    buyer.Name = item["buyerName"].ToString();
                    buyer.INN = item["buyerInn"]?.ToString();

                    var buyerKey = buyer.Key;
                    if (buyerSellerDictionary.ContainsKey(buyerKey))
                        buyer = buyerSellerDictionary[buyerKey];
                    else
                        buyerSellerDictionary.Add(buyerKey, buyer);
                    mod.Buyer = buyer;
                    // в json как число, можно не проверять
                    mod.WoodFromSeller = item["woodVolumeSeller"].ToString();
                    var seller = new BuyerSellerModel();
                    seller.Name = item["sellerName"].ToString();
                    seller.INN = item["sellerInn"]?.ToString();

                    var sellerKey = seller.Key;
                    if (buyerSellerDictionary.ContainsKey(sellerKey))
                        seller = buyerSellerDictionary[sellerKey];
                    else
                        buyerSellerDictionary.Add(sellerKey, seller);
                    mod.Seller = seller;


                    dealsDic.Add(mod.DeclarationNumber, mod);

                }
            }
            Database.Insert(dealsDic.Select(x => x.Value).ToList());
        }
    }
}
