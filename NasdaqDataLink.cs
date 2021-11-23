/*
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
 *
*/

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Net;
using NodaTime;
using ProtoBuf;
using System.IO;
using QuantConnect.Data;

namespace QuantConnect.DataSource
{
    /// <summary>
    /// Nasdaq Data Link dataset
    /// </summary>
    [ProtoContract(SkipConstructor = true)]
    public class NasdaqDataLink : DynamicData
    {
        private bool _isInitialized;
        private readonly List<string> _propertyNames = new List<string>();
        private readonly string _valueColumn;
        private static string _authCode = "your_api_key";

        /// <summary>
        /// Static constructor for the <see cref="NasdaqDataLink"/> class
        /// </summary>
        static NasdaqDataLink()
        {
            // The NasdaqDataLink API now requires TLS 1.2 for API requests (since 9/18/2018).
            // NET 4.5.2 and below does not enable this more secure protocol by default, so we add it in here
            ServicePointManager.SecurityProtocol |= SecurityProtocolType.Tls12;
        }
        
        /// <summary>
        /// Flag indicating whether or not the Nasdaq Data Link auth code has been set yet
        /// </summary>
        public static bool IsAuthCodeSet
        {
            get;
            private set;
        }
        
        /// <summary>
        /// Default NasdaqDataLink constructor uses Close as its value column
        /// </summary>
        public NasdaqDataLink() : this("Close")
        {
        }

        /// <summary>
        /// Constructor for creating customized NasdaqDataLink instance which doesn't use "Close" as its value item.
        /// </summary>
        /// <param name="valueColumnName"></param>
        protected NasdaqDataLink(string valueColumnName)
        {
            _valueColumn = valueColumnName;
        }

        /// <summary>
        /// Using the Nasdaq Data Link V3 API automatically set the URL for the dataset.
        /// </summary>
        /// <param name="config">Subscription configuration object</param>
        /// <param name="date">Date of the data file we're looking for</param>
        /// <param name="isLiveMode">true if we're in live mode, false for backtesting mode</param>
        /// <returns>STRING API Url for Nasdaq Data Link.</returns>
        public override SubscriptionDataSource GetSource(SubscriptionDataConfig config, DateTime date, bool isLiveMode)
        {
            var source = $"https://data.nasdaq.com/api/v3/datasets/{config.Symbol.Value}.csv?order=asc&api_key={_authCode}";
            return new SubscriptionDataSource(source, SubscriptionTransportMedium.RemoteFile);
        }

        /// <summary>
        /// Parses the data from the line provided and loads it into LEAN
        /// </summary>
        /// <param name="config">Subscription configuration</param>
        /// <param name="line">Line of data</param>
        /// <param name="date">Date</param>
        /// <param name="isLiveMode">Is live mode</param>
        /// <returns>New instance</returns>
        public override BaseData Reader(SubscriptionDataConfig config, string line, DateTime date, bool isLiveMode)
        {
            // be sure to instantiate the correct type
            var data = (NasdaqDataLink) Activator.CreateInstance(GetType());
            data.Symbol = config.Symbol;
            var csv = line.Split(',');

            if (!_isInitialized)
            {
                _isInitialized = true;
                foreach (var propertyName in csv)
                {
                    var property = propertyName.Trim();     // Trim white space in property's name
                    data.SetProperty(property, 0m);
                    _propertyNames.Add(property);
                }
                // Returns null at this point where we are only reading the properties names
                return null;
            }

            data.Time = DateTime.ParseExact(csv[0], "yyyy-MM-dd", CultureInfo.InvariantCulture);
            data.EndTime = data.Time + TimeSpan.FromDays(1);

            for (var i = 1; i < csv.Length; i++)
            {
                var value = csv[i].ToDecimal();
                data.SetProperty(_propertyNames[i], value);
            }

            // Let's set Value to be the second column
            data.Value = (decimal)data.GetProperty(_propertyNames[1]);
            
            /*
            // We can also set Value property as a `close` property by
            data.Value = (decimal)data.GetProperty(_valueColumn);
            */

            return data;
        }

        /// <summary>
        /// Set the auth code for the Nasdaq Data Link set to the QuantConnect auth code.
        /// </summary>
        /// <param name="authCode"></param>
        public static void SetAuthCode(string authCode)
        {
            if (string.IsNullOrWhiteSpace(authCode)) return;

            _authCode = authCode;
            IsAuthCodeSet = true;
        }

        /// <summary>
        /// Indicates whether the data source is tied to an underlying symbol and requires that corporate events be applied to it as well, such as renames and delistings
        /// </summary>
        /// <returns>false</returns>
        public override bool RequiresMapping()
        {
            return false;
        }

        /// <summary>
        /// Indicates whether the data is sparse.
        /// If true, we disable logging for missing files
        /// </summary>
        /// <returns>true</returns>
        public override bool IsSparseData()
        {
            return true;
        }

        /// <summary>
        /// Gets the default resolution for this data and security type
        /// </summary>
        public override Resolution DefaultResolution()
        {
            return Resolution.Daily;
        }

        /// <summary>
        /// Gets the supported resolution for this data and security type
        /// </summary>
        public override List<Resolution> SupportedResolutions()
        {
            return DailyResolution;
        }

        /// <summary>
        /// Specifies the data time zone for this data type. This is useful for custom data types
        /// </summary>
        /// <returns>The <see cref="T:NodaTime.DateTimeZone" /> of this data type</returns>
        public override DateTimeZone DataTimeZone()
        {
            return DateTimeZone.Utc;
        }
    }
}