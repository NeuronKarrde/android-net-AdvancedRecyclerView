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
using AndroidX.RecyclerView.Widget;

namespace Com.H6ah4i.Android.Widget.Advrecyclerview.Decoration
{
    /// <summary>
    /// Item decoration which draws drop shadow of each item views.
    /// </summary>
    public class ItemShadowDecorator : RecyclerView.ItemDecoration
    {
        private readonly NinePatchDrawable mShadowDrawable;
        private readonly Rect mShadowPadding = new Rect();
        private readonly bool mCastShadowForTransparentBackgroundItem;
        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="shadow">9-patch drawable used for drop shadow</param>
        public ItemShadowDecorator(NinePatchDrawable shadow) : this(shadow, true)
        {
        }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="shadow">9-patch drawable used for drop shadow</param>
        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="shadow">9-patch drawable used for drop shadow</param>
        /// <param name="castShadowForTransparentBackgroundItem">Whether to cast shadows for transparent items</param>
        public ItemShadowDecorator(NinePatchDrawable shadow, bool castShadowForTransparentBackgroundItem)
        {
            mShadowDrawable = shadow;
            mShadowDrawable.GetPadding(mShadowPadding);
            mCastShadowForTransparentBackgroundItem = castShadowForTransparentBackgroundItem;
        }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="shadow">9-patch drawable used for drop shadow</param>
        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="shadow">9-patch drawable used for drop shadow</param>
        /// <param name="castShadowForTransparentBackgroundItem">Whether to cast shadows for transparent items</param>
        public override void OnDraw(Canvas c, RecyclerView parent, RecyclerView.State state)
        {
            int childCount = parent.ChildCount;
            if (childCount == 0)
            {
                return;
            }

            for (int i = 0; i < childCount; i++)
            {
                View child = parent.GetChildAt(i);
                if (!ShouldDrawDropShadow(child))
                {
                    continue;
                }

                int tx = (int)(child.TranslationX + 0.5F);
                int ty = (int)(child.TranslationY + 0.5F);
                int left = child.Left - mShadowPadding.Left;
                int right = child.Right + mShadowPadding.Right;
                int top = child.Top - mShadowPadding.Top;
                int bottom = child.Bottom + mShadowPadding.Bottom;
                mShadowDrawable.SetBounds(left + tx, top + ty, right + tx, bottom + ty);
                mShadowDrawable.Draw(c);
            }
        }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="shadow">9-patch drawable used for drop shadow</param>
        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="shadow">9-patch drawable used for drop shadow</param>
        /// <param name="castShadowForTransparentBackgroundItem">Whether to cast shadows for transparent items</param>
        private bool ShouldDrawDropShadow(View child)
        {
            if (child.Visibility != ViewStates.Visible)
            {
                return false;
            }

            if (child.Alpha != 1F)
            {
                return false;
            }

            Drawable background = child.Background;
            if (background == null)
            {
                return false;
            }

            if (!mCastShadowForTransparentBackgroundItem && (background is ColorDrawable))
            {

                //noinspection RedundantCast,RedundantIfStatement
                if (((ColorDrawable)background).Alpha == 0)
                {
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="shadow">9-patch drawable used for drop shadow</param>
        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="shadow">9-patch drawable used for drop shadow</param>
        /// <param name="castShadowForTransparentBackgroundItem">Whether to cast shadows for transparent items</param>
        //noinspection RedundantCast,RedundantIfStatement
        public override void GetItemOffsets(Rect outRect, View view, RecyclerView parent, RecyclerView.State state)
        {
            outRect.Set(0, 0, 0, 0);
        }
    }
}