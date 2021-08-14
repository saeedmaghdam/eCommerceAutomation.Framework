using System;
using System.Collections.Generic;
using System.Linq;

namespace eCommerceAutomation.Framework.Models
{
    [Serializable]
    public class WebsiteMetadataModel
    {
        public int InStockQuantity
        {
            get;
            set;
        }

        public int MinimumQuantity
        {
            get;
            set;
        }

        public decimal OldPrice
        {
            get;
            set;
        }

        public decimal Price
        {
            get;
            set;
        }

        public List<Tuple<int, decimal>> WholesalePrices
        {
            get;
            set;
        }

        public bool IsInStock
        {
            get;
            set;
        }

        public WebsiteMetadataModel()
        {
            WholesalePrices = new List<Tuple<int, decimal>>();
        }

        public override bool Equals(object obj)
        {
            var inputModel = obj as WebsiteMetadataModel;
            if (inputModel == null)
                return false;

            return (InStockQuantity, MinimumQuantity, OldPrice, Price, WholesalePrices, IsInStock).Equals((inputModel.InStockQuantity, inputModel.MinimumQuantity, inputModel.OldPrice, inputModel.Price, inputModel.WholesalePrices, inputModel.IsInStock));
        }

        public override string ToString()
        {
            return $"{InStockQuantity}_{MinimumQuantity}_{OldPrice}_{Price}_{string.Join(",", WholesalePrices.Select(x => $"{x.Item1}_{x.Item2}"))}_{IsInStock}".ToString();
        }

        public override int GetHashCode()
        {
            return (InStockQuantity, MinimumQuantity, OldPrice, Price, WholesalePrices, IsInStock).GetHashCode();
        }
    }
}
