﻿using Newtonsoft.Json;
using System.Collections.Generic;
using System.Threading.Tasks;
using static Pineapple.Common.Preconditions;

namespace Polygon.Net
{
    public partial class PolygonClient
    {
        private const string TICKERS_ENDPOINT = "/vX/reference/tickers";
        private const string TICKERS_ENDPOINT_V1 = "/v1/meta/symbols";

        private const string EXCHANGES_ENDPOINT = "/v1/meta/exchanges";

        private const string FINANCIALS_ENDPOINT = "/v2/reference/financials";

        public async Task<TickersResponse> GetTickersAsync(
            string ticker = null,
            string tickerlt = null,
            string tickerlte = null,
            string tickergt = null,
            string tickergte = null,
            string type = null,
            string market = null,
            string exchange = null,
            string cusip = null,
            string date = null,
            bool? active = null,
            string sort = null,
            string order = null,
            int? limit = null,
            string nextUrl = null, 
            bool expandAbbreviations = false)
        {
            var queryParams = new Dictionary<string, string>
            {
                { nameof(ticker), ticker },
                { "ticker.lt", tickerlt },
                { "ticker.lte", tickerlte },
                { "ticker.gt", tickergt },
                { "ticker.gte", tickergte },
                { nameof(type), type },
                { nameof(market), market },
                { nameof(exchange), exchange },
                { nameof(cusip), cusip },
                { nameof(date), FormatDateString(date) },
                { nameof(active), active?.ToString() },
                { nameof(sort), sort },
                { nameof(order), order },
                { nameof(limit), limit?.ToString() },
            };

            string requestUrl;
            if(nextUrl != null)
            {
                requestUrl = nextUrl;
            }
            else
            {
                var queryParamStr = GetQueryParameterString(queryParams);
                requestUrl = $"{ _polygonSettings.ApiBaseUrl }{ TICKERS_ENDPOINT }{ queryParamStr }";
            }
            
            var contentStr = await Get(requestUrl).ConfigureAwait(false);

            var tickers = JsonConvert.DeserializeObject<TickersResponse>(contentStr);

            if (!expandAbbreviations) return tickers;
            
            for ( var i = 0; i < tickers.Results.Count; i++ )
            {
                tickers.Results[i] = _mapper.Map<TickerInfo>(tickers.Results[i]);
            }

            return tickers;
        }

        public async Task<TickerDetailsResponse> GetTickerDetailsAsync(string ticker, string date = null, bool expandAbbreviations = false)
        {
            CheckIsNotNullOrWhitespace(nameof(ticker), ticker);

            var requestUrl = $"{ _polygonSettings.ApiBaseUrl }{ TICKERS_ENDPOINT }/{ ticker }";

            if (date != null)
            {
                requestUrl += $"?date={ FormatDateString(date) }";
            }

            var contentStr = await Get(requestUrl).ConfigureAwait(false);

            var details = JsonConvert.DeserializeObject<TickerDetailsResponse>(contentStr);

            if (details != null && expandAbbreviations)
                details.Results = _mapper.Map<TickerDetailsInfo>(details.Results);

            return details;
        }
        
        public async Task<TickerDetailsInfoV1> GetTickerDetailsV1Async(string stocksTicker, bool expandAbbreviations = false)
        {
            CheckIsNotNullOrWhitespace(nameof(stocksTicker), stocksTicker);

            var requestUrl = $"{ _polygonSettings.ApiBaseUrl }{ TICKERS_ENDPOINT_V1 }/{ stocksTicker }/company";

            var contentStr = await Get(requestUrl).ConfigureAwait(false);

            var details = JsonConvert.DeserializeObject<TickerDetailsInfoV1>(contentStr);

            return expandAbbreviations ? _mapper.Map<TickerDetailsInfoV1>(details) : details;
        }

        public async Task<List<ExchangeInfo>> GetStockExchangesAsync()
        {
            var requestUrl = $"{ _polygonSettings.ApiBaseUrl }{ EXCHANGES_ENDPOINT }";

            var contentStr = await Get(requestUrl).ConfigureAwait(false);

            return JsonConvert.DeserializeObject<List<ExchangeInfo>>(contentStr);
        }

        public async Task<StockFinancialsResponse> GetStockFinancialsAsync(
            string stocksTicker,
            string type = null,
            string sort = null,
            int? limit = null,
            string nextUrl = null)
        {
            CheckIsNotNullOrWhitespace(nameof(stocksTicker), stocksTicker);

            var queryParams = new Dictionary<string, string>
            {
                { nameof(type), type },
                { nameof(sort), sort },
                { nameof(limit), limit?.ToString() },
            };

            string requestUrl;
            if (nextUrl != null)
            {
                requestUrl = nextUrl;
            }
            else
            {
                var queryParamStr = GetQueryParameterString(queryParams);
                requestUrl = $"{ _polygonSettings.ApiBaseUrl }{ FINANCIALS_ENDPOINT }/{ stocksTicker }{ queryParamStr }";
            }

            var contentStr = await Get(requestUrl).ConfigureAwait(false);

            return JsonConvert.DeserializeObject<StockFinancialsResponse>(contentStr);
        }
    }
}