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
    class AdaptersSet
    {
        public static long NO_SEGMENTED_POSITION = -1;
        private BridgeAdapterDataObserver.ISubscriber mSubscriber;
        private IList<ComposedChildAdapterTag> mAdapterTags;
        private List<RecyclerView.Adapter> mAdapters;
        private List<RecyclerView.Adapter> mUniqueAdapters;
        private IList<ComposedChildAdapterDataObserver> mObservers;
        public AdaptersSet(BridgeAdapterDataObserver.ISubscriber bridgeSubscriber)
        {
            mSubscriber = bridgeSubscriber;
            mAdapterTags = new List<ComposedChildAdapterTag>();
            mAdapters = new List<RecyclerView.Adapter>();
            mUniqueAdapters = new List<RecyclerView.Adapter>();
            mObservers = new List<ComposedChildAdapterDataObserver>();
        }

        public virtual ComposedChildAdapterTag AddAdapter(RecyclerView.Adapter adapter, int position)
        {
            ComposedChildAdapterTag tag = new ComposedChildAdapterTag();
            mAdapterTags.Insert(position, tag);
            mAdapters.Insert(position, adapter);
            ComposedChildAdapterDataObserver observer;
            int uniqueAdapterIndex = mUniqueAdapters.IndexOf(adapter);
            if (uniqueAdapterIndex >= 0)
            {
                observer = mObservers[uniqueAdapterIndex];
            }
            else
            {
                observer = new ComposedChildAdapterDataObserver(mSubscriber, adapter);
                mObservers.Add(observer);
                mUniqueAdapters.Add(adapter);
                adapter.RegisterAdapterDataObserver(observer);
            }

            observer.RegisterChildAdapterTag(tag);
            return tag;
        }

        public virtual RecyclerView.Adapter RemoveAdapter(ComposedChildAdapterTag tag)
        {
            int segment = GetAdapterSegment(tag);
            if (segment < 0)
            {
                return null;
            }

            RecyclerView.Adapter adapter = mAdapters[segment];
            mAdapterTags.RemoveAt(segment);
            mAdapterTags.RemoveAt(segment);
            int uniqueAdapterIndex = mUniqueAdapters.IndexOf(adapter);
            if (uniqueAdapterIndex < 0)
            {
                throw new InvalidOperationException("Something wrong. Inconsistency detected.");
            }

            ComposedChildAdapterDataObserver observer = mObservers[uniqueAdapterIndex];
            observer.UnregisterChildAdapterTag(tag);
            if (!observer.HasChildAdapters())
            {
                adapter.UnregisterAdapterDataObserver(observer);
            }

            return adapter;
        }

        public virtual int GetAdapterSegment(ComposedChildAdapterTag tag)
        {
            return mAdapterTags.IndexOf(tag);
        }

        public virtual int GetSegmentCount()
        {
            return mAdapters.Count;
        }

        public virtual RecyclerView.Adapter GetAdapter(int segment)
        {
            return mAdapters[segment];
        }

        public virtual ComposedChildAdapterTag GetTag(int segment)
        {
            return mAdapterTags[segment];
        }

        public static int ExtractSegment(long segmentedPosition)
        {
            return (int)(segmentedPosition >>> 32);
        }

        public static int ExtractSegmentOffset(long segmentedPosition)
        {
            return (int)(segmentedPosition & 0xFFFFFFFF);
        }

        public static long ComposeSegmentedPosition(int segment, int offset)
        {
            return (((long)segment) << 32) | (offset & 0xFFFFFFFF);
        }

        public virtual void Release()
        {
            mAdapterTags.Clear();
            mAdapters.Clear();
            int numUniqueAdapters = mUniqueAdapters.Count;
            for (int i = 0; i < numUniqueAdapters; i++)
            {
                ComposedChildAdapterDataObserver observer = mObservers[i];
                RecyclerView.Adapter adapter = mUniqueAdapters[i];
                adapter.UnregisterAdapterDataObserver(observer);
                observer.Release();
            }

            mUniqueAdapters.Clear();
            mObservers.Clear();
        }

        public virtual List<RecyclerView.Adapter> GetUniqueAdaptersList()
        {
            return mUniqueAdapters;
        }
    }
}