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
using Android.Util;
using Android.Views;
using Com.H6ah4i.Android.Widget.Advrecyclerview.Adapter;
using Com.H6ah4i.Android.Widget.Advrecyclerview.Swipeable;
using Com.H6ah4i.Android.Widget.Advrecyclerview.Swipeable.Action;
using Com.H6ah4i.Android.Widget.Advrecyclerview.Utils;
using Java.Util;
using AndroidX.RecyclerView.Widget;

namespace Com.H6ah4i.Android.Widget.Advrecyclerview.Draggable
{
    class DraggableItemWrapperAdapter : SimpleWrapperAdapter, ISwipeableItemAdapter
    {
        private static readonly string TAG = "ARVDraggableWrapper";
        private static readonly int STATE_FLAG_INITIAL_VALUE = -1;
        // private interface Constants : DraggableItemConstants
        // {
        // }

        private static readonly bool LOCAL_LOGV = false;
        private static readonly bool LOCAL_LOGD = false;
        private static readonly bool LOCAL_LOGI = true;
        private static readonly bool DEBUG_BYPASS_MOVE_OPERATION_MODE = false;
        private RecyclerViewDragDropManager mDragDropManager;
        private IDraggableItemAdapter mDraggableItemAdapter;
        private RecyclerView.ViewHolder mDraggingItemViewHolder;
        private DraggingItemInfo mDraggingItemInfo;
        private ItemDraggableRange mDraggableRange;
        private int mDraggingItemInitialPosition = RecyclerView.NoPosition;
        private int mDraggingItemCurrentPosition = RecyclerView.NoPosition;
        private int mItemMoveMode;
        private bool mCallingDragStarted;
        public DraggableItemWrapperAdapter(RecyclerViewDragDropManager manager, RecyclerView.Adapter adapter) : base(adapter)
        {
            if (manager == null)
            {
                throw new ArgumentException("manager cannot be null");
            }

            mDragDropManager = manager;
        }

        protected override void OnRelease()
        {
            base.OnRelease();
            mDraggingItemViewHolder = null;
            mDraggableItemAdapter = null;
            mDragDropManager = null;
        }

        public override RecyclerView.ViewHolder OnCreateViewHolder(ViewGroup parent, int viewType)
        {
            RecyclerView.ViewHolder holder = base.OnCreateViewHolder(parent, viewType);
            if (holder is IDraggableItemViewHolder viewHolder)
            {
                viewHolder.DragStateFlags = STATE_FLAG_INITIAL_VALUE;
            }

            return holder;
        }

        public override void OnBindViewHolder(RecyclerView.ViewHolder holder, int position)
        {
            if (IsDragging())
            {
                long draggingItemId = mDraggingItemInfo.id;
                long itemId = holder.ItemId;
                int origPosition = ConvertToOriginalPosition(position, mDraggingItemInitialPosition, mDraggingItemCurrentPosition, mItemMoveMode);
                if (itemId == draggingItemId && holder != mDraggingItemViewHolder)
                {
                    if (LOCAL_LOGI)
                    {
                        Log.Info(TAG, "a new view holder object for the currently dragging item is assigned");
                    }

                    mDraggingItemViewHolder = holder;
                    mDragDropManager.OnNewDraggingItemViewBound(holder);
                }

                var flags = DraggableItemConstants.STATE_FLAG_DRAGGING;
                if (itemId == draggingItemId)
                {
                    flags |= DraggableItemConstants.STATE_FLAG_IS_ACTIVE;
                }

                if (mDraggableRange.CheckInRange(position))
                {
                    flags |= DraggableItemConstants.STATE_FLAG_IS_IN_RANGE;
                }

                SafeUpdateFlags(holder, (int)flags);
                base.OnBindViewHolder(holder, origPosition);
            }
            else
            {
                SafeUpdateFlags(holder, 0);
                base.OnBindViewHolder(holder, position);
            }
        }

        public override long GetItemId(int position)
        {
            if (IsDragging())
            {
                int origPosition = ConvertToOriginalPosition(position, mDraggingItemInitialPosition, mDraggingItemCurrentPosition, mItemMoveMode);
                return base.GetItemId(origPosition);
            }
            else
            {
                return base.GetItemId(position);
            }
        }

        public override int GetItemViewType(int position)
        {
            if (IsDragging())
            {
                int origPosition = ConvertToOriginalPosition(position, mDraggingItemInitialPosition, mDraggingItemCurrentPosition, mItemMoveMode);
                return base.GetItemViewType(origPosition);
            }
            else
            {
                return base.GetItemViewType(position);
            }
        }

        protected static int ConvertToOriginalPosition(int position, int dragInitial, int dragCurrent, int itemMoveMode)
        {
            if (dragInitial < 0 || dragCurrent < 0)
            {

                // not dragging
                return position;
            }
            else if (itemMoveMode == RecyclerViewDragDropManager.ITEM_MOVE_MODE_DEFAULT)
            {
                if ((dragInitial == dragCurrent) || ((position < dragInitial) && (position < dragCurrent)) || (position > dragInitial) && (position > dragCurrent))
                {
                    return position;
                }
                else if (dragCurrent < dragInitial)
                {
                    if (position == dragCurrent)
                    {
                        return dragInitial;
                    }
                    else
                    {
                        return position - 1;
                    }
                } // if (dragCurrent > dragInitial)
                else
                {

                    // if (dragCurrent > dragInitial)
                    if (position == dragCurrent)
                    {
                        return dragInitial;
                    }
                    else
                    {
                        return position + 1;
                    }
                }
            }
            else if (itemMoveMode == RecyclerViewDragDropManager.ITEM_MOVE_MODE_SWAP)
            {
                if (position == dragCurrent)
                {
                    return dragInitial;
                }
                else if (position == dragInitial)
                {
                    return dragCurrent;
                }
                else
                {
                    return position;
                }
            }
            else
            {
                throw new InvalidOperationException("unexpected state");
            }
        }

        // not dragging
        // if (dragCurrent > dragInitial)
        protected override void OnHandleWrappedAdapterChanged()
        {
            if (ShouldCancelDragOnDataUpdated())
            {
                CancelDrag();
            }
            else
            {
                base.OnHandleWrappedAdapterChanged();
            }
        }

        // not dragging
        // if (dragCurrent > dragInitial)
        protected override void OnHandleWrappedAdapterItemRangeChanged(int positionStart, int itemCount)
        {
            if (ShouldCancelDragOnDataUpdated())
            {
                CancelDrag();
            }
            else
            {
                base.OnHandleWrappedAdapterItemRangeChanged(positionStart, itemCount);
            }
        }

        // not dragging
        // if (dragCurrent > dragInitial)
        protected override void OnHandleWrappedAdapterItemRangeInserted(int positionStart, int itemCount)
        {
            if (ShouldCancelDragOnDataUpdated())
            {
                CancelDrag();
            }
            else
            {
                base.OnHandleWrappedAdapterItemRangeInserted(positionStart, itemCount);
            }
        }

        // not dragging
        // if (dragCurrent > dragInitial)
        protected override void OnHandleWrappedAdapterItemRangeRemoved(int positionStart, int itemCount)
        {
            if (ShouldCancelDragOnDataUpdated())
            {
                CancelDrag();
            }
            else
            {
                base.OnHandleWrappedAdapterItemRangeRemoved(positionStart, itemCount);
            }
        }

        // not dragging
        // if (dragCurrent > dragInitial)
        protected override void OnHandleWrappedAdapterRangeMoved(int fromPosition, int toPosition, int itemCount)
        {
            if (ShouldCancelDragOnDataUpdated())
            {
                CancelDrag();
            }
            else
            {
                base.OnHandleWrappedAdapterRangeMoved(fromPosition, toPosition, itemCount);
            }
        }

        // not dragging
        // if (dragCurrent > dragInitial)
        private bool ShouldCancelDragOnDataUpdated()
        {

            //noinspection SimplifiableIfStatement
            if (DEBUG_BYPASS_MOVE_OPERATION_MODE)
            {
                return false;
            }

            return IsDragging() && !mCallingDragStarted;
        }

        // not dragging
        // if (dragCurrent > dragInitial)
        //noinspection SimplifiableIfStatement
        private void CancelDrag()
        {
            if (mDragDropManager != null)
            {
                mDragDropManager.CancelDrag();
            }
        }

        // not dragging
        // if (dragCurrent > dragInitial)
        //noinspection SimplifiableIfStatement
        // NOTE: This method is called from RecyclerViewDragDropManager
        /*package*/
        public virtual void StartDraggingItem(DraggingItemInfo draggingItemInfo, RecyclerView.ViewHolder holder, ItemDraggableRange range, int wrappedItemPosition, int itemMoveMode)
        {
            if (LOCAL_LOGD)
            {
                Log.Debug(TAG, "onDragItemStarted(holder = " + holder + ")");
            }

            if (DEBUG_BYPASS_MOVE_OPERATION_MODE)
            {
                return;
            }

            if (holder.ItemId == RecyclerView.NoId)
            {
                throw new InvalidOperationException("dragging target must provides valid ID");
            }

            mDraggableItemAdapter = WrapperAdapterUtils.FindWrappedAdapter<IDraggableItemAdapter>(this, wrappedItemPosition);
            if (mDraggableItemAdapter == null)
            {
                throw new InvalidOperationException("DraggableItemAdapter not found!");
            }

            mDraggingItemInitialPosition = mDraggingItemCurrentPosition = wrappedItemPosition;
            mDraggingItemInfo = draggingItemInfo;
            mDraggingItemViewHolder = holder;
            mDraggableRange = range;
            mItemMoveMode = itemMoveMode;
        }

        public virtual void OnDragItemStarted()
        {
            mCallingDragStarted = true;
            mDraggableItemAdapter.OnItemDragStarted(GetDraggingItemInitialPosition());
            mCallingDragStarted = false;
        }
        
        public virtual void OnDragItemFinished(int draggingItemInitialPosition, int draggingItemCurrentPosition, bool result)
        {
            if (LOCAL_LOGD)
            {
                Log.Debug(TAG, "onDragItemFinished(draggingItemInitialPosition = " + draggingItemInitialPosition + ", draggingItemCurrentPosition = " + draggingItemCurrentPosition + ", result = " + result + ")");
            }

            if (DEBUG_BYPASS_MOVE_OPERATION_MODE)
            {
                return;
            }

            IDraggableItemAdapter draggableItemAdapter = mDraggableItemAdapter;
            mDraggingItemInitialPosition = RecyclerView.NoPosition;
            mDraggingItemCurrentPosition = RecyclerView.NoPosition;
            mDraggableRange = null;
            mDraggingItemInfo = null;
            mDraggingItemViewHolder = null;
            mDraggableItemAdapter = null;
            if (result && (draggingItemCurrentPosition != draggingItemInitialPosition))
            {

                // apply to wrapped adapter
                draggableItemAdapter.OnMoveItem(draggingItemInitialPosition, draggingItemCurrentPosition);
            }

            draggableItemAdapter.OnItemDragFinished(draggingItemInitialPosition, draggingItemCurrentPosition, result);
        }

        public override void OnViewRecycled(RecyclerView.ViewHolder holder, int viewType)
        {
            if (IsDragging())
            {
                mDragDropManager.OnItemViewRecycled(holder);
                mDraggingItemViewHolder = mDragDropManager.GetDraggingItemViewHolder();
            }

            base.OnViewRecycled(holder, viewType);
        }

        public virtual bool CanStartDrag(RecyclerView.ViewHolder holder, int position, int x, int y)
        {
            if (LOCAL_LOGV)
            {
                Log.Verbose(TAG, "canStartDrag(holder = " + holder + ", position = " + position + ", x = " + x + ", y = " + y + ")");
            }

            IDraggableItemAdapter draggableItemAdapter = WrapperAdapterUtils.FindWrappedAdapter<IDraggableItemAdapter>(this, position);
            if (draggableItemAdapter == null)
            {
                return false;
            }

            return draggableItemAdapter.OnCheckCanStartDrag(holder, position, x, y);
        }

        // not dragging
        // if (dragCurrent > dragInitial)
        //noinspection SimplifiableIfStatement
        // NOTE: This method is called from RecyclerViewDragDropManager
        /*package*/
        // NOTE: This method is called from RecyclerViewDragDropManager
        /*package*/
        // NOTE: This method is called from RecyclerViewDragDropManager
        /*package*/
        // apply to wrapped adapter
        // NOTE: This method is called from RecyclerViewDragDropManager
        /*package*/
        // NOTE: This method is called from RecyclerViewDragDropManager
        /*package*/
        public virtual bool CanDropItems(int draggingPosition, int dropPosition)
        {
            if (LOCAL_LOGV)
            {
                Log.Verbose(TAG, "canDropItems(draggingPosition = " + draggingPosition + ", dropPosition = " + dropPosition + ")");
            }

            return mDraggableItemAdapter.OnCheckCanDrop(draggingPosition, dropPosition);
        }

        // not dragging
        // if (dragCurrent > dragInitial)
        //noinspection SimplifiableIfStatement
        // NOTE: This method is called from RecyclerViewDragDropManager
        /*package*/
        // NOTE: This method is called from RecyclerViewDragDropManager
        /*package*/
        // NOTE: This method is called from RecyclerViewDragDropManager
        /*package*/
        // apply to wrapped adapter
        // NOTE: This method is called from RecyclerViewDragDropManager
        /*package*/
        // NOTE: This method is called from RecyclerViewDragDropManager
        /*package*/
        // NOTE: This method is called from RecyclerViewDragDropManager
        /*package*/
        public virtual ItemDraggableRange GetItemDraggableRange(RecyclerView.ViewHolder holder, int position)
        {
            if (LOCAL_LOGV)
            {
                Log.Verbose(TAG, "getItemDraggableRange(holder = " + holder + ", position = " + position + ")");
            }

            IDraggableItemAdapter draggableItemAdapter = WrapperAdapterUtils.FindWrappedAdapter<IDraggableItemAdapter>(this, position);
            if (draggableItemAdapter == null)
            {
                return null;
            }

            return draggableItemAdapter.OnGetItemDraggableRange(holder, position);
        }

        public virtual void MoveItem(int fromPosition, int toPosition, int layoutType)
        {
            if (LOCAL_LOGD)
            {
                Log.Debug(TAG, "onMoveItem(fromPosition = " + fromPosition + ", toPosition = " + toPosition + ")");
            }

            if (DEBUG_BYPASS_MOVE_OPERATION_MODE)
            {
                mDraggableItemAdapter.OnMoveItem(fromPosition, toPosition);
                return;
            }

            int origFromPosition = ConvertToOriginalPosition(fromPosition, mDraggingItemInitialPosition, mDraggingItemCurrentPosition, mItemMoveMode);
            if (origFromPosition != mDraggingItemInitialPosition)
            {
                throw new InvalidOperationException("onMoveItem() - may be a bug or has duplicate IDs  --- " + "mDraggingItemInitialPosition = " + mDraggingItemInitialPosition + ", " + "mDraggingItemCurrentPosition = " + mDraggingItemCurrentPosition + ", " + "origFromPosition = " + origFromPosition + ", " + "fromPosition = " + fromPosition + ", " + "toPosition = " + toPosition);
            }

            mDraggingItemCurrentPosition = toPosition;

            // NOTE:
            // Don't move items in wrapped adapter here.
            // notify to observers
            if (mItemMoveMode == RecyclerViewDragDropManager.ITEM_MOVE_MODE_DEFAULT && CustomRecyclerViewUtils.IsLinearLayout(layoutType))
            {
                NotifyItemMoved(fromPosition, toPosition);
            }
            else
            {
                NotifyDataSetChanged();
            }
        }

        protected virtual bool IsDragging()
        {
            return (mDraggingItemInfo != null);
        }

        public virtual int GetDraggingItemInitialPosition()
        {
            return mDraggingItemInitialPosition;
        }

        public virtual int GetDraggingItemCurrentPosition()
        {
            return mDraggingItemCurrentPosition;
        }

        private static void SafeUpdateFlags(RecyclerView.ViewHolder holder, int flags)
        {
            if (!(holder is IDraggableItemViewHolder holder2))
            {
                return;
            }

            int curFlags = holder2.DragStateFlags;
            var mask = ~DraggableItemConstants.STATE_FLAG_IS_UPDATED;

            // append UPDATED flag
            if ((curFlags == STATE_FLAG_INITIAL_VALUE) || (((curFlags ^ flags) & (int)mask) != 0))
            {
                flags |= (int)DraggableItemConstants.STATE_FLAG_IS_UPDATED;
            }

            holder2.DragStateFlags = flags;
        }

        // not dragging
        // if (dragCurrent > dragInitial)
        //noinspection SimplifiableIfStatement
        // NOTE: This method is called from RecyclerViewDragDropManager
        /*package*/
        // NOTE: This method is called from RecyclerViewDragDropManager
        /*package*/
        // NOTE: This method is called from RecyclerViewDragDropManager
        /*package*/
        // apply to wrapped adapter
        // NOTE: This method is called from RecyclerViewDragDropManager
        /*package*/
        // NOTE: This method is called from RecyclerViewDragDropManager
        /*package*/
        // NOTE: This method is called from RecyclerViewDragDropManager
        /*package*/
        // NOTE: This method is called from RecyclerViewDragDropManager
        /*package*/
        // NOTE:
        // Don't move items in wrapped adapter here.
        // notify to observers
        /*package*/
        /*package*/
        // append UPDATED flag
        private int GetOriginalPosition(int position)
        {
            int correctedPosition;
            if (IsDragging())
            {
                correctedPosition = ConvertToOriginalPosition(position, mDraggingItemInitialPosition, mDraggingItemCurrentPosition, mItemMoveMode);
            }
            else
            {
                correctedPosition = position;
            }

            return correctedPosition;
        }

        // not dragging
        // if (dragCurrent > dragInitial)
        //noinspection SimplifiableIfStatement
        // NOTE: This method is called from RecyclerViewDragDropManager
        /*package*/
        // NOTE: This method is called from RecyclerViewDragDropManager
        /*package*/
        // NOTE: This method is called from RecyclerViewDragDropManager
        /*package*/
        // apply to wrapped adapter
        // NOTE: This method is called from RecyclerViewDragDropManager
        /*package*/
        // NOTE: This method is called from RecyclerViewDragDropManager
        /*package*/
        // NOTE: This method is called from RecyclerViewDragDropManager
        /*package*/
        // NOTE: This method is called from RecyclerViewDragDropManager
        /*package*/
        // NOTE:
        // Don't move items in wrapped adapter here.
        // notify to observers
        /*package*/
        /*package*/
        // append UPDATED flag
        //
        // SwipeableItemAdapter implementations
        //
        public int OnGetSwipeReactionType(RecyclerView.ViewHolder holder, int position, int x, int y)
        {
            RecyclerView.Adapter adapter = GetWrappedAdapter();
            if (!(adapter is ISwipeableItemAdapter))
            {
                return RecyclerViewSwipeManager.ReactionCanNotSwipeAny;
            }

            int correctedPosition = GetOriginalPosition(position);
            ISwipeableItemAdapter swipeableItemAdapter = (ISwipeableItemAdapter)adapter;
            return swipeableItemAdapter.OnGetSwipeReactionType(holder, correctedPosition, x, y);
        }

        // not dragging
        // if (dragCurrent > dragInitial)
        //noinspection SimplifiableIfStatement
        // NOTE: This method is called from RecyclerViewDragDropManager
        /*package*/
        // NOTE: This method is called from RecyclerViewDragDropManager
        /*package*/
        // NOTE: This method is called from RecyclerViewDragDropManager
        /*package*/
        // apply to wrapped adapter
        // NOTE: This method is called from RecyclerViewDragDropManager
        /*package*/
        // NOTE: This method is called from RecyclerViewDragDropManager
        /*package*/
        // NOTE: This method is called from RecyclerViewDragDropManager
        /*package*/
        // NOTE: This method is called from RecyclerViewDragDropManager
        /*package*/
        // NOTE:
        // Don't move items in wrapped adapter here.
        // notify to observers
        /*package*/
        /*package*/
        // append UPDATED flag
        //
        // SwipeableItemAdapter implementations
        //
        public void OnSwipeItemStarted(RecyclerView.ViewHolder holder, int position)
        {
            RecyclerView.Adapter adapter = GetWrappedAdapter();
            if (!(adapter is ISwipeableItemAdapter))
            {
                return;
            }

            int correctedPosition = GetOriginalPosition(position);
            ((ISwipeableItemAdapter)adapter).OnSwipeItemStarted(holder, correctedPosition);
        }

        // not dragging
        // if (dragCurrent > dragInitial)
        //noinspection SimplifiableIfStatement
        // NOTE: This method is called from RecyclerViewDragDropManager
        /*package*/
        // NOTE: This method is called from RecyclerViewDragDropManager
        /*package*/
        // NOTE: This method is called from RecyclerViewDragDropManager
        /*package*/
        // apply to wrapped adapter
        // NOTE: This method is called from RecyclerViewDragDropManager
        /*package*/
        // NOTE: This method is called from RecyclerViewDragDropManager
        /*package*/
        // NOTE: This method is called from RecyclerViewDragDropManager
        /*package*/
        // NOTE: This method is called from RecyclerViewDragDropManager
        /*package*/
        // NOTE:
        // Don't move items in wrapped adapter here.
        // notify to observers
        /*package*/
        /*package*/
        // append UPDATED flag
        //
        // SwipeableItemAdapter implementations
        //
        public void OnSetSwipeBackground(RecyclerView.ViewHolder holder, int position, int type)
        {
            RecyclerView.Adapter adapter = GetWrappedAdapter();
            if (!(adapter is ISwipeableItemAdapter))
            {
                return;
            }

            int correctedPosition = GetOriginalPosition(position);
            ISwipeableItemAdapter swipeableItemAdapter = (ISwipeableItemAdapter)adapter;
            swipeableItemAdapter.OnSetSwipeBackground(holder, correctedPosition, type);
        }

        // not dragging
        // if (dragCurrent > dragInitial)
        //noinspection SimplifiableIfStatement
        // NOTE: This method is called from RecyclerViewDragDropManager
        /*package*/
        // NOTE: This method is called from RecyclerViewDragDropManager
        /*package*/
        // NOTE: This method is called from RecyclerViewDragDropManager
        /*package*/
        // apply to wrapped adapter
        // NOTE: This method is called from RecyclerViewDragDropManager
        /*package*/
        // NOTE: This method is called from RecyclerViewDragDropManager
        /*package*/
        // NOTE: This method is called from RecyclerViewDragDropManager
        /*package*/
        // NOTE: This method is called from RecyclerViewDragDropManager
        /*package*/
        // NOTE:
        // Don't move items in wrapped adapter here.
        // notify to observers
        /*package*/
        /*package*/
        // append UPDATED flag
        //
        // SwipeableItemAdapter implementations
        //
        public SwipeResultAction OnSwipeItem(RecyclerView.ViewHolder holder, int position, int result)
        {
            RecyclerView.Adapter adapter = GetWrappedAdapter();
            if (!(adapter is ISwipeableItemAdapter))
            {
                return new SwipeResultActionDefault();
            }

            int correctedPosition = GetOriginalPosition(position);
            return ((ISwipeableItemAdapter)adapter).OnSwipeItem(holder, correctedPosition, result);
        }
    }
}