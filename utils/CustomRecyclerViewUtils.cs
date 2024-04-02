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
using Android.Graphics;
using Android.Views;
using AndroidX.Core.View;
using AndroidX.RecyclerView.Widget;

namespace Com.H6ah4i.Android.Widget.Advrecyclerview.Utils
{
    public class CustomRecyclerViewUtils
    {
        public const int ORIENTATION_UNKNOWN = -1;
        public const int ORIENTATION_HORIZONTAL = OrientationHelper.Horizontal; // = 0
        public const int ORIENTATION_VERTICAL = OrientationHelper.Vertical; // = 1
        public const int LAYOUT_TYPE_UNKNOWN = -1;
        public const int LAYOUT_TYPE_LINEAR_HORIZONTAL = 0;
        public const int LAYOUT_TYPE_LINEAR_VERTICAL = 1;
        public const int LAYOUT_TYPE_GRID_HORIZONTAL = 2;
        public const int LAYOUT_TYPE_GRID_VERTICAL = 3;
        public const int LAYOUT_TYPE_STAGGERED_GRID_HORIZONTAL = 4;
        public const int LAYOUT_TYPE_STAGGERED_GRID_VERTICAL = 5;
        public const int INVALID_SPAN_ID = -1;
        public const int INVALID_SPAN_COUNT = -1;
        public static RecyclerView.ViewHolder FindChildViewHolderUnderWithoutTranslation(RecyclerView rv, float x, float y)
        {
            View child = FindChildViewUnderWithoutTranslation(rv, x, y);
            return (child != null) ? rv.GetChildViewHolder(child) : null;
        }

        public static int GetLayoutType(RecyclerView rv)
        {
            return GetLayoutType(rv.GetLayoutManager());
        }

        public static int ExtractOrientation(int layoutType)
        {
            switch (layoutType)
            {
                case LAYOUT_TYPE_UNKNOWN:
                    return ORIENTATION_UNKNOWN;
                case LAYOUT_TYPE_LINEAR_HORIZONTAL:
                case LAYOUT_TYPE_GRID_HORIZONTAL:
                case LAYOUT_TYPE_STAGGERED_GRID_HORIZONTAL:
                    return ORIENTATION_HORIZONTAL;
                case LAYOUT_TYPE_LINEAR_VERTICAL:
                case LAYOUT_TYPE_GRID_VERTICAL:
                case LAYOUT_TYPE_STAGGERED_GRID_VERTICAL:
                    return ORIENTATION_VERTICAL;
                default:
                    throw new ArgumentException("Unknown layout type (= " + layoutType + ")");
                    break;
            }
        }

        public static int GetLayoutType(RecyclerView.LayoutManager layoutManager)
        {
            if (layoutManager is GridLayoutManager)
            {
                if (((GridLayoutManager)layoutManager).Orientation == RecyclerView.Horizontal)
                {
                    return LAYOUT_TYPE_GRID_HORIZONTAL;
                }
                else
                {
                    return LAYOUT_TYPE_GRID_VERTICAL;
                }
            }
            else if (layoutManager is LinearLayoutManager)
            {
                if (((LinearLayoutManager)layoutManager).Orientation == RecyclerView.Horizontal)
                {
                    return LAYOUT_TYPE_LINEAR_HORIZONTAL;
                }
                else
                {
                    return LAYOUT_TYPE_LINEAR_VERTICAL;
                }
            }
            else if (layoutManager is StaggeredGridLayoutManager)
            {
                if (((StaggeredGridLayoutManager)layoutManager).Orientation == StaggeredGridLayoutManager.Horizontal)
                {
                    return LAYOUT_TYPE_STAGGERED_GRID_HORIZONTAL;
                }
                else
                {
                    return LAYOUT_TYPE_STAGGERED_GRID_VERTICAL;
                }
            }
            else
            {
                return LAYOUT_TYPE_UNKNOWN;
            }
        }

        private static View FindChildViewUnderWithoutTranslation(ViewGroup parent, float x, float y)
        {
            int count = parent.ChildCount;
            for (int i = count - 1; i >= 0; i--)
            {
                View child = parent.GetChildAt(i);
                if (x >= child.Left && x <= child.Right && y >= child.Top && y <= child.Bottom)
                {
                    return child;
                }
            }

            return null;
        }

        public static RecyclerView.ViewHolder FindChildViewHolderUnderWithTranslation(RecyclerView rv, float x, float y)
        {
            View child = rv.FindChildViewUnder(x, y);
            return (child != null) ? rv.GetChildViewHolder(child) : null;
        }

        public static Rect GetLayoutMargins(View v, Rect outMargins)
        {
            ViewGroup.LayoutParams layoutParams = v.LayoutParameters;
            if (layoutParams is ViewGroup.MarginLayoutParams)
            {
                ViewGroup.MarginLayoutParams marginLayoutParams = (ViewGroup.MarginLayoutParams)layoutParams;
                outMargins.Left = marginLayoutParams.LeftMargin;
                outMargins.Right = marginLayoutParams.RightMargin;
                outMargins.Top = marginLayoutParams.TopMargin;
                outMargins.Bottom = marginLayoutParams.BottomMargin;
            }
            else
            {
                outMargins.Left = outMargins.Right = outMargins.Top = outMargins.Bottom = 0;
            }

            return outMargins;
        }

        public static Rect GetDecorationOffsets(RecyclerView.LayoutManager layoutManager, View view, Rect outDecorations)
        {
            outDecorations.Left = layoutManager.GetLeftDecorationWidth(view);
            outDecorations.Right = layoutManager.GetRightDecorationWidth(view);
            outDecorations.Top = layoutManager.GetTopDecorationHeight(view);
            outDecorations.Bottom = layoutManager.GetBottomDecorationHeight(view);
            return outDecorations;
        }

        public static Rect GetViewBounds(View v, Rect outBounds)
        {
            outBounds.Left = v.Left;
            outBounds.Right = v.Right;
            outBounds.Top = v.Top;
            outBounds.Bottom = v.Bottom;
            return outBounds;
        }

        public static int FindFirstVisibleItemPosition(RecyclerView rv, bool includesPadding)
        {
            RecyclerView.LayoutManager layoutManager = rv.GetLayoutManager();
            if (layoutManager is LinearLayoutManager)
            {
                if (includesPadding)
                {
                    return FindFirstVisibleItemPositionIncludesPadding((LinearLayoutManager)layoutManager);
                }
                else
                {
                    return (((LinearLayoutManager)layoutManager).FindFirstVisibleItemPosition());
                }
            }
            else
            {
                return RecyclerView.NoPosition;
            }
        }

        public static int FindLastVisibleItemPosition(RecyclerView rv, bool includesPadding)
        {
            RecyclerView.LayoutManager layoutManager = rv.GetLayoutManager();
            if (layoutManager is LinearLayoutManager)
            {
                if (includesPadding)
                {
                    return FindLastVisibleItemPositionIncludesPadding((LinearLayoutManager)layoutManager);
                }
                else
                {
                    return (((LinearLayoutManager)layoutManager).FindLastVisibleItemPosition());
                }
            }
            else
            {
                return RecyclerView.NoPosition;
            }
        }

        public static int FindFirstCompletelyVisibleItemPosition(RecyclerView rv)
        {
            RecyclerView.LayoutManager layoutManager = rv.GetLayoutManager();
            if (layoutManager is LinearLayoutManager)
            {
                return (((LinearLayoutManager)layoutManager).FindFirstCompletelyVisibleItemPosition());
            }
            else
            {
                return RecyclerView.NoPosition;
            }
        }

        public static int FindLastCompletelyVisibleItemPosition(RecyclerView rv)
        {
            RecyclerView.LayoutManager layoutManager = rv.GetLayoutManager();
            if (layoutManager is LinearLayoutManager)
            {
                return (((LinearLayoutManager)layoutManager).FindLastCompletelyVisibleItemPosition());
            }
            else
            {
                return RecyclerView.NoPosition;
            }
        }

        public static int GetSynchronizedPosition(RecyclerView.ViewHolder holder)
        {
            int pos1 = holder.LayoutPosition;
            int pos2 = holder.AdapterPosition;
            if (pos1 == pos2)
            {
                return pos1;
            }
            else
            {
                return RecyclerView.NoPosition;
            }
        }

        public static int GetSpanCount(RecyclerView rv)
        {
            RecyclerView.LayoutManager layoutManager = rv.GetLayoutManager();
            if (layoutManager is GridLayoutManager)
            {
                return ((GridLayoutManager)layoutManager).SpanCount;
            }
            else if (layoutManager is StaggeredGridLayoutManager)
            {
                return ((StaggeredGridLayoutManager)layoutManager).SpanCount;
            }
            else
            {
                return 1;
            }
        }

        public static int GetOrientation(RecyclerView rv)
        {
            return GetOrientation(rv.GetLayoutManager());
        }

        public static int GetOrientation(RecyclerView.LayoutManager layoutManager)
        {
            if (layoutManager is GridLayoutManager)
            {
                return ((GridLayoutManager)layoutManager).Orientation;
            }
            else if (layoutManager is LinearLayoutManager)
            {
                return ((LinearLayoutManager)layoutManager).Orientation;
            }
            else if (layoutManager is StaggeredGridLayoutManager)
            {
                return ((StaggeredGridLayoutManager)layoutManager).Orientation;
            }
            else
            {
                return ORIENTATION_UNKNOWN;
            }
        }

        private static int FindFirstVisibleItemPositionIncludesPadding(LinearLayoutManager lm)
        {
            View child = FindOneVisibleChildIncludesPadding(lm, 0, lm.ChildCount, false, true);
            return child == null ? RecyclerView.NoPosition : lm.GetPosition(child);
        }

        private static int FindLastVisibleItemPositionIncludesPadding(LinearLayoutManager lm)
        {
            View child = FindOneVisibleChildIncludesPadding(lm, lm.ChildCount - 1, -1, false, true);
            return child == null ? RecyclerView.NoPosition : lm.GetPosition(child);
        }

        // This method is a modified version of the LinearLayoutManager.findOneVisibleChild().
        private static View FindOneVisibleChildIncludesPadding(LinearLayoutManager lm, int fromIndex, int toIndex, bool completelyVisible, bool acceptPartiallyVisible)
        {
            bool isVertical = (lm.Orientation == RecyclerView.Vertical);
            int start = 0;
            int end = (isVertical) ? lm.Height : lm.Width;
            int next = toIndex > fromIndex ? 1 : -1;
            View partiallyVisible = null;
            for (int i = fromIndex; i != toIndex; i += next)
            {
                View child = lm.GetChildAt(i);
                int childStart = (isVertical) ? child.Top : child.Left;
                int childEnd = (isVertical) ? child.Bottom : child.Right;
                if (childStart < end && childEnd > start)
                {
                    if (completelyVisible)
                    {
                        if (childStart >= start && childEnd <= end)
                        {
                            return child;
                        }
                        else if (acceptPartiallyVisible && partiallyVisible == null)
                        {
                            partiallyVisible = child;
                        }
                    }
                    else
                    {
                        return child;
                    }
                }
            }

            return partiallyVisible;
        }

        public static int SafeGetAdapterPosition(RecyclerView.ViewHolder holder)
        {
            return (holder != null) ? holder.AdapterPosition : RecyclerView.NoPosition;
        }

        public static int SafeGetLayoutPosition(RecyclerView.ViewHolder holder)
        {
            return (holder != null) ? holder.LayoutPosition : RecyclerView.NoPosition;
        }

        public static View FindViewByPosition(RecyclerView.LayoutManager layoutManager, int position)
        {
            return (position != RecyclerView.NoPosition) ? layoutManager.FindViewByPosition(position) : null;
        }

        public static int GetSpanIndex(RecyclerView.ViewHolder holder)
        {
            View itemView = GetLaidOutItemView(holder);
            if (itemView == null)
            {
                return INVALID_SPAN_ID;
            }

            ViewGroup.LayoutParams lp = itemView.LayoutParameters;
            if (lp is StaggeredGridLayoutManager.LayoutParams)
            {
                return ((StaggeredGridLayoutManager.LayoutParams)lp).SpanIndex;
            }
            else if (lp is GridLayoutManager.LayoutParams)
            {
                return ((GridLayoutManager.LayoutParams)lp).SpanIndex;
            }
            else if (lp is RecyclerView.LayoutParams)
            {
                return 0;
            }
            else
            {
                return INVALID_SPAN_ID;
            }
        }

        public static int GetSpanSize(RecyclerView.ViewHolder holder)
        {
            View itemView = GetLaidOutItemView(holder);
            if (itemView == null)
            {
                return INVALID_SPAN_COUNT;
            }

            ViewGroup.LayoutParams lp = itemView.LayoutParameters;
            if (lp is StaggeredGridLayoutManager.LayoutParams)
            {
                bool isFullSpan = ((StaggeredGridLayoutManager.LayoutParams)lp).FullSpan;
                if (isFullSpan)
                {
                    RecyclerView rv = (RecyclerView)itemView.Parent;
                    return GetSpanCount(rv);
                }
                else
                {
                    return 1;
                }
            }
            else if (lp is GridLayoutManager.LayoutParams)
            {
                return ((GridLayoutManager.LayoutParams)lp).SpanSize;
            }
            else if (lp is RecyclerView.LayoutParams)
            {
                return 1;
            }
            else
            {
                return INVALID_SPAN_COUNT;
            }
        }

        public static bool IsFullSpan(RecyclerView.ViewHolder holder)
        {
            View itemView = GetLaidOutItemView(holder);
            if (itemView == null)
            {
                return true;
            }

            ViewGroup.LayoutParams lp = itemView.LayoutParameters;
            if (lp is StaggeredGridLayoutManager.LayoutParams)
            {
                return ((StaggeredGridLayoutManager.LayoutParams)lp).FullSpan;
            }
            else if (lp is GridLayoutManager.LayoutParams)
            {
                RecyclerView rv = (RecyclerView)itemView.Parent;
                int spanCount = GetSpanCount(rv);
                int spanSize = ((GridLayoutManager.LayoutParams)lp).SpanSize;
                return (spanCount == spanSize);
            }
            else if (lp is RecyclerView.LayoutParams)
            {
                return true;
            }
            else
            {
                return true;
            }
        }

        private static View GetLaidOutItemView(RecyclerView.ViewHolder holder)
        {
            if (holder == null)
            {
                return null;
            }

            View itemView = holder.ItemView;
            if (!ViewCompat.IsLaidOut(itemView))
            {
                return null;
            }

            return itemView;
        }

        public static bool IsLinearLayout(int layoutType)
        {
            return ((layoutType == LAYOUT_TYPE_LINEAR_VERTICAL) || (layoutType == LAYOUT_TYPE_LINEAR_HORIZONTAL));
        }

        public static bool IsGridLayout(int layoutType)
        {
            return ((layoutType == LAYOUT_TYPE_GRID_VERTICAL) || (layoutType == LAYOUT_TYPE_GRID_HORIZONTAL));
        }

        public static bool IsStaggeredGridLayout(int layoutType)
        {
            return ((layoutType == LAYOUT_TYPE_STAGGERED_GRID_VERTICAL) || (layoutType == LAYOUT_TYPE_STAGGERED_GRID_HORIZONTAL));
        }
    }
}