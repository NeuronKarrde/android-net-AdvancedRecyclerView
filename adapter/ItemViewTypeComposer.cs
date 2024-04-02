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

namespace Com.H6ah4i.Android.Widget.Advrecyclerview.Adapter
{
    /// <summary>
    /// Utility class providing "Composed item view type" related definitions and methods.
    /// <p>
    /// Spec:
    /// <table summary="Bit usages of composed item view type">
    /// <tr><th>bit 31</th><td>Expandable group flag  (1: expandable group / 0: normal item)</td></tr>
    /// <tr><th>bit 30-24</th><td>View type segment</td></tr>
    /// <tr><th>bit 23-0</th><td>Wrapped view type code</td></tr>
    /// </table>
    /// </p>
    /// </summary>
    public class ItemViewTypeComposer
    {
        /// <summary>
        /// Bit offset of the expandable flag part.
        /// </summary>
        public static readonly int BIT_OFFSET_EXPANDABLE_FLAG = 31;
        /// <summary>
        /// Bit offset of the segment part.
        /// </summary>
        public static readonly int BIT_OFFSET_SEGMENT = 24;
        /// <summary>
        /// Bit offset of the wrapped view type part.
        /// </summary>
        public static readonly int BIT_OFFSET_WRAPPED_VIEW_TYPE = 0;
        // ---
        /// <summary>
        /// Bit width of the expandable flag part.
        /// </summary>
        public static readonly int BIT_WIDTH_EXPANDABLE_FLAG = 1;
        /// <summary>
        /// Bit width of the segment part.
        /// </summary>
        public static readonly int BIT_WIDTH_SEGMENT = 7;
        /// <summary>
        /// Bit width of the wrapped view type part.
        /// </summary>
        public static readonly int BIT_WIDTH_WRAPPED_VIEW_TYPE = 24;
        /// <summary>
        /// Bit mask of the expandable flag part.
        /// </summary>
        public static readonly int BIT_MASK_EXPANDABLE_FLAG = (1 << (BIT_WIDTH_EXPANDABLE_FLAG - 1)) << BIT_OFFSET_EXPANDABLE_FLAG;
        // ---
        /// <summary>
        /// Bit mask of the segment part.
        /// </summary>
        public static readonly int BIT_MASK_SEGMENT = ((1 << BIT_WIDTH_SEGMENT) - 1) << BIT_OFFSET_SEGMENT;
        /// <summary>
        /// Bit mask of the wrapped view type part.
        /// </summary>
        public static readonly int BIT_MASK_WRAPPED_VIEW_TYPE = ((1 << BIT_WIDTH_WRAPPED_VIEW_TYPE) - 1) << BIT_OFFSET_WRAPPED_VIEW_TYPE;
        // ---
        /// <summary>
        /// Minimum value of segment.
        /// </summary>
        public static readonly int MIN_SEGMENT = 0;
        /// <summary>
        /// Maximum value of segment.
        /// </summary>
        public static readonly int MAX_SEGMENT = (1 << BIT_WIDTH_SEGMENT) - 1;
        /// <summary>
        /// Minimum value of wrapped view type.
        /// </summary>
        public static readonly int MIN_WRAPPED_VIEW_TYPE = -(1 << (BIT_WIDTH_WRAPPED_VIEW_TYPE - 1));
        /// <summary>
        /// Maximum value of wrapped view type.
        /// </summary>
        public static readonly int MAX_WRAPPED_VIEW_TYPE = (1 << (BIT_WIDTH_WRAPPED_VIEW_TYPE - 1)) - 1;
        private ItemViewTypeComposer()
        {
        }

        /// <summary>
        /// Extracts "Segment" part from composed view type.
        /// </summary>
        /// <param name="composedViewType">Composed view type</param>
        /// <returns>Segment part</returns>
        public static int ExtractSegmentPart(int composedViewType)
        {
            return (composedViewType & BIT_MASK_SEGMENT) >>> BIT_OFFSET_SEGMENT;
        }

        /// <summary>
        /// Extracts "Wrapped view type" part from composed view type.
        /// </summary>
        /// <param name="composedViewType">Composed view type</param>
        /// <returns>Wrapped view type part</returns>
        public static int ExtractWrappedViewTypePart(int composedViewType)
        {
            return (composedViewType << (32 - BIT_WIDTH_WRAPPED_VIEW_TYPE - BIT_OFFSET_WRAPPED_VIEW_TYPE)) >> (32 - BIT_WIDTH_WRAPPED_VIEW_TYPE);
        }

        /// <summary>
        /// Checks the composed view type is a expandable group or not.
        /// </summary>
        /// <param name="composedViewType">Composed view type</param>
        /// <returns>True if the specified composed composed view type is an expandable group item view type. Otherwise, false.</returns>
        public static bool IsExpandableGroup(int composedViewType)
        {
            return (composedViewType & BIT_MASK_EXPANDABLE_FLAG) != 0;
        }

        /// <summary>
        /// Makes a composed ID with specified segment and wrapped ID.
        /// </summary>
        /// <param name="segment">Segment</param>
        /// <param name="wrappedViewType">Wrapped view type</param>
        /// <returns>Composed View type.</returns>
        public static int ComposeSegment(int segment, int wrappedViewType)
        {
            if (segment < MIN_SEGMENT || segment > MAX_SEGMENT)
            {
                throw new ArgumentException("Segment value is out of range. (segment = " + segment + ")");
            }

            return (segment << BIT_OFFSET_SEGMENT) | (wrappedViewType & (BIT_MASK_EXPANDABLE_FLAG | BIT_MASK_WRAPPED_VIEW_TYPE));
        }
    }
}