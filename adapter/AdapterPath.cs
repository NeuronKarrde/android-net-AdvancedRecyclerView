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
    /// Adapter path. This class represents how nested {@link WrapperAdapter}s route items.
    /// </summary>
    public class AdapterPath
    {
        private readonly IList<AdapterPathSegment> mSegments = new List<AdapterPathSegment>();
        /// <summary>
        /// Constructor.
        /// </summary>
        public AdapterPath()
        {
        }

        /// <summary>
        /// Appends path segment.
        /// </summary>
        /// <param name="wrapResult">The result object returned by {@link WrapperAdapter#wrapPosition(AdapterPathSegment, int)}.</param>
        /// <returns>{@link AdapterPath} instance itself.</returns>
        public virtual AdapterPath Append(UnwrapPositionResult wrapResult)
        {
            return Append(wrapResult.adapter, wrapResult.tag);
        }

        /// <summary>
        /// Appends path segment.
        /// </summary>
        /// <param name="adapter">The adapter</param>
        /// <param name="tag">The tag object</param>
        /// <returns>{@link AdapterPath} instance itself.</returns>
        public virtual AdapterPath Append(RecyclerView.Adapter adapter, object tag)
        {
            return Append(new AdapterPathSegment(adapter, tag));
        }

        /// <summary>
        /// Appends path segment.
        /// </summary>
        /// <param name="segment">The path segment</param>
        /// <returns>{@link AdapterPath} instance itself.</returns>
        public virtual AdapterPath Append(AdapterPathSegment segment)
        {
            mSegments.Add(segment);
            return this;
        }

        /// <summary>
        /// Clears path segments.
        /// </summary>
        /// <returns>{@link AdapterPath} instance itself.</returns>
        public virtual AdapterPath Clear()
        {
            mSegments.Clear();
            return this;
        }

        /// <summary>
        /// Gets whether the path is empty.
        /// </summary>
        /// <returns>True if the path is empty. Otherwise, false.</returns>
        public virtual bool IsEmpty()
        {
            return !mSegments.Any();
        }

        /// <summary>
        /// Gets path segments.
        /// </summary>
        /// <returns>The collection of path segments.</returns>
        public virtual IList<AdapterPathSegment> Segments()
        {
            return mSegments;
        }

        /// <summary>
        /// Retrieves the first path segment.
        /// </summary>
        /// <returns>The first path segment.</returns>
        public virtual AdapterPathSegment FirstSegment()
        {
            return mSegments.FirstOrDefault();
        }

        /// <summary>
        /// Retrieves the last path segment.
        /// </summary>
        /// <returns>THe last path segment.</returns>
        public virtual AdapterPathSegment LastSegment()
        {
            return mSegments.LastOrDefault();
        }
    }
}