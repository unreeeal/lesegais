using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lesegais.Domain.Models
{
    public class BuyerSellerModel
    {
        public string Name { get; set; }
        public string INN { get; set; }
        /// <summary>
        /// Ключ по которому смотрим уникальность покупателя продавца
        /// </summary>
        public string Key => Name + (INN ?? "");
    }
}
