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

using System;
using QuantConnect.DataSource;

namespace QuantConnect.Data.Custom
{
    /// <summary>
    /// Quandl Data Type (Deprecated, Use NasdaqDataLink instead.)
    /// </summary>
    [ObsoleteAttribute("Quandl is obsolete. Use NasdaqDataLink instead.", false)]
    public class Quandl : NasdaqDataLink
    {
        /// <summary>
        /// Default <see cref="Quandl"/> constructor uses Close as its value column
        /// </summary>
        [ObsoleteAttribute("Quandl is obsolete. Use NasdaqDataLink instead.", false)]
        public Quandl() : base()
        {
        }

        /// <summary>
        /// Constructor for creating customized <see cref="Quandl"/> instance which doesn't use close, price, settle or value as its value item.
        /// </summary>
        /// <param name="valueColumnName">The name of the column we want to use as reference, the Value property</param>
        [ObsoleteAttribute("Quandl is obsolete. Use NasdaqDataLink instead.", false)]
        protected Quandl(string valueColumnName) : base(valueColumnName)
        {
        }
    }
}
