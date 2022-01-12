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
*/

using QuantConnect.Indicators;

namespace QuantConnect.DataSource
{
    /// <summary>
    /// The algorithm creates new indicator value with the existing indicator method by Indicator Extensions
    /// Demonstration of using the external custom datasource Nasdaq to request the IBM and FB daily data
    /// </summary>
    /// <meta name="tag" content="using data" />
    /// <meta name="tag" content="using quantconnect" />
    /// <meta name="tag" content="custom data" />
    /// <meta name="tag" content="indicators" />
    /// <meta name="tag" content="indicator classes" />
    /// <meta name="tag" content="plotting indicators" />
    /// <meta name="tag" content="charting" />
    public class DemostrationWithIndicator : QCAlgorithm
    {
        private const string _ibm = "WIKI/IBM";
        private const string _fb = "WIKI/FB";
        private SimpleMovingAverage _smaIBM;
        private SimpleMovingAverage _smaFB;
        private IndicatorBase<IndicatorDataPoint> _ratio;

        /// <summary>
        /// Initialize the data and resolution you require for your strategy
        /// </summary>
        public override void Initialize()
        {
            SetStartDate(2014, 1, 1);
            SetEndDate(2018, 1, 1);
            SetCash(25000);

            // NasdaqDataLink.SetAuthCode("your-api-key");
            // Define the symbol and "type" of our generic data
            AddData<NasdaqDataLink>(_ibm, Resolution.Daily);
            AddData<NasdaqCustomColumns>(_fb, Resolution.Daily);
            // Set up default Indicators, these are just 'identities' of the closing price
            _smaIBM = SMA(_ibm, 1);
            _smaFB = SMA(_fb, 1);
            // This will create a new indicator whose value is smaFB / smaIBM
            _ratio = _smaFB.Over(_smaIBM);
        }

        /// <summary>
        /// Custom data event handler:
        /// </summary>
        /// <param name="data">NasdaqDataLink - dictionary Bars of Nasdaq Data</param>
        public void OnData(NasdaqDataLink data)
        {
            // Wait for all indicators to fully initialize
            if (_smaIBM.IsReady && _smaFB.IsReady && _ratio.IsReady)
            {
                if (!Portfolio.Invested && _ratio > 1)
                {
                    MarketOrder(_ibm, 100);
                }
                else if (_ratio < 1)
                {
                    Liquidate();
                }
                // plot all indicators
                PlotIndicator("SMA", _smaIBM, _smaFB);
                PlotIndicator("Ratio", _ratio);
            }
        }
    }

    /// <summary>
    /// This class assigns new column name to match the the external datasource setting.
    /// </summary>
    public class NasdaqCustomColumns : NasdaqDataLink
    {
        public NasdaqCustomColumns() : base(valueColumnName: "adj. close") { }
    }
}
