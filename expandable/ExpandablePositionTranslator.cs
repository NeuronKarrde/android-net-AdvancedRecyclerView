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
using Java.Util;
using AndroidX.RecyclerView.Widget;

namespace Com.H6ah4i.Android.Widget.Advrecyclerview.Expandable
{
    class ExpandablePositionTranslator
    {
        public static readonly int BUILD_OPTION_DEFAULT = 0;
        public static readonly int BUILD_OPTION_EXPANDED_ALL = 1;
        public static readonly int BUILD_OPTION_COLLAPSED_ALL = 2;
        private static readonly int ALLOCATE_UNIT = 256;
        private static readonly long FLAG_EXPANDED = 0x0000000080000000;
        private static readonly long LOWER_31BIT_MASK = 0x000000007fffffff;
        private static readonly long LOWER_32BIT_MASK = 0x00000000ffffffff;
        private static readonly long UPPER_32BIT_MASK = unchecked((long)0xffffffff00000000UL);
        /*
         * bit 64-32: offset  (use for caching purpose)
         * bit 31:    expanded or not
         * bit 30-0:  child count
         */
        private long[] mCachedGroupPosInfo;
        /*
         * bit 31: reserved
         * bit 30-0: group id
         */
        private int[] mCachedGroupId;
        private int mGroupCount;
        private int mExpandedGroupCount;
        private int mExpandedChildCount;
        private int mEndOfCalculatedOffsetGroupPosition = RecyclerView.NoPosition;
        private IExpandableItemAdapter mAdapter;
        public ExpandablePositionTranslator()
        {
        }

        public virtual void Build(IExpandableItemAdapter adapter, int option, bool defaultExpandedState)
        {
            int groupCount = adapter.GroupCount;
            EnlargeArraysIfNeeded(groupCount, false);
            long[] info = mCachedGroupPosInfo;
            int[] ids = mCachedGroupId;
            int expandedGroupCount = 0;
            int expandedChildCount = 0;
            for (int i = 0; i < groupCount; i++)
            {
                long groupId = adapter.GetGroupId(i);
                int childCount = adapter.GetChildCount(i);
                bool expanded;
                if (option == BUILD_OPTION_EXPANDED_ALL)
                {
                    expanded = true;
                }
                else if (option == BUILD_OPTION_COLLAPSED_ALL)
                {
                    expanded = false;
                }
                else
                {
                    expanded = defaultExpandedState || adapter.GetInitialGroupExpandedState(i);
                }

                info[i] = (((long)(i + expandedChildCount) << 32) | childCount) | (expanded ? FLAG_EXPANDED : 0);
                ids[i] = (int)(groupId & LOWER_32BIT_MASK);
                if (expanded)
                {
                    expandedGroupCount += 1;
                    expandedChildCount += childCount;
                }
            }

            mAdapter = adapter;
            mGroupCount = groupCount;
            mExpandedGroupCount = expandedGroupCount;
            mExpandedChildCount = expandedChildCount;
            mEndOfCalculatedOffsetGroupPosition = Math.Max(0, groupCount - 1);
        }

        public virtual void RestoreExpandedGroupItems(long[] restoreGroupIds, IExpandableItemAdapter adapter, RecyclerViewExpandableItemManager.IOnGroupExpandListener expandListener, RecyclerViewExpandableItemManager.IOnGroupCollapseListener collapseListener)
        {
            if (restoreGroupIds == null || restoreGroupIds.Length == 0)
            {
                return;
            }

            if (mCachedGroupPosInfo == null)
            {
                return;
            }


            // make ID + position packed array
            long[] idAndPos = new long[mGroupCount];
            for (int i = 0; i < mGroupCount; i++)
            {
                idAndPos[i] = ((long)mCachedGroupId[i] << 32) | i;
            }


            // sort both arrays
            Arrays.Sort(idAndPos);
            bool fromUser = false;

            // find matched items & apply
            int index = 0;

            //noinspection ForLoopReplaceableByForEach
            for (int i = 0; i < restoreGroupIds.Length; i++)
            {
                int id1 = (int)(restoreGroupIds[i] >>> 32);
                bool expanded = ((restoreGroupIds[i] & FLAG_EXPANDED) != 0);
                for (int j = index; j < idAndPos.Length; j++)
                {
                    int id2 = (int)(idAndPos[j] >>> 32);
                    int position = (int)(idAndPos[j] & LOWER_31BIT_MASK);
                    if (id2 < id1)
                    {
                        index = j;
                    }
                    else if (id2 == id1)
                    {

                        // matched
                        index = j + 1;
                        if (expanded)
                        {
                            if (adapter == null || adapter.OnHookGroupExpand(position, fromUser, null))
                            {
                                if (ExpandGroup(position))
                                {
                                    if (expandListener != null)
                                    {
                                        expandListener.OnGroupExpand(position, fromUser, null);
                                    }
                                }
                            }
                        }
                        else
                        {
                            if (adapter == null || adapter.OnHookGroupCollapse(position, fromUser, null))
                            {
                                if (CollapseGroup(position))
                                {
                                    if (collapseListener != null)
                                    {
                                        collapseListener.OnGroupCollapse(position, fromUser, null);
                                    }
                                }
                            }
                        }
                    } // id2 > id1
                    else
                    {

                        // id2 > id1
                        break;
                    }
                }
            }
        }

        public virtual long[] GetSavedStateArray()
        {

            // bit 64-32: group id
            // bit 31:    expanded or not
            // bit 30-0:  reserved
            long[] expandedGroups = new long[mGroupCount];
            for (int i = 0; i < mGroupCount; i++)
            {
                long t = mCachedGroupPosInfo[i];
                expandedGroups[i] = ((long)mCachedGroupId[i] << 32) | (t & FLAG_EXPANDED);
            }

            Arrays.Sort(expandedGroups);
            return expandedGroups;
        }

        public virtual int GetItemCount()
        {
            return mGroupCount + mExpandedChildCount;
        }

        public virtual bool IsGroupExpanded(int groupPosition)
        {
            return ((mCachedGroupPosInfo[groupPosition] & FLAG_EXPANDED) != 0);
        }

        public virtual int GetChildCount(int groupPosition)
        {
            return (int)(mCachedGroupPosInfo[groupPosition] & LOWER_31BIT_MASK);
        }

        public virtual int GetVisibleChildCount(int groupPosition)
        {
            if (IsGroupExpanded(groupPosition))
            {
                return GetChildCount(groupPosition);
            }
            else
            {
                return 0;
            }
        }

        public virtual bool CollapseGroup(int groupPosition)
        {
            if ((mCachedGroupPosInfo[groupPosition] & FLAG_EXPANDED) == 0)
            {
                return false;
            }

            int childCount = (int)(mCachedGroupPosInfo[groupPosition] & LOWER_31BIT_MASK);
            mCachedGroupPosInfo[groupPosition] &= (~FLAG_EXPANDED);
            mExpandedGroupCount -= 1;
            mExpandedChildCount -= childCount;
            mEndOfCalculatedOffsetGroupPosition = Math.Min(mEndOfCalculatedOffsetGroupPosition, groupPosition);

            // requires notifyItemRangeRemoved()
            return true;
        }

        public virtual bool ExpandGroup(int groupPosition)
        {
            if ((mCachedGroupPosInfo[groupPosition] & FLAG_EXPANDED) != 0)
            {
                return false;
            }

            int childCount = (int)(mCachedGroupPosInfo[groupPosition] & LOWER_31BIT_MASK);
            mCachedGroupPosInfo[groupPosition] |= FLAG_EXPANDED;
            mExpandedGroupCount += 1;
            mExpandedChildCount += childCount;
            mEndOfCalculatedOffsetGroupPosition = Math.Min(mEndOfCalculatedOffsetGroupPosition, groupPosition);

            // requires notifyItemRangeInserted()
            return true;
        }

        public virtual void MoveGroupItem(int fromGroupPosition, int toGroupPosition)
        {
            if (fromGroupPosition == toGroupPosition)
            {
                return;
            }

            long tmp1 = mCachedGroupPosInfo[fromGroupPosition];
            int tmp2 = mCachedGroupId[fromGroupPosition];
            if (toGroupPosition < fromGroupPosition)
            {

                // shift to backward
                for (int i = fromGroupPosition; i > toGroupPosition; i--)
                {
                    mCachedGroupPosInfo[i] = mCachedGroupPosInfo[i - 1];
                    mCachedGroupId[i] = mCachedGroupId[i - 1];
                }
            }
            else
            {

                // shift to forward
                for (int i = fromGroupPosition; i < toGroupPosition; i++)
                {
                    mCachedGroupPosInfo[i] = mCachedGroupPosInfo[i + 1];
                    mCachedGroupId[i] = mCachedGroupId[i + 1];
                }
            }

            mCachedGroupPosInfo[toGroupPosition] = tmp1;
            mCachedGroupId[toGroupPosition] = tmp2;
            int minPosition = Math.Min(fromGroupPosition, toGroupPosition);
            if (minPosition > 0)
            {
                mEndOfCalculatedOffsetGroupPosition = Math.Min(mEndOfCalculatedOffsetGroupPosition, minPosition - 1);
            }
            else
            {
                mEndOfCalculatedOffsetGroupPosition = RecyclerView.NoPosition;
            }
        }

        public virtual void MoveChildItem(int fromGroupPosition, int fromChildPosition, int toGroupPosition, int toChildPosition)
        {
            if (fromGroupPosition == toGroupPosition)
            {
                return;
            }

            int fromChildCount = (int)(mCachedGroupPosInfo[fromGroupPosition] & LOWER_31BIT_MASK);
            int toChildCount = (int)(mCachedGroupPosInfo[toGroupPosition] & LOWER_31BIT_MASK);
            if (fromChildCount == 0)
            {
                throw new InvalidOperationException("moveChildItem(" + "fromGroupPosition = " + fromGroupPosition + ", fromChildPosition = " + fromChildPosition + ", toGroupPosition = " + toGroupPosition + ", toChildPosition = " + toChildPosition + ")  --- may be a bug.");
            }

            mCachedGroupPosInfo[fromGroupPosition] = (mCachedGroupPosInfo[fromGroupPosition] & (UPPER_32BIT_MASK | FLAG_EXPANDED)) | (fromChildCount - 1);
            mCachedGroupPosInfo[toGroupPosition] = (mCachedGroupPosInfo[toGroupPosition] & (UPPER_32BIT_MASK | FLAG_EXPANDED)) | (toChildCount + 1);
            if ((mCachedGroupPosInfo[fromGroupPosition] & FLAG_EXPANDED) != 0)
            {
                mExpandedChildCount -= 1;
            }

            if ((mCachedGroupPosInfo[toGroupPosition] & FLAG_EXPANDED) != 0)
            {
                mExpandedChildCount += 1;
            }

            int minPosition = Math.Min(fromGroupPosition, toGroupPosition);
            if (minPosition > 0)
            {
                mEndOfCalculatedOffsetGroupPosition = Math.Min(mEndOfCalculatedOffsetGroupPosition, minPosition - 1);
            }
            else
            {
                mEndOfCalculatedOffsetGroupPosition = RecyclerView.NoPosition;
            }
        }

        public virtual long GetExpandablePosition(int flatPosition)
        {
            if (flatPosition == RecyclerView.NoPosition)
            {
                return ExpandableAdapterHelper.NO_EXPANDABLE_POSITION;
            }

            int groupCount = mGroupCount;

            // final int startIndex = 0;
            int startIndex = BinarySearchGroupPositionByFlatPosition(mCachedGroupPosInfo, mEndOfCalculatedOffsetGroupPosition, flatPosition);
            long expandablePosition = ExpandableAdapterHelper.NO_EXPANDABLE_POSITION;
            int endOfCalculatedOffsetGroupPosition = mEndOfCalculatedOffsetGroupPosition;
            int offset = (startIndex == 0) ? 0 : (int)(mCachedGroupPosInfo[startIndex] >>> 32);
            for (int i = startIndex; i < groupCount; i++)
            {
                long t = mCachedGroupPosInfo[i];

                // update offset info
                mCachedGroupPosInfo[i] = (((long)offset << 32) | (t & LOWER_32BIT_MASK));
                endOfCalculatedOffsetGroupPosition = i;
                if (offset >= flatPosition)
                {

                    // found (group item)
                    expandablePosition = ExpandableAdapterHelper.GetPackedPositionForGroup(i);
                    break;
                }
                else
                {
                    offset += 1;
                }

                if ((t & FLAG_EXPANDED) != 0)
                {
                    int childCount = (int)(t & LOWER_31BIT_MASK);
                    if ((childCount > 0) && (offset + childCount - 1) >= flatPosition)
                    {

                        // found (child item)
                        expandablePosition = ExpandableAdapterHelper.GetPackedPositionForChild(i, (flatPosition - offset));
                        break;
                    }
                    else
                    {
                        offset += childCount;
                    }
                }
            }

            mEndOfCalculatedOffsetGroupPosition = Math.Max(mEndOfCalculatedOffsetGroupPosition, endOfCalculatedOffsetGroupPosition);
            return expandablePosition;
        }

        public virtual int GetFlatPosition(long packedPosition)
        {
            if (packedPosition == ExpandableAdapterHelper.NO_EXPANDABLE_POSITION)
            {
                return RecyclerView.NoPosition;
            }

            int groupPosition = ExpandableAdapterHelper.GetPackedPositionGroup(packedPosition);
            int childPosition = ExpandableAdapterHelper.GetPackedPositionChild(packedPosition);
            int groupCount = mGroupCount;
            if (!(groupPosition >= 0 && groupPosition < groupCount))
            {
                return RecyclerView.NoPosition;
            }

            if (childPosition != RecyclerView.NoPosition)
            {
                if (!IsGroupExpanded(groupPosition))
                {
                    return RecyclerView.NoPosition;
                }
            }


            // final int startIndex = 0;
            int startIndex = Math.Max(0, Math.Min(groupPosition, mEndOfCalculatedOffsetGroupPosition));
            int endOfCalculatedOffsetGroupPosition = mEndOfCalculatedOffsetGroupPosition;
            int offset = (int)(mCachedGroupPosInfo[startIndex] >>> 32);
            int flatPosition = RecyclerView.NoPosition;
            for (int i = startIndex; i < groupCount; i++)
            {
                long t = mCachedGroupPosInfo[i];

                // update offset info
                mCachedGroupPosInfo[i] = (((long)offset << 32) | (t & LOWER_32BIT_MASK));
                endOfCalculatedOffsetGroupPosition = i;
                int childCount = (int)(t & LOWER_31BIT_MASK);
                if (i == groupPosition)
                {
                    if (childPosition == RecyclerView.NoPosition)
                    {
                        flatPosition = offset;
                    }
                    else if (childPosition < childCount)
                    {
                        flatPosition = (offset + 1) + childPosition;
                    }

                    break;
                }
                else
                {
                    offset += 1;
                    if ((t & FLAG_EXPANDED) != 0)
                    {
                        offset += childCount;
                    }
                }
            }

            mEndOfCalculatedOffsetGroupPosition = Math.Max(mEndOfCalculatedOffsetGroupPosition, endOfCalculatedOffsetGroupPosition);
            return flatPosition;
        }

        private static int BinarySearchGroupPositionByFlatPosition(long[] array, int endArrayPosition, int flatPosition)
        {
            if (endArrayPosition <= 0)
            {
                return 0;
            }

            int v1 = (int)(array[0] >>> 32);
            int v2 = (int)(array[endArrayPosition] >>> 32);
            if (flatPosition <= v1)
            {
                return 0;
            }
            else if (flatPosition >= v2)
            {
                return endArrayPosition;
            }

            int lastS = 0;
            int s = 0;
            int e = endArrayPosition;
            while (s < e)
            {
                int mid = (s + e) >>> 1;
                int v = (int)(array[mid] >>> 32);
                if (v < flatPosition)
                {
                    lastS = s;
                    s = mid + 1;
                }
                else
                {
                    e = mid;
                }
            }

            return lastS;
        }

        public virtual void RemoveChildItem(int groupPosition, int childPosition)
        {
            RemoveChildItems(groupPosition, childPosition, 1);
        }

        public virtual void RemoveChildItems(int groupPosition, int childPositionStart, int count)
        {
            long t = mCachedGroupPosInfo[groupPosition];
            int curCount = (int)(t & LOWER_31BIT_MASK);
            if (!((childPositionStart >= 0) && ((childPositionStart + count) <= curCount)))
            {
                throw new InvalidOperationException("Invalid child position " + "removeChildItems(groupPosition = " + groupPosition + ", childPosition = " + childPositionStart + ", count = " + count + ")");
            }

            if ((t & FLAG_EXPANDED) != 0)
            {
                mExpandedChildCount -= count;
            }

            mCachedGroupPosInfo[groupPosition] = (t & (UPPER_32BIT_MASK | FLAG_EXPANDED)) | (curCount - count);
            mEndOfCalculatedOffsetGroupPosition = Math.Min(mEndOfCalculatedOffsetGroupPosition, groupPosition - 1);
        }

        public virtual void InsertChildItem(int groupPosition, int childPosition)
        {
            InsertChildItems(groupPosition, childPosition, 1);
        }

        public virtual void InsertChildItems(int groupPosition, int childPositionStart, int count)
        {
            long t = mCachedGroupPosInfo[groupPosition];
            int curCount = (int)(t & LOWER_31BIT_MASK);
            if (!((childPositionStart >= 0) && (childPositionStart <= curCount)))
            {
                throw new InvalidOperationException("Invalid child position " + "insertChildItems(groupPosition = " + groupPosition + ", childPositionStart = " + childPositionStart + ", count = " + count + ")");
            }

            if ((t & FLAG_EXPANDED) != 0)
            {
                mExpandedChildCount += count;
            }

            mCachedGroupPosInfo[groupPosition] = (t & (UPPER_32BIT_MASK | FLAG_EXPANDED)) | (curCount + count);
            mEndOfCalculatedOffsetGroupPosition = Math.Min(mEndOfCalculatedOffsetGroupPosition, groupPosition);
        }

        public virtual int InsertGroupItems(int groupPosition, int count, bool expanded)
        {
            if (count <= 0)
            {
                return 0;
            }


            //noinspection UnnecessaryLocalVariable
            int n = count;
            EnlargeArraysIfNeeded(mGroupCount + n, true);

            // shift to backward
            IExpandableItemAdapter adapter = mAdapter;
            long[] info = mCachedGroupPosInfo;
            int[] ids = mCachedGroupId;
            int start = mGroupCount - 1 + n;
            int end = groupPosition - 1 + n;
            for (int i = start; i > end; i--)
            {
                info[i] = info[i - n];
                ids[i] = ids[i - n];
            }


            // insert items
            long expandedFlag = (expanded) ? FLAG_EXPANDED : 0;
            int insertedChildCount = 0;
            int end2 = groupPosition + n;
            for (int i = groupPosition; i < end2; i++)
            {
                long groupId = adapter.GetGroupId(i);
                int childCount = adapter.GetChildCount(i);
                info[i] = (((long)i << 32) | childCount) | expandedFlag;
                ids[i] = (int)(groupId & LOWER_32BIT_MASK);
                insertedChildCount += childCount;
            }

            mGroupCount += n;
            if (expanded)
            {
                mExpandedGroupCount += n;
                mExpandedChildCount += insertedChildCount;
            }

            int calculatedOffset = (mGroupCount == 0) ? RecyclerView.NoPosition : (groupPosition - 1);
            mEndOfCalculatedOffsetGroupPosition = Math.Min(mEndOfCalculatedOffsetGroupPosition, calculatedOffset);
            return (expanded) ? (n + insertedChildCount) : n;
        }

        public virtual int InsertGroupItem(int groupPosition, bool expanded)
        {
            return InsertGroupItems(groupPosition, 1, expanded);
        }

        public virtual int RemoveGroupItems(int groupPosition, int count)
        {
            if (count <= 0)
            {
                return 0;
            }


            //noinspection UnnecessaryLocalVariable
            int n = count;
            int removedVisibleItemCount = 0;
            for (int i = 0; i < n; i++)
            {
                long t = mCachedGroupPosInfo[groupPosition + i];
                if ((t & FLAG_EXPANDED) != 0)
                {
                    int visibleChildCount = (int)(t & LOWER_31BIT_MASK);
                    removedVisibleItemCount += visibleChildCount;
                    mExpandedChildCount -= visibleChildCount;
                    mExpandedGroupCount -= 1;
                }
            }

            removedVisibleItemCount += n;
            mGroupCount -= n;

            // shift to forward
            for (int i = groupPosition; i < mGroupCount; i++)
            {
                mCachedGroupPosInfo[i] = mCachedGroupPosInfo[i + n];
                mCachedGroupId[i] = mCachedGroupId[i + n];
            }

            int calculatedOffset = (mGroupCount == 0) ? RecyclerView.NoPosition : (groupPosition - 1);
            mEndOfCalculatedOffsetGroupPosition = Math.Min(mEndOfCalculatedOffsetGroupPosition, calculatedOffset);
            return removedVisibleItemCount;
        }

        public virtual int RemoveGroupItem(int groupPosition)
        {
            return RemoveGroupItems(groupPosition, 1);
        }

        private void EnlargeArraysIfNeeded(int size, bool preserveData)
        {
            int allocSize = (size + (2 * ALLOCATE_UNIT - 1)) & ~(ALLOCATE_UNIT - 1);
            long[] curInfo = mCachedGroupPosInfo;
            int[] curId = mCachedGroupId;
            long[] newInfo = curInfo;
            int[] newId = curId;
            if (curInfo == null || curInfo.Length < size)
            {
                newInfo = new long[allocSize];
            }

            if (curId == null || curId.Length < size)
            {
                newId = new int[allocSize];
            }

            if (preserveData)
            {
                if (curInfo != null && curInfo != newInfo)
                {
                    Java.Lang.JavaSystem.Arraycopy(curInfo, 0, newInfo, 0, curInfo.Length);
                }

                if (curId != null && curId != newId)
                {
                    Java.Lang.JavaSystem.Arraycopy(curId, 0, newId, 0, curId.Length);
                }
            }

            mCachedGroupPosInfo = newInfo;
            mCachedGroupId = newId;
        }

        public virtual int GetExpandedGroupsCount()
        {
            return mExpandedGroupCount;
        }

        public virtual int GetCollapsedGroupsCount()
        {
            return mGroupCount - mExpandedGroupCount;
        }

        public virtual bool IsAllExpanded()
        {
            return !IsEmpty() && (mExpandedGroupCount == mGroupCount);
        }

        public virtual bool IsAllCollapsed()
        {
            return IsEmpty() || (mExpandedGroupCount == 0);
        }

        public virtual bool IsEmpty()
        {
            return mGroupCount == 0;
        }
    }
}