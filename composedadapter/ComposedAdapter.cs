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

using Android.Views;
using AndroidX.RecyclerView.Widget;
using Com.H6ah4i.Android.Widget.Advrecyclerview.Adapter;
using Com.H6ah4i.Android.Widget.Advrecyclerview.Utils;
using Object = Java.Lang.Object;

namespace Com.H6ah4i.Android.Widget.Advrecyclerview.Composedadapter
{
    /// <summary>
    /// A wrapper adapter which can compose and manage several children adapters.
    /// </summary>
    public class ComposedAdapter : RecyclerView.Adapter, IWrapperAdapter, BridgeAdapterDataObserver.ISubscriber
    {
        /// <summary>
        /// Corresponding segmented position value of {@link RecyclerView#NO_POSITION}.
        /// </summary>
        public static long NO_SEGMENTED_POSITION = AdaptersSet.NO_SEGMENTED_POSITION;
        /// <summary>
        /// Corresponding segmented position value of {@link RecyclerView#NO_POSITION}.
        /// </summary>
        private AdaptersSet mAdaptersSet;
        /// <summary>
        /// Corresponding segmented position value of {@link RecyclerView#NO_POSITION}.
        /// </summary>
        private SegmentedPositionTranslator mSegmentedPositionTranslator;
        /// <summary>
        /// Corresponding segmented position value of {@link RecyclerView#NO_POSITION}.
        /// </summary>
        private SegmentedViewTypeTranslator mViewTypeTranslator;
        public ComposedAdapter()
        {
            mAdaptersSet = new AdaptersSet(this);
            mSegmentedPositionTranslator = new SegmentedPositionTranslator(mAdaptersSet);
            mViewTypeTranslator = new SegmentedViewTypeTranslator();
            SetHasStableIds(true);
        }

        public void GetWrappedAdapters(List<RecyclerView.Adapter> adapters)
        {
            if (mAdaptersSet != null)
            {
                adapters.AddRange(mAdaptersSet.GetUniqueAdaptersList());
            }
        }

        public void Release()
        {
            OnRelease();
        }

        protected virtual void OnRelease()
        {
            if (mAdaptersSet != null)
            {
                mAdaptersSet.Release();
                mAdaptersSet = null;
            }

            if (mSegmentedPositionTranslator != null)
            {
                mSegmentedPositionTranslator.Release();
                mSegmentedPositionTranslator = null;
            }

            mViewTypeTranslator = null;
        }

        public virtual int GetChildAdapterCount()
        {
            return mAdaptersSet.GetSegmentCount();
        }

        public virtual ComposedChildAdapterTag AddAdapter(RecyclerView.Adapter adapter)
        {
            return AddAdapter(adapter, GetChildAdapterCount());
        }

        public virtual ComposedChildAdapterTag AddAdapter(RecyclerView.Adapter adapter, int position)
        {
            if (HasObservers && HasStableIds)
            {
                if (!adapter.HasStableIds)
                {
                    throw new InvalidOperationException("Wrapped child adapter must has stable IDs");
                }
            }

            ComposedChildAdapterTag tag = mAdaptersSet.AddAdapter(adapter, position);
            int segment = mAdaptersSet.GetAdapterSegment(tag);
            mSegmentedPositionTranslator.InvalidateSegment(segment);

            // NOTE: Need to assume as data set change here because view types and item IDs are completely changed!
            NotifyDataSetChanged();
            return tag;
        }

        public virtual bool RemoveAdapter(ComposedChildAdapterTag tag)
        {
            int segment = mAdaptersSet.GetAdapterSegment(tag);
            if (segment < 0)
            {
                return false;
            }

            mAdaptersSet.RemoveAdapter(tag);
            mSegmentedPositionTranslator.InvalidateSegment(segment);

            // NOTE: Need to assume as data set change here because view types and item IDs are completely changed!
            NotifyDataSetChanged();
            return true;
        }

        public virtual int GetSegment(ComposedChildAdapterTag tag)
        {
            return mAdaptersSet.GetAdapterSegment(tag);
        }

        public virtual long GetSegmentedPosition(int flatPosition)
        {
            return mSegmentedPositionTranslator.GetSegmentedPosition(flatPosition);
        }

        public static int ExtractSegmentPart(long segmentedPosition)
        {
            return AdaptersSet.ExtractSegment(segmentedPosition);
        }

        public static int ExtractSegmentOffsetPart(long segmentedPosition)
        {
            return AdaptersSet.ExtractSegmentOffset(segmentedPosition);
        }

        public void SetHasStableIds(bool hasStableIds)
        {
        
            // checks all children adapters support stable IDs
            if (hasStableIds && !HasStableIds)
            {
                int numSegments = mAdaptersSet.GetSegmentCount();
                for (int i = 0; i < numSegments; i++)
                {
                    RecyclerView.Adapter adapter = mAdaptersSet.GetAdapter(i);
                    if (!adapter.HasStableIds)
                    {
                        throw new InvalidOperationException("All child adapters must support stable IDs");
                    }
                }
            }
        
            HasStableIds = hasStableIds;
        }

        public override long GetItemId(int position)
        {
            long segmentedPosition = GetSegmentedPosition(position);
            int segment = AdaptersSet.ExtractSegment(segmentedPosition);
            int offset = AdaptersSet.ExtractSegmentOffset(segmentedPosition);
            RecyclerView.Adapter adapter = mAdaptersSet.GetAdapter(segment);
            int rawViewType = adapter.GetItemViewType(offset);
            long rawId = adapter.GetItemId(offset);
            int wrappedViewType = mViewTypeTranslator.WrapItemViewType(segment, rawViewType);
            int wrappedSegment = ItemViewTypeComposer.ExtractSegmentPart(wrappedViewType);
            return ItemIdComposer.ComposeSegment(wrappedSegment, rawId);
        }

        public override int GetItemViewType(int position)
        {
            long segmentedPosition = GetSegmentedPosition(position);
            int segment = AdaptersSet.ExtractSegment(segmentedPosition);
            int offset = AdaptersSet.ExtractSegmentOffset(segmentedPosition);
            RecyclerView.Adapter adapter = mAdaptersSet.GetAdapter(segment);
            int rawViewType = adapter.GetItemViewType(offset);
            return mViewTypeTranslator.WrapItemViewType(segment, rawViewType);
        }

        public override RecyclerView.ViewHolder OnCreateViewHolder(ViewGroup parent, int viewType)
        {
            long packedViewType = mViewTypeTranslator.UnwrapViewType(viewType);
            int segment = SegmentedViewTypeTranslator.ExtractWrapperSegment(packedViewType);
            int origViewType = SegmentedViewTypeTranslator.ExtractWrappedViewType(packedViewType);
            RecyclerView.Adapter adapter = mAdaptersSet.GetAdapter(segment);
            return adapter.OnCreateViewHolder(parent, origViewType);
        }

        public override void OnBindViewHolder(RecyclerView.ViewHolder holder, int position)
        {
            long segmentedPosition = GetSegmentedPosition(position);
            int segment = AdaptersSet.ExtractSegment(segmentedPosition);
            int offset = AdaptersSet.ExtractSegmentOffset(segmentedPosition);
            RecyclerView.Adapter adapter = mAdaptersSet.GetAdapter(segment);
            adapter.OnBindViewHolder(holder, offset);
        }

        public override void OnBindViewHolder(RecyclerView.ViewHolder holder, int position, IList<Java.Lang.Object> payloads)
        {
            long segmentedPosition = GetSegmentedPosition(position);
            int segment = AdaptersSet.ExtractSegment(segmentedPosition);
            int offset = AdaptersSet.ExtractSegmentOffset(segmentedPosition);
            RecyclerView.Adapter adapter = mAdaptersSet.GetAdapter(segment);
            adapter.OnBindViewHolder(holder, offset, payloads);
        }

        public override void OnAttachedToRecyclerView(RecyclerView recyclerView)
        {
            List<RecyclerView.Adapter> adapters = mAdaptersSet.GetUniqueAdaptersList();
            for (int i = 0; i < adapters.Count; i++)
            {
                adapters[i].OnAttachedToRecyclerView(recyclerView);
            }
        }

        public override void OnDetachedFromRecyclerView(RecyclerView recyclerView)
        {
            List<RecyclerView.Adapter> adapters = mAdaptersSet.GetUniqueAdaptersList();
            for (int i = 0; i < adapters.Count; i++)
            {
                adapters[i].OnDetachedFromRecyclerView(recyclerView);
            }
        }

        public override void OnViewAttachedToWindow(Object holder)
        {
            var vh = (RecyclerView.ViewHolder)holder;
            OnViewAttachedToWindow(vh, vh.ItemViewType);
        }

        public void OnViewAttachedToWindow(RecyclerView.ViewHolder holder, int viewType)
        {
            long packedViewType = mViewTypeTranslator.UnwrapViewType(viewType);
            int segment = SegmentedViewTypeTranslator.ExtractWrapperSegment(packedViewType);
            int wrappedViewType = SegmentedViewTypeTranslator.ExtractWrappedViewType(packedViewType);
            RecyclerView.Adapter adapter = mAdaptersSet.GetAdapter(segment);
            WrappedAdapterUtils.InvokeOnViewAttachedToWindow(adapter, holder, wrappedViewType);
        }

        public override void OnViewDetachedFromWindow(Object holder)
        {
            var vh = (RecyclerView.ViewHolder)holder;
            OnViewDetachedFromWindow(vh, vh.ItemViewType);
        }

        public void OnViewDetachedFromWindow(RecyclerView.ViewHolder holder, int viewType)
        {
            long packedViewType = mViewTypeTranslator.UnwrapViewType(viewType);
            int segment = SegmentedViewTypeTranslator.ExtractWrapperSegment(packedViewType);
            int wrappedViewType = SegmentedViewTypeTranslator.ExtractWrappedViewType(packedViewType);
            RecyclerView.Adapter adapter = mAdaptersSet.GetAdapter(segment);
            WrappedAdapterUtils.InvokeOnViewDetachedFromWindow(adapter, holder, wrappedViewType);
        }

        public override void OnViewRecycled(Object holder)
        {
            var vh = (RecyclerView.ViewHolder)holder;
            OnViewRecycled(vh, vh.ItemViewType);
        }

        public void OnViewRecycled(RecyclerView.ViewHolder holder, int viewType)
        {
            long packedViewType = mViewTypeTranslator.UnwrapViewType(viewType);
            int segment = SegmentedViewTypeTranslator.ExtractWrapperSegment(packedViewType);
            int wrappedViewType = SegmentedViewTypeTranslator.ExtractWrappedViewType(packedViewType);
            RecyclerView.Adapter adapter = mAdaptersSet.GetAdapter(segment);
            WrappedAdapterUtils.InvokeOnViewRecycled(adapter, holder, wrappedViewType);
        }

        public override bool OnFailedToRecycleView(Object holder)
        {
            var vh = (RecyclerView.ViewHolder)holder;
            return OnFailedToRecycleView(vh, vh.ItemViewType);
        }

        public bool OnFailedToRecycleView(RecyclerView.ViewHolder holder, int viewType)
        {
            long packedViewType = mViewTypeTranslator.UnwrapViewType(viewType);
            int segment = SegmentedViewTypeTranslator.ExtractWrapperSegment(packedViewType);
            int wrappedViewType = SegmentedViewTypeTranslator.ExtractWrappedViewType(packedViewType);
            RecyclerView.Adapter adapter = mAdaptersSet.GetAdapter(segment);
            return WrappedAdapterUtils.InvokeOnFailedToRecycleView(adapter, holder, wrappedViewType);
        }

        public override int ItemCount => mSegmentedPositionTranslator.GetTotalItemCount();

        public void UnwrapPosition(UnwrapPositionResult dest, int position)
        {
            long segmentedPosition = mSegmentedPositionTranslator.GetSegmentedPosition(position);
            if (segmentedPosition != AdaptersSet.NO_SEGMENTED_POSITION)
            {
                int segment = AdaptersSet.ExtractSegment(segmentedPosition);
                int offset = AdaptersSet.ExtractSegmentOffset(segmentedPosition);
                dest.adapter = mAdaptersSet.GetAdapter(segment);
                dest.position = offset;
                dest.tag = mAdaptersSet.GetTag(segment);
            }
        }

        public int WrapPosition(AdapterPathSegment pathSegment, int position)
        {
            if (pathSegment.tag != null)
            {
                ComposedChildAdapterTag tag = (ComposedChildAdapterTag)pathSegment.tag;
                int segment = mAdaptersSet.GetAdapterSegment(tag);
                return mSegmentedPositionTranslator.GetFlatPosition(segment, position);
            }
            else
            {
                return RecyclerView.NoPosition;
            }
        }

        public void OnBridgedAdapterChanged(RecyclerView.Adapter source, object tag)
        {
            OnHandleWrappedAdapterChanged(source, (IList<ComposedChildAdapterTag>)tag);
        }

        public void OnBridgedAdapterItemRangeChanged(RecyclerView.Adapter source, object tag, int positionStart, int itemCount)
        {
            OnHandleWrappedAdapterItemRangeChanged(source, (IList<ComposedChildAdapterTag>)tag, positionStart, itemCount);
        }

        public void OnBridgedAdapterItemRangeChanged(RecyclerView.Adapter source, object tag, int positionStart, int itemCount, Object payload)
        {
            OnHandleWrappedAdapterItemRangeChanged(source, (IList<ComposedChildAdapterTag>)tag, positionStart, itemCount, payload);
        }

        public void OnBridgedAdapterItemRangeInserted(RecyclerView.Adapter source, object tag, int positionStart, int itemCount)
        {
            OnHandleWrappedAdapterItemRangeInserted(source, (IList<ComposedChildAdapterTag>)tag, positionStart, itemCount);
        }

        public void OnBridgedAdapterItemRangeRemoved(RecyclerView.Adapter source, object tag, int positionStart, int itemCount)
        {
            OnHandleWrappedAdapterItemRangeRemoved(source, (IList<ComposedChildAdapterTag>)tag, positionStart, itemCount);
        }

        public void OnBridgedAdapterRangeMoved(RecyclerView.Adapter source, object tag, int fromPosition, int toPosition, int itemCount)
        {
            OnHandleWrappedAdapterRangeMoved(source, (IList<ComposedChildAdapterTag>)tag, fromPosition, toPosition, itemCount);
        }

        protected virtual void OnHandleWrappedAdapterChanged(RecyclerView.Adapter sourceAdapter, IList<ComposedChildAdapterTag> sourceTags)
        {
            mSegmentedPositionTranslator.InvalidateAll();
            NotifyDataSetChanged();
        }

        protected virtual void OnHandleWrappedAdapterItemRangeChanged(RecyclerView.Adapter sourceAdapter, IList<ComposedChildAdapterTag> sourceTags, int localPositionStart, int itemCount)
        {
            int nTags = sourceTags.Count;
            for (int i = 0; i < nTags; i++)
            {
                int adapterSegment = mAdaptersSet.GetAdapterSegment(sourceTags[i]);
                int positionStart = mSegmentedPositionTranslator.GetFlatPosition(adapterSegment, localPositionStart);
                NotifyItemRangeChanged(positionStart, itemCount);
            }
        }

        protected virtual void OnHandleWrappedAdapterItemRangeChanged(RecyclerView.Adapter sourceAdapter, IList<ComposedChildAdapterTag> sourceTags, int localPositionStart, int itemCount, Object payload)
        {
            int nTags = sourceTags.Count;
            for (int i = 0; i < nTags; i++)
            {
                int adapterSegment = mAdaptersSet.GetAdapterSegment(sourceTags[i]);
                int positionStart = mSegmentedPositionTranslator.GetFlatPosition(adapterSegment, localPositionStart);
                NotifyItemRangeChanged(positionStart, itemCount, payload);
            }
        }

        protected virtual void OnHandleWrappedAdapterItemRangeInserted(RecyclerView.Adapter sourceAdapter, IList<ComposedChildAdapterTag> sourceTags, int localPositionStart, int itemCount)
        {
            if (itemCount <= 0)
            {
                return;
            }

            int nTags = sourceTags.Count;
            if (nTags == 1)
            {
                int adapterSegment = mAdaptersSet.GetAdapterSegment(sourceTags[0]);
                mSegmentedPositionTranslator.InvalidateSegment(adapterSegment);
                int positionStart = mSegmentedPositionTranslator.GetFlatPosition(adapterSegment, localPositionStart);
                NotifyItemRangeInserted(positionStart, itemCount);
            }
            else
            {
                for (int i = 0; i < nTags; i++)
                {
                    int adapterSegment = mAdaptersSet.GetAdapterSegment(sourceTags[i]);
                    mSegmentedPositionTranslator.InvalidateSegment(adapterSegment);
                }

                NotifyDataSetChanged();
            }
        }

        protected virtual void OnHandleWrappedAdapterItemRangeRemoved(RecyclerView.Adapter sourceAdapter, IList<ComposedChildAdapterTag> sourceTags, int localPositionStart, int itemCount)
        {
            if (itemCount <= 0)
            {
                return;
            }

            int nTags = sourceTags.Count;
            if (nTags == 1)
            {
                int adapterSegment = mAdaptersSet.GetAdapterSegment(sourceTags[0]);
                mSegmentedPositionTranslator.InvalidateSegment(adapterSegment);
                int positionStart = mSegmentedPositionTranslator.GetFlatPosition(adapterSegment, localPositionStart);
                NotifyItemRangeRemoved(positionStart, itemCount);
            }
            else
            {
                for (int i = 0; i < nTags; i++)
                {
                    int adapterSegment = mAdaptersSet.GetAdapterSegment(sourceTags[i]);
                    mSegmentedPositionTranslator.InvalidateSegment(adapterSegment);
                }

                NotifyDataSetChanged();
            }
        }

        protected virtual void OnHandleWrappedAdapterRangeMoved(RecyclerView.Adapter sourceAdapter, IList<ComposedChildAdapterTag> sourceTags, int localFromPosition, int localToPosition, int itemCount)
        {
            if (itemCount != 1)
            {
                throw new InvalidOperationException("itemCount should be always 1  (actual: " + itemCount + ")");
            }

            int nTags = sourceTags.Count;
            if (nTags == 1)
            {
                int adapterSegment = mAdaptersSet.GetAdapterSegment(sourceTags[0]);
                int fromPosition = mSegmentedPositionTranslator.GetFlatPosition(adapterSegment, localFromPosition);
                int toPosition = mSegmentedPositionTranslator.GetFlatPosition(adapterSegment, localToPosition);
                NotifyItemMoved(fromPosition, toPosition);
            }
            else
            {
                NotifyDataSetChanged();
            }
        }
    }
}