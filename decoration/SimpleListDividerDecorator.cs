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
using Android.Graphics.Drawables;
using Android.Views;
using AndroidX.Core.View;
using AndroidX.RecyclerView.Widget;

namespace Com.H6ah4i.Android.Widget.Advrecyclerview.Decoration
{
    /// <summary>
    /// Item decoration which draws item divider between each items.
    /// </summary>
    public class SimpleListDividerDecorator : RecyclerView.ItemDecoration
    {
        private readonly Drawable mHorizontalDrawable;
        private readonly Drawable mVerticalDrawable;
        private readonly int mHorizontalDividerHeight;
        private readonly int mVerticalDividerWidth;
        private readonly bool mOverlap;
        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="divider">horizontal divider drawable</param>
        /// <param name="overlap">whether the divider is drawn overlapped on bottom of the item.</param>
        public SimpleListDividerDecorator(Drawable divider, bool overlap) : this(divider, null, overlap)
        {
        }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="divider">horizontal divider drawable</param>
        /// <param name="overlap">whether the divider is drawn overlapped on bottom of the item.</param>
        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="horizontalDivider">horizontal divider drawable</param>
        /// <param name="verticalDivider">vertical divider drawable</param>
        /// <param name="overlap">whether the divider is drawn overlapped on bottom (or right) of the item.</param>
        public SimpleListDividerDecorator(Drawable horizontalDivider, Drawable verticalDivider, bool overlap)
        {
            mHorizontalDrawable = horizontalDivider;
            mVerticalDrawable = verticalDivider;
            mHorizontalDividerHeight = (mHorizontalDrawable != null) ? mHorizontalDrawable.IntrinsicHeight : 0;
            mVerticalDividerWidth = (mVerticalDrawable != null) ? mVerticalDrawable.IntrinsicWidth : 0;
            mOverlap = overlap;
        }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="divider">horizontal divider drawable</param>
        /// <param name="overlap">whether the divider is drawn overlapped on bottom of the item.</param>
        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="horizontalDivider">horizontal divider drawable</param>
        /// <param name="verticalDivider">vertical divider drawable</param>
        /// <param name="overlap">whether the divider is drawn overlapped on bottom (or right) of the item.</param>
        public override void OnDrawOver(Canvas c, RecyclerView parent, RecyclerView.State state)
        {
            int childCount = parent.ChildCount;
            if (childCount == 0)
            {
                return;
            }

            float xPositionThreshold = (mOverlap) ? 1F : (mVerticalDividerWidth + 1F); // [px]
            float yPositionThreshold = (mOverlap) ? 1F : (mHorizontalDividerHeight + 1F); // [px]
            float zPositionThreshold = 1F; // [px]
            for (int i = 0; i < childCount - 1; i++)
            {
                View child = parent.GetChildAt(i);
                View nextChild = parent.GetChildAt(i + 1);
                if ((child.Visibility != ViewStates.Visible) || (nextChild.Visibility != ViewStates.Visible))
                {
                    continue;
                }


                // check if the next item is placed at the bottom or right
                float childBottom = child.Bottom + child.TranslationY;
                float nextChildTop = nextChild.Top + nextChild.TranslationY;
                float childRight = child.Right + child.TranslationX;
                float nextChildLeft = nextChild.Left + nextChild.TranslationX;
                if (!(((mHorizontalDividerHeight != 0) && (Math.Abs(nextChildTop - childBottom) < yPositionThreshold)) || ((mVerticalDividerWidth != 0) && (Math.Abs(nextChildLeft - childRight) < xPositionThreshold))))
                {
                    continue;
                }


                // check if the next item is placed on the same plane
                float childZ = ViewCompat.GetTranslationZ(child) + ViewCompat.GetElevation(child);
                float nextChildZ = ViewCompat.GetTranslationZ(nextChild) + ViewCompat.GetElevation(nextChild);
                if (!(Math.Abs(nextChildZ - childZ) < zPositionThreshold))
                {
                    continue;
                }

                float childAlpha = child.Alpha;
                float nextChildAlpha = nextChild.Alpha;
                int tx = (int)(child.TranslationX + 0.5F);
                int ty = (int)(child.TranslationY + 0.5F);
                if (mHorizontalDividerHeight != 0)
                {
                    int left = child.Left;
                    int right = child.Right;
                    int top = child.Bottom - (mOverlap ? mHorizontalDividerHeight : 0);
                    int bottom = top + mHorizontalDividerHeight;
                    mHorizontalDrawable.Alpha=((int)((0.5F * 255) * (childAlpha + nextChildAlpha) + 0.5F));
                    mHorizontalDrawable.SetBounds(left + tx, top + ty, right + tx, bottom + ty);
                    mHorizontalDrawable.Draw(c);
                }

                if (mVerticalDividerWidth != 0)
                {
                    int left = child.Right - (mOverlap ? mVerticalDividerWidth : 0);
                    int right = left + mVerticalDividerWidth;
                    int top = child.Top;
                    int bottom = child.Bottom;
                    mVerticalDrawable.Alpha=((int)((0.5F * 255) * (childAlpha + nextChildAlpha) + 0.5F));
                    mVerticalDrawable.SetBounds(left + tx, top + ty, right + tx, bottom + ty);
                    mVerticalDrawable.Draw(c);
                }
            }
        }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="divider">horizontal divider drawable</param>
        /// <param name="overlap">whether the divider is drawn overlapped on bottom of the item.</param>
        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="horizontalDivider">horizontal divider drawable</param>
        /// <param name="verticalDivider">vertical divider drawable</param>
        /// <param name="overlap">whether the divider is drawn overlapped on bottom (or right) of the item.</param>
        // [px]
        // [px]
        // [px]
        // check if the next item is placed at the bottom or right
        // check if the next item is placed on the same plane
        public override void GetItemOffsets(Rect outRect, View view, RecyclerView parent, RecyclerView.State state)
        {
            if (mOverlap)
            {
                outRect.Set(0, 0, 0, 0);
            }
            else
            {
                outRect.Set(0, 0, mVerticalDividerWidth, mHorizontalDividerHeight);
            }
        }
    }
}