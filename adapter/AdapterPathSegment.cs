/*
 *    Copyright (C) 2016 Haruki Hasegawa
 *
 *    Licensed under the Apache License, Version 2.0 (the "License");
 *    you may not use this file except in compliance with the License.
 *    You may obtain a copy of the License at
 *
 *        http://www.apache.org/licenses/LICENSE-2.0
 *
 *    Unless required by applicable law or agreed to in writing, software
 *    distributed under the License is distributed on an "AS IS" BASIS,
 *    WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 *    See the License for the specific language governing permissions and
 *    limitations under the License.
 */

using AndroidX.RecyclerView.Widget;

namespace Com.H6ah4i.Android.Widget.Advrecyclerview.Adapter
{
    /// <summary>
    /// Adapter path segment
    /// </summary>
    public class AdapterPathSegment
    {
        /// <summary>
        /// Adapter
        /// </summary>
        public readonly RecyclerView.Adapter adapter;
        /// <summary>
        /// Tag object
        /// </summary>
        public readonly object tag;
        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="adapter">The adapter</param>
        /// <param name="tag">The tag object</param>
        public AdapterPathSegment(RecyclerView.Adapter adapter, object tag)
        {
            this.adapter = adapter;
            this.tag = tag;
        }
    }
}