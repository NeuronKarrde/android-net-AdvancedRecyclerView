/*
 *    Copyright (C) 2015 Haruki Hasegawa
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
using Android.OS;
using Android.Views;
using Com.H6ah4i.Android.Widget.Advrecyclerview.Adapter;
using Com.H6ah4i.Android.Widget.Advrecyclerview.Utils;
using Java.Util;
using AndroidX.RecyclerView.Widget;
using Object = Java.Lang.Object;

namespace Com.H6ah4i.Android.Widget.Advrecyclerview.Expandable
{
    public class RecyclerViewExpandableItemManager
    {
        private static readonly string TAG = "ARVExpandableItemMgr";
        public static readonly long NO_EXPANDABLE_POSITION = ExpandableAdapterHelper.NO_EXPANDABLE_POSITION;
        public interface IOnGroupExpandListener
        {
            void OnGroupExpand(int groupPosition, bool fromUser, Java.Lang.Object payload);
        }
        public interface IOnGroupCollapseListener
        {
            void OnGroupCollapse(int groupPosition, bool fromUser, Java.Lang.Object payload);
        }
        private SavedState mSavedState;
        private RecyclerView mRecyclerView;
        private ExpandableRecyclerViewWrapperAdapter mWrapperAdapter;
        private RecyclerView.IOnItemTouchListener mInternalUseOnItemTouchListener;
        private IOnGroupExpandListener mOnGroupExpandListener;
        private IOnGroupCollapseListener mOnGroupCollapseListener;
        private long mTouchedItemId = RecyclerView.NoId;
        private int mTouchSlop;
        private int mInitialTouchX;
        private int mInitialTouchY;
        public RecyclerViewExpandableItemManager(IParcelable savedState)
        {
            mInternalUseOnItemTouchListener = new AnonymousOnItemTouchListener(this);
            if (savedState is SavedState)
            {
                mSavedState = (SavedState)savedState;
            }
        }

        private sealed class AnonymousOnItemTouchListener : Java.Lang.Object, RecyclerView.IOnItemTouchListener
        {
            public AnonymousOnItemTouchListener(RecyclerViewExpandableItemManager parent)
            {
                this.parent = parent;
            }

            private readonly RecyclerViewExpandableItemManager parent;
            public bool OnInterceptTouchEvent(RecyclerView rv, MotionEvent e)
            {
                return this.parent.OnInterceptTouchEvent(rv, e);
            }

            public void OnTouchEvent(RecyclerView rv, MotionEvent e)
            {
            }

            public void OnRequestDisallowInterceptTouchEvent(bool disallowIntercept)
            {
            }
        }
        public virtual bool IsReleased()
        {
            return (mInternalUseOnItemTouchListener == null);
        }
        public virtual void AttachRecyclerView(RecyclerView rv)
        {
            if (IsReleased())
            {
                throw new InvalidOperationException("Accessing released object");
            }

            if (mRecyclerView != null)
            {
                throw new InvalidOperationException("RecyclerView instance has already been set");
            }

            mRecyclerView = rv;
            mRecyclerView.AddOnItemTouchListener(mInternalUseOnItemTouchListener);
            mTouchSlop = ViewConfiguration.Get(mRecyclerView.Context).ScaledTouchSlop;
        }
        public virtual void Release()
        {
            if (mRecyclerView != null && mInternalUseOnItemTouchListener != null)
            {
                mRecyclerView.RemoveOnItemTouchListener(mInternalUseOnItemTouchListener);
            }

            mInternalUseOnItemTouchListener = null;
            mOnGroupExpandListener = null;
            mOnGroupCollapseListener = null;
            mRecyclerView = null;
            mSavedState = null;
        }
        public virtual RecyclerView.Adapter CreateWrappedAdapter(RecyclerView.Adapter adapter)
        {
            if (!adapter.HasStableIds)
            {
                throw new ArgumentException("The passed adapter does not support stable IDs");
            }

            if (mWrapperAdapter != null)
            {
                throw new InvalidOperationException("already have a wrapped adapter");
            }

            long[] adapterSavedState = (mSavedState != null) ? mSavedState.adapterSavedState : null;
            mSavedState = null;
            mWrapperAdapter = new ExpandableRecyclerViewWrapperAdapter(this, adapter, adapterSavedState);
            mWrapperAdapter.SetOnGroupExpandListener(mOnGroupExpandListener);
            mOnGroupExpandListener = null;
            mWrapperAdapter.SetOnGroupCollapseListener(mOnGroupCollapseListener);
            mOnGroupCollapseListener = null;
            return mWrapperAdapter;
        }
        public virtual IParcelable GetSavedState()
        {
            long[] adapterSavedState = null;
            if (mWrapperAdapter != null)
            {
                adapterSavedState = mWrapperAdapter.GetExpandedItemsSavedStateArray();
            }

            return new SavedState(adapterSavedState);
        }
        /*package*/
        public virtual bool OnInterceptTouchEvent(RecyclerView rv, MotionEvent e)
        {
            if (mWrapperAdapter == null)
            {
                return false;
            }

            var action = e.ActionMasked;
            switch (action)
            {
                case MotionEventActions.Down:
                    HandleActionDown(rv, e);
                    break;
                case MotionEventActions.Up:
                case MotionEventActions.Cancel:
                    if (HandleActionUpOrCancel(rv, e))
                    {
                        return false;
                    }

                    break;
            }

            return false;
        }
        /*package*/
        private void HandleActionDown(RecyclerView rv, MotionEvent e)
        {
            RecyclerView.ViewHolder holder = CustomRecyclerViewUtils.FindChildViewHolderUnderWithTranslation(rv, e.GetX(), e.GetY());
            mInitialTouchX = (int)(e.GetX() + 0.5F);
            mInitialTouchY = (int)(e.GetY() + 0.5F);
            if (holder is IExpandableItemViewHolder)
            {
                mTouchedItemId = holder.ItemId;
            }
            else
            {
                mTouchedItemId = RecyclerView.NoId;
            }
        }
        /*package*/
        private bool HandleActionUpOrCancel(RecyclerView rv, MotionEvent e)
        {
            long touchedItemId = mTouchedItemId;
            int initialTouchX = mInitialTouchX;
            int initialTouchY = mInitialTouchY;
            mTouchedItemId = RecyclerView.NoId;
            mInitialTouchX = 0;
            mInitialTouchY = 0;
            if (!((touchedItemId != RecyclerView.NoId) && (e.ActionMasked == MotionEventActions.Up)))
            {
                return false;
            }

            if (mRecyclerView.IsComputingLayout)
            {
                return false;
            }

            int touchX = (int)(e.GetX() + 0.5F);
            int touchY = (int)(e.GetY() + 0.5F);
            int diffX = touchX - initialTouchX;
            int diffY = touchY - initialTouchY;
            if (!((Math.Abs(diffX) < mTouchSlop) && (Math.Abs(diffY) < mTouchSlop)))
            {
                return false;
            }

            RecyclerView.ViewHolder holder = CustomRecyclerViewUtils.FindChildViewHolderUnderWithTranslation(rv, e.GetX(), e.GetY());
            if (!((holder != null) && (holder.ItemId == touchedItemId)))
            {
                return false;
            }

            RecyclerView.Adapter rootAdapter = mRecyclerView.GetAdapter();
            int rootItemPosition = CustomRecyclerViewUtils.GetSynchronizedPosition(holder);
            int wrappedItemPosition = WrapperAdapterUtils.UnwrapPosition(rootAdapter, mWrapperAdapter, rootItemPosition);
            if (wrappedItemPosition == RecyclerView.NoPosition)
            {
                return false;
            }

            View view = holder.ItemView;
            int translateX = (int)(view.TranslationX + 0.5F);
            int translateY = (int)(view.TranslationY + 0.5F);
            int viewX = touchX - (view.Left + translateX);
            int viewY = touchY - (view.Top + translateY);
            return mWrapperAdapter.OnTapItem(holder, wrappedItemPosition, viewX, viewY);
        }
        /*package*/
        public virtual void ExpandAll()
        {
            if (mWrapperAdapter != null)
            {
                mWrapperAdapter.ExpandAll();
            }
        }
        /*package*/
        public virtual void CollapseAll()
        {
            if (mWrapperAdapter != null)
            {
                mWrapperAdapter.CollapseAll();
            }
        }
        /*package*/
        public virtual bool ExpandGroup(int groupPosition)
        {
            return ExpandGroup(groupPosition, null);
        }
        /*package*/
        public virtual bool ExpandGroup(int groupPosition, Java.Lang.Object payload)
        {
            return (mWrapperAdapter != null) && mWrapperAdapter.ExpandGroup(groupPosition, false, payload);
        }
        /*package*/
        public virtual bool CollapseGroup(int groupPosition)
        {
            return CollapseGroup(groupPosition, null);
        }
        /*package*/
        public virtual bool CollapseGroup(int groupPosition, Java.Lang.Object payload)
        {
            return (mWrapperAdapter != null) && mWrapperAdapter.CollapseGroup(groupPosition, false, payload);
        }
        /*package*/
        public virtual long GetExpandablePosition(int flatPosition)
        {
            if (mWrapperAdapter == null)
            {
                return ExpandableAdapterHelper.NO_EXPANDABLE_POSITION;
            }

            return mWrapperAdapter.GetExpandablePosition(flatPosition);
        }
        /*package*/
        public virtual int GetFlatPosition(long packedPosition)
        {
            if (mWrapperAdapter == null)
            {
                return RecyclerView.NoPosition;
            }

            return mWrapperAdapter.GetFlatPosition(packedPosition);
        }
        /*package*/
        public static int GetPackedPositionChild(long packedPosition)
        {
            return ExpandableAdapterHelper.GetPackedPositionChild(packedPosition);
        }
        /*package*/
        public static long GetPackedPositionForChild(int groupPosition, int childPosition)
        {
            return ExpandableAdapterHelper.GetPackedPositionForChild(groupPosition, childPosition);
        }
        /*package*/
        public static long GetPackedPositionForGroup(int groupPosition)
        {
            return ExpandableAdapterHelper.GetPackedPositionForGroup(groupPosition);
        }
        /*package*/
        public static int GetPackedPositionGroup(long packedPosition)
        {
            return ExpandableAdapterHelper.GetPackedPositionGroup(packedPosition);
        }
        /*package*/
        public virtual bool IsGroupExpanded(int groupPosition)
        {
            return (mWrapperAdapter != null) && mWrapperAdapter.IsGroupExpanded(groupPosition);
        }
        /*package*/
        public static long GetCombinedChildId(long groupId, long childId)
        {
            return ItemIdComposer.ComposeExpandableChildId(groupId, childId);
        }
        /*package*/
        public static long GetCombinedGroupId(long groupId)
        {
            return ItemIdComposer.ComposeExpandableGroupId(groupId);
        }
        /*package*/
        public static bool IsGroupViewType(int rawViewType)
        {
            return ItemViewTypeComposer.IsExpandableGroup(rawViewType);
        }
        /*package*/
        public static int GetGroupViewType(int rawViewType)
        {
            return ItemViewTypeComposer.ExtractWrappedViewTypePart(rawViewType);
        }
        /*package*/
        public static int GetChildViewType(int rawViewType)
        {
            return ItemViewTypeComposer.ExtractWrappedViewTypePart(rawViewType);
        }
        /*package*/
        public static bool IsGroupItemId(long rawId)
        {
            return ItemIdComposer.IsExpandableGroup(rawId);
        }
        /*package*/
        public static long GetGroupItemId(long rawId)
        {
            return ItemIdComposer.ExtractExpandableGroupIdPart(rawId);
        }
        /*package*/
        public static long GetChildItemId(long rawId)
        {
            return ItemIdComposer.ExtractExpandableChildIdPart(rawId);
        }
        /*package*/
        public virtual void SetOnGroupExpandListener(IOnGroupExpandListener listener)
        {
            if (mWrapperAdapter != null)
            {
                mWrapperAdapter.SetOnGroupExpandListener(listener);
            }
            else
            {
                mOnGroupExpandListener = listener;
            }
        }
        /*package*/
        public virtual void SetOnGroupCollapseListener(IOnGroupCollapseListener listener)
        {
            if (mWrapperAdapter != null)
            {
                mWrapperAdapter.SetOnGroupCollapseListener(listener);
            }
            else
            {
                mOnGroupCollapseListener = listener;
            }
        }
        /*package*/
        public virtual void RestoreState(IParcelable savedState)
        {
            RestoreState(savedState, false, false);
        }
        /*package*/
        public virtual void RestoreState(IParcelable savedState, bool callHooks, bool callListeners)
        {
            if (savedState == null)
            {
                return;
            }

            if (!(savedState is SavedState))
            {
                throw new ArgumentException("Illegal saved state object passed");
            }

            if (!((mWrapperAdapter != null) && (mRecyclerView != null)))
            {
                throw new InvalidOperationException("RecyclerView has not been attached");
            }

            mWrapperAdapter.RestoreState(((SavedState)savedState).adapterSavedState, callHooks, callListeners);
        }
        /*package*/
        public virtual void NotifyGroupItemChanged(int groupPosition)
        {
            mWrapperAdapter.NotifyGroupItemChanged(groupPosition, null);
        }
        /*package*/
        public virtual void NotifyGroupItemChanged(int groupPosition, Java.Lang.Object payload)
        {
            mWrapperAdapter.NotifyGroupItemChanged(groupPosition, payload);
        }
        /*package*/
        public virtual void NotifyGroupAndChildrenItemsChanged(int groupPosition)
        {
            mWrapperAdapter.NotifyGroupAndChildrenItemsChanged(groupPosition, null);
        }
        /*package*/
        public virtual void NotifyGroupAndChildrenItemsChanged(int groupPosition, Java.Lang.Object payload)
        {
            mWrapperAdapter.NotifyGroupAndChildrenItemsChanged(groupPosition, payload);
        }
        /*package*/
        public virtual void NotifyChildrenOfGroupItemChanged(int groupPosition)
        {
            mWrapperAdapter.NotifyChildrenOfGroupItemChanged(groupPosition, null);
        }
        /*package*/
        public virtual void NotifyChildrenOfGroupItemChanged(int groupPosition, Java.Lang.Object payload)
        {
            mWrapperAdapter.NotifyChildrenOfGroupItemChanged(groupPosition, payload);
        }
        /*package*/
        public virtual void NotifyChildItemChanged(int groupPosition, int childPosition)
        {
            mWrapperAdapter.NotifyChildItemChanged(groupPosition, childPosition, null);
        }
        /*package*/
        public virtual void NotifyChildItemChanged(int groupPosition, int childPosition, Java.Lang.Object payload)
        {
            mWrapperAdapter.NotifyChildItemChanged(groupPosition, childPosition, payload);
        }
        /*package*/
        public virtual void NotifyChildItemRangeChanged(int groupPosition, int childPositionStart, int itemCount)
        {
            mWrapperAdapter.NotifyChildItemRangeChanged(groupPosition, childPositionStart, itemCount, null);
        }
        /*package*/
        public virtual void NotifyChildItemRangeChanged(int groupPosition, int childPositionStart, int itemCount, Java.Lang.Object payload)
        {
            mWrapperAdapter.NotifyChildItemRangeChanged(groupPosition, childPositionStart, itemCount, payload);
        }
        /*package*/
        public virtual void NotifyGroupItemInserted(int groupPosition)
        {
            NotifyGroupItemInserted(groupPosition, DefaultGroupsExpandedState);
        }
        /*package*/
        public virtual void NotifyGroupItemInserted(int groupPosition, bool expanded)
        {
            mWrapperAdapter.NotifyGroupItemInserted(groupPosition, expanded);
        }
        /*package*/
        public virtual void NotifyGroupItemRangeInserted(int groupPositionStart, int itemCount)
        {
            NotifyGroupItemRangeInserted(groupPositionStart, itemCount, DefaultGroupsExpandedState);
        }
        /*package*/
        public virtual void NotifyGroupItemRangeInserted(int groupPositionStart, int itemCount, bool expanded)
        {
            mWrapperAdapter.NotifyGroupItemRangeInserted(groupPositionStart, itemCount, expanded);
        }
        /*package*/
        public virtual void NotifyChildItemInserted(int groupPosition, int childPosition)
        {
            mWrapperAdapter.NotifyChildItemInserted(groupPosition, childPosition);
        }
        /*package*/
        public virtual void NotifyChildItemRangeInserted(int groupPosition, int childPositionStart, int itemCount)
        {
            mWrapperAdapter.NotifyChildItemRangeInserted(groupPosition, childPositionStart, itemCount);
        }
        /*package*/
        public virtual void NotifyGroupItemRemoved(int groupPosition)
        {
            mWrapperAdapter.NotifyGroupItemRemoved(groupPosition);
        }
        /*package*/
        public virtual void NotifyGroupItemRangeRemoved(int groupPositionStart, int itemCount)
        {
            mWrapperAdapter.NotifyGroupItemRangeRemoved(groupPositionStart, itemCount);
        }
        /*package*/
        public virtual void NotifyChildItemRemoved(int groupPosition, int childPosition)
        {
            mWrapperAdapter.NotifyChildItemRemoved(groupPosition, childPosition);
        }
        /*package*/
        public virtual void NotifyChildItemRangeRemoved(int groupPosition, int childPositionStart, int itemCount)
        {
            mWrapperAdapter.NotifyChildItemRangeRemoved(groupPosition, childPositionStart, itemCount);
        }
        /*package*/
        public virtual void NotifyGroupItemMoved(int fromGroupPosition, int toGroupPosition)
        {
            mWrapperAdapter.NotifyGroupItemMoved(fromGroupPosition, toGroupPosition);
        }
        /*package*/
        public virtual void NotifyChildItemMoved(int groupPosition, int fromChildPosition, int toChildPosition)
        {
            mWrapperAdapter.NotifyChildItemMoved(groupPosition, fromChildPosition, toChildPosition);
        }
        /*package*/
        public virtual void NotifyChildItemMoved(int fromGroupPosition, int fromChildPosition, int toGroupPosition, int toChildPosition)
        {
            mWrapperAdapter.NotifyChildItemMoved(fromGroupPosition, fromChildPosition, toGroupPosition, toChildPosition);
        }
        /*package*/
        public virtual int GetGroupCount()
        {
            return mWrapperAdapter.GetGroupCount();
        }
        /*package*/
        public virtual int GetChildCount(int groupPosition)
        {
            return mWrapperAdapter.GetChildCount(groupPosition);
        }
        /*package*/
        public virtual void ScrollToGroup(int groupPosition, int childItemHeight)
        {
            ScrollToGroup(groupPosition, childItemHeight, 0, 0, null);
        }
        /*package*/
        public virtual void ScrollToGroup(int groupPosition, int childItemHeight, int topMargin, int bottomMargin)
        {
            int totalChildrenHeight = GetChildCount(groupPosition) * childItemHeight;
            ScrollToGroupWithTotalChildrenHeight(groupPosition, totalChildrenHeight, topMargin, bottomMargin, null);
        }
        /*package*/
        public virtual void ScrollToGroup(int groupPosition, int childItemHeight, int topMargin, int bottomMargin, AdapterPath path)
        {
            int totalChildrenHeight = GetChildCount(groupPosition) * childItemHeight;
            ScrollToGroupWithTotalChildrenHeight(groupPosition, totalChildrenHeight, topMargin, bottomMargin, path);
        }
        /*package*/
        public virtual void ScrollToGroupWithTotalChildrenHeight(int groupPosition, int totalChildrenHeight, int topMargin, int bottomMargin)
        {
            ScrollToGroupWithTotalChildrenHeight(groupPosition, totalChildrenHeight, topMargin, bottomMargin, null);
        }
        /*package*/
        public virtual void ScrollToGroupWithTotalChildrenHeight(int groupPosition, int totalChildrenHeight, int topMargin, int bottomMargin, AdapterPath path)
        {
            long packedPosition = RecyclerViewExpandableItemManager.GetPackedPositionForGroup(groupPosition);
            int flatPosition = GetFlatPosition(packedPosition);
            if (path != null)
            {
                flatPosition = WrapperAdapterUtils.WrapPosition(path, mWrapperAdapter, mRecyclerView.GetAdapter(), flatPosition);
            }

            RecyclerView.ViewHolder vh = mRecyclerView.FindViewHolderForLayoutPosition(flatPosition);
            if (vh == null)
            {
                return;
            }

            if (!IsGroupExpanded(groupPosition))
            {
                totalChildrenHeight = 0;
            }

            int groupItemTop = vh.ItemView.Top;
            int groupItemBottom = vh.ItemView.Bottom;
            int parentHeight = mRecyclerView.Height;
            int topRoom = groupItemTop;
            int bottomRoom = parentHeight - groupItemBottom;
            if (topRoom <= topMargin)
            {
                int parentTopPadding = mRecyclerView.PaddingTop;
                int itemTopMargin = ((RecyclerView.LayoutParams)vh.ItemView.LayoutParameters).TopMargin;
                int offset = topMargin - parentTopPadding - itemTopMargin;
                ((LinearLayoutManager)mRecyclerView.GetLayoutManager()).ScrollToPositionWithOffset(flatPosition, offset);
            }
            else if (bottomRoom >= (totalChildrenHeight + bottomMargin))
            {
            }
            else
            {
                int scrollAmount = Math.Max(0, totalChildrenHeight + bottomMargin - bottomRoom);
                scrollAmount = Math.Min(topRoom - topMargin, scrollAmount);
                mRecyclerView.SmoothScrollBy(0, scrollAmount);
            }
        }
        /*package*/
        public virtual int GetExpandedGroupsCount()
        {
            return mWrapperAdapter.GetExpandedGroupsCount();
        }
        /*package*/
        public virtual int GetCollapsedGroupsCount()
        {
            return mWrapperAdapter.GetCollapsedGroupsCount();
        }
        /*package*/
        public virtual bool IsAllGroupsExpanded()
        {
            return mWrapperAdapter.IsAllGroupsExpanded();
        }
        /*package*/
        public virtual bool IsAllGroupsCollapsed()
        {
            return mWrapperAdapter.IsAllGroupsCollapsed();
        }
        public virtual bool DefaultGroupsExpandedState { get; set; }
        /*package*/
        public class SavedState : Java.Lang.Object, IParcelable
        {
            public readonly long[] adapterSavedState;
            public SavedState(long[] adapterSavedState)
            {
                this.adapterSavedState = adapterSavedState;
                CREATOR = new AnonymousCreator();
            }

            public SavedState(Parcel parcel) : this(parcel.CreateLongArray())
            {
            }

            public virtual int DescribeContents()
            {
                return 0;
            }

            public void WriteToParcel(Parcel dest, ParcelableWriteFlags flags)
            {
                dest.WriteLongArray(this.adapterSavedState);
            }

            public readonly IParcelableCreator CREATOR;
            private sealed class AnonymousCreator : Java.Lang.Object, IParcelableCreator
            {
                public AnonymousCreator()
                {
                }
                
                public Object? CreateFromParcel(Parcel? source)
                {
                    return new RecyclerViewExpandableItemManager.SavedState(source);
                }

                public Object[]? NewArray(int size)
                {
                    return new SavedState[size];
                }
            }
        }
    }
}