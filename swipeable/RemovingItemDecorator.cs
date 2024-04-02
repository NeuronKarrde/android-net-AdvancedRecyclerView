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
using Java.Lang.Ref;
using AndroidX.Core.View;
using AndroidX.RecyclerView.Widget;
using Java.Lang;

namespace Com.H6ah4i.Android.Widget.Advrecyclerview.Swipeable
{
    class RemovingItemDecorator : RecyclerView.ItemDecoration
    {
        private const string TAG = "RemovingItemDecorator";
        private const int NOTIFY_REMOVAL_EFFECT_PHASE_1 = 0;
        private const int NOTIFY_REMOVAL_EFFECT_END = 1;
        private const long ADDITIONAL_REMOVE_DURATION = 50; // workaround: to avoid the gap between the below item
        // workaround: to avoid the gap between the below item
        private RecyclerView mRecyclerView;
        // workaround: to avoid the gap between the below item
        private RecyclerView.ViewHolder mSwipingItem;
        // workaround: to avoid the gap between the below item
        private readonly long mSwipingItemId;
        // workaround: to avoid the gap between the below item
        private readonly Rect mSwipingItemBounds = new Rect();
        // workaround: to avoid the gap between the below item
        private int mTranslationX;
        // workaround: to avoid the gap between the below item
        private int mTranslationY;
        // workaround: to avoid the gap between the below item
        private long mStartTime;
        // workaround: to avoid the gap between the below item
        private readonly long mRemoveAnimationDuration;
        // workaround: to avoid the gap between the below item
        private readonly long mMoveAnimationDuration;
        // workaround: to avoid the gap between the below item
        private IInterpolator mMoveAnimationInterpolator;
        // workaround: to avoid the gap between the below item
        private Drawable mSwipeBackgroundDrawable;
        // workaround: to avoid the gap between the below item
        private readonly bool mHorizontal;
        // workaround: to avoid the gap between the below item
        private int mPendingNotificationMask = 0;
        // workaround: to avoid the gap between the below item
        public RemovingItemDecorator(RecyclerView rv, RecyclerView.ViewHolder swipingItem, int result, long removeAnimationDuration, long moveAnimationDuration)
        {
            mRecyclerView = rv;
            mSwipingItem = swipingItem;
            mSwipingItemId = swipingItem.ItemId;
            mHorizontal = (result == RecyclerViewSwipeManager.ResultSwipedLeft || result == RecyclerViewSwipeManager.ResultSwipedRight);
            mRemoveAnimationDuration = removeAnimationDuration + ADDITIONAL_REMOVE_DURATION;
            mMoveAnimationDuration = moveAnimationDuration;
            mTranslationX = (int)(swipingItem.ItemView.TranslationX + 0.5F);
            mTranslationY = (int)(swipingItem.ItemView.TranslationY + 0.5F);
            CustomRecyclerViewUtils.GetViewBounds(mSwipingItem.ItemView, mSwipingItemBounds);
        }

        // workaround: to avoid the gap between the below item
        public virtual void SetMoveAnimationInterpolator(IInterpolator interpolator)
        {
            mMoveAnimationInterpolator = interpolator;
        }

        // workaround: to avoid the gap between the below item
        public override void OnDraw(Canvas c, RecyclerView parent, RecyclerView.State state)
        {
            long elapsedTime = GetElapsedTime(mStartTime);
            float scale = DetermineBackgroundScaleSwipeCompletedSuccessfully(elapsedTime);
            FillSwipingItemBackground(c, mSwipeBackgroundDrawable, scale);
            if (mSwipingItemId == mSwipingItem.ItemId)
            {
                mTranslationX = (int)(mSwipingItem.ItemView.TranslationX + 0.5F);
                mTranslationY = (int)(mSwipingItem.ItemView.TranslationY + 0.5F);
            }

            if (RequiresContinuousAnimationAfterSwipeCompletedSuccessfully(elapsedTime))
            {
                PostInvalidateOnAnimation();
            }
        }

        // workaround: to avoid the gap between the below item
        private bool RequiresContinuousAnimationAfterSwipeCompletedSuccessfully(long elapsedTime)
        {
            return (elapsedTime >= mRemoveAnimationDuration) && (elapsedTime < (mRemoveAnimationDuration + mMoveAnimationDuration));
        }

        // workaround: to avoid the gap between the below item
        private float DetermineBackgroundScaleSwipeCompletedSuccessfully(long elapsedTime)
        {
            float heightScale = 0F;
            if (elapsedTime < mRemoveAnimationDuration)
            {
                heightScale = 1F;
            }
            else if (elapsedTime < (mRemoveAnimationDuration + mMoveAnimationDuration))
            {
                if (mMoveAnimationDuration != 0)
                {
                    heightScale = 1F - (float)(elapsedTime - mRemoveAnimationDuration) / mMoveAnimationDuration;
                    if (mMoveAnimationInterpolator != null)
                    {
                        heightScale = mMoveAnimationInterpolator.GetInterpolation(heightScale);
                    }
                }
            }

            return heightScale;
        }

        // workaround: to avoid the gap between the below item
        private void FillSwipingItemBackground(Canvas c, Drawable drawable, float scale)
        {
            Rect bounds = mSwipingItemBounds;
            int translationX = mTranslationX;
            int translationY = mTranslationY;
            float hScale = (mHorizontal) ? 1F : scale;
            float vScale = (mHorizontal) ? scale : 1F;
            int width = (int)(hScale * bounds.Width() + 0.5F);
            int height = (int)(vScale * bounds.Height() + 0.5F);
            if ((height == 0) || (width == 0) || (drawable == null))
            {
                return;
            }

            int savedCount = c.Save();
            c.ClipRect(bounds.Left + translationX, bounds.Top + translationY, bounds.Left + translationX + width, bounds.Top + translationY + height);

            // c.drawColor(0xffff0000); // <-- debug
            c.Translate(bounds.Left + translationX - (bounds.Width() - width) / 2, bounds.Top + translationY - (bounds.Height() - height) / 2);
            drawable.SetBounds(0, 0, bounds.Width(), bounds.Height());
            drawable.Draw(c);
            c.RestoreToCount(savedCount);
        }

        // workaround: to avoid the gap between the below item
        // c.drawColor(0xffff0000); // <-- debug
        private void PostInvalidateOnAnimation()
        {
            ViewCompat.PostInvalidateOnAnimation(mRecyclerView);
        }

        // workaround: to avoid the gap between the below item
        // c.drawColor(0xffff0000); // <-- debug
        public virtual void Start()
        {
            var containerView = SwipeableViewHolderUtils.GetSwipeableContainerView(mSwipingItem);
            ViewCompat.Animate(containerView).Cancel();
            mRecyclerView.AddItemDecoration(this);
            mStartTime = Java.Lang.JavaSystem.CurrentTimeMillis();
            mTranslationY = (int)(mSwipingItem.ItemView.TranslationY + 0.5F);
            mSwipeBackgroundDrawable = mSwipingItem.ItemView.Background;
            PostInvalidateOnAnimation();
            NotifyDelayed(NOTIFY_REMOVAL_EFFECT_PHASE_1, mRemoveAnimationDuration);
        }

        // workaround: to avoid the gap between the below item
        // c.drawColor(0xffff0000); // <-- debug
        private void NotifyDelayed(int code, long delay)
        {
            int mask = (1 << code);
            if ((mPendingNotificationMask & mask) != 0)
            {
                return;
            }

            mPendingNotificationMask |= mask;
            DelayedNotificationRunner notification = new DelayedNotificationRunner(this, code);
            ViewCompat.PostOnAnimationDelayed(mRecyclerView, notification, delay);
        }

        // workaround: to avoid the gap between the below item
        // c.drawColor(0xffff0000); // <-- debug
        /*package*/
        public virtual void OnDelayedNotification(int code)
        {
            int mask = (1 << code);
            long elapsedTime = GetElapsedTime(mStartTime);
            mPendingNotificationMask &= (~mask);
            switch (code)
            {
                case NOTIFY_REMOVAL_EFFECT_PHASE_1:
                    if (elapsedTime < mRemoveAnimationDuration)
                    {
                        NotifyDelayed(NOTIFY_REMOVAL_EFFECT_PHASE_1, (mRemoveAnimationDuration - elapsedTime));
                    }
                    else
                    {
                        PostInvalidateOnAnimation();
                        NotifyDelayed(NOTIFY_REMOVAL_EFFECT_END, mMoveAnimationDuration);
                    }

                    break;
                case NOTIFY_REMOVAL_EFFECT_END:
                    Finish();
                    break;
            }
        }

        // workaround: to avoid the gap between the below item
        // c.drawColor(0xffff0000); // <-- debug
        /*package*/
        private void Finish()
        {
            mRecyclerView.RemoveItemDecoration(this);
            PostInvalidateOnAnimation(); // this is required to avoid remnant of the decoration
            mRecyclerView = null;
            mSwipingItem = null;
            mTranslationY = 0;
            mMoveAnimationInterpolator = null;
        }

        // workaround: to avoid the gap between the below item
        // c.drawColor(0xffff0000); // <-- debug
        /*package*/
        // this is required to avoid remnant of the decoration
        protected static long GetElapsedTime(long initialTime)
        {
            long curTime = Java.Lang.JavaSystem.CurrentTimeMillis();
            return (curTime >= initialTime) ? (curTime - initialTime) : Long.MaxValue;
        }

        // workaround: to avoid the gap between the below item
        // c.drawColor(0xffff0000); // <-- debug
        /*package*/
        // this is required to avoid remnant of the decoration
        private class DelayedNotificationRunner : Java.Lang.Object, IRunnable
        {
            private WeakReference<RemovingItemDecorator> mRefDecorator;
            private readonly int mCode;
            public DelayedNotificationRunner(RemovingItemDecorator decorator, int code)
            {
                mRefDecorator = new WeakReference<RemovingItemDecorator>(decorator);
                mCode = code;
            }

            public virtual void Run()
            {
                if (mRefDecorator.TryGetTarget(out RemovingItemDecorator decorator))
                {
                    mRefDecorator.SetTarget(null);
                    mRefDecorator = null;
                    if (decorator != null)
                    {
                        decorator.OnDelayedNotification(mCode);
                    }
                }
               
            }
        }
    }
}