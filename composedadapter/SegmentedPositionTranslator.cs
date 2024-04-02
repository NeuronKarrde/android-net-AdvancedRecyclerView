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
using Com.H6ah4i.Android.Widget.Advrecyclerview.Adapter;
using Java.Util;

namespace Com.H6ah4i.Android.Widget.Advrecyclerview.Composedadapter
{
    class SegmentedPositionTranslator
    {
        private static readonly int NO_CACHED_SEGMENT = 0;
        private static readonly int NO_CACHED_ITEM_COUNT = -1;
        private AdaptersSet mAdaptersSet;
        private int mLastOffsetCachedSegment;
        private int[] mSegmentItemCountCache;
        private int[] mSegmentOffsetCache;
        private int mCachedTotalItemCount;
        public SegmentedPositionTranslator(AdaptersSet adaptersSet)
        {
            mAdaptersSet = adaptersSet;
            mLastOffsetCachedSegment = NO_CACHED_SEGMENT;
            mCachedTotalItemCount = NO_CACHED_ITEM_COUNT;
            mSegmentItemCountCache = new int[ItemViewTypeComposer.MAX_SEGMENT + 1]; // NOTE: +1 room
            mSegmentOffsetCache = new int[ItemViewTypeComposer.MAX_SEGMENT + 1]; // NOTE: +1 room
            Arrays.Fill(mSegmentItemCountCache, NO_CACHED_ITEM_COUNT);
        }

        public virtual int GetTotalItemCount()
        {
            if (mCachedTotalItemCount == NO_CACHED_ITEM_COUNT)
            {
                mCachedTotalItemCount = CountTotalItems();
            }

            return mCachedTotalItemCount;
        }

        public virtual int GetFlatPosition(int segment, int offset)
        {
            return GetSegmentOffset(segment) + offset;
        }

        public virtual long GetSegmentedPosition(int flatPosition)
        {
            if (flatPosition == RecyclerView.NoPosition)
            {
                return RecyclerView.NoPosition;
            }

            int binSearchResult = Arrays.BinarySearch(mSegmentOffsetCache, 0, mLastOffsetCachedSegment, flatPosition);
            int loopStartIndex;
            int segment;
            int localOffset;
            if (binSearchResult >= 0)
            {
                loopStartIndex = binSearchResult;
                segment = loopStartIndex;
                localOffset = 0;
            }
            else
            {
                loopStartIndex = Math.Max(0, (~binSearchResult) - 1);
                segment = -1;
                localOffset = -1;
            }

            int nSegments = mAdaptersSet.GetSegmentCount();
            int segmentOffset = mSegmentOffsetCache[loopStartIndex];
            for (int i = loopStartIndex; i < nSegments; i++)
            {
                int count = GetSegmentItemCount(i);
                if ((segmentOffset + count) > flatPosition)
                {
                    localOffset = flatPosition - segmentOffset;
                    segment = i;
                    break;
                }

                segmentOffset += count;
            }

            if (segment >= 0)
            {
                return AdaptersSet.ComposeSegmentedPosition(segment, localOffset);
            }
            else
            {
                return AdaptersSet.NO_SEGMENTED_POSITION;
            }
        }

        private int CountTotalItems()
        {
            int segmentCount = mAdaptersSet.GetSegmentCount();
            if (segmentCount == 0)
            {
                return 0;
            }

            int lastSegment = segmentCount - 1;
            int lastSegmentOffset = GetSegmentOffset(lastSegment);
            int lastSegmentCount = GetSegmentItemCount(lastSegment);
            return (lastSegmentOffset + lastSegmentCount);
        }

        public virtual int GetSegmentOffset(int segment)
        {
            if (segment <= mLastOffsetCachedSegment)
            {

                // cache hit
                return mSegmentOffsetCache[segment];
            }
            else
            {

                // cache miss
                int nSegments = mAdaptersSet.GetSegmentCount();
                int loopStartIndex = mLastOffsetCachedSegment;
                int offset = mSegmentOffsetCache[loopStartIndex];
                for (int i = loopStartIndex; i < segment; i++)
                {
                    offset += GetSegmentItemCount(i);
                }

                return offset;
            }
        }

        public virtual int GetSegmentItemCount(int segment)
        {
            if (mSegmentItemCountCache[segment] != NO_CACHED_ITEM_COUNT)
            {

                // cache hit
                return mSegmentItemCountCache[segment];
            }
            else
            {

                // cache miss
                int count = mAdaptersSet.GetAdapter(segment).ItemCount;
                mSegmentItemCountCache[segment] = count;
                if (segment == mLastOffsetCachedSegment)
                {
                    mSegmentOffsetCache[segment + 1] = mSegmentOffsetCache[segment] + count;
                    mLastOffsetCachedSegment = segment + 1;
                }

                return count;
            }
        }

        public virtual void InvalidateSegment(int segment)
        {
            mCachedTotalItemCount = NO_CACHED_ITEM_COUNT;
            mLastOffsetCachedSegment = Math.Min(mLastOffsetCachedSegment, segment);
            mSegmentItemCountCache[segment] = NO_CACHED_ITEM_COUNT;
        }

        public virtual void InvalidateAll()
        {
            mCachedTotalItemCount = NO_CACHED_ITEM_COUNT;
            mLastOffsetCachedSegment = NO_CACHED_SEGMENT;
            Arrays.Fill(mSegmentItemCountCache, NO_CACHED_ITEM_COUNT);
        }

        public virtual void Release()
        {
            mAdaptersSet = null;
            mSegmentItemCountCache = null;
            mSegmentOffsetCache = null;
        }
    }
}