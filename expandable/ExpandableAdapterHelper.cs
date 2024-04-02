/*
 *    Copyright (C) 2015 Haruki Hasegawa
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

namespace Com.H6ah4i.Android.Widget.Advrecyclerview.Expandable
{
    class ExpandableAdapterHelper
    {
        public static readonly long NO_EXPANDABLE_POSITION = unchecked((long)0xffffffffffffffffUL);
        private static readonly long LOWER_32BIT_MASK = 0x00000000ffffffff;
        private static readonly long LOWER_31BIT_MASK = 0x000000007fffffff;
        public static long GetPackedPositionForChild(int groupPosition, int childPosition)
        {
            return ((long)childPosition << 32) | (groupPosition & LOWER_32BIT_MASK);
        }

        public static long GetPackedPositionForGroup(int groupPosition)
        {
            return ((long)RecyclerView.NoPosition << 32) | (groupPosition & LOWER_32BIT_MASK);
        }

        public static int GetPackedPositionChild(long packedPosition)
        {
            return (int)(packedPosition >>> 32);
        }

        public static int GetPackedPositionGroup(long packedPosition)
        {
            return (int)(packedPosition & LOWER_32BIT_MASK);
        }

        private ExpandableAdapterHelper()
        {
        }
    }
}