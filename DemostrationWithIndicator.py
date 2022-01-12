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
### The algorithm creates new indicator value with the existing indicator method by Indicator Extensions
### Demonstration of using the external custom Nasdaq data to request the IBM and FB daily data
### </summary>
### <meta name="tag" content="using data" />
### <meta name="tag" content="using quantconnect" />
### <meta name="tag" content="custom data" />
### <meta name="tag" content="indicators" />
### <meta name="tag" content="indicator classes" />
### <meta name="tag" content="plotting indicators" />
### <meta name="tag" content="charting" />
class DemostrationWithIndicator(QCAlgorithm):

    # Initialize the data and resolution you require for your strategy
    def Initialize(self):

        self.SetStartDate(2014,1,1) 
        self.SetEndDate(2018,1,1)  
        self.SetCash(25000)
        
        self.ibm = 'WIKI/IBM'
        self.fb = 'WIKI/FB'
        
        # NasdaqDataLink.SetAuthCode("your-api-key")
        # Define the symbol and "type" of our generic data
        self.AddData(NasdaqDataLink, self.ibm, Resolution.Daily)
        self.AddData(NasdaqCustomColumns, self.fb, Resolution.Daily)
        
        # Set up default Indicators, these are just 'identities' of the closing price
        self.ibm_sma = self.SMA(self.ibm, 1, Resolution.Daily)
        self.fb_sma = self.SMA(self.fb, 1, Resolution.Daily)
        
        # This will create a new indicator whose value is fb_sma / ibm_sma
        self.ratio = IndicatorExtensions.Over(self.fb_sma, self.ibm_sma)
        
        # Plot indicators each time they update using the PlotIndicator function
        self.PlotIndicator("Ratio", self.ratio)
        self.PlotIndicator("Data", self.ibm_sma, self.fb_sma)
    
    # OnData event is the primary entry point for your algorithm. Each new data point will be pumped in here.
    def OnData(self, data):
        # Wait for all indicators to fully initialize
        if not (self.ibm_sma.IsReady and self.fb_sma.IsReady and self.ratio.IsReady): return
        if not self.Portfolio.Invested and self.ratio.Current.Value > 1:
            self.MarketOrder(self.ibm, 100)
        elif self.ratio.Current.Value < 1:
                self.Liquidate()

class NasdaqCustomColumns(NasdaqDataLink):
    def __init__(self):
        self.ValueColumnName = "adj. close"

