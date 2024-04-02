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

using Android.Views;
using Com.H6ah4i.Android.Widget.Advrecyclerview.Adapter;
using Com.H6ah4i.Android.Widget.Advrecyclerview.Draggable;
using Com.H6ah4i.Android.Widget.Advrecyclerview.Swipeable;
using Com.H6ah4i.Android.Widget.Advrecyclerview.Swipeable.Action;
using Com.H6ah4i.Android.Widget.Advrecyclerview.Utils;
using Java.Util;
using AndroidX.RecyclerView.Widget;
using Com.H6ah4i.Android.Widget.Advrecyclerview.Expandable.Annotation;

namespace Com.H6ah4i.Android.Widget.Advrecyclerview.Expandable
{
    class ExpandableRecyclerViewWrapperAdapter : SimpleWrapperAdapter, IDraggableItemAdapter, ISwipeableItemAdapter
    {
        private static readonly string TAG = "ARVExpandableWrapper";

        //
        // NOTE: Make accessible with short name
        private static readonly int VIEW_TYPE_FLAG_IS_GROUP = ItemViewTypeComposer.BIT_MASK_EXPANDABLE_FLAG;
        //
        // NOTE: Make accessible with short name
        private static readonly int STATE_FLAG_INITIAL_VALUE = -1;
        //
        // NOTE: Make accessible with short name
        private IExpandableItemAdapter mExpandableItemAdapter;
        //
        // NOTE: Make accessible with short name
        private RecyclerViewExpandableItemManager mExpandableListManager;
        //
        // NOTE: Make accessible with short name
        private ExpandablePositionTranslator mPositionTranslator;
        //
        // NOTE: Make accessible with short name
        private int mDraggingItemGroupRangeStart = RecyclerView.NoPosition;
        //
        // NOTE: Make accessible with short name
        private int mDraggingItemGroupRangeEnd = RecyclerView.NoPosition;
        //
        // NOTE: Make accessible with short name
        private int mDraggingItemChildRangeStart = RecyclerView.NoPosition;
        //
        // NOTE: Make accessible with short name
        private int mDraggingItemChildRangeEnd = RecyclerView.NoPosition;
        //
        // NOTE: Make accessible with short name
        private int mSavedFromGroupPosition = RecyclerView.NoPosition;
        //
        // NOTE: Make accessible with short name
        private int mSavedFromChildPosition = RecyclerView.NoPosition;
        //
        // NOTE: Make accessible with short name
        private int mSavedToGroupPosition = RecyclerView.NoPosition;
        //
        // NOTE: Make accessible with short name
        private int mSavedToChildPosition = RecyclerView.NoPosition;
        //
        // NOTE: Make accessible with short name
        private RecyclerViewExpandableItemManager.IOnGroupExpandListener mOnGroupExpandListener;
        //
        // NOTE: Make accessible with short name
        private RecyclerViewExpandableItemManager.IOnGroupCollapseListener mOnGroupCollapseListener;
        //
        // NOTE: Make accessible with short name
        public ExpandableRecyclerViewWrapperAdapter(RecyclerViewExpandableItemManager manager, RecyclerView.Adapter adapter, long[] expandedItemsSavedState) : base(adapter)
        {
            mExpandableItemAdapter = GetExpandableItemAdapter(adapter);
            if (mExpandableItemAdapter == null)
            {
                throw new ArgumentException("adapter does not implement ExpandableItemAdapter");
            }

            if (manager == null)
            {
                throw new ArgumentException("manager cannot be null");
            }

            mExpandableListManager = manager;
            mPositionTranslator = new ExpandablePositionTranslator();
            mPositionTranslator.Build(mExpandableItemAdapter, ExpandablePositionTranslator.BUILD_OPTION_DEFAULT, mExpandableListManager.DefaultGroupsExpandedState);
            if (expandedItemsSavedState != null)
            {

                // NOTE: do not call hook routines and listener methods
                mPositionTranslator.RestoreExpandedGroupItems(expandedItemsSavedState, null, null, null);
            }
        }

        //
        // NOTE: Make accessible with short name
        // NOTE: do not call hook routines and listener methods
        protected override void OnRelease()
        {
            base.OnRelease();
            mExpandableItemAdapter = null;
            mExpandableListManager = null;
            mOnGroupExpandListener = null;
            mOnGroupCollapseListener = null;
        }

        public override int ItemCount => mPositionTranslator.GetItemCount();

        //
        // NOTE: Make accessible with short name
        // NOTE: do not call hook routines and listener methods
        public override long GetItemId(int position)
        {
            if (mExpandableItemAdapter == null)
            {
                return RecyclerView.NoId;
            }

            long expandablePosition = mPositionTranslator.GetExpandablePosition(position);
            int groupPosition = ExpandableAdapterHelper.GetPackedPositionGroup(expandablePosition);
            int childPosition = ExpandableAdapterHelper.GetPackedPositionChild(expandablePosition);
            if (childPosition == RecyclerView.NoPosition)
            {
                long groupId = mExpandableItemAdapter.GetGroupId(groupPosition);
                return ItemIdComposer.ComposeExpandableGroupId(groupId);
            }
            else
            {
                long groupId = mExpandableItemAdapter.GetGroupId(groupPosition);
                long childId = mExpandableItemAdapter.GetChildId(groupPosition, childPosition);
                return ItemIdComposer.ComposeExpandableChildId(groupId, childId);
            }
        }

        //
        // NOTE: Make accessible with short name
        // NOTE: do not call hook routines and listener methods
        public override int GetItemViewType(int position)
        {
            if (mExpandableItemAdapter == null)
            {
                return 0;
            }

            long expandablePosition = mPositionTranslator.GetExpandablePosition(position);
            int groupPosition = ExpandableAdapterHelper.GetPackedPositionGroup(expandablePosition);
            int childPosition = ExpandableAdapterHelper.GetPackedPositionChild(expandablePosition);
            int type;
            if (childPosition == RecyclerView.NoPosition)
            {
                type = mExpandableItemAdapter.GetGroupItemViewType(groupPosition);
            }
            else
            {
                type = mExpandableItemAdapter.GetChildItemViewType(groupPosition, childPosition);
            }

            if ((type & VIEW_TYPE_FLAG_IS_GROUP) != 0)
            {
                throw new InvalidOperationException("Illegal view type (type = " + Java.Lang.Integer.ToHexString(type) + ")");
            }

            return (childPosition == RecyclerView.NoPosition) ? (type | VIEW_TYPE_FLAG_IS_GROUP) : (type);
        }

        //
        // NOTE: Make accessible with short name
        // NOTE: do not call hook routines and listener methods
        public override RecyclerView.ViewHolder OnCreateViewHolder(ViewGroup parent, int viewType)
        {
            if (mExpandableItemAdapter == null)
            {
                throw new InvalidOperationException();
            }

            int maskedViewType = (viewType & (~VIEW_TYPE_FLAG_IS_GROUP));
            RecyclerView.ViewHolder holder;
            if ((viewType & VIEW_TYPE_FLAG_IS_GROUP) != 0)
            {
                holder = mExpandableItemAdapter.OnCreateGroupViewHolder(parent, maskedViewType);
            }
            else
            {
                holder = mExpandableItemAdapter.OnCreateChildViewHolder(parent, maskedViewType);
            }

            if (holder is IExpandableItemViewHolder viewHolder)
            {
                viewHolder.ExpandStateFlags = (STATE_FLAG_INITIAL_VALUE);
            }

            return holder;
        }

        //
        // NOTE: Make accessible with short name
        // NOTE: do not call hook routines and listener methods
        public override void OnBindViewHolder(RecyclerView.ViewHolder holder, int position, IList<Java.Lang.Object> payloads)
        {
            if (mExpandableItemAdapter == null)
            {
                return;
            }

            long expandablePosition = mPositionTranslator.GetExpandablePosition(position);
            int groupPosition = ExpandableAdapterHelper.GetPackedPositionGroup(expandablePosition);
            int childPosition = ExpandableAdapterHelper.GetPackedPositionChild(expandablePosition);
            int viewType = (holder.ItemViewType & (~VIEW_TYPE_FLAG_IS_GROUP));

            // update flags
            int flags = 0;
            if (childPosition == RecyclerView.NoPosition)
            {
                flags |= (int)ExpandableItemStateFlags.STATE_FLAG_IS_GROUP;
            }
            else
            {
                flags |= (int)ExpandableItemStateFlags.STATE_FLAG_IS_CHILD;
            }

            if (mPositionTranslator.IsGroupExpanded(groupPosition))
            {
                flags |= (int)ExpandableItemStateFlags.STATE_FLAG_IS_EXPANDED;
            }

            SafeUpdateExpandStateFlags(holder, flags);
            CorrectItemDragStateFlags(holder, groupPosition, childPosition);
            if (childPosition == RecyclerView.NoPosition)
            {
                mExpandableItemAdapter.OnBindGroupViewHolder(holder, groupPosition, viewType, payloads);
            }
            else
            {
                mExpandableItemAdapter.OnBindChildViewHolder(holder, groupPosition, childPosition, viewType, payloads);
            }
        }

        //
        // NOTE: Make accessible with short name
        // NOTE: do not call hook routines and listener methods
        // update flags
        private void RebuildPositionTranslator()
        {
            if (mPositionTranslator != null)
            {
                long[] savedState = mPositionTranslator.GetSavedStateArray();
                mPositionTranslator.Build(mExpandableItemAdapter, ExpandablePositionTranslator.BUILD_OPTION_DEFAULT, mExpandableListManager.DefaultGroupsExpandedState);

                // NOTE: do not call hook routines and listener methods
                mPositionTranslator.RestoreExpandedGroupItems(savedState, null, null, null);
            }
        }

        public override void OnViewRecycled(RecyclerView.ViewHolder holder, int viewType)
        {
            if (holder is IExpandableItemViewHolder viewHolder)
            {
                viewHolder.ExpandStateFlags = (STATE_FLAG_INITIAL_VALUE);
            }

            base.OnViewRecycled(holder, viewType);
        }

        protected override void OnHandleWrappedAdapterChanged()
        {
            RebuildPositionTranslator();
            base.OnHandleWrappedAdapterChanged();
        }

        protected override void OnHandleWrappedAdapterItemRangeChanged(int positionStart, int itemCount)
        {
            base.OnHandleWrappedAdapterItemRangeChanged(positionStart, itemCount);
        }

        protected override void OnHandleWrappedAdapterItemRangeInserted(int positionStart, int itemCount)
        {
            RebuildPositionTranslator();
            base.OnHandleWrappedAdapterItemRangeInserted(positionStart, itemCount);
        }

        protected override void OnHandleWrappedAdapterItemRangeRemoved(int positionStart, int itemCount)
        {
            if (itemCount == 1)
            {
                long expandablePosition = mPositionTranslator.GetExpandablePosition(positionStart);
                int groupPosition = ExpandableAdapterHelper.GetPackedPositionGroup(expandablePosition);
                int childPosition = ExpandableAdapterHelper.GetPackedPositionChild(expandablePosition);
                if (childPosition == RecyclerView.NoPosition)
                {
                    mPositionTranslator.RemoveGroupItem(groupPosition);
                }
                else
                {
                    mPositionTranslator.RemoveChildItem(groupPosition, childPosition);
                }
            }
            else
            {
                RebuildPositionTranslator();
            }

            base.OnHandleWrappedAdapterItemRangeRemoved(positionStart, itemCount);
        }

        protected override void OnHandleWrappedAdapterRangeMoved(int fromPosition, int toPosition, int itemCount)
        {
            RebuildPositionTranslator();
            base.OnHandleWrappedAdapterRangeMoved(fromPosition, toPosition, itemCount);
        }

        public bool OnCheckCanStartDrag(RecyclerView.ViewHolder holder, int position, int x, int y)
        {
            if (mExpandableItemAdapter is not IExpandableDraggableItemAdapter adapter)
            {
                return false;
            }

            //noinspection UnnecessaryLocalVariable
            int flatPosition = position;
            long expandablePosition = mPositionTranslator.GetExpandablePosition(flatPosition);
            int groupPosition = ExpandableAdapterHelper.GetPackedPositionGroup(expandablePosition);
            int childPosition = ExpandableAdapterHelper.GetPackedPositionChild(expandablePosition);
            bool canStart;
            if (childPosition == RecyclerView.NoPosition)
            {
                canStart = adapter.OnCheckGroupCanStartDrag(holder, groupPosition, x, y);
            }
            else
            {
                canStart = adapter.OnCheckChildCanStartDrag(holder, groupPosition, childPosition, x, y);
            }

            mDraggingItemGroupRangeStart = RecyclerView.NoPosition;
            mDraggingItemGroupRangeEnd = RecyclerView.NoPosition;
            mDraggingItemChildRangeStart = RecyclerView.NoPosition;
            mDraggingItemChildRangeEnd = RecyclerView.NoPosition;
            return canStart;
        }

        public ItemDraggableRange OnGetItemDraggableRange(RecyclerView.ViewHolder holder, int position)
        {
            if (!(mExpandableItemAdapter is IExpandableDraggableItemAdapter))
            {
                return null;
            }

            if (mExpandableItemAdapter.GroupCount < 1)
            {
                return null;
            }

            IExpandableDraggableItemAdapter adapter = (IExpandableDraggableItemAdapter)mExpandableItemAdapter;

            //noinspection UnnecessaryLocalVariable
            int flatPosition = position;
            long expandablePosition = mPositionTranslator.GetExpandablePosition(flatPosition);
            int groupPosition = ExpandableAdapterHelper.GetPackedPositionGroup(expandablePosition);
            int childPosition = ExpandableAdapterHelper.GetPackedPositionChild(expandablePosition);
            if (childPosition == RecyclerView.NoPosition)
            {

                // group
                ItemDraggableRange range = adapter.OnGetGroupItemDraggableRange(holder, groupPosition);
                if (range == null)
                {
                    int lastGroup = Math.Max(0, mExpandableItemAdapter.GroupCount - 1);
                    int start = 0;
                    int end = Math.Max(start, mPositionTranslator.GetItemCount() - mPositionTranslator.GetVisibleChildCount(lastGroup) - 1);
                    return new ItemDraggableRange(start, end);
                }
                else if (IsGroupPositionRange(range))
                {
                    long startPackedGroupPosition = ExpandableAdapterHelper.GetPackedPositionForGroup(range.GetStart());
                    long endPackedGroupPosition = ExpandableAdapterHelper.GetPackedPositionForGroup(range.GetEnd());
                    int start = mPositionTranslator.GetFlatPosition(startPackedGroupPosition);
                    int end = mPositionTranslator.GetFlatPosition(endPackedGroupPosition);
                    if (range.GetEnd() > groupPosition)
                    {
                        end += mPositionTranslator.GetVisibleChildCount(range.GetEnd());
                    }

                    mDraggingItemGroupRangeStart = range.GetStart();
                    mDraggingItemGroupRangeEnd = range.GetEnd();
                    return new ItemDraggableRange(start, end);
                }
                else
                {
                    throw new InvalidOperationException("Invalid range specified: " + range);
                }
            }
            else
            {

                // child
                ItemDraggableRange range = adapter.OnGetChildItemDraggableRange(holder, groupPosition, childPosition);

                // NOTE:
                // This method returns actual drag-sortable range, but the visual drag-sortable range would be different.
                // Thus appending the STATE_FLAG_IS_IN_RANGE flag at correctItemDragStateFlags() to avoid visual corruption.
                if (range == null)
                {
                    int start = 1; // 1 --- to avoid swapping with the first group item
                    return new ItemDraggableRange(start, Math.Max(start, mPositionTranslator.GetItemCount() - 1));
                }
                else if (IsGroupPositionRange(range))
                {
                    long startPackedGroupPosition = ExpandableAdapterHelper.GetPackedPositionForGroup(range.GetStart());
                    long endPackedGroupPosition = ExpandableAdapterHelper.GetPackedPositionForGroup(range.GetEnd());
                    int end = mPositionTranslator.GetFlatPosition(endPackedGroupPosition) + mPositionTranslator.GetVisibleChildCount(range.GetEnd());
                    int start = mPositionTranslator.GetFlatPosition(startPackedGroupPosition) + 1;
                    start = Math.Min(start, end);
                    mDraggingItemGroupRangeStart = range.GetStart();
                    mDraggingItemGroupRangeEnd = range.GetEnd();
                    return new ItemDraggableRange(start, end);
                }
                else if (IsChildPositionRange(range))
                {
                    int maxChildrenPos = Math.Max(mPositionTranslator.GetVisibleChildCount(groupPosition) - 1, 0);
                    int childStart = Math.Min(range.GetStart(), maxChildrenPos);
                    int childEnd = Math.Min(range.GetEnd(), maxChildrenPos);
                    long startPackedChildPosition = ExpandableAdapterHelper.GetPackedPositionForChild(groupPosition, childStart);
                    long endPackedChildPosition = ExpandableAdapterHelper.GetPackedPositionForChild(groupPosition, childEnd);
                    int start = mPositionTranslator.GetFlatPosition(startPackedChildPosition);
                    int end = mPositionTranslator.GetFlatPosition(endPackedChildPosition);
                    mDraggingItemChildRangeStart = childStart;
                    mDraggingItemChildRangeEnd = childEnd;
                    return new ItemDraggableRange(start, end);
                }
                else
                {
                    throw new InvalidOperationException("Invalid range specified: " + range);
                }
            }
        }

        public bool OnCheckCanDrop(int draggingPosition, int dropPosition)
        {
            if (!(mExpandableItemAdapter is IExpandableDraggableItemAdapter adapter))
            {
                return true;
            }

            if (mExpandableItemAdapter.GroupCount < 1)
            {
                return false;
            }

            //noinspection UnnecessaryLocalVariable
            int draggingFlatPosition = draggingPosition;
            long draggingExpandablePosition = mPositionTranslator.GetExpandablePosition(draggingFlatPosition);
            int draggingGroupPosition = ExpandableAdapterHelper.GetPackedPositionGroup(draggingExpandablePosition);
            int draggingChildPosition = ExpandableAdapterHelper.GetPackedPositionChild(draggingExpandablePosition);

            //noinspection UnnecessaryLocalVariable
            int dropFlatPosition = dropPosition;
            long dropExpandablePosition = mPositionTranslator.GetExpandablePosition(dropFlatPosition);
            int dropGroupPosition = ExpandableAdapterHelper.GetPackedPositionGroup(dropExpandablePosition);
            int dropChildPosition = ExpandableAdapterHelper.GetPackedPositionChild(dropExpandablePosition);
            bool draggingIsGroup = (draggingChildPosition == RecyclerView.NoPosition);
            bool dropIsGroup = (dropChildPosition == RecyclerView.NoPosition);
            if (draggingIsGroup)
            {

                // dragging: group
                bool canDrop;
                if (draggingGroupPosition == dropGroupPosition)
                {
                    canDrop = dropIsGroup;
                }
                else if (draggingFlatPosition < dropFlatPosition)
                {
                    bool isDropGroupExpanded = mPositionTranslator.IsGroupExpanded(dropGroupPosition);
                    int dropGroupVisibleChildren = mPositionTranslator.GetVisibleChildCount(dropGroupPosition);
                    if (dropIsGroup)
                    {
                        canDrop = (!isDropGroupExpanded);
                    }
                    else
                    {
                        canDrop = (dropChildPosition == (dropGroupVisibleChildren - 1));
                    }
                } // draggingFlatPosition > dropFlatPosition
                else
                {

                    // draggingFlatPosition > dropFlatPosition
                    canDrop = dropIsGroup;
                }

                if (canDrop)
                {
                    return adapter.OnCheckGroupCanDrop(draggingGroupPosition, dropGroupPosition);
                }
                else
                {
                    return false;
                }
            }
            else
            {

                // dragging: child
                bool isDropGroupExpanded = mPositionTranslator.IsGroupExpanded(dropGroupPosition);
                bool canDrop;
                int modDropGroupPosition = dropGroupPosition;
                int modDropChildPosition = dropChildPosition;
                if (draggingFlatPosition < dropFlatPosition)
                {
                    canDrop = true;
                    if (dropIsGroup)
                    {
                        if (isDropGroupExpanded)
                        {
                            modDropChildPosition = 0;
                        }
                        else
                        {
                            modDropChildPosition = mPositionTranslator.GetChildCount(modDropGroupPosition);
                        }
                    }
                } // draggingFlatPosition > dropFlatPosition
                else
                {

                    // draggingFlatPosition > dropFlatPosition
                    if (dropIsGroup)
                    {
                        if (modDropGroupPosition > 0)
                        {
                            modDropGroupPosition -= 1;
                            modDropChildPosition = mPositionTranslator.GetChildCount(modDropGroupPosition);
                            canDrop = true;
                        }
                        else
                        {
                            canDrop = false;
                        }
                    }
                    else
                    {
                        canDrop = true;
                    }
                }

                if (canDrop)
                {
                    return adapter.OnCheckChildCanDrop(draggingGroupPosition, draggingChildPosition, modDropGroupPosition, modDropChildPosition);
                }
                else
                {
                    return false;
                }
            }
        }

        public void OnItemDragStarted(int position)
        {
            if (!(mExpandableItemAdapter is IExpandableDraggableItemAdapter))
            {
                return;
            }

            IExpandableDraggableItemAdapter adapter = (IExpandableDraggableItemAdapter)mExpandableItemAdapter;

            //noinspection UnnecessaryLocalVariable
            int flatPosition = position;
            long draggingExpandablePosition = mPositionTranslator.GetExpandablePosition(flatPosition);
            int groupPosition = ExpandableAdapterHelper.GetPackedPositionGroup(draggingExpandablePosition);
            int childPosition = ExpandableAdapterHelper.GetPackedPositionChild(draggingExpandablePosition);
            if (childPosition == RecyclerView.NoPosition)
            {

                // group
                adapter.OnGroupDragStarted(groupPosition);
            }
            else
            {

                // child
                adapter.OnChildDragStarted(groupPosition, childPosition);
            }
        }

        public void OnItemDragFinished(int fromPosition, int toPosition, bool result)
        {
            int fromGroupPosition = mSavedFromGroupPosition;
            int fromChildPosition = mSavedFromChildPosition;
            int toGroupPosition = mSavedToGroupPosition;
            int toChildPosition = mSavedToChildPosition;
            mDraggingItemGroupRangeStart = RecyclerView.NoPosition;
            mDraggingItemGroupRangeEnd = RecyclerView.NoPosition;
            mDraggingItemChildRangeStart = RecyclerView.NoPosition;
            mDraggingItemChildRangeEnd = RecyclerView.NoPosition;
            mSavedFromGroupPosition = RecyclerView.NoPosition;
            mSavedFromChildPosition = RecyclerView.NoPosition;
            mSavedToGroupPosition = RecyclerView.NoPosition;
            mSavedToChildPosition = RecyclerView.NoPosition;
            if (!(mExpandableItemAdapter is IExpandableDraggableItemAdapter))
            {
                return;
            }

            if (fromGroupPosition == RecyclerView.NoPosition && fromChildPosition == RecyclerView.NoPosition)
            {

                // onMoveItem is not called, so in this case we can safely use the mPositionTranslator here.
                long fromDraggingExpandablePosition = mPositionTranslator.GetExpandablePosition(fromPosition);
                fromGroupPosition = ExpandableAdapterHelper.GetPackedPositionGroup(fromDraggingExpandablePosition);
                fromChildPosition = ExpandableAdapterHelper.GetPackedPositionChild(fromDraggingExpandablePosition);
                toGroupPosition = fromGroupPosition;
                toChildPosition = fromChildPosition;
            }

            IExpandableDraggableItemAdapter adapter = (IExpandableDraggableItemAdapter)mExpandableItemAdapter;
            if (fromChildPosition == RecyclerView.NoPosition)
            {

                // group
                adapter.OnGroupDragFinished(fromGroupPosition, toGroupPosition, result);
            }
            else
            {

                // child
                adapter.OnChildDragFinished(fromGroupPosition, fromChildPosition, toGroupPosition, toChildPosition, result);
            }
        }

        private static bool IsGroupPositionRange(ItemDraggableRange range)
        {
            if (range.GetType().Equals(typeof(GroupPositionItemDraggableRange)))
            {
                return true;
            }
            else if (range.GetType().Equals(typeof(ItemDraggableRange)))
            {

                // NOTE: ItemDraggableRange is regarded as group position
                return true;
            }

            return false;
        }

        private static bool IsChildPositionRange(ItemDraggableRange range)
        {
            return range.GetType().Equals(typeof(ChildPositionItemDraggableRange));
        }

        public void OnMoveItem(int fromPosition, int toPosition)
        {
            if (!(mExpandableItemAdapter is IExpandableDraggableItemAdapter))
            {
                return;
            }

            IExpandableDraggableItemAdapter adapter = (IExpandableDraggableItemAdapter)mExpandableItemAdapter;
            long expandableFromPosition = mPositionTranslator.GetExpandablePosition(fromPosition);
            int fromGroupPosition = ExpandableAdapterHelper.GetPackedPositionGroup(expandableFromPosition);
            int fromChildPosition = ExpandableAdapterHelper.GetPackedPositionChild(expandableFromPosition);
            long expandableToPosition = mPositionTranslator.GetExpandablePosition(toPosition);
            int toGroupPosition = ExpandableAdapterHelper.GetPackedPositionGroup(expandableToPosition);
            int toChildPosition = ExpandableAdapterHelper.GetPackedPositionChild(expandableToPosition);
            bool fromIsGroup = (fromChildPosition == RecyclerView.NoPosition);
            bool toIsGroup = (toChildPosition == RecyclerView.NoPosition);
            int modToChildPosition = toChildPosition;
            int modToGroupPosition = toGroupPosition;
            int actualToFlatPosition = fromPosition;
            if (fromIsGroup && toIsGroup)
            {
                adapter.OnMoveGroupItem(fromGroupPosition, toGroupPosition);
                mPositionTranslator.MoveGroupItem(fromGroupPosition, toGroupPosition);
                actualToFlatPosition = toPosition;
            }
            else if (!fromIsGroup && !toIsGroup)
            {

                // correct child position
                if (fromGroupPosition != toGroupPosition)
                {
                    if (fromPosition < toPosition)
                    {
                        modToChildPosition = toChildPosition + 1;
                    }
                    else
                    {
                        modToChildPosition = toChildPosition;
                    }
                }

                actualToFlatPosition = mPositionTranslator.GetFlatPosition(ExpandableAdapterHelper.GetPackedPositionForChild(fromGroupPosition, modToChildPosition));
                adapter.OnMoveChildItem(fromGroupPosition, fromChildPosition, toGroupPosition, modToChildPosition);
                mPositionTranslator.MoveChildItem(fromGroupPosition, fromChildPosition, toGroupPosition, modToChildPosition);
            } /*&& toIsGroup NOTE: toIsGroup is always true here*/
            else if (!fromIsGroup)
            {
                if (toPosition < fromPosition)
                {
                    if (toGroupPosition == 0)
                    {

                        // insert at the top
                        modToGroupPosition = toGroupPosition;
                        modToChildPosition = 0;
                    }
                    else
                    {

                        // insert at the end
                        modToGroupPosition = toGroupPosition - 1;
                        modToChildPosition = mPositionTranslator.GetChildCount(modToGroupPosition);
                    }
                }
                else
                {
                    if (mPositionTranslator.IsGroupExpanded(toGroupPosition))
                    {

                        // insert at the top
                        modToGroupPosition = toGroupPosition;
                        modToChildPosition = 0;
                    }
                    else
                    {

                        // insert at the end
                        modToGroupPosition = toGroupPosition;
                        modToChildPosition = mPositionTranslator.GetChildCount(modToGroupPosition);
                    }
                }

                if (fromGroupPosition == modToGroupPosition)
                {
                    int lastIndex = Math.Max(0, mPositionTranslator.GetChildCount(modToGroupPosition) - 1);
                    modToChildPosition = Math.Min(modToChildPosition, lastIndex);
                }

                if (!((fromGroupPosition == modToGroupPosition) && (fromChildPosition == modToChildPosition)))
                {
                    if (mPositionTranslator.IsGroupExpanded(toGroupPosition))
                    {
                        actualToFlatPosition = toPosition;
                    }
                    else
                    {
                        actualToFlatPosition = RecyclerView.NoPosition;
                    }

                    adapter.OnMoveChildItem(fromGroupPosition, fromChildPosition, modToGroupPosition, modToChildPosition);
                    mPositionTranslator.MoveChildItem(fromGroupPosition, fromChildPosition, modToGroupPosition, modToChildPosition);
                }
            } // if (fromIsGroup && !toIsGroup)
            else
            {

                // if (fromIsGroup && !toIsGroup)
                if (fromGroupPosition != toGroupPosition)
                {
                    actualToFlatPosition = mPositionTranslator.GetFlatPosition(ExpandableAdapterHelper.GetPackedPositionForGroup(toGroupPosition));
                    adapter.OnMoveGroupItem(fromGroupPosition, toGroupPosition);
                    mPositionTranslator.MoveGroupItem(fromGroupPosition, toGroupPosition);
                }
            }

            if (actualToFlatPosition != fromPosition)
            {
                if (actualToFlatPosition != RecyclerView.NoPosition)
                {
                    NotifyItemMoved(fromPosition, actualToFlatPosition);
                }
                else
                {
                    NotifyItemRemoved(fromPosition);
                }
            }


            // save from & to positions for onItemDragFinished
            mSavedFromGroupPosition = fromGroupPosition;
            mSavedFromChildPosition = fromChildPosition;
            mSavedToGroupPosition = modToGroupPosition;
            mSavedToChildPosition = modToChildPosition;
        }

        public int OnGetSwipeReactionType(RecyclerView.ViewHolder holder, int position, int x, int y)
        {
            if (!(mExpandableItemAdapter is IBaseExpandableSwipeableItemAdapter))
            {
                return RecyclerViewSwipeManager.ReactionCanNotSwipeAny;
            }

            IBaseExpandableSwipeableItemAdapter adapter = (IBaseExpandableSwipeableItemAdapter)mExpandableItemAdapter;

            //noinspection UnnecessaryLocalVariable
            int flatPosition = position;
            long expandablePosition = mPositionTranslator.GetExpandablePosition(flatPosition);
            int groupPosition = ExpandableAdapterHelper.GetPackedPositionGroup(expandablePosition);
            int childPosition = ExpandableAdapterHelper.GetPackedPositionChild(expandablePosition);
            if (childPosition == RecyclerView.NoPosition)
            {
                return adapter.OnGetGroupItemSwipeReactionType(holder, groupPosition, x, y);
            }
            else
            {
                return adapter.OnGetChildItemSwipeReactionType(holder, groupPosition, childPosition, x, y);
            }
        }

        public void OnSwipeItemStarted(RecyclerView.ViewHolder holder, int position)
        {
            if (!(mExpandableItemAdapter is IBaseExpandableSwipeableItemAdapter))
            {
                return;
            }

            IBaseExpandableSwipeableItemAdapter adapter = (IBaseExpandableSwipeableItemAdapter)mExpandableItemAdapter;

            //noinspection UnnecessaryLocalVariable
            int flatPosition = position;
            long expandablePosition = mPositionTranslator.GetExpandablePosition(flatPosition);
            int groupPosition = ExpandableAdapterHelper.GetPackedPositionGroup(expandablePosition);
            int childPosition = ExpandableAdapterHelper.GetPackedPositionChild(expandablePosition);
            if (childPosition == RecyclerView.NoPosition)
            {
                adapter.OnSwipeGroupItemStarted(holder, groupPosition);
            }
            else
            {
                adapter.OnSwipeChildItemStarted(holder, groupPosition, childPosition);
            }
        }

        public void OnSetSwipeBackground(RecyclerView.ViewHolder holder, int position, int type)
        {
            if (!(mExpandableItemAdapter is IBaseExpandableSwipeableItemAdapter))
            {
                return;
            }

            IBaseExpandableSwipeableItemAdapter adapter = (IBaseExpandableSwipeableItemAdapter)mExpandableItemAdapter;

            //noinspection UnnecessaryLocalVariable
            int flatPosition = position;
            long expandablePosition = mPositionTranslator.GetExpandablePosition(flatPosition);
            int groupPosition = ExpandableAdapterHelper.GetPackedPositionGroup(expandablePosition);
            int childPosition = ExpandableAdapterHelper.GetPackedPositionChild(expandablePosition);
            if (childPosition == RecyclerView.NoPosition)
            {
                adapter.OnSetGroupItemSwipeBackground(holder, groupPosition, type);
            }
            else
            {
                adapter.OnSetChildItemSwipeBackground(holder, groupPosition, childPosition, type);
            }
        }

        public SwipeResultAction OnSwipeItem(RecyclerView.ViewHolder holder, int position, int result)
        {
            if (!(mExpandableItemAdapter is IBaseExpandableSwipeableItemAdapter))
            {
                return null;
            }

            if (position == RecyclerView.NoPosition)
            {
                return null;
            }

            IBaseExpandableSwipeableItemAdapter adapter = (IBaseExpandableSwipeableItemAdapter)mExpandableItemAdapter;

            //noinspection UnnecessaryLocalVariable
            int flatPosition = position;
            long expandablePosition = mPositionTranslator.GetExpandablePosition(flatPosition);
            int groupPosition = ExpandableAdapterHelper.GetPackedPositionGroup(expandablePosition);
            int childPosition = ExpandableAdapterHelper.GetPackedPositionChild(expandablePosition);
            return ExpandableSwipeableItemInternalUtils.InvokeOnSwipeItem(adapter, holder, groupPosition, childPosition, result);
        }

        public virtual bool OnTapItem(RecyclerView.ViewHolder holder, int position, int x, int y)
        {
            if (mExpandableItemAdapter == null)
            {
                return false;
            }


            //noinspection UnnecessaryLocalVariable
            int flatPosition = position;
            long expandablePosition = mPositionTranslator.GetExpandablePosition(flatPosition);
            int groupPosition = ExpandableAdapterHelper.GetPackedPositionGroup(expandablePosition);
            int childPosition = ExpandableAdapterHelper.GetPackedPositionChild(expandablePosition);
            if (childPosition != RecyclerView.NoPosition)
            {
                return false;
            }

            bool expand = !(mPositionTranslator.IsGroupExpanded(groupPosition));
            bool result = mExpandableItemAdapter.OnCheckCanExpandOrCollapseGroup(holder, groupPosition, x, y, expand);
            if (!result)
            {
                return false;
            }

            if (expand)
            {
                ExpandGroup(groupPosition, true, null);
            }
            else
            {
                CollapseGroup(groupPosition, true, null);
            }

            return true;
        }

        public virtual void ExpandAll()
        {
            if (!mPositionTranslator.IsEmpty() && !mPositionTranslator.IsAllExpanded())
            {
                mPositionTranslator.Build(mExpandableItemAdapter, ExpandablePositionTranslator.BUILD_OPTION_EXPANDED_ALL, mExpandableListManager.DefaultGroupsExpandedState);
                NotifyDataSetChanged();
            }
        }

        public virtual void CollapseAll()
        {
            if (!mPositionTranslator.IsEmpty() && !mPositionTranslator.IsAllCollapsed())
            {
                mPositionTranslator.Build(mExpandableItemAdapter, ExpandablePositionTranslator.BUILD_OPTION_COLLAPSED_ALL, mExpandableListManager.DefaultGroupsExpandedState);
                NotifyDataSetChanged();
            }
        }

        public virtual bool CollapseGroup(int groupPosition, bool fromUser, Java.Lang.Object payload)
        {
            if (!mPositionTranslator.IsGroupExpanded(groupPosition))
            {
                return false;
            }


            // call hook method
            if (!mExpandableItemAdapter.OnHookGroupCollapse(groupPosition, fromUser, payload))
            {
                return false;
            }

            if (mPositionTranslator.CollapseGroup(groupPosition))
            {
                long packedPosition = ExpandableAdapterHelper.GetPackedPositionForGroup(groupPosition);
                int flatPosition = mPositionTranslator.GetFlatPosition(packedPosition);
                int childCount = mPositionTranslator.GetChildCount(groupPosition);
                NotifyItemRangeRemoved(flatPosition + 1, childCount);
            }

            {
                long packedPosition = ExpandableAdapterHelper.GetPackedPositionForGroup(groupPosition);
                int flatPosition = mPositionTranslator.GetFlatPosition(packedPosition);
                NotifyItemChanged(flatPosition, payload);
            }


            // raise onGroupCollapse() event
            if (mOnGroupCollapseListener != null)
            {
                mOnGroupCollapseListener.OnGroupCollapse(groupPosition, fromUser, payload);
            }

            return true;
        }

        public virtual bool ExpandGroup(int groupPosition, bool fromUser, Java.Lang.Object payload)
        {
            if (mPositionTranslator.IsGroupExpanded(groupPosition))
            {
                return false;
            }


            // call hook method
            if (!mExpandableItemAdapter.OnHookGroupExpand(groupPosition, fromUser, payload))
            {
                return false;
            }

            if (mPositionTranslator.ExpandGroup(groupPosition))
            {
                long packedPosition = ExpandableAdapterHelper.GetPackedPositionForGroup(groupPosition);
                int flatPosition = mPositionTranslator.GetFlatPosition(packedPosition);
                int childCount = mPositionTranslator.GetChildCount(groupPosition);
                NotifyItemRangeInserted(flatPosition + 1, childCount);
            }

            {
                long packedPosition = ExpandableAdapterHelper.GetPackedPositionForGroup(groupPosition);
                int flatPosition = mPositionTranslator.GetFlatPosition(packedPosition);
                NotifyItemChanged(flatPosition, payload);
            }


            // raise onGroupExpand() event
            if (mOnGroupExpandListener != null)
            {
                mOnGroupExpandListener.OnGroupExpand(groupPosition, fromUser, payload);
            }

            return true;
        }

        public virtual bool IsGroupExpanded(int groupPosition)
        {
            return mPositionTranslator.IsGroupExpanded(groupPosition);
        }

        public virtual long GetExpandablePosition(int flatPosition)
        {
            return mPositionTranslator.GetExpandablePosition(flatPosition);
        }

        public virtual int GetFlatPosition(long packedPosition)
        {
            return mPositionTranslator.GetFlatPosition(packedPosition);
        }

        public virtual long[] GetExpandedItemsSavedStateArray()
        {
            if (mPositionTranslator != null)
            {
                return mPositionTranslator.GetSavedStateArray();
            }
            else
            {
                return null;
            }
        }

        public virtual void SetOnGroupExpandListener(RecyclerViewExpandableItemManager.IOnGroupExpandListener listener)
        {
            mOnGroupExpandListener = listener;
        }

        public virtual void SetOnGroupCollapseListener(RecyclerViewExpandableItemManager.IOnGroupCollapseListener listener)
        {
            mOnGroupCollapseListener = listener;
        }

        public virtual void RestoreState(long[] adapterSavedState, bool callHook, bool callListeners)
        {
            mPositionTranslator.RestoreExpandedGroupItems(adapterSavedState, (callHook ? mExpandableItemAdapter : null), (callListeners ? mOnGroupExpandListener : null), (callListeners ? mOnGroupCollapseListener : null));
        }

        public virtual void NotifyGroupItemChanged(int groupPosition, Java.Lang.Object payload)
        {
            long packedPosition = ExpandableAdapterHelper.GetPackedPositionForGroup(groupPosition);
            int flatPosition = mPositionTranslator.GetFlatPosition(packedPosition);
            if (flatPosition != RecyclerView.NoPosition)
            {
                NotifyItemChanged(flatPosition, payload);
            }
        }

        public virtual void NotifyGroupAndChildrenItemsChanged(int groupPosition, Java.Lang.Object payload)
        {
            long packedPosition = ExpandableAdapterHelper.GetPackedPositionForGroup(groupPosition);
            int flatPosition = mPositionTranslator.GetFlatPosition(packedPosition);
            int visibleChildCount = mPositionTranslator.GetVisibleChildCount(groupPosition);
            if (flatPosition != RecyclerView.NoPosition)
            {
                NotifyItemRangeChanged(flatPosition, 1 + visibleChildCount, payload);
            }
        }

        public virtual void NotifyChildrenOfGroupItemChanged(int groupPosition, Java.Lang.Object payload)
        {
            int visibleChildCount = mPositionTranslator.GetVisibleChildCount(groupPosition);

            // notify if the group is expanded
            if (visibleChildCount > 0)
            {
                long packedPosition = ExpandableAdapterHelper.GetPackedPositionForChild(groupPosition, 0);
                int flatPosition = mPositionTranslator.GetFlatPosition(packedPosition);
                if (flatPosition != RecyclerView.NoPosition)
                {
                    NotifyItemRangeChanged(flatPosition, visibleChildCount, payload);
                }
            }
        }

        public virtual void NotifyChildItemChanged(int groupPosition, int childPosition, Java.Lang.Object payload)
        {
            NotifyChildItemRangeChanged(groupPosition, childPosition, 1, payload);
        }

        public virtual void NotifyChildItemRangeChanged(int groupPosition, int childPositionStart, int itemCount, Java.Lang.Object payload)
        {
            int visibleChildCount = mPositionTranslator.GetVisibleChildCount(groupPosition);

            // notify if the group is expanded
            if ((visibleChildCount > 0) && (childPositionStart < visibleChildCount))
            {
                long packedPosition = ExpandableAdapterHelper.GetPackedPositionForChild(groupPosition, 0);
                int flatPosition = mPositionTranslator.GetFlatPosition(packedPosition);
                if (flatPosition != RecyclerView.NoPosition)
                {
                    int startPosition = flatPosition + childPositionStart;
                    int count = Math.Min(itemCount, (visibleChildCount - childPositionStart));
                    NotifyItemRangeChanged(startPosition, count, payload);
                }
            }
        }

        public virtual void NotifyChildItemInserted(int groupPosition, int childPosition)
        {
            mPositionTranslator.InsertChildItem(groupPosition, childPosition);
            long packedPosition = ExpandableAdapterHelper.GetPackedPositionForChild(groupPosition, childPosition);
            int flatPosition = mPositionTranslator.GetFlatPosition(packedPosition);
            if (flatPosition != RecyclerView.NoPosition)
            {
                NotifyItemInserted(flatPosition);
            }
        }

        public virtual void NotifyChildItemRangeInserted(int groupPosition, int childPositionStart, int itemCount)
        {
            mPositionTranslator.InsertChildItems(groupPosition, childPositionStart, itemCount);
            long packedPosition = ExpandableAdapterHelper.GetPackedPositionForChild(groupPosition, childPositionStart);
            int flatPosition = mPositionTranslator.GetFlatPosition(packedPosition);
            if (flatPosition != RecyclerView.NoPosition)
            {
                NotifyItemRangeInserted(flatPosition, itemCount);
            }
        }

        public virtual void NotifyChildItemRemoved(int groupPosition, int childPosition)
        {
            long packedPosition = ExpandableAdapterHelper.GetPackedPositionForChild(groupPosition, childPosition);
            int flatPosition = mPositionTranslator.GetFlatPosition(packedPosition);
            mPositionTranslator.RemoveChildItem(groupPosition, childPosition);
            if (flatPosition != RecyclerView.NoPosition)
            {
                NotifyItemRemoved(flatPosition);
            }
        }

        public virtual void NotifyChildItemRangeRemoved(int groupPosition, int childPositionStart, int itemCount)
        {
            long packedPosition = ExpandableAdapterHelper.GetPackedPositionForChild(groupPosition, childPositionStart);
            int flatPosition = mPositionTranslator.GetFlatPosition(packedPosition);
            mPositionTranslator.RemoveChildItems(groupPosition, childPositionStart, itemCount);
            if (flatPosition != RecyclerView.NoPosition)
            {
                NotifyItemRangeRemoved(flatPosition, itemCount);
            }
        }

        public virtual void NotifyGroupItemInserted(int groupPosition, bool expanded)
        {
            int insertedCount = mPositionTranslator.InsertGroupItem(groupPosition, expanded);
            if (insertedCount > 0)
            {
                long packedPosition = ExpandableAdapterHelper.GetPackedPositionForGroup(groupPosition);
                int flatPosition = mPositionTranslator.GetFlatPosition(packedPosition);
                NotifyItemInserted(flatPosition);

                // raise onGroupExpand() event
                RaiseOnGroupExpandedSequentially(groupPosition, 1, false, null);
            }
        }

        public virtual void NotifyGroupItemRangeInserted(int groupPositionStart, int count, bool expanded)
        {
            int insertedCount = mPositionTranslator.InsertGroupItems(groupPositionStart, count, expanded);
            if (insertedCount > 0)
            {
                long packedPosition = ExpandableAdapterHelper.GetPackedPositionForGroup(groupPositionStart);
                int flatPosition = mPositionTranslator.GetFlatPosition(packedPosition);
                NotifyItemRangeInserted(flatPosition, insertedCount);
                RaiseOnGroupExpandedSequentially(groupPositionStart, count, false, null);
            }
        }

        public virtual void NotifyGroupItemMoved(int fromGroupPosition, int toGroupPosition)
        {
            long packedFrom = RecyclerViewExpandableItemManager.GetPackedPositionForGroup(fromGroupPosition);
            long packedTo = RecyclerViewExpandableItemManager.GetPackedPositionForGroup(toGroupPosition);
            int flatFrom = GetFlatPosition(packedFrom);
            int flatTo = GetFlatPosition(packedTo);
            bool fromExpanded = IsGroupExpanded(fromGroupPosition);
            bool toExpanded = IsGroupExpanded(toGroupPosition);
            mPositionTranslator.MoveGroupItem(fromGroupPosition, toGroupPosition);
            if (!fromExpanded && !toExpanded)
            {
                NotifyItemMoved(flatFrom, flatTo);
            }
            else
            {
                NotifyDataSetChanged();
            }
        }

        public virtual void NotifyChildItemMoved(int groupPosition, int fromChildPosition, int toChildPosition)
        {
            NotifyChildItemMoved(groupPosition, fromChildPosition, groupPosition, toChildPosition);
        }

        public virtual void NotifyChildItemMoved(int fromGroupPosition, int fromChildPosition, int toGroupPosition, int toChildPosition)
        {
            long packedFrom = RecyclerViewExpandableItemManager.GetPackedPositionForChild(fromGroupPosition, fromChildPosition);
            long packedTo = RecyclerViewExpandableItemManager.GetPackedPositionForChild(toGroupPosition, toChildPosition);
            int flatFrom = GetFlatPosition(packedFrom);
            int flatTo = GetFlatPosition(packedTo);
            mPositionTranslator.MoveChildItem(fromGroupPosition, fromChildPosition, toGroupPosition, toChildPosition);
            if (flatFrom != RecyclerView.NoPosition && flatTo != RecyclerView.NoPosition)
            {
                NotifyItemMoved(flatFrom, flatTo);
            }
            else if (flatFrom != RecyclerView.NoPosition)
            {
                NotifyItemRemoved(flatFrom);
            }
            else if (flatTo != RecyclerView.NoPosition)
            {
                NotifyItemInserted(flatTo);
            }
        }

        private void RaiseOnGroupExpandedSequentially(int groupPositionStart, int count, bool fromUser, Java.Lang.Object payload)
        {
            if (mOnGroupExpandListener != null)
            {
                for (int i = 0; i < count; i++)
                {
                    mOnGroupExpandListener.OnGroupExpand(groupPositionStart + i, fromUser, payload);
                }
            }
        }

        public virtual void NotifyGroupItemRemoved(int groupPosition)
        {
            long packedPosition = ExpandableAdapterHelper.GetPackedPositionForGroup(groupPosition);
            int flatPosition = mPositionTranslator.GetFlatPosition(packedPosition);
            int removedCount = mPositionTranslator.RemoveGroupItem(groupPosition);
            if (removedCount > 0)
            {
                NotifyItemRangeRemoved(flatPosition, removedCount);
            }
        }

        public virtual void NotifyGroupItemRangeRemoved(int groupPositionStart, int count)
        {
            long packedPosition = ExpandableAdapterHelper.GetPackedPositionForGroup(groupPositionStart);
            int flatPosition = mPositionTranslator.GetFlatPosition(packedPosition);
            int removedCount = mPositionTranslator.RemoveGroupItems(groupPositionStart, count);
            if (removedCount > 0)
            {
                NotifyItemRangeRemoved(flatPosition, removedCount);
            }
        }

        public virtual int GetGroupCount()
        {
            return mExpandableItemAdapter.GroupCount;
        }
        public virtual int GetChildCount(int groupPosition)
        {
            return mExpandableItemAdapter.GetChildCount(groupPosition);
        }

        public virtual int GetExpandedGroupsCount()
        {
            return mPositionTranslator.GetExpandedGroupsCount();
        }

        public virtual int GetCollapsedGroupsCount()
        {
            return mPositionTranslator.GetCollapsedGroupsCount();
        }

        public virtual bool IsAllGroupsExpanded()
        {
            return mPositionTranslator.IsAllExpanded();
        }

        public virtual bool IsAllGroupsCollapsed()
        {
            return mPositionTranslator.IsAllCollapsed();
        }

        private static IExpandableItemAdapter GetExpandableItemAdapter(RecyclerView.Adapter adapter)
        {
            return WrapperAdapterUtils.FindWrappedAdapter<IExpandableItemAdapter>(adapter);
        }

        private static void SafeUpdateExpandStateFlags(RecyclerView.ViewHolder holder, int flags)
        {
            if (!(holder is IExpandableItemViewHolder holder2))
            {
                return;
            }

            int curFlags = holder2.ExpandStateFlags;
            int mask = ~(int)ExpandableItemStateFlags.STATE_FLAG_IS_UPDATED;

            // append HAS_EXPANDED_STATE_CHANGED flag
            if ((curFlags != STATE_FLAG_INITIAL_VALUE) && (((curFlags ^ flags) & (int)ExpandableItemStateFlags.STATE_FLAG_IS_EXPANDED) != 0))
            {
                flags |= (int)ExpandableItemStateFlags.STATE_FLAG_HAS_EXPANDED_STATE_CHANGED;
            }


            // append UPDATED flag
            if ((curFlags == STATE_FLAG_INITIAL_VALUE) || (((curFlags ^ flags) & mask) != 0))
            {
                flags |= (int)ExpandableItemStateFlags.STATE_FLAG_IS_UPDATED;
            }

            holder2.ExpandStateFlags = (flags);
        }

        private void CorrectItemDragStateFlags(RecyclerView.ViewHolder holder, int groupPosition, int childPosition)
        {
            if (!(holder is IDraggableItemViewHolder))
            {
                return;
            }

            IDraggableItemViewHolder holder2 = (IDraggableItemViewHolder)holder;
            bool groupRangeSpecified = (mDraggingItemGroupRangeStart != RecyclerView.NoPosition) && (mDraggingItemGroupRangeEnd != RecyclerView.NoPosition);
            bool childRangeSpecified = (mDraggingItemChildRangeStart != RecyclerView.NoPosition) && (mDraggingItemChildRangeEnd != RecyclerView.NoPosition);
            bool isInGroupRange = (groupPosition >= mDraggingItemGroupRangeStart) && (groupPosition <= mDraggingItemGroupRangeEnd);
            bool isInChildRange = (groupPosition != RecyclerView.NoPosition) && (childPosition >= mDraggingItemChildRangeStart) && (childPosition <= mDraggingItemChildRangeEnd);
            int flags = holder2.DragStateFlags;
            bool needCorrection = false;
            if (((flags & (int)DraggableItemConstants.STATE_FLAG_DRAGGING) != 0) && ((flags & (int)DraggableItemConstants.STATE_FLAG_IS_IN_RANGE) == 0))
            {
                if (!groupRangeSpecified || isInGroupRange)
                {
                    if (!childRangeSpecified || (childRangeSpecified && isInChildRange))
                    {
                        needCorrection = true;
                    }
                }
            }

            if (needCorrection)
            {
                holder2.DragStateFlags = (flags | (int)DraggableItemConstants.STATE_FLAG_IS_IN_RANGE | (int)DraggableItemConstants.STATE_FLAG_IS_UPDATED);
            }
        }
    }
}