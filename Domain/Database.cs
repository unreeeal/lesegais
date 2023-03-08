using Lesegais.Domain.Models;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Lesegais.Domain
{
    public class Database
    {
        private const string CONNECTION_STRING = @"Data Source=(localdb)\MSSQLLocalDB;Initial Catalog=lesegais";
        private const string BUYERSELLER_PROCEDURE_NAME = "BuyerSellerInsertOrUpdateGetId";
        private const string INSERT_RECORD_PROCEDURE_NAME = "InsertOrUpdate";
        private const int BATCH_SIZE_FOR_SQL = 500;

        static Database()
        {

        }
        public static void Insert(List<LesegaisRecordModel> list)
        {

            string escapeQuotes(string s) => Regex.Replace(s, "\"|'", x => x.Groups[0].Value + x.Groups[0].Value);
            string replaceCommas(string s) => s.Replace(",", ".");

            //групируем сначала по продавцам. затем по покупателям
            var sellers = list.GroupBy(x => x.Seller).ToList();
            // данные в базу будем заносить пачками по 500 продавцом, считаем количество "пачек"
            int steps = sellers.Count() / BATCH_SIZE_FOR_SQL + 1;
            using (var con = new SqlConnection(CONNECTION_STRING))
            {
                con.Open();
                for (int i = 0; i < steps; i++)
                {
                    // разбиваем на пачки по 500 продавцов
                    var sellersPart = sellers.Skip(i * BATCH_SIZE_FOR_SQL).Take(BATCH_SIZE_FOR_SQL).ToList();
                    var sb = new StringBuilder();
                    sb.AppendLine("declare @buyerId int;");
                    sb.AppendLine("declare @sellerId int;");

                    foreach (var sellerGroup in sellersPart)
                    {
                        //запоминаем Id продавца
                        sb.AppendLine($"exec @sellerId= {BUYERSELLER_PROCEDURE_NAME} N'{escapeQuotes(sellerGroup.Key.Name)}','{escapeQuotes(sellerGroup.Key.INN)}'; ");
                       
                        // групируем покупателей 
                        foreach (var buyerGroup in sellerGroup.GroupBy(x => x.Buyer))
                        {
                            sb.AppendLine($"exec @buyerId= {BUYERSELLER_PROCEDURE_NAME} N'{escapeQuotes(buyerGroup.Key.Name)}','{buyerGroup.Key.INN}'; ");
                            foreach (var deal in buyerGroup)
                            {
                                sb.AppendLine($"exec {INSERT_RECORD_PROCEDURE_NAME}  '{deal.DeclarationNumber}',@buyerId,@sellerId,'{deal.Date}',{replaceCommas(deal.WoodFromSeller)},{replaceCommas(deal.WoodFromBuyer)};");

                            }
                        }
                    }
                    var query = sb.ToString();
                    using (var cmd = new SqlCommand(query, con))
                    {
                        cmd.ExecuteNonQuery();
                    }

                }

            }
        }
    }
}
