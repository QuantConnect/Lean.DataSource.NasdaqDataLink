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

using System;
using QuantConnect.Algorithm;

namespace QuantConnect.DataSource
{
    /// <summary>
    /// Futures demonstration algorithm.
    /// QuantConnect allows importing generic data sources! This example demonstrates importing a futures
    /// data from the popular open data source Nasdaq. QuantConnect has a special deal with Nasdaq giving you access
    /// to Stevens Continuous Futurs (SCF) for free. If you'd like to download SCF for local backtesting, you can download it through data.nasdaq.com.
    /// </summary>
    /// <meta name="tag" content="using data" />
    /// <meta name="tag" content="nasdaq" />
    /// <meta name="tag" content="custom data" />
    /// <meta name="tag" content="futures" />
    public class NasdaqFuturesDataAlgorithm : QCAlgorithm
    {
        private string _crude = "SHFE/SCF2021";

        /// <summary>
        /// Initialize the data and resolution you require for your strategy
        /// </summary>
        public override void Initialize()
        {
            SetStartDate(2019, 10, 1);
            SetEndDate(2020, 12, 31);
            SetCash(25000);

            //NasdaqDataLink.SetAuthCode("your-nasdaq-token");
            AddData<NasdaqFuture>(_crude, Resolution.Daily);
        }

        /// <summary>
        /// Data Event Handler: New data arrives here. "TradeBars" type is a dictionary of strings so you can access it by symbol.
        /// </summary>
        /// <param name="data">Data.</param>
        public void OnData(NasdaqDataLink data)
        {
            if (!Portfolio.HoldStock)
            {
                SetHoldings(_crude, 1);
                Debug(Time.ToStringInvariant("u") + " Purchased Crude Oil: " + _crude);
            }
        }

        /// <summary>
        /// Custom nasdaq data type for setting customized value column name. Value column is used for the primary trading calculations and charting.
        /// </summary>
        public class NasdaqFuture : NasdaqDataLink
        {
            /// <summary>
            /// Initializes a new instance of the <see cref="NasdaqFuture"/> class.
            /// </summary>
            public NasdaqFuture()
                : base(valueColumnName: "Settle")
            {
            }
        }
    }
}
