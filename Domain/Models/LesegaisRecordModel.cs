using System;
using System.Collections.Generic;
using System.Security.Policy;

namespace Lesegais.Domain.Models
{

    public class LesegaisRecordModel
    {

        public string DeclarationNumber { get; set; }
        public string Date { get; set; }
        public string WoodFromBuyer { get; set; }
        public string WoodFromSeller { get; set; }
        public BuyerSellerModel Buyer { get; set; }
        public BuyerSellerModel Seller { get; set; }


    }
}
