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
using ProtoBuf;
using NodaTime;
using System.Net;
using System.Linq;
using QuantConnect.Data;
using QuantConnect.Logging;
using System.Globalization;
using System.Collections.Generic;
using QuantConnect.Configuration;

namespace QuantConnect.DataSource
{
    /// <summary>
    /// Nasdaq Data Link dataset
    /// </summary>
    [ProtoContract(SkipConstructor = true)]
    public class NasdaqDataLink : DynamicData
    {
        private static string _authCode = "your_api_key";
        private bool _isInitialized;

        /// <summary>
        /// Stores the index of the "date" field in the CSV file headers.
        /// </summary>
        private int _indexDateTime;

        /// <summary>
        /// Stores the date format string used for parsing date values.
        /// The format is set dynamically based on the property name (e.g., "yyyy", "yyyy-MM", "yyyy-MM-dd").
        /// </summary>
        private string parsingFormatDateTime;

        private readonly List<string> _propertyNames = new List<string>();

        // The NasdaqDataLink will use one of these column names if they are available and another option is not provided
        private readonly List<string> _keywords = new List<string> { "close", "price", "settle", "value" };

        /// <summary>
        /// Name of the column is going to be used for the field Value
        /// </summary>
        /// <remarks>This field will be set in the Python class constructor
        /// which inherits from NasdaqDataLink. It was made to allow the user to
        /// set a specified column to be used as a value when working in Python.</remarks>
        protected string ValueColumnName
        {
            set => SetValueColumnName(value);
        }

        /// <summary>
        /// Static constructor for the <see cref="NasdaqDataLink"/> class
        /// </summary>
        static NasdaqDataLink()
        {
            // The NasdaqDataLink API now requires TLS 1.2 for API requests (since 9/18/2018).
            // NET 4.5.2 and below does not enable this more secure protocol by default, so we add it in here
            ServicePointManager.SecurityProtocol |= SecurityProtocolType.Tls12;

            // Set the authentication token in NasdaqDataLink if it is set in Config
            var potentialNasdaqToken = Config.Get("nasdaq-auth-token");

            if (!string.IsNullOrEmpty(potentialNasdaqToken))
            {
                SetAuthCode(potentialNasdaqToken);
            }
            else
            {
                var potentialQuandlToken = Config.Get("quandl-auth-token");

                if (!string.IsNullOrEmpty(potentialQuandlToken))
                {
                    SetAuthCode(potentialQuandlToken);
                    Log.Error("NasdaqDataLink(): 'quandl-auth-token' is obsolete please use 'nasdaq-auth-token' instead.");
                }
            }
        }

        /// <summary>
        /// Default <see cref="NasdaqDataLink"/> constructor uses Close as its value column
        /// </summary>
        public NasdaqDataLink()
        {
        }

        /// <summary>
        /// Constructor for creating customized <see cref="NasdaqDataLink"/> instance which doesn't use close, price, settle or value as its value item.
        /// </summary>
        /// <param name="valueColumnName">The name of the column we want to use as reference, the Value property</param>
        protected NasdaqDataLink(string valueColumnName)
        {
            SetValueColumnName(valueColumnName);
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
        /// Using the Nasdaq Data Link V3 API automatically set the URL for the dataset.
        /// </summary>
        /// <param name="config">Subscription configuration object</param>
        /// <param name="date">Date of the data file we're looking for</param>
        /// <param name="isLiveMode">true if we're in live mode, false for backtesting mode</param>
        /// <returns>STRING API Url for Nasdaq Data Link.</returns>
        public override SubscriptionDataSource GetSource(SubscriptionDataConfig config, DateTime date, bool isLiveMode)
        {
            var source = $"https://data.nasdaq.com/api/v3/datatables/{config.Symbol.Value}.csv?api_key={_authCode}";
            return new SubscriptionDataSource(source, SubscriptionTransportMedium.RemoteFile) { Sort = true };
        }

        /// <summary>
        /// Parses the data from the line provided and loads it into LEAN
        /// </summary>
        /// <param name="config">Subscription configuration</param>
        /// <param name="line">CSV line of data from the souce</param>
        /// <param name="date">Date of the requested line</param>
        /// <param name="isLiveMode">Is live mode</param>
        /// <returns>New instance</returns>
        public override BaseData Reader(SubscriptionDataConfig config, string line, DateTime date, bool isLiveMode)
        {
            // be sure to instantiate the correct type
            var data = (NasdaqDataLink)Activator.CreateInstance(GetType());
            data.Symbol = config.Symbol;
            var csv = line.Split(',');

            if (!_isInitialized)
            {
                _isInitialized = true;

                for (int i = 0; i < csv.Length; i++)
                {
                    var propertyName = csv[i];

                    if (_indexDateTime == 0)
                    {
                        switch (propertyName.ToLower())
                        {
                            case "date":
                                parsingFormatDateTime = "yyyy-MM-dd";
                                _indexDateTime = i;
                                break;
                            case "year":
                                parsingFormatDateTime = "yyyy";
                                _indexDateTime = i;
                                break;
                            case "report_month":
                                parsingFormatDateTime = "yyyy-MM";
                                _indexDateTime = i;
                                break;
                        }
                    }

                    var property = propertyName.Trim().ToLowerInvariant();
                    data.SetProperty(property, 0m);
                    _propertyNames.Add(property);
                }


                // Returns null at this point where we are only reading the properties names
                return null;
            }

            for (var i = 0; i < csv.Length; i++)
            {
                if (string.IsNullOrEmpty(csv[i]))
                {
                    continue;
                }

                if (i == _indexDateTime)
                {
                    data.Time = DateTime.ParseExact(csv[_indexDateTime], parsingFormatDateTime, CultureInfo.InvariantCulture);
                    data.SetProperty(_propertyNames[i], data.Time);
                }
                else if (decimal.TryParse(csv[i], NumberStyles.AllowExponent | NumberStyles.Float, CultureInfo.InvariantCulture, out var value))
                {
                    data.SetProperty(_propertyNames[i], value);
                }
                else
                {
                    data.SetProperty(_propertyNames[i], csv[i]);
                }
            }

            var valueColumnName = _keywords.Intersect(_propertyNames).FirstOrDefault();

            if (valueColumnName != null)
            {
                // If the dataset has any column matches the keywords, set .Value as the first common element with it/them
                data.Value = (decimal)data.GetProperty(valueColumnName);
            }

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
            return AllResolutions;
        }

        /// <summary>
        /// Specifies the data time zone for this data type. This is useful for custom data types
        /// </summary>
        /// <returns>The <see cref="T:NodaTime.DateTimeZone" /> of this data type</returns>
        public override DateTimeZone DataTimeZone()
        {
            return DateTimeZone.Utc;
        }

        /// <summary>
        /// The end time of this data. Some data covers spans (trade bars) and as such we want
        /// to know the entire time span covered
        /// </summary>
        public override DateTime EndTime
        {
            get { return Time + Period; }
            set { Time = value - Period; }
        }

        /// <summary>
        /// Gets a time span of one day
        /// </summary>
        public TimeSpan Period
        {
            get { return QuantConnect.Time.OneDay; }
        }

        /// <summary>
        /// Inserts the name of the column at first position in _keywords list
        /// </summary>
        /// <param name="valueColumnName">Name of the column to be used as Value</param>
        private void SetValueColumnName(string valueColumnName)
        {
            valueColumnName = valueColumnName.Trim().ToLowerInvariant();

            if (string.IsNullOrWhiteSpace(valueColumnName)) return;

            // Insert the value column name at the beginning of the keywords list
            _keywords.Insert(0, valueColumnName);
        }
    }
}
