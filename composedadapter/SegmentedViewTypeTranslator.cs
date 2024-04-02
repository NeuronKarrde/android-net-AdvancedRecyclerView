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
using Android.Util;
using Com.H6ah4i.Android.Widget.Advrecyclerview.Adapter;

namespace Com.H6ah4i.Android.Widget.Advrecyclerview.Composedadapter
{
    class SegmentedViewTypeTranslator
    {
        private SparseIntArray mWrapSegmentMap = new SparseIntArray();
        private SparseIntArray mUnwrapSegmentMap = new SparseIntArray();
        public SegmentedViewTypeTranslator()
        {
        }

        public virtual int WrapItemViewType(int segment, int viewType)
        {
            int packedSegments = (segment << 16) | ItemViewTypeComposer.ExtractSegmentPart(viewType);
            int flattenSegments;
            int index = mWrapSegmentMap.IndexOfKey(packedSegments);
            if (index >= 0)
            {
                flattenSegments = mWrapSegmentMap.ValueAt(index);
            }
            else
            {
                flattenSegments = mWrapSegmentMap.Size() + 1;
                if (flattenSegments > ItemViewTypeComposer.MAX_SEGMENT)
                {
                    throw new InvalidOperationException("Failed to allocate a new wrapped view type.");
                }

                mWrapSegmentMap.Put(packedSegments, flattenSegments);
                mUnwrapSegmentMap.Put(flattenSegments, packedSegments);
            }

            return ItemViewTypeComposer.ComposeSegment(flattenSegments, viewType);
        }

        public virtual long UnwrapViewType(int viewType)
        {
            int flattenSegment = ItemViewTypeComposer.ExtractSegmentPart(viewType);
            int index = mUnwrapSegmentMap.IndexOfKey(flattenSegment);
            if (index < 0)
            {
                throw new InvalidOperationException("Corresponding wrapped view type is not found!");
            }

            int packedSegments = mUnwrapSegmentMap.ValueAt(index);

            //noinspection BooleanMethodIsAlwaysInverted
            long packedViewType = (((long)packedSegments) << 32) | (((long)viewType) & 0xFFFFFFFF);
            return packedViewType;
        }

        public static int ExtractWrappedViewType(long packedViewType)
        {
            int segment = (int)(packedViewType >>> 32) & 0xFFFF;
            int viewType = (int)(packedViewType & 0xFFFFFFFF);
            return ItemViewTypeComposer.ComposeSegment(segment, viewType);
        }

        public static int ExtractWrapperSegment(long packedViewType)
        {
            return (int)((packedViewType >>> 48) & 0xFFFF);
        }
    }
}