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
using QuantConnect.DataSource;

namespace QuantConnect.Python
{
    /// <summary>
    /// Dynamic data class for Python algorithms.
    /// </summary>
    [ObsoleteAttribute("PythonQuandl is obsolete. Use NasdaqDataLink instead.", false)]
    public class PythonQuandl : NasdaqDataLink
    {
        /// <summary>
        /// Constructor for initialising the PythonQuandl class
        /// </summary>
        [ObsoleteAttribute("PythonQuandl is obsolete. Use NasdaqDataLink instead.", false)]
        public PythonQuandl() : base()
        {
            //Empty constructor required for fast-reflection initialization
        }
    }
}
