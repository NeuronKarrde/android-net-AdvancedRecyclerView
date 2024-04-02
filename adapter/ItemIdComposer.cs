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
    /// Utility class providing "Composed item ID" related definitions and methods.
    /// <p>
    /// Spec:
    /// <table summary="Bit usages of composed item ID">
    /// <tr><th>bit 63</th><td>Reserved</td></tr>
    /// <tr><th>bit 62-56</th><td>View type segment</td></tr>
    /// <tr><th>bit 55-28</th><td>Group ID</td></tr>
    /// <tr><th>bit 27-0</th><td>Child ID</td></tr>
    /// </table>
    /// </p>
    /// </summary>
    public class ItemIdComposer
    {
        /// <summary>
        /// Bit offset of the reserved sign flag part.
        /// </summary>
        public static readonly int BIT_OFFSET_RESERVED_SIGN_FLAG = 63;
        /// <summary>
        /// Bit offset of the segment part.
        /// </summary>
        public static readonly int BIT_OFFSET_SEGMENT = 56;
        /// <summary>
        /// Bit offset of the group ID part.
        /// </summary>
        public static readonly int BIT_OFFSET_GROUP_ID = 28;
        /// <summary>
        /// Bit offset of the child ID part.
        /// </summary>
        public static readonly int BIT_OFFSET_CHILD_ID = 0;
        // ---
        /// <summary>
        /// Bit width of the reserved sign flag part.
        /// </summary>
        public static readonly int BIT_WIDTH_RESERVED_SIGN_FLAG = 1;
        /// <summary>
        /// Bit width of the segment part.
        /// </summary>
        public static readonly int BIT_WIDTH_SEGMENT = 7;
        /// <summary>
        /// Bit width of the expandable group ID part.
        /// </summary>
        public static readonly int BIT_WIDTH_GROUP_ID = 28;
        /// <summary>
        /// Bit width of the expandable child ID part.
        /// </summary>
        public static readonly int BIT_WIDTH_CHILD_ID = 28;
        // ---
        /// <summary>
        /// Bit mask of the reserved sign flag part.
        /// </summary>
        public static readonly long BIT_MASK_RESERVED_SIGN_FLAG = ((1 << BIT_WIDTH_RESERVED_SIGN_FLAG) - 1) << BIT_OFFSET_RESERVED_SIGN_FLAG;
        /// <summary>
        /// Bit mask of the segment part.
        /// </summary>
        public static readonly long BIT_MASK_SEGMENT = ((1 << BIT_WIDTH_SEGMENT) - 1) << BIT_OFFSET_SEGMENT;
        /// <summary>
        /// Bit mask of the expandable group ID part.
        /// </summary>
        public static readonly long BIT_MASK_GROUP_ID = ((1 << BIT_WIDTH_GROUP_ID) - 1) << BIT_OFFSET_GROUP_ID;
        /// <summary>
        /// Bit mask of the expandable child ID part.
        /// </summary>
        public static readonly long BIT_MASK_CHILD_ID = ((1 << BIT_WIDTH_CHILD_ID) - 1) << BIT_OFFSET_CHILD_ID;
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
        /// Minimum value of group ID.
        /// </summary>
        public static readonly long MIN_GROUP_ID = -(1 << (BIT_WIDTH_GROUP_ID - 1));
        /// <summary>
        /// Maximum value of group ID.
        /// </summary>
        public static readonly long MAX_GROUP_ID = (1 << (BIT_WIDTH_GROUP_ID - 1)) - 1;
        /// <summary>
        /// Minimum value of child ID.
        /// </summary>
        public static readonly long MIN_CHILD_ID = -(1 << (BIT_WIDTH_CHILD_ID - 1));
        /// <summary>
        /// Maximum value of child ID.
        /// </summary>
        public static readonly long MAX_CHILD_ID = (1 << (BIT_WIDTH_CHILD_ID - 1)) - 1;
        /// <summary>
        /// Minimum value of wrapped ID (= group + child) ID.
        /// </summary>
        public static readonly long MIN_WRAPPED_ID = -(1 << (BIT_WIDTH_GROUP_ID + BIT_WIDTH_CHILD_ID - 1));
        /// <summary>
        /// Minimum value of wrapped ID (= group + child) ID.
        /// </summary>
        public static readonly long MAX_WRAPPED_ID = (1 << (BIT_WIDTH_GROUP_ID + BIT_WIDTH_CHILD_ID - 1)) - 1;
        private ItemIdComposer()
        {
        }

        /// <summary>
        /// Makes a composed ID which represents a child item of an expandable group.
        /// </summary>
        /// <param name="groupId">Group item ID</param>
        /// <param name="childId">Child item ID</param>
        /// <returns>Composed expandable child ID</returns>
        public static long ComposeExpandableChildId(long groupId, long childId)
        {
            if (groupId < MIN_GROUP_ID || groupId > MAX_GROUP_ID)
            {
                throw new ArgumentException("Group ID value is out of range. (groupId = " + groupId + ")");
            }

            if (childId < MIN_CHILD_ID || childId > MAX_CHILD_ID)
            {
                throw new ArgumentException("Child ID value is out of range. (childId = " + childId + ")");
            }


            //noinspection PointlessBitwiseExpression
            return ((groupId << BIT_OFFSET_GROUP_ID) & BIT_MASK_GROUP_ID) | ((childId << BIT_OFFSET_CHILD_ID) & BIT_MASK_CHILD_ID);
        }

        /// <summary>
        /// Makes a composed ID which represents an expandable group item.
        /// </summary>
        /// <param name="groupId">Group item ID</param>
        /// <returns>Composed expandable group ID</returns>
        public static long ComposeExpandableGroupId(long groupId)
        {
            if (groupId < MIN_GROUP_ID || groupId > MAX_GROUP_ID)
            {
                throw new ArgumentException("Group ID value is out of range. (groupId = " + groupId + ")");
            }


            //noinspection PointlessBitwiseExpression
            return ((groupId << BIT_OFFSET_GROUP_ID) & BIT_MASK_GROUP_ID) | ((RecyclerView.NoId << BIT_OFFSET_CHILD_ID) & BIT_MASK_CHILD_ID);
        }

        /// <summary>
        /// Checks the composed ID is a expandable group or not.
        /// </summary>
        /// <param name="composedId">Composed ID</param>
        /// <returns>True if the specified composed ID is an expandable group ID. Otherwise, false.</returns>
        public static bool IsExpandableGroup(long composedId)
        {
            return (composedId != RecyclerView.NoId) && ((composedId & BIT_MASK_CHILD_ID) == BIT_MASK_CHILD_ID);
        }

        /// <summary>
        /// Extracts "Segment" part from composed ID.
        /// </summary>
        /// <param name="composedId">Composed ID</param>
        /// <returns>Segment part</returns>
        public static int ExtractSegmentPart(long composedId)
        {
            return (int)((composedId & BIT_MASK_SEGMENT) >>> BIT_OFFSET_SEGMENT);
        }

        /// <summary>
        /// Extracts "Group ID" part from composed ID.
        /// </summary>
        /// <param name="composedId">Composed ID</param>
        /// <returns>Group ID part. If the specified composed ID is not an expandable group, returns {@link RecyclerView#NO_ID}.</returns>
        public static long ExtractExpandableGroupIdPart(long composedId)
        {
            if ((composedId == RecyclerView.NoId) || !IsExpandableGroup(composedId))
            {
                return RecyclerView.NoId;
            }

            return (composedId << (64 - BIT_WIDTH_GROUP_ID - BIT_OFFSET_GROUP_ID)) >> (64 - BIT_WIDTH_GROUP_ID);
        }

        /// <summary>
        /// Extracts "Child ID" part from composed ID.
        /// </summary>
        /// <param name="composedId">Composed ID</param>
        /// <returns>Child ID part. If the specified composed ID is not a child of an expandable group, returns {@link RecyclerView#NO_ID}.</returns>
        public static long ExtractExpandableChildIdPart(long composedId)
        {
            if ((composedId == RecyclerView.NoId) || IsExpandableGroup(composedId))
            {
                return RecyclerView.NoId;
            }

            return (composedId << (64 - BIT_WIDTH_CHILD_ID - BIT_OFFSET_CHILD_ID)) >> (64 - BIT_WIDTH_CHILD_ID);
        }

        /// <summary>
        /// Extracts "Wrapped ID" (group ID + child ID) part from composed ID.
        /// </summary>
        /// <param name="composedId">Composed ID</param>
        /// <returns>Wrapped ID part.</returns>
        public static long ExtractWrappedIdPart(long composedId)
        {
            if (composedId == RecyclerView.NoId)
            {
                return RecyclerView.NoId;
            }

            return (composedId << (64 - BIT_WIDTH_GROUP_ID - BIT_WIDTH_CHILD_ID - BIT_OFFSET_CHILD_ID)) >> (64 - (BIT_WIDTH_GROUP_ID + BIT_WIDTH_CHILD_ID));
        }

        /// <summary>
        /// Makes a composed ID with specified segment and wrapped ID.
        /// </summary>
        /// <param name="segment">Segment</param>
        /// <param name="wrappedId">Wrapped ID</param>
        /// <returns>Composed ID.</returns>
        public static long ComposeSegment(int segment, long wrappedId)
        {
            if (segment < MIN_SEGMENT || segment > MAX_SEGMENT)
            {
                throw new ArgumentException("Segment value is out of range. (segment = " + segment + ")");
            }

            return (((long)segment) << BIT_OFFSET_SEGMENT) | (wrappedId & (BIT_MASK_RESERVED_SIGN_FLAG | BIT_MASK_GROUP_ID | BIT_MASK_CHILD_ID));
        }
    }
}