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
using Android.Views.Animations;
using Com.H6ah4i.Android.Widget.Advrecyclerview.Utils;
using AndroidX.Core.View;
using AndroidX.RecyclerView.Widget;

namespace Com.H6ah4i.Android.Widget.Advrecyclerview.Draggable
{
    class DraggingItemDecorator : BaseDraggableItemDecorator
    {
        private static readonly string TAG = "DraggingItemDecorator";
        private int mTranslationX;
        private int mTranslationY;
        private Bitmap mDraggingItemImage;
        private int mTranslationLeftLimit;
        private int mTranslationRightLimit;
        private int mTranslationTopLimit;
        private int mTranslationBottomLimit;
        private int mTouchPositionX;
        private int mTouchPositionY;
        private NinePatchDrawable mShadowDrawable;
        private readonly Rect mShadowPadding = new Rect();
        private bool mStarted;
        private bool mIsScrolling;
        private ItemDraggableRange mRange;
        private int mLayoutOrientation;
        private int mLayoutType;
        private DraggingItemInfo mDraggingItemInfo;
        private Paint mPaint;
        private long mStartMillis;
        private long mStartAnimationDurationMillis = 0;
        private float mTargetDraggingItemScale = 1F;
        private float mTargetDraggingItemRotation = 0F;
        private float mTargetDraggingItemAlpha = 1F;
        private float mInitialDraggingItemScaleX;
        private float mInitialDraggingItemScaleY;
        private IInterpolator mScaleInterpolator = null;
        private IInterpolator mRotationInterpolator = null;
        private IInterpolator mAlphaInterpolator = null;
        private float mLastDraggingItemScaleX;
        private float mLastDraggingItemScaleY;
        private float mLastDraggingItemRotation;
        private float mLastDraggingItemAlpha;
        public DraggingItemDecorator(RecyclerView recyclerView, RecyclerView.ViewHolder draggingItem, ItemDraggableRange range) : base(recyclerView, draggingItem)
        {
            mRange = range;
            mPaint = new Paint();
        }

        private static int Clip(int value, int min, int max)
        {
            return Math.Min(Math.Max(value, min), max);
        }

        private static View FindRangeFirstItem(RecyclerView rv, ItemDraggableRange range, int firstVisiblePosition, int lastVisiblePosition)
        {
            if (firstVisiblePosition == RecyclerView.NoPosition || lastVisiblePosition == RecyclerView.NoPosition)
            {
                return null;
            }

            View v = null;
            int childCount = rv.ChildCount;
            for (int i = 0; i < childCount; i++)
            {
                View v2 = rv.GetChildAt(i);
                RecyclerView.ViewHolder vh = rv.GetChildViewHolder(v2);
                if (vh != null)
                {
                    int position = vh.LayoutPosition;
                    if ((position >= firstVisiblePosition) && (position <= lastVisiblePosition) && range.CheckInRange(position))
                    {
                        v = v2;
                        break;
                    }
                }
            }

            return v;
        }

        private static View FindRangeLastItem(RecyclerView rv, ItemDraggableRange range, int firstVisiblePosition, int lastVisiblePosition)
        {
            if (firstVisiblePosition == RecyclerView.NoPosition || lastVisiblePosition == RecyclerView.NoPosition)
            {
                return null;
            }

            View v = null;
            int childCount = rv.ChildCount;
            for (int i = childCount - 1; i >= 0; i--)
            {
                View v2 = rv.GetChildAt(i);
                RecyclerView.ViewHolder vh = rv.GetChildViewHolder(v2);
                if (vh != null)
                {
                    int position = vh.LayoutPosition;
                    if ((position >= firstVisiblePosition) && (position <= lastVisiblePosition) && range.CheckInRange(position))
                    {
                        v = v2;
                        break;
                    }
                }
            }

            return v;
        }

        public override void OnDrawOver(Canvas c, RecyclerView parent, RecyclerView.State state)
        {
            if (mDraggingItemImage == null)
            {
                return;
            }

            int elapsedMillis = (int)Math.Min(Java.Lang.JavaSystem.CurrentTimeMillis() - mStartMillis, mStartAnimationDurationMillis);
            float t = (mStartAnimationDurationMillis > 0) ? ((float)elapsedMillis / mStartAnimationDurationMillis) : 1F;
            float tScale = GetInterpolation(mScaleInterpolator, t);
            float scaleX = tScale * (mTargetDraggingItemScale - mInitialDraggingItemScaleX) + mInitialDraggingItemScaleX;
            float scaleY = tScale * (mTargetDraggingItemScale - mInitialDraggingItemScaleY) + mInitialDraggingItemScaleY;
            float alpha = GetInterpolation(mAlphaInterpolator, t) * (mTargetDraggingItemAlpha - 1F) + 1F;
            float rotation = GetInterpolation(mRotationInterpolator, t) * mTargetDraggingItemRotation;
            if (scaleX > 0F && scaleY > 0F && alpha > 0F)
            {
                mPaint.Alpha=((int)(alpha * 255));
                int savedCount = c.Save();
                c.Translate(mTranslationX + mDraggingItemInfo.grabbedPositionX, mTranslationY + mDraggingItemInfo.grabbedPositionY);
                c.Scale(scaleX, scaleY);
                c.Rotate(rotation);
                c.Translate(-(mShadowPadding.Left + mDraggingItemInfo.grabbedPositionX), -(mShadowPadding.Top + mDraggingItemInfo.grabbedPositionY));
                c.DrawBitmap(mDraggingItemImage, 0, 0, mPaint);
                c.RestoreToCount(savedCount);
            }

            if (t < 1F)
            {
                ViewCompat.PostInvalidateOnAnimation(mRecyclerView);
            }

            mLastDraggingItemScaleX = scaleX;
            mLastDraggingItemScaleY = scaleY;
            mLastDraggingItemRotation = rotation;
            mLastDraggingItemAlpha = alpha;
        }

        public virtual void SetupDraggingItemEffects(DraggingItemEffectsInfo info)
        {
            mStartAnimationDurationMillis = info.durationMillis;
            mTargetDraggingItemScale = info.scale;
            mScaleInterpolator = info.scaleInterpolator;
            mTargetDraggingItemRotation = info.rotation;
            mRotationInterpolator = info.rotationInterpolator;
            mTargetDraggingItemAlpha = info.alpha;
            mAlphaInterpolator = info.alphaInterpolator;
        }

        public virtual void Start(DraggingItemInfo draggingItemInfo, int touchX, int touchY)
        {
            if (mStarted)
            {
                return;
            }

            View itemView = mDraggingItemViewHolder.ItemView;
            mDraggingItemInfo = draggingItemInfo;
            mDraggingItemImage = CreateDraggingItemImage(itemView, mShadowDrawable);
            mTranslationLeftLimit = mRecyclerView.PaddingLeft;
            mTranslationTopLimit = mRecyclerView.PaddingTop;
            mLayoutOrientation = CustomRecyclerViewUtils.GetOrientation(mRecyclerView);
            mLayoutType = CustomRecyclerViewUtils.GetLayoutType(mRecyclerView);
            mInitialDraggingItemScaleX = itemView.ScaleX;
            mInitialDraggingItemScaleY = itemView.ScaleY;
            mLastDraggingItemScaleX = 1F;
            mLastDraggingItemScaleY = 1F;
            mLastDraggingItemRotation = 0F;
            mLastDraggingItemAlpha = 1F;

            // hide
            itemView.Visibility = ViewStates.Invisible;
            Update(touchX, touchY, true);
            mRecyclerView.AddItemDecoration(this);
            mStartMillis = Java.Lang.JavaSystem.CurrentTimeMillis();
            mStarted = true;
        }

        // hide
        public virtual void UpdateDraggingItemView(DraggingItemInfo info, RecyclerView.ViewHolder vh)
        {
            if (!mStarted)
            {
                return;
            }

            if (mDraggingItemViewHolder != vh)
            {
                InvalidateDraggingItem();
                mDraggingItemViewHolder = vh;
            }

            mDraggingItemImage = CreateDraggingItemImage(vh.ItemView, mShadowDrawable);
            mDraggingItemInfo = info;
            Refresh(true);
        }

        // hide
        public virtual void Finish(bool animate)
        {
            if (mStarted)
            {
                mRecyclerView.RemoveItemDecoration(this);
            }

            RecyclerView.ItemAnimator itemAnimator = mRecyclerView.GetItemAnimator();
            if (itemAnimator != null)
            {
                itemAnimator.EndAnimations();
            }

            mRecyclerView.StopScroll();

            // return to default position
            UpdateDraggingItemPosition(mTranslationX, mTranslationY);
            if (mDraggingItemViewHolder != null)
            {
                MoveToDefaultPosition(mDraggingItemViewHolder.ItemView, mLastDraggingItemScaleX, mLastDraggingItemScaleY, mLastDraggingItemRotation, mLastDraggingItemAlpha, animate);
            }


            // show
            if (mDraggingItemViewHolder != null)
            {
                mDraggingItemViewHolder.ItemView.Visibility = ViewStates.Visible;
            }

            mDraggingItemViewHolder = null;
            if (mDraggingItemImage != null)
            {
                mDraggingItemImage.Recycle();
                mDraggingItemImage = null;
            }

            mRange = null;
            mTranslationX = 0;
            mTranslationY = 0;
            mTranslationLeftLimit = 0;
            mTranslationRightLimit = 0;
            mTranslationTopLimit = 0;
            mTranslationBottomLimit = 0;
            mTouchPositionX = 0;
            mTouchPositionY = 0;
            mStarted = false;
        }

        // hide
        // return to default position
        // show
        public virtual bool Update(int touchX, int touchY, bool force)
        {
            mTouchPositionX = touchX;
            mTouchPositionY = touchY;
            return Refresh(force);
        }

        // hide
        // return to default position
        // show
        public virtual bool Refresh(bool force)
        {
            int prevTranslationX = mTranslationX;
            int prevTranslationY = mTranslationY;
            UpdateTranslationOffset();
            bool updated = (prevTranslationX != mTranslationX) || (prevTranslationY != mTranslationY);
            if (updated || force)
            {
                UpdateDraggingItemPosition(mTranslationX, mTranslationY);
                ViewCompat.PostInvalidateOnAnimation(mRecyclerView);
            }

            return updated;
        }

        // hide
        // return to default position
        // show
        public virtual void SetShadowDrawable(NinePatchDrawable shadowDrawable)
        {
            mShadowDrawable = shadowDrawable;
            if (mShadowDrawable != null)
            {
                mShadowDrawable.GetPadding(mShadowPadding);
            }
        }

        // hide
        // return to default position
        // show
        public virtual int GetDraggingItemTranslationY()
        {
            return mTranslationY;
        }

        // hide
        // return to default position
        // show
        public virtual int GetDraggingItemTranslationX()
        {
            return mTranslationX;
        }

        // hide
        // return to default position
        // show
        public virtual int GetDraggingItemMoveOffsetY()
        {
            return mTranslationY - mDraggingItemInfo.initialItemTop;
        }

        // hide
        // return to default position
        // show
        public virtual int GetDraggingItemMoveOffsetX()
        {
            return mTranslationX - mDraggingItemInfo.initialItemLeft;
        }

        // hide
        // return to default position
        // show
        private void UpdateTranslationOffset()
        {
            RecyclerView rv = mRecyclerView;
            int childCount = rv.ChildCount;
            if (childCount > 0)
            {
                mTranslationLeftLimit = 0;
                mTranslationRightLimit = rv.Width - mDraggingItemInfo.width;
                mTranslationTopLimit = 0;
                mTranslationBottomLimit = rv.Height - mDraggingItemInfo.height;
                switch (mLayoutOrientation)
                {
                    case CustomRecyclerViewUtils.ORIENTATION_VERTICAL:
                    {
                        mTranslationTopLimit = -mDraggingItemInfo.height;
                        mTranslationBottomLimit = rv.Height;
                        mTranslationLeftLimit += rv.PaddingLeft;
                        mTranslationRightLimit -= rv.PaddingRight;
                        break;
                    }

                    case CustomRecyclerViewUtils.ORIENTATION_HORIZONTAL:
                    {
                        mTranslationTopLimit += rv.PaddingTop;
                        mTranslationBottomLimit -= rv.PaddingBottom;
                        mTranslationLeftLimit = -mDraggingItemInfo.width;
                        mTranslationRightLimit = rv.Width;
                        break;
                    }
                }

                mTranslationRightLimit = Math.Max(mTranslationLeftLimit, mTranslationRightLimit);
                mTranslationBottomLimit = Math.Max(mTranslationTopLimit, mTranslationBottomLimit);
                if (!mIsScrolling)
                {
                    int firstVisiblePosition = CustomRecyclerViewUtils.FindFirstVisibleItemPosition(rv, true);
                    int lastVisiblePosition = CustomRecyclerViewUtils.FindLastVisibleItemPosition(rv, true);
                    View firstChild = FindRangeFirstItem(rv, mRange, firstVisiblePosition, lastVisiblePosition);
                    View lastChild = FindRangeLastItem(rv, mRange, firstVisiblePosition, lastVisiblePosition);
                    switch (mLayoutOrientation)
                    {
                        case CustomRecyclerViewUtils.ORIENTATION_VERTICAL:
                        {
                            if (firstChild != null)
                            {
                                mTranslationTopLimit = Math.Min(mTranslationBottomLimit, firstChild.Top);
                            }

                            if (lastChild != null)
                            {
                                int limit = Math.Max(0, lastChild.Bottom - mDraggingItemInfo.height);
                                mTranslationBottomLimit = Math.Min(mTranslationBottomLimit, limit);
                            }

                            break;
                        }

                        case CustomRecyclerViewUtils.ORIENTATION_HORIZONTAL:
                        {
                            if (firstChild != null)
                            {
                                mTranslationLeftLimit = Math.Min(mTranslationLeftLimit, firstChild.Left);
                            }

                            if (lastChild != null)
                            {
                                int limit = Math.Max(0, lastChild.Right - mDraggingItemInfo.width);
                                mTranslationRightLimit = Math.Min(mTranslationRightLimit, limit);
                            }

                            break;
                        }
                    }
                }
            }
            else
            {
                mTranslationRightLimit = mTranslationLeftLimit = rv.PaddingLeft;
                mTranslationBottomLimit = mTranslationTopLimit = rv.PaddingTop;
            }

            mTranslationX = mTouchPositionX - mDraggingItemInfo.grabbedPositionX;
            mTranslationY = mTouchPositionY - mDraggingItemInfo.grabbedPositionY;
            if (CustomRecyclerViewUtils.IsLinearLayout(mLayoutType))
            {
                mTranslationX = Clip(mTranslationX, mTranslationLeftLimit, mTranslationRightLimit);
                mTranslationY = Clip(mTranslationY, mTranslationTopLimit, mTranslationBottomLimit);
            }
        }

        // hide
        // return to default position
        // show
        private static int ToSpanAlignedPosition(int position, int spanCount)
        {
            if (position == RecyclerView.NoPosition)
            {
                return RecyclerView.NoPosition;
            }

            return (position / spanCount) * spanCount;
        }

        // hide
        // return to default position
        // show
        public virtual bool IsReachedToTopLimit()
        {
            return (mTranslationY == mTranslationTopLimit);
        }

        // hide
        // return to default position
        // show
        public virtual bool IsReachedToBottomLimit()
        {
            return (mTranslationY == mTranslationBottomLimit);
        }

        // hide
        // return to default position
        // show
        public virtual bool IsReachedToLeftLimit()
        {
            return (mTranslationX == mTranslationLeftLimit);
        }

        // hide
        // return to default position
        // show
        public virtual bool IsReachedToRightLimit()
        {
            return (mTranslationX == mTranslationRightLimit);
        }

        // hide
        // return to default position
        // show
        private Bitmap CreateDraggingItemImage(View v, NinePatchDrawable shadow)
        {
            int viewTop = v.Top;
            int viewLeft = v.Left;
            int viewWidth = v.Width;
            int viewHeight = v.Height;
            int canvasWidth = viewWidth + mShadowPadding.Left + mShadowPadding.Right;
            int canvasHeight = viewHeight + mShadowPadding.Top + mShadowPadding.Bottom;
            v.Measure(View.MeasureSpec.MakeMeasureSpec(viewWidth, MeasureSpecMode.Exactly), View.MeasureSpec.MakeMeasureSpec(viewHeight, MeasureSpecMode.Exactly));
            v.Layout(viewLeft, viewTop, viewLeft + viewWidth, viewTop + viewHeight);
            Bitmap bitmap = Bitmap.CreateBitmap(canvasWidth, canvasHeight, Bitmap.Config.Argb8888);
            Canvas canvas = new Canvas(bitmap);
            if (shadow != null)
            {
                shadow.SetBounds(0, 0, canvasWidth, canvasHeight);
                shadow.Draw(canvas);
            }

            int savedCount = canvas.Save();

            // NOTE: Explicitly set clipping rect. This is required on Gingerbread.
            canvas.ClipRect(mShadowPadding.Left, mShadowPadding.Top, canvasWidth - mShadowPadding.Right, canvasHeight - mShadowPadding.Bottom);
            canvas.Translate(mShadowPadding.Left, mShadowPadding.Top);
            v.Draw(canvas);
            canvas.RestoreToCount(savedCount);
            return bitmap;
        }

        // hide
        // return to default position
        // show
        // NOTE: Explicitly set clipping rect. This is required on Gingerbread.
        private void UpdateDraggingItemPosition(float translationX, int translationY)
        {

            // NOTE: Need to update the view position to make other decorations work properly while dragging
            if (mDraggingItemViewHolder != null)
            {
                SetItemTranslation(mRecyclerView, mDraggingItemViewHolder, translationX - mDraggingItemViewHolder.ItemView.Left, translationY - mDraggingItemViewHolder.ItemView.Top);
            }
        }

        // hide
        // return to default position
        // show
        // NOTE: Explicitly set clipping rect. This is required on Gingerbread.
        // NOTE: Need to update the view position to make other decorations work properly while dragging
        public virtual void SetIsScrolling(bool isScrolling)
        {
            if (mIsScrolling == isScrolling)
            {
                return;
            }

            mIsScrolling = isScrolling;
        }

        // hide
        // return to default position
        // show
        // NOTE: Explicitly set clipping rect. This is required on Gingerbread.
        // NOTE: Need to update the view position to make other decorations work properly while dragging
        public virtual int GetTranslatedItemPositionTop()
        {
            return mTranslationY;
        }

        // hide
        // return to default position
        // show
        // NOTE: Explicitly set clipping rect. This is required on Gingerbread.
        // NOTE: Need to update the view position to make other decorations work properly while dragging
        public virtual int GetTranslatedItemPositionBottom()
        {
            return mTranslationY + mDraggingItemInfo.height;
        }

        // hide
        // return to default position
        // show
        // NOTE: Explicitly set clipping rect. This is required on Gingerbread.
        // NOTE: Need to update the view position to make other decorations work properly while dragging
        public virtual int GetTranslatedItemPositionLeft()
        {
            return mTranslationX;
        }

        // hide
        // return to default position
        // show
        // NOTE: Explicitly set clipping rect. This is required on Gingerbread.
        // NOTE: Need to update the view position to make other decorations work properly while dragging
        public virtual int GetTranslatedItemPositionRight()
        {
            return mTranslationX + mDraggingItemInfo.width;
        }

        // hide
        // return to default position
        // show
        // NOTE: Explicitly set clipping rect. This is required on Gingerbread.
        // NOTE: Need to update the view position to make other decorations work properly while dragging
        public virtual void InvalidateDraggingItem()
        {
            if (mDraggingItemViewHolder != null)
            {
                mDraggingItemViewHolder.ItemView.TranslationX=(0);
                mDraggingItemViewHolder.ItemView.TranslationY=(0);
                mDraggingItemViewHolder.ItemView.Visibility = ViewStates.Visible;
            }

            mDraggingItemViewHolder = null;
        }

        // hide
        // return to default position
        // show
        // NOTE: Explicitly set clipping rect. This is required on Gingerbread.
        // NOTE: Need to update the view position to make other decorations work properly while dragging
        public virtual void SetDraggingItemViewHolder(RecyclerView.ViewHolder holder)
        {
            if (mDraggingItemViewHolder != null)
            {
                throw new InvalidOperationException("A new view holder is attempt to be assigned before invalidating the older one");
            }

            mDraggingItemViewHolder = holder;
            holder.ItemView.Visibility = ViewStates.Invisible;
        }

        // hide
        // return to default position
        // show
        // NOTE: Explicitly set clipping rect. This is required on Gingerbread.
        // NOTE: Need to update the view position to make other decorations work properly while dragging
        private static float GetInterpolation(IInterpolator interpolator, float input)
        {
            return (interpolator != null) ? interpolator.GetInterpolation(input) : input;
        }
    }
}