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
using System.IO;
using System.Linq;
using ProtoBuf.Meta;
using Newtonsoft.Json;
using Python.Runtime;
using NUnit.Framework;
using QuantConnect.Data;
using QuantConnect.DataSource;
using QuantConnect.Data.Custom;

namespace QuantConnect.DataLibrary.Tests
{
    [TestFixture]
    public class NasdaqDataLinkTests
    {
        [Test]
        public void JsonRoundTrip()
        {
            var expected = CreateNewInstance();
            var type = expected.GetType();
            var serialized = JsonConvert.SerializeObject(expected);
            var result = JsonConvert.DeserializeObject(serialized, type);

            AssertAreEqual(expected, result);
        }

        [Test]
        public void ProtobufRoundTrip()
        {
            var expected = CreateNewInstance();
            var type = expected.GetType();

            RuntimeTypeModel.Default[typeof(BaseData)].AddSubType(2000, type);

            using (var stream = new MemoryStream())
            {
                Serializer.Serialize(stream, expected);

                stream.Position = 0;

                var result = Serializer.Deserialize(type, stream);

                AssertAreEqual(expected, result, filterByCustomAttributes: true);
            }
        }

        [Test]
        public void Clone()
        {
            var expected = CreateNewInstance();
            var result = expected.Clone();

            AssertAreEqual(expected, result);
        }

        [Test]
        public void QuandlIdentical()
        {
            var new_ = CreateNewInstance();
            var old_ = CreateQuandlInstance();

            AssertAreEqual(new_, old_);
        }

        [Test]
        public void ValueColumn()
        {
            const int expected = 999;

            var newInstance = new IndexNasdaqDataLink();

            var symbol = Symbol.Create("UMICH/SOC1", 0, "empty");
            var config = new SubscriptionDataConfig(typeof(IndexNasdaqDataLink), symbol,
                Resolution.Daily, TimeZones.Utc, TimeZones.Utc, true, true, false, true);

            newInstance.Reader(config, "date,open,high,low,close,transactions,index", DateTime.UtcNow, false);
            var data = newInstance.Reader(config, $"2021-12-02,100,101,100,101,1000,{expected}", DateTime.UtcNow, false);

            Assert.AreEqual(expected, data.Value);
        }

        [TestCase("QDL/FON", "contract_code,type,date,market_participation,producer_merchant_processor_user_longs,producer_merchant_processor_user_shorts,swap_dealer_longs,swap_dealer_shorts,swap_dealer_spreads,money_manager_longs,money_manager_shorts,money_manager_spreads,other_reportable_longs,other_reportable_shorts,other_reportable_spreads,total_reportable_longs,total_reportable_shorts,non_reportable_longs,non_reportable_shorts", "967654,FO_OLD,2017-12-26,27122.0,9984.0,19225.0,1945.0,1405.0,1596.0,0.0,150.0,0.0,8316.0,597.0,1464.0,23305.0,24437.0,3817.0,2685.0", Description = "Commodity Futures Trading Commission Reports: Futures and Options Metrics: OI and NT")]
        [TestCase("QDL/LFON", "contract_code,type,date,market_participation,non_commercial_longs,non_commercial_shorts,non_commercial_spreads,commercial_longs,commercial_shorts,total_reportable_longs,total_reportable_shorts,non_reportable_longs,non_reportable_shorts", "ZB9105,FO_L_OLD_OI,2018-11-27,100.0,68.6,68.6,31.4,0.0,0.0,100.0,100.0,0.0,0.0", Description = "Commodity Futures Trading Commission Reports: Legacy Futures and Options Metrics: OI and NT")]
        [TestCase("QDL/FCR", "contract_code,type,date,largest_4_longs_gross,largest_4_shorts_gross,largest_8_longs_gross,largest_8_shorts_gross,largest_4_longs_net,largest_4_shorts_net,largest_8_longs_net,largest_8_shorts_net", "ZB9105,F_L_ALL_CR,2018-10-30,87.6,99.0,97.8,100.0,13.7,16.1,15.8,16.3", Description = "Commodity Futures Trading Commission Reports: Futures and Options Metrics: CR")]
        [TestCase("QDL/BCHAIN", "code,date,value", "TRFUS,2020-08-28,1197064.8298", Description = "Bitcoin Data Insights")]
        [TestCase("QDL/ODA", "indicator,date,value", "ZWE_PPPSH,2018-12-31,0.028", Description = "IMF Cross Country Macroeconomic Statistics")]
        [TestCase("QDL/ODA", "indicator,date,value", "ZWE_PPPSH,1997-12-31,", Description = "IMF Cross Country Macroeconomic Statistics")]
        [TestCase("QDL/JODI", "energy,code,country,date,value,notes", "OIL,TPSDKT,ZAF,2024-04-30,0.0000,3", Description = "JODI Oil World Database")]
        [TestCase("QDL/JODI", "energy,code,country,date,value,notes", "OIL,TPSDKT,TTO,2005-03-31,,3", Description = "JODI Oil World Database")]
        [TestCase("QDL/BITFINEX", "code,date,high,low,mid,last,bid,ask,volume", "ZRXUSD,2024-09-13,0.30349,0.28357,0.29886,0.29922,0.29865,0.29907,236649.194029", Description = "Bitfinex Crypto Coins Exchange Rate")]
        [TestCase("QDL/BITFINEX", "code,date,high,low,mid,last,bid,ask,volume", "ZRXBTC,2022-02-20,1.493e-05,1.448e-05,1.487e-05,1.489e-05,1.485e-05,1.489e-05,6907.80795766", Description = "Bitfinex Crypto Coins Exchange Rate")]
        [TestCase("QDL/OPEC", "date,value", "2024-01-12,80.18", Description = "Organization of the Petroleum Exporting Countries")]
        [TestCase("QDL/LME", "item_code,country_code,date,opening_stock,delivered_in,delivered_out,closing_stock,open_tonnage,cancelled_tonnage", "ZIJ,UTO,2024-07-12,0.0,0.0,0.0,0.0,0.0,0.0", Description = "Metal Stocks Breakdown Report")]
        [TestCase("QDL/LME", "item_code,country_code,date,opening_stock,delivered_in,delivered_out,closing_stock,open_tonnage,cancelled_tonnage", "ZII,UNE,2020-07-23,26075.0,0.0,0.0,26075.0,14425.0,11650.0", Description = "Metal Stocks Breakdown Report")]
        [TestCase("ZILLOW/DATA", "indicator_id,region_id,date,value", "ZSFH,99999,2024-04-30,481777.608668988", Description = "Zillow Real Estate Data")]
        [TestCase("ZILLOW/DATA", "indicator_id,region_id,date,value", "ZSFH,99993,2008-07-31,139908.0", Description = "Zillow Real Estate Data")]
        [TestCase("WB/DATA", "series_id,country_code,country_name,year,value", "VC.PKP.TOTL.UN,XKX,Kosovo,2017,357.0", Description = "World Bank Data")]
        [TestCase("WB/DATA", "series_id,country_code,country_name,year,value", "VC.IHR.PSRC.P5,ALB,Albania,1998,20.4196832752429", Description = "World Bank Data")]
        [TestCase("WASDE/DATA", "code,report_month,region,commodity,item,year,period,value,min_value,max_value", "WHEAT_WORLD_19,2024-02,World Less China,Wheat,Production,2023/24 Proj.,Jan,648.32,,", Description = "World Agricultural Supply and Demand Estimates")]
        [TestCase("WASDE/DATA", "code,report_month,region,commodity,item,year,period,value,min_value,max_value", "WHEAT_WORLD_19,2022-08,N. Africa 7/,Wheat,Production,2022/23 Proj.,Jul,17.15,,", Description = "World Agricultural Supply and Demand Estimates")]
        [TestCase("WASDE/DATA", "code,report_month,region,commodity,item,year,period,value,min_value,max_value", "WHEAT_WORLD_19,2021-05,Brazil,Wheat,Beginning Stocks,2021/22 Proj.,May,0.64,,", Description = "World Agricultural Supply and Demand Estimates")]
        public void CreateDifferentNasdaqDataSymbolWithVariousProperties(string nasdaqDataName, string csvHeader, string csvData)
        {
            var nasdaq = new NasdaqDataLink();

            var symbol = Symbol.Create(nasdaqDataName, SecurityType.Base, "empty");

            var config = new SubscriptionDataConfig(typeof(NasdaqDataLink), symbol, Resolution.Daily, TimeZones.Utc, TimeZones.Utc, true, true, false, true);

            var dateTimeUtcNow = DateTime.UtcNow;

            nasdaq.Reader(config, csvHeader, dateTimeUtcNow, false);
            var data = nasdaq.Reader(config, csvData, dateTimeUtcNow, false);

            Assert.That(data.Time, Is.Not.EqualTo(default));
            Assert.GreaterOrEqual(data.Value, 0m);
        }

        [Test]
        public void PythonValueColumn()
        {
            PythonEngine.Initialize();
            var expected = 999;
            dynamic instance;
            using (Py.GIL())
            {
                PyObject test = PyModule.FromString("testModule",
                    @"
from QuantConnect.DataSource import *

class Test(NasdaqDataLink):
    def __init__(self):
        super().__init__()
        self.ValueColumnName = 'adj. close'").GetAttr("Test");
                instance = test.CreateType().GetBaseDataInstance();
            }

            var symbol = Symbol.Create("UMICH/SOC1", 0, "empty");
            var config = new SubscriptionDataConfig(typeof(NasdaqDataLink), symbol,
                Resolution.Daily, TimeZones.Utc, TimeZones.Utc, true, true, false, true);

            instance.Reader(config, "date,open,high,low,close,transactions,adj. close", DateTime.UtcNow, false);
            var data = instance.Reader(config, $"2021-12-02,100,101,100,101,1000,{expected}", DateTime.UtcNow, false);

            Assert.AreEqual(expected, data.Value);
        }
		
		[Test]
        public void TwoPythonValueColumn()
        {
            PythonEngine.Initialize();
            var ibmExpected = 999;
            var spyExpected = 111;

            dynamic ibmInstance;
            dynamic spyInstance;

            using (Py.GIL())
            {
                PyObject module = PyModule.FromString("testModule",
                    @"
from QuantConnect.DataSource import *

class CustomIBM(NasdaqDataLink):
    def __init__(self):
        super().__init__()
        self.ValueColumnName = 'adj. close'

class CustomSPY(NasdaqDataLink):
    def __init__(self):
        super().__init__()
        self.ValueColumnName = 'adj. volume'");
                ibmInstance = module.GetAttr("CustomIBM").CreateType().GetBaseDataInstance();
                spyInstance = module.GetAttr("CustomSPY").CreateType().GetBaseDataInstance();
            }

            var ibm = Symbol.Create("IBM", 0, "empty");
            var spy = Symbol.Create("SPY", 0, "empty");
            var ibmConfig = new SubscriptionDataConfig(typeof(NasdaqDataLink), ibm,
                Resolution.Daily, TimeZones.Utc, TimeZones.Utc, true, true, false, true);
            var spyConfig = new SubscriptionDataConfig(typeof(NasdaqDataLink), spy,
                Resolution.Daily, TimeZones.Utc, TimeZones.Utc, true, true, false, true);

            ibmInstance.Reader(ibmConfig, "date,open,high,low,close,transactions,adj. close, adj. volume", DateTime.UtcNow, false);
            spyInstance.Reader(spyConfig, "date,open,high,low,close,transactions,adj. close, adj. volume", DateTime.UtcNow, false);
            var ibmData = ibmInstance.Reader(ibmConfig, $"2021-12-02,100,101,100,101,1000,{ibmExpected}, {spyExpected}", DateTime.UtcNow, false);
            var spyData = spyInstance.Reader(spyConfig, $"2021-12-02,100,101,100,101,1000,{ibmExpected}, {spyExpected}", DateTime.UtcNow, false);

            Assert.AreEqual(ibmExpected, ibmData.Value);
            Assert.AreEqual(spyExpected, spyData.Value);
        }

        private void AssertAreEqual(object expected, object result, bool filterByCustomAttributes = false)
        {
            foreach (var propertyInfo in expected.GetType().GetProperties())
            {
                // we skip Symbol which isn't protobuffed
                if (filterByCustomAttributes && propertyInfo.CustomAttributes.Count() != 0)
                {
                    Assert.AreEqual(propertyInfo.GetValue(expected), propertyInfo.GetValue(result));
                }
            }
            foreach (var fieldInfo in expected.GetType().GetFields())
            {
                Assert.AreEqual(fieldInfo.GetValue(expected), fieldInfo.GetValue(result));
            }
        }

        private BaseData CreateNewInstance()
        {
            return new NasdaqDataLink
            {
                Symbol = Symbol.Create("UMICH/SOC1", 0, "empty"),
                Time = new DateTime(2021, 9, 30),
                DataType = MarketDataType.Base,
                Value = 72.8m
            };
        }

        private BaseData CreateQuandlInstance()
        {
            return new Quandl
            {
                Symbol = Symbol.Create("UMICH/SOC1", 0, "empty"),
                Time = new DateTime(2021, 9, 30),
                DataType = MarketDataType.Base,
                Value = 72.8m
            };
        }

        public class IndexNasdaqDataLink : NasdaqDataLink
        {
            public IndexNasdaqDataLink() : base("index")
            {
            }
        }
    }
}
