﻿/*
 * QUANTCONNECT.COM - Democratizing Finance, Empowering Individuals.
 * Lean Algorithmic Trading Engine v2.0. Copyright 2014 QuantConnect Corporation.
 *
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
*/

using Newtonsoft.Json;
using QuantConnect.Data.UniverseSelection;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;

namespace QuantConnect.Data.Custom.Estimize
{
    /// <summary>
    /// Financial estimates for the specified company
    /// </summary>
    public class EstimizeEstimate : BaseData
    {
        /// <summary>
        /// The unique identifier for the estimate
        /// </summary>
        [JsonProperty(PropertyName = "id")]
        public string Id { get; set; }

        /// <summary>
        /// The ticker of the company being estimated
        /// </summary>
        [JsonProperty(PropertyName = "ticker")]
        public string Ticker { get; set; }

        /// <summary>
        /// The fiscal year of the quarter being estimated
        /// </summary>
        [JsonProperty(PropertyName = "fiscal_year")]
        public int FiscalYear { get; set; }

        /// <summary>
        /// The fiscal quarter of the quarter being estimated
        /// </summary>
        [JsonProperty(PropertyName = "fiscal_quarter")]
        public int FiscalQuarter { get; set; }

        /// <summary>
        /// The time that the estimate was created (UTC)
        /// </summary>
        [JsonProperty(PropertyName = "created_at")]
        public DateTime CreatedAt { get; set; }

        /// <summary>
        /// The time that the estimate was created (UTC)
        /// </summary>
        public override DateTime EndTime => CreatedAt;

        /// <summary>
        /// The estimated earnings per share for the company in the specified fiscal quarter
        /// </summary>
        [JsonProperty(PropertyName = "eps")]
        public decimal? Eps { get; set; }

        /// <summary>
        /// The estimated earnings per share for the company in the specified fiscal quarter
        /// </summary>
        public override decimal Value => Eps ?? 0m;

        /// <summary>
        /// The estimated revenue for the company in the specified fiscal quarter
        /// </summary>
        [JsonProperty(PropertyName = "revenue")]
        public decimal? Revenue { get; set; }

        /// <summary>
        /// The unique identifier for the author of the estimate
        /// </summary>
        [JsonProperty(PropertyName = "username")]
        public string UserName { get; set; }

        /// <summary>
        /// The author of the estimate
        /// </summary>
        [JsonProperty(PropertyName = "analyst_id")]
        public string AnalystId { get; set; }

        /// <summary>
        /// A boolean value which indicates whether we have flagged this estimate internally as erroneous
        /// (spam, wrong accounting standard, etc)
        /// </summary>
        [JsonProperty(PropertyName = "flagged")]
        public bool Flagged { get; set; }

        /// <summary>
        /// Required for successful Json.NET deserialization
        /// </summary>
        public EstimizeEstimate()
        {
        }

        /// <summary>
        /// Creates a new instance of EstimizeEstimate from a CSV line
        /// </summary>
        /// <param name="csvLine">CSV line</param>
        public EstimizeEstimate(string csvLine)
        {
            // CreatedAt[0], Id[1], AnalystId[2], UserName[3], FiscalYear[4], FiscalQuarter[5], Eps[6], Revenue[7], Flagged[8]"
            var csv = csvLine.Split(',');

            CreatedAt = DateTime.ParseExact(csv[0], "yyyyMMdd HH:mm:ss", CultureInfo.InvariantCulture);
            Id = csv[1];
            AnalystId = csv[2];
            UserName = csv[3];
            FiscalYear = Convert.ToInt32(csv[4], CultureInfo.InvariantCulture);
            FiscalQuarter = Convert.ToInt32(csv[5], CultureInfo.InvariantCulture);
            Eps = string.IsNullOrWhiteSpace(csv[6]) ? (decimal?)null : Convert.ToDecimal(csv[6], CultureInfo.InvariantCulture);
            Revenue =  string.IsNullOrWhiteSpace(csv[7]) ? (decimal?)null : Convert.ToDecimal(csv[7], CultureInfo.InvariantCulture);
            Flagged = Convert.ToBoolean(csv[8]);
        }

        /// <summary>
        /// Return the Subscription Data Source gained from the URL
        /// </summary>
        /// <param name="config">Configuration object</param>
        /// <param name="date">Date of this source file</param>
        /// <param name="isLiveMode">true if we're in live mode, false for backtesting mode</param>
        /// <returns>Subscription Data Source.</returns>
        public override SubscriptionDataSource GetSource(SubscriptionDataConfig config, DateTime date, bool isLiveMode)
        {
            if (!config.Symbol.Value.EndsWith(".E"))
            {
                throw new ArgumentException($"EstimizeEstimate.GetSource(): Invalid symbol {config.Symbol}");
            }

            var symbol = config.Symbol.Value;
            symbol = symbol.Substring(0, symbol.Length - 2);

            var source = Path.Combine(
                Globals.DataFolder,
                "alternative",
                "estimize",
                "estimate",
                $"{symbol.ToLower()}.csv"
            );
            return new SubscriptionDataSource(source, SubscriptionTransportMedium.LocalFile, FileFormat.Csv);
        }

        /// <summary>
        /// Reader converts each line of the data source into BaseData objects.
        /// </summary>
        /// <param name="config">Subscription data config setup object</param>
        /// <param name="line">Content of the source document</param>
        /// <param name="date">Date of the requested data</param>
        /// <param name="isLiveMode">true if we're in live mode, false for backtesting mode</param>
        /// <returns>
        /// Estimize Estimate object
        /// </returns>
        public override BaseData Reader(SubscriptionDataConfig config, string line, DateTime date, bool isLiveMode)
        {
            return new EstimizeEstimate(line)
            {
                Symbol = config.Symbol
            };
        }

        /// <summary>
        /// Formats a string with the Estimize Estimate information.
        /// </summary>
        public override string ToString()
        {
            return $"{Ticker}(Q{FiscalQuarter} {FiscalYear}) :: EPS: {Eps} Revenue: {Revenue} on {EndTime:yyyyMMdd} by {UserName}({AnalystId})";
        }
    }
}