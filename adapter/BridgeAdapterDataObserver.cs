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
using Object = Java.Lang.Object;

namespace Com.H6ah4i.Android.Widget.Advrecyclerview.Adapter
{
    /// <summary>
    /// This class behaves like a "proxy" which bridges
    /// {@link RecyclerView.AdapterDataObserver} events to another subscriber object.
    /// </summary>
    public class BridgeAdapterDataObserver : RecyclerView.AdapterDataObserver
    {
        /// <summary>
        /// The subscriber interface.
        /// </summary>
        public interface ISubscriber
        {
            void OnBridgedAdapterChanged(RecyclerView.Adapter source, object tag);
            void OnBridgedAdapterItemRangeChanged(RecyclerView.Adapter source, object tag, int positionStart, int itemCount);
            void OnBridgedAdapterItemRangeChanged(RecyclerView.Adapter source, object tag, int positionStart, int itemCount, Object payload);
           
            void OnBridgedAdapterItemRangeInserted(RecyclerView.Adapter source, object tag, int positionStart, int itemCount);
            void OnBridgedAdapterItemRangeRemoved(RecyclerView.Adapter source, object tag, int positionStart, int itemCount);
            void OnBridgedAdapterRangeMoved(RecyclerView.Adapter source, object tag, int fromPosition, int toPosition, int itemCount);
        }

        private readonly WeakReference<ISubscriber> mRefSubscriber;
        
        private readonly WeakReference<RecyclerView.Adapter> mRefSourceHolder;
        
        private readonly object mTag;
       
        public BridgeAdapterDataObserver(ISubscriber subscriber, RecyclerView.Adapter sourceAdapter, object tag)
        {
            mRefSubscriber = new WeakReference<ISubscriber>(subscriber);
            mRefSourceHolder = new WeakReference<RecyclerView.Adapter>(sourceAdapter);
            mTag = tag;
        }

        public virtual object GetTag()
        {
            return mTag;
        }

        public override void OnChanged()
        {
            if (mRefSubscriber.TryGetTarget(out ISubscriber subscriber) &&  mRefSourceHolder.TryGetTarget(out RecyclerView.Adapter source))
            {
                subscriber.OnBridgedAdapterChanged(source, mTag);
            }
        }

        public override void OnItemRangeChanged(int positionStart, int itemCount)
        {
            if (mRefSubscriber.TryGetTarget(out ISubscriber subscriber) &&  mRefSourceHolder.TryGetTarget(out RecyclerView.Adapter source))
            {
                subscriber.OnBridgedAdapterItemRangeChanged(source, mTag, positionStart, itemCount);
            }
        }

        public override void OnItemRangeChanged(int positionStart, int itemCount, Object? payload)
        {
            if (mRefSubscriber.TryGetTarget(out ISubscriber subscriber) &&  mRefSourceHolder.TryGetTarget(out RecyclerView.Adapter source))
            {
                subscriber.OnBridgedAdapterItemRangeChanged(source, mTag, positionStart, itemCount, payload);
            }
        }

        public override void OnItemRangeInserted(int positionStart, int itemCount)
        {
            if (mRefSubscriber.TryGetTarget(out ISubscriber subscriber) &&  mRefSourceHolder.TryGetTarget(out RecyclerView.Adapter source))
            {
                subscriber.OnBridgedAdapterItemRangeInserted(source, mTag, positionStart, itemCount);
            }
        }

        public override void OnItemRangeRemoved(int positionStart, int itemCount)
        {
            if (mRefSubscriber.TryGetTarget(out ISubscriber subscriber) &&  mRefSourceHolder.TryGetTarget(out RecyclerView.Adapter source))
            {
                subscriber.OnBridgedAdapterItemRangeRemoved(source, mTag, positionStart, itemCount);
            }
        }

        public override void OnItemRangeMoved(int fromPosition, int toPosition, int itemCount)
        {
            if (mRefSubscriber.TryGetTarget(out ISubscriber subscriber) &&  mRefSourceHolder.TryGetTarget(out RecyclerView.Adapter source))
            {
                subscriber.OnBridgedAdapterRangeMoved(source, mTag, fromPosition, toPosition, itemCount);
            }
        }
    }
}