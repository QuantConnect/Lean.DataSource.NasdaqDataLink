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
### Using the underlying dynamic data class "NasdaqDataLink" QuantConnect take care of the data
### importing and definition for you. Simply point QuantConnect to the NasdaqDataLink Short Code.
### The NasdaqDataLink object has properties which match the spreadsheet headers.
### If you have multiple nasdaq streams look at data.Symbol to distinguish them.
### </summary>
### <meta name="tag" content="custom data" />
### <meta name="tag" content="using data" />
### <meta name="tag" content="nasdaq" />
class NasdaqImporterAlgorithm(QCAlgorithm):

    def Initialize(self):
        '''Initialise the data and resolution required, as well as the cash and start-end dates for your algorithm. All algorithms must initialized.'''
        self.nasdaqCode = "WIKI/IBM"
        ## Optional argument - personal token necessary for restricted dataset
        # NasdaqDataLink.SetAuthCode("your-nasdaq-token")
        self.SetStartDate(2014,4,1)                                 #Set Start Date
        self.SetEndDate(datetime.today() - timedelta(1))            #Set End Date
        self.SetCash(25000)                                         #Set Strategy Cash
        self.AddData(NasdaqCustomColumns, self.nasdaqCode, Resolution.Daily, TimeZones.NewYork)
        self.sma = self.SMA(self.nasdaqCode, 14)

    def OnData(self, data):
        '''OnData event is the primary entry point for your algorithm. Each new data point will be pumped in here.'''
        if not self.Portfolio.HoldStock:
            self.SetHoldings(self.nasdaqCode, 1)
            self.Debug("Purchased {0} >> {1}".format(self.nasdaqCode, self.Time))

        self.Plot(self.nasdaqCode, "PriceSMA", self.sma.Current.Value)

# NasdaqDataLink often doesn't use close columns so need to tell LEAN which is the "value" column.
class NasdaqkCustomColumns(PythonNasdaq):
    '''Custom nasdaq data type for setting customized value column name. Value column is used for the primary trading calculations and charting.'''
    def __init__(self):
        # Define ValueColumnName: cannot be None, Empty or non-existant column name
        self.ValueColumnName = "adj. close"