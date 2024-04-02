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
    /// The result object of {@link WrapperAdapter#unwrapPosition(UnwrapPositionResult, int)}.
    /// This class is mutable that is why it is intended to reuse the same instance multiple times to avoid object creations.
    /// </summary>
    public class UnwrapPositionResult
    {
        /// <summary>
        /// Adapter
        /// </summary>
        public RecyclerView.Adapter adapter;
        /// <summary>
        /// Tag object
        /// 
        /// <p>The tag object can be used to identify the path.
        /// (e.g.: wrapper adapter can use a same child adapter multiple times)</p>
        /// </summary>
        public object tag;
        /// <summary>
        /// Unwrapped position
        /// </summary>
        public int position = RecyclerView.NoPosition;
        /// <summary>
        /// Clear fields
        /// </summary>
        public virtual void Clear()
        {
            adapter = null;
            tag = null;
            position = RecyclerView.NoPosition;
        }

        /// <summary>
        /// Returns the result is valid.
        /// </summary>
        /// <returns>True if the result object indicates valid position. Otherwise, false.</returns>
        public virtual bool IsValid()
        {
            return (adapter != null) && (position != RecyclerView.NoPosition);
        }
    }
}