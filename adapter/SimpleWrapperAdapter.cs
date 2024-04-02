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
using Android.Views;
using AndroidX.RecyclerView.Widget;
using Com.H6ah4i.Android.Widget.Advrecyclerview.Utils;
using Java.Util;

namespace Com.H6ah4i.Android.Widget.Advrecyclerview.Adapter
{
    /// <summary>
    /// A simple wrapper class. It just bypasses all methods and events to the wrapped adapter.
    /// Use this class as a default implementation of {@link WrapperAdapter}, so extend it
    /// and override each methods to build your own specialized adapter!
    /// </summary>
    public class SimpleWrapperAdapter : RecyclerView.Adapter, IWrapperAdapter, BridgeAdapterDataObserver.ISubscriber
    {
        private static readonly string TAG = "ARVSimpleWAdapter";
        private static readonly bool LOCAL_LOGD = false;
        private RecyclerView.Adapter mWrappedAdapter;
        private BridgeAdapterDataObserver mBridgeObserver;
        protected static readonly IList<Java.Lang.Object> FULL_UPDATE_PAYLOADS = new List<Java.Lang.Object>();
        
        public SimpleWrapperAdapter(RecyclerView.Adapter adapter)
        {
            mWrappedAdapter = adapter;
            mBridgeObserver = new BridgeAdapterDataObserver(this, mWrappedAdapter, null);
            mWrappedAdapter.RegisterAdapterDataObserver(mBridgeObserver);
            base.HasStableIds = mWrappedAdapter.HasStableIds;
        }

        public virtual bool IsWrappedAdapterAlive()
        {
            return mWrappedAdapter != null;
        }

        public virtual RecyclerView.Adapter GetWrappedAdapter()
        {
            return mWrappedAdapter;
        }

        public void GetWrappedAdapters(List<RecyclerView.Adapter> adapters)
        {
            if (mWrappedAdapter != null)
            {
                adapters.Add(mWrappedAdapter);
            }
        }

        public void Release()
        {
            OnRelease();
            if (mWrappedAdapter != null && mBridgeObserver != null)
            {
                mWrappedAdapter.UnregisterAdapterDataObserver(mBridgeObserver);
            }

            mWrappedAdapter = null;
            mBridgeObserver = null;
        }

        public override void OnAttachedToRecyclerView(RecyclerView recyclerView)
        {
            if (IsWrappedAdapterAlive())
                mWrappedAdapter.OnAttachedToRecyclerView(recyclerView);
        }

        public override void OnDetachedFromRecyclerView(RecyclerView recyclerView)
        {
            if (IsWrappedAdapterAlive())
                mWrappedAdapter.OnDetachedFromRecyclerView(recyclerView);
        }

        public void OnViewAttachedToWindow(RecyclerView.ViewHolder holder)
        {
            OnViewAttachedToWindow(holder, holder.ItemViewType);
        }

        public void OnViewAttachedToWindow(RecyclerView.ViewHolder holder, int viewType)
        {
            if (IsWrappedAdapterAlive())
            {
                WrappedAdapterUtils.InvokeOnViewAttachedToWindow(mWrappedAdapter, holder, viewType);
            }
        }

        public void OnViewDetachedFromWindow(RecyclerView.ViewHolder holder)
        {
            OnViewDetachedFromWindow(holder, holder.ItemViewType);
        }

        public void OnViewDetachedFromWindow(RecyclerView.ViewHolder holder, int viewType)
        {
            if (IsWrappedAdapterAlive())
            {
                WrappedAdapterUtils.InvokeOnViewDetachedFromWindow(mWrappedAdapter, holder, viewType);
            }
        }

        public void OnViewRecycled(RecyclerView.ViewHolder holder)
        {
            OnViewRecycled(holder, holder.ItemViewType);
        }

        public virtual void OnViewRecycled(RecyclerView.ViewHolder holder, int viewType)
        {
            if (IsWrappedAdapterAlive())
            {
                WrappedAdapterUtils.InvokeOnViewRecycled(mWrappedAdapter, holder, viewType);
            }
        }

        public bool OnFailedToRecycleView(RecyclerView.ViewHolder holder)
        {
            return OnFailedToRecycleView(holder, holder.ItemViewType);
        }

        public bool OnFailedToRecycleView(RecyclerView.ViewHolder holder, int viewType)
        {
            bool shouldBeRecycled = false;
            if (IsWrappedAdapterAlive())
            {
                shouldBeRecycled = WrappedAdapterUtils.InvokeOnFailedToRecycleView(mWrappedAdapter, holder, viewType);
            }

            if (shouldBeRecycled)
            {
                return true;
            }

            return base.OnFailedToRecycleView(holder);
        }

        public void SetHasStableIds(bool hasStableIds)
        {
            base.HasStableIds = hasStableIds;
            if (IsWrappedAdapterAlive())
                mWrappedAdapter.HasStableIds = hasStableIds;
        }
        
        public override RecyclerView.ViewHolder OnCreateViewHolder(ViewGroup parent, int viewType)
        {
            return mWrappedAdapter.OnCreateViewHolder(parent, viewType);
        }

        public override void OnBindViewHolder(RecyclerView.ViewHolder holder, int position)
        {
            OnBindViewHolder(holder, position, FULL_UPDATE_PAYLOADS);
        }

        public override void OnBindViewHolder(RecyclerView.ViewHolder holder, int position, IList<Java.Lang.Object> payloads)
        {
            if (IsWrappedAdapterAlive())
                mWrappedAdapter.OnBindViewHolder(holder, position, payloads);
        }

        public override int ItemCount =>  IsWrappedAdapterAlive() ? mWrappedAdapter.ItemCount : 0;

        public override long GetItemId(int position)
        {
            return mWrappedAdapter.GetItemId(position);
        }

        public override int GetItemViewType(int position)
        {
            return mWrappedAdapter.GetItemViewType(position);
        }

        public virtual void UnwrapPosition(UnwrapPositionResult dest, int position)
        {
            dest.adapter = GetWrappedAdapter();
            dest.position = position;
        }

        public int WrapPosition(AdapterPathSegment pathSegment, int position)
        {
            if (pathSegment.adapter == GetWrappedAdapter())
            {
                return position;
            }
            else
            {
                return RecyclerView.NoPosition;
            }
        }

        protected virtual void OnRelease()
        {
        }

        protected virtual void OnHandleWrappedAdapterChanged()
        {
            NotifyDataSetChanged();
        }

        protected virtual void OnHandleWrappedAdapterItemRangeChanged(int positionStart, int itemCount)
        {
            NotifyItemRangeChanged(positionStart, itemCount);
        }

        protected virtual void OnHandleWrappedAdapterItemRangeChanged(int positionStart, int itemCount, Java.Lang.Object? payload)
        {
            NotifyItemRangeChanged(positionStart, itemCount, payload);
        }

        protected virtual void OnHandleWrappedAdapterItemRangeInserted(int positionStart, int itemCount)
        {
            NotifyItemRangeInserted(positionStart, itemCount);
        }

        protected virtual void OnHandleWrappedAdapterItemRangeRemoved(int positionStart, int itemCount)
        {
            NotifyItemRangeRemoved(positionStart, itemCount);
        }

        protected virtual void OnHandleWrappedAdapterRangeMoved(int fromPosition, int toPosition, int itemCount)
        {
            if (itemCount != 1)
            {
                throw new InvalidOperationException("itemCount should be always 1  (actual: " + itemCount + ")");
            }

            NotifyItemMoved(fromPosition, toPosition);
        }

        public void OnBridgedAdapterChanged(RecyclerView.Adapter source, object tag)
        {
            if (LOCAL_LOGD)
            {
                Log.Debug(TAG, "onBridgedAdapterChanged");
            }

            OnHandleWrappedAdapterChanged();
        }

        public void OnBridgedAdapterItemRangeChanged(RecyclerView.Adapter source, object tag, int positionStart, int itemCount)
        {
            if (LOCAL_LOGD)
            {
                Log.Debug(TAG, "onBridgedAdapterItemRangeChanged(positionStart = " + positionStart + ", itemCount = " + itemCount + ")");
            }

            OnHandleWrappedAdapterItemRangeChanged(positionStart, itemCount);
        }

        public void OnBridgedAdapterItemRangeChanged(RecyclerView.Adapter sourceAdapter, object tag, int positionStart, int itemCount, Java.Lang.Object payload)
        {
            if (LOCAL_LOGD)
            {
                Log.Debug(TAG, "onBridgedAdapterItemRangeChanged(positionStart = " + positionStart + ", itemCount = " + itemCount + ", payload = " + payload + ")");
            }

            OnHandleWrappedAdapterItemRangeChanged(positionStart, itemCount, payload);
        }

        public void OnBridgedAdapterItemRangeInserted(RecyclerView.Adapter sourceAdapter, object tag, int positionStart, int itemCount)
        {
            if (LOCAL_LOGD)
            {
                Log.Debug(TAG, "onBridgedAdapterItemRangeInserted(positionStart = " + positionStart + ", itemCount = " + itemCount + ")");
            }

            OnHandleWrappedAdapterItemRangeInserted(positionStart, itemCount);
        }

        public void OnBridgedAdapterItemRangeRemoved(RecyclerView.Adapter sourceAdapter, object tag, int positionStart, int itemCount)
        {
            if (LOCAL_LOGD)
            {
                Log.Debug(TAG, "onBridgedAdapterItemRangeRemoved(positionStart = " + positionStart + ", itemCount = " + itemCount + ")");
            }

            OnHandleWrappedAdapterItemRangeRemoved(positionStart, itemCount);
        }

        public void OnBridgedAdapterRangeMoved(RecyclerView.Adapter sourceAdapter, object tag, int fromPosition, int toPosition, int itemCount)
        {
            if (LOCAL_LOGD)
            {
                Log.Debug(TAG, "onBridgedAdapterRangeMoved(fromPosition = " + fromPosition + ", toPosition = " + toPosition + ", itemCount = " + itemCount + ")");
            }

            OnHandleWrappedAdapterRangeMoved(fromPosition, toPosition, itemCount);
        }
    }
}