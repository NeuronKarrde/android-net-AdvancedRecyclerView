/*
 *    Copyright (C) 2016 Haruki Hasegawa
 *
 *    Licensed under the Apache License, Version 2.0 (the "License");
 *    you may not use this file except in compliance with the License.
 *    You may obtain a copy of the License at
 *
 *        http:
 *
 *    Unless required by applicable law or agreed to in writing, software
 *    distributed under the License is distributed on an "AS IS" BASIS,
 *    WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 *    See the License for the specific language governing permissions and
 *    limitations under the License.
 */
using Android.Views;
using Com.H6ah4i.Android.Widget.Advrecyclerview.Adapter;
using Com.H6ah4i.Android.Widget.Advrecyclerview.Composedadapter;
using Java.Util;
using AndroidX.RecyclerView.Widget;
using Java.Lang;
using Object = System.Object;

namespace Com.H6ah4i.Android.Widget.Advrecyclerview.Headerfooter
{
    public abstract class AbstractHeaderFooterWrapperAdapter : ComposedAdapter
    {
        public static readonly int SEGMENT_TYPE_HEADER = 0;
        public static readonly int SEGMENT_TYPE_NORMAL = 1;
        public static readonly int SEGMENT_TYPE_FOOTER = 2;
        private RecyclerView.Adapter mHeaderAdapter;
        private RecyclerView.Adapter mWrappedAdapter;
        private RecyclerView.Adapter mFooterAdapter;
        private ComposedChildAdapterTag mHeaderAdapterTag;
        private ComposedChildAdapterTag mWrappedAdapterTag;
        private ComposedChildAdapterTag mFooterAdapterTag;
        public AbstractHeaderFooterWrapperAdapter()
        {
        }
        public virtual AbstractHeaderFooterWrapperAdapter SetAdapter(RecyclerView.Adapter adapter)
        {
            if (mWrappedAdapter != null)
            {
                throw new InvalidOperationException("setAdapter() can call only once");
            }

            mWrappedAdapter = adapter;
            mHeaderAdapter = OnCreateHeaderAdapter();
            mFooterAdapter = OnCreateFooterAdapter();
            bool hasStableIds = adapter.HasStableIds;
            mHeaderAdapter.HasStableIds = (hasStableIds);
            mFooterAdapter.HasStableIds = (hasStableIds);
            SetHasStableIds(hasStableIds);
            mHeaderAdapterTag = AddAdapter(mHeaderAdapter);
            mWrappedAdapterTag = AddAdapter(mWrappedAdapter);
            mFooterAdapterTag = AddAdapter(mFooterAdapter);
            return this;
        }
        protected override void OnRelease()
        {
            base.OnRelease();
            mHeaderAdapterTag = null;
            mWrappedAdapterTag = null;
            mFooterAdapterTag = null;
            mHeaderAdapter = null;
            mWrappedAdapter = null;
            mFooterAdapter = null;
        }
        protected virtual RecyclerView.Adapter OnCreateHeaderAdapter()
        {
            return new BaseHeaderAdapter(this);
        }
        protected virtual RecyclerView.Adapter OnCreateFooterAdapter()
        {
            return new BaseFooterAdapter(this);
        }
        public virtual RecyclerView.Adapter GetHeaderAdapter()
        {
            return mHeaderAdapter;
        }
        public virtual RecyclerView.Adapter GetFooterAdapter()
        {
            return mFooterAdapter;
        }
        public virtual RecyclerView.Adapter GetWrappedAdapter()
        {
            return mWrappedAdapter;
        }
        public virtual AdapterPathSegment GetWrappedAdapterSegment()
        {
            return new AdapterPathSegment(mWrappedAdapter, mWrappedAdapterTag);
        }
        public virtual AdapterPathSegment GetHeaderSegment()
        {
            return new AdapterPathSegment(mHeaderAdapter, mHeaderAdapterTag);
        }
        public virtual AdapterPathSegment GetFooterSegment()
        {
            return new AdapterPathSegment(mFooterAdapter, mFooterAdapterTag);
        }
        public abstract RecyclerView.ViewHolder OnCreateHeaderItemViewHolder(ViewGroup parent, int viewType);
        public abstract RecyclerView.ViewHolder OnCreateFooterItemViewHolder(ViewGroup parent, int viewType);
        public abstract void OnBindHeaderItemViewHolder(RecyclerView.ViewHolder holder, int localPosition);
        public abstract void OnBindFooterItemViewHolder(RecyclerView.ViewHolder holder, int localPosition);
        public virtual void OnBindHeaderItemViewHolder(RecyclerView.ViewHolder holder, int localPosition, IList<Java.Lang.Object> payloads)
        {
            OnBindHeaderItemViewHolder(holder, localPosition);
        }
        public virtual void OnBindFooterItemViewHolder(RecyclerView.ViewHolder holder, int localPosition, IList<Java.Lang.Object> payloads)
        {
            OnBindFooterItemViewHolder(holder, localPosition);
        }
        public abstract int HeaderItemCount { get; }
        public abstract int FooterItemCount { get; }
        public virtual long GetHeaderItemId(int localPosition)
        {
            if (HasStableIds)
            {
                return RecyclerView.NoId;
            }

            return localPosition;
        }
        public virtual long GetFooterItemId(int localPosition)
        {
            if (HasStableIds)
            {
                return RecyclerView.NoId;
            }

            return localPosition;
        }
        public virtual int GetHeaderItemViewType(int localPosition)
        {
            return 0;
        }
        public virtual int GetFooterItemViewType(int localPosition)
        {
            return 0;
        }
        public class BaseHeaderAdapter : RecyclerView.Adapter
        {
            protected AbstractHeaderFooterWrapperAdapter mHolder;

            public BaseHeaderAdapter(AbstractHeaderFooterWrapperAdapter holder)
            {
                mHolder = holder;
            }

            public override int ItemCount => mHolder.HeaderItemCount;

            public override long GetItemId(int position)
            {
                return mHolder.GetHeaderItemId(position);
            }

            public override int GetItemViewType(int position)
            {
                return mHolder.GetHeaderItemViewType(position);
            }

            public override RecyclerView.ViewHolder OnCreateViewHolder(ViewGroup parent, int viewType)
            {
                return mHolder.OnCreateHeaderItemViewHolder(parent, viewType);
            }

            public override void OnBindViewHolder(RecyclerView.ViewHolder holder, int position)
            {
                throw new IllegalStateException();
            }

            public override void OnBindViewHolder(RecyclerView.ViewHolder holder, int position, IList<Java.Lang.Object> payloads)
            {
                mHolder.OnBindHeaderItemViewHolder(holder, position, payloads);
            }

        }
        
        public class BaseFooterAdapter : RecyclerView.Adapter
        {
            protected AbstractHeaderFooterWrapperAdapter mHolder;
            public BaseFooterAdapter(AbstractHeaderFooterWrapperAdapter holder)
            {
                mHolder = holder;
            }

            public override int ItemCount => mHolder.FooterItemCount;

            public override long GetItemId(int position)
            {
                return mHolder.GetFooterItemId(position);
            }

            public override int GetItemViewType(int position)
            {
                return mHolder.GetFooterItemViewType(position);
            }

            public override RecyclerView.ViewHolder OnCreateViewHolder(ViewGroup parent, int viewType)
            {
                return mHolder.OnCreateFooterItemViewHolder(parent, viewType);
            }

            public override void OnBindViewHolder(RecyclerView.ViewHolder holder, int position)
            {
                throw new InvalidOperationException();
            }

            public override void OnBindViewHolder(RecyclerView.ViewHolder holder, int position, IList<Java.Lang.Object> payloads)
            {
                mHolder.OnBindFooterItemViewHolder(holder, position, payloads);
            }
        }
    }
}