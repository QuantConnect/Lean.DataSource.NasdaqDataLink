# QUANTCONNECT.COM - Democratizing Finance, Empowering Individuals.
# Lean Algorithmic Trading Engine v2.0. Copyright 2014 QuantConnect Corporation.
#
# Licensed under the Apache License, Version 2.0 (the "License");
# you may not use this file except in compliance with the License.
# You may obtain a copy of the License at http://www.apache.org/licenses/LICENSE-2.0
#
# Unless required by applicable law or agreed to in writing, software
# distributed under the License is distributed on an "AS IS" BASIS,
# WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
# See the License for the specific language governing permissions and
# limitations under the License.

from AlgorithmImports import *

### <summary>
### Futures demonstration algorithm.
### QuantConnect allows importing generic data sources! This example demonstrates importing a futures
### data from the popular open data source NasdaqDataLink. QuantConnect has a special deal with NasdaqDataLink giving you access
### to Stevens Continuous Futurs (SCF) for free. If you'd like to download SCF for local backtesting, you can download it through data.nasdaq.com.
### </summary>
### <meta name="tag" content="using data" />
### <meta name="tag" content="nasdaq" />
### <meta name="tag" content="custom data" />
### <meta name="tag" content="futures" />
class NasdaqFuturesDataAlgorithm(QCAlgorithm):

    def Initialize(self):
        ''' Initialize the data and resolution you require for your strategy '''
        self.SetStartDate(2019, 10, 1)
        self.SetEndDate(2020, 12, 31)
        self.SetCash(25000)

        # NasdaqDataLink.SetAuthCode("your-nasdaq-token")
        # Symbol corresponding to the nasdaq code
        self.crude = "SHFE/SCF2021"
        self.AddData(NasdaqFuture, self.crude, Resolution.Daily)


    def OnData(self, data):
        '''Data Event Handler: New data arrives here. "TradeBars" type is a dictionary of strings so you can access it by symbol.'''
        if self.Portfolio.HoldStock: return

        self.SetHoldings(self.crude, 1)
        self.Debug(str(self.Time) + str(" Purchased Crude Oil: ") + self.crude)


class NasdaqFuture(PythonNasdaq):
    '''Custom nasdaq data type for setting customized value column name. Value column is used for the primary trading calculations and charting.'''
    def __init__(self):
        # Define ValueColumnName: cannot be None, Empty or non-existant column name
        # If ValueColumnName is "Close", do not use PythonNasdaq, use NasdaqDataLink:
        # self.AddData[NasdaqDataLinkFuture](self.crude, Resolution.Daily)
        self.ValueColumnName = "Settle"
