using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using eCommerceAutomation.Framework.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace eCommerceAutomation.Framework
{
    public class CommonHelper
    {
        private readonly IServiceProvider _serviceProvider;

        private readonly string _productMarketDataPatchEndpoint;
        private readonly string _productAvailableStatusPatchEndpoint;
        private readonly string _productUnavailableStatusPatchEndpoint;
        private readonly string _apiKey;
        private readonly decimal _fixedAdjustmentRatio;

        public decimal FixedAdjustmentRatio => _fixedAdjustmentRatio;

        public CommonHelper(IServiceProvider serviceProvider, IConfiguration configuration)
        {
            _serviceProvider = serviceProvider;

            _productMarketDataPatchEndpoint = configuration.GetSection("ProductMarketDataPatchEndpoint").Value;
            _productAvailableStatusPatchEndpoint = configuration.GetSection("ProductAvailableStatusPatchEndpoint").Value;
            _productUnavailableStatusPatchEndpoint = configuration.GetSection("ProductUnavailableStatusPatchEndpoint").Value;
            _apiKey = configuration.GetSection("APIKey").Value;
            _fixedAdjustmentRatio = decimal.Parse(configuration.GetSection("FixedAdjustmentRatio").Value);
        }

        public async Task<HttpResponseMessage> UpdateProductUsingWebsiteMetadataModel(string productExternalId, WebsiteMetadataModel model)
        {
            var updateProductModel = new UpdateProductModel()
            {
                Price = model.Price,
                TierPrices = model.WholesalePrices,
                MinimumQuantity = model.MinimumQuantity,
            };
            var json = JsonSerializer.Serialize(updateProductModel);
            var data = new StringContent(json, Encoding.UTF8, "application/json");
            var url = string.Format(_productMarketDataPatchEndpoint, productExternalId);
            HttpResponseMessage response;

            using (var scope = _serviceProvider.CreateScope())
            {
                using (var client = scope.ServiceProvider.GetService<IHttpClientFactory>().CreateClient())
                {
                    var request = new HttpRequestMessage(new HttpMethod("PATCH"), url);
                    request.Content = data;
                    client.DefaultRequestHeaders.Add("x-api-key", _apiKey);

                    response = await client.SendAsync(request);
                }
            }

            return response;
        }

        public async Task<HttpResponseMessage> UnavailableProduct(string productExternalId)
        {
            var url = string.Format(_productUnavailableStatusPatchEndpoint, productExternalId);
            HttpResponseMessage response;

            using (var scope = _serviceProvider.CreateScope())
            {
                using (var client = scope.ServiceProvider.GetService<IHttpClientFactory>().CreateClient())
                {
                    var request = new HttpRequestMessage(new HttpMethod("PATCH"), url);

                    response = await client.SendAsync(request);
                }
            }

            return response;
        }

        public async Task<HttpResponseMessage> AvailableProduct(string productExternalId)
        {
            var url = string.Format(_productAvailableStatusPatchEndpoint, productExternalId);

            HttpResponseMessage response;

            using (var scope = _serviceProvider.CreateScope())
            {
                using (var client = scope.ServiceProvider.GetService<IHttpClientFactory>().CreateClient())
                {
                    var request = new HttpRequestMessage(new HttpMethod("PATCH"), url);

                    response = await client.SendAsync(request);
                }
            }

            return response;
        }

        public decimal CustomPriceAdjustment(decimal? price, string adjustmentValue)
        {
            if (!price.HasValue)
                return 0;

            if (!string.IsNullOrEmpty(adjustmentValue))
            {
                var adjustmentPrice = 0M;
                var adjustmentRatio = 0M;
                var adjustmentMode = string.Empty;

                var adjustment = adjustmentValue;

                var operation = "+";
                if (adjustment.IndexOf("+") != -1)
                {
                    adjustment = adjustment.Replace("+", "");

                    operation = "+";
                }
                else if (adjustment.IndexOf("-") != -1)
                {
                    adjustment = adjustment.Replace("-", "");

                    operation = "-";
                }

                if (adjustment.IndexOf("%") != -1)
                {
                    // Ratio adjustment mode

                    adjustmentMode = "ratio";

                    adjustment = adjustment.Replace("%", "");

                    adjustmentRatio = decimal.Parse(adjustment);
                }
                else
                {
                    // Price adjustment mode

                    adjustmentMode = "price";

                    adjustmentPrice = decimal.Parse(adjustment);
                }

                if (adjustmentMode == "ratio")
                {
                    if (operation == "+")
                    {
                        return price.Value + price.Value * adjustmentRatio / 100;
                    }
                    else
                    {
                        return price.Value - price.Value * adjustmentRatio / 100;
                    }
                }
                else if (adjustmentMode == "price")
                {
                    if (operation == "+")
                    {
                        return price.Value + adjustmentPrice;
                    }
                    else
                    {
                        return price.Value - adjustmentPrice;
                    }
                }
            }

            return price.Value;
        }

        public WebsiteMetadataModel WebsitePriceAdjustment(WebsiteMetadataModel metadata, decimal? price, string priceAdjustment)
        {
            var random = new Random();
            var newMetadata = new WebsiteMetadataModel();

            newMetadata.InStockQuantity = metadata.InStockQuantity;
            newMetadata.MinimumQuantity = metadata.MinimumQuantity;
            newMetadata.IsInStock = metadata.IsInStock;
            newMetadata.OldPrice = CustomPriceAdjustment(metadata.OldPrice * _fixedAdjustmentRatio, priceAdjustment);
            newMetadata.Price = CustomPriceAdjustment(metadata.Price * _fixedAdjustmentRatio, priceAdjustment);

            var tierPrices = metadata.WholesalePrices.OrderBy(x => x.Item1).ToList();
            if (tierPrices.Any())
            {
                var newTierPrices = new List<Tuple<int, decimal>>();
                foreach (var tierPrice in tierPrices)
                    newTierPrices.Add(new Tuple<int, decimal>(tierPrice.Item1, CustomPriceAdjustment(tierPrice.Item2, priceAdjustment)));
                newMetadata.WholesalePrices = newTierPrices;
            }

            return newMetadata;
        }

        public WebsiteMetadataModel WebsitePriceAdjustment(WebsiteMetadataModel metadata, string retailPriceAdjustment, string priceAdjustment)
        {
            var random = new Random();
            var newMetadata = new WebsiteMetadataModel();

            newMetadata.InStockQuantity = metadata.InStockQuantity;
            newMetadata.MinimumQuantity = metadata.MinimumQuantity;
            newMetadata.IsInStock = metadata.IsInStock;
            newMetadata.OldPrice = CustomPriceAdjustment(metadata.OldPrice * _fixedAdjustmentRatio, retailPriceAdjustment);
            newMetadata.Price = CustomPriceAdjustment(metadata.Price * _fixedAdjustmentRatio, retailPriceAdjustment);

            var tierPrices = metadata.WholesalePrices.OrderBy(x => x.Item1).ToList();
            if (tierPrices.Any())
            {
                var newTierPrices = new List<Tuple<int, decimal>>();
                foreach (var tierPrice in tierPrices)
                    newTierPrices.Add(new Tuple<int, decimal>(tierPrice.Item1, CustomPriceAdjustment(tierPrice.Item2, priceAdjustment)));
                newMetadata.WholesalePrices = newTierPrices;
            }

            return newMetadata;
        }
    }
}
