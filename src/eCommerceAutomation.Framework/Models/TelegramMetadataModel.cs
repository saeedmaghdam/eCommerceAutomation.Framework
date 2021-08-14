using System;
using System.Collections.Generic;

namespace eCommerceAutomation.Framework.Models
{
    [Serializable]
    public class TelegramMetadataModel
    {
        public IEnumerable<decimal> Prices
        {
            get;
            set;
        }

        public string PlainText
        {
            get;
            set;
        }
    }
}
