using System;
using System.Collections.Generic;

namespace eCommerceAutomation.Framework.Models
{
    [Serializable]
    public class UpdateProductModel
    {
        public decimal? Price
        {
            get;
            set;
        }

        public List<Tuple<int, decimal>> TierPrices
        {
            get;
            set;
        }

        public int? InStockQuantity
        {
            get;
            set;
        }

        public bool? IsAvailable
        {
            get;
            set;
        }

        public int? MinimumQuantity
        {
            get;
            set;
        }
    }
}
