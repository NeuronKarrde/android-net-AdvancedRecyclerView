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
using Android.OS;
using Android.Views;
using Android.Views.Animations;
using Com.H6ah4i.Android.Widget.Advrecyclerview.Swipeable.Action;
using Java.Lang.Ref;
using Java.Util;
using AndroidX.Core.View;
using AndroidX.RecyclerView.Widget;
using Java.Lang;
using Math = System.Math;

namespace Com.H6ah4i.Android.Widget.Advrecyclerview.Swipeable
{
    public class ItemSlidingAnimator
    {
        private const string TAG = "ItemSlidingAnimator";
        public const int DIR_LEFT = 0;
        public const  int DIR_UP = 1;
        public const int DIR_RIGHT = 2;
        public const int DIR_DOWN = 3;
        private readonly SwipeableItemWrapperAdapter mAdapter;
        private readonly IInterpolator mSlideToDefaultPositionAnimationInterpolator = new AccelerateDecelerateInterpolator();
        private readonly IInterpolator mSlideToSpecifiedPositionAnimationInterpolator = new DecelerateInterpolator();
        private readonly IInterpolator mSlideToOutsideOfWindowAnimationInterpolator = new AccelerateInterpolator(0.8F);
        private readonly List<RecyclerView.ViewHolder> mActive;
        private readonly IList<WeakReference<ViewHolderDeferredProcess>> mDeferredProcesses;
        private readonly int[] mTmpLocation = new int[2];
        private readonly Rect mTmpRect = new Rect();
        private int mImmediatelySetTranslationThreshold;
        public ItemSlidingAnimator(SwipeableItemWrapperAdapter adapter)
        {
            mAdapter = adapter;
            mActive = new List<RecyclerView.ViewHolder>();
            mDeferredProcesses = new List<WeakReference<ViewHolderDeferredProcess>>();
        }

        public virtual void SlideToDefaultPosition(RecyclerView.ViewHolder holder, bool horizontal, bool shouldAnimate, long duration)
        {
            CancelDeferredProcess(holder);
            SlideToSpecifiedPositionInternal(holder, 0, false, horizontal, shouldAnimate, mSlideToDefaultPositionAnimationInterpolator, duration, null);
        }

        public virtual void SlideToOutsideOfWindow(RecyclerView.ViewHolder holder, int dir, bool shouldAnimate, long duration)
        {
            CancelDeferredProcess(holder);
            SlideToOutsideOfWindowInternal(holder, dir, shouldAnimate, duration, null);
        }

        public virtual void SlideToSpecifiedPosition(RecyclerView.ViewHolder holder, float amount, bool proportionalAmount, bool horizontal, bool shouldAnimate, long duration)
        {
            CancelDeferredProcess(holder);
            SlideToSpecifiedPositionInternal(holder, amount, proportionalAmount, horizontal, shouldAnimate, mSlideToSpecifiedPositionAnimationInterpolator, duration, null);
        }

        public virtual bool FinishSwipeSlideToDefaultPosition(RecyclerView.ViewHolder holder, bool horizontal, bool shouldAnimate, long duration, int itemPosition, SwipeResultAction resultAction)
        {
            CancelDeferredProcess(holder);
            return SlideToSpecifiedPositionInternal(holder, 0, false, horizontal, shouldAnimate, mSlideToDefaultPositionAnimationInterpolator, duration, new SwipeFinishInfo(itemPosition, resultAction));
        }

        public virtual bool FinishSwipeSlideToOutsideOfWindow(RecyclerView.ViewHolder holder, int dir, bool shouldAnimate, long duration, int itemPosition, SwipeResultAction resultAction)
        {
            CancelDeferredProcess(holder);
            return SlideToOutsideOfWindowInternal(holder, dir, shouldAnimate, duration, new SwipeFinishInfo(itemPosition, resultAction));
        }

        private void CancelDeferredProcess(RecyclerView.ViewHolder holder)
        {
            int n = mDeferredProcesses.Count;
            for (int i = n - 1; i >= 0; i--)
            {
                mDeferredProcesses[i].TryGetTarget(out ViewHolderDeferredProcess dp);
                if (dp != null && dp.HasTargetViewHolder(holder))
                {
                    holder.ItemView.RemoveCallbacks(dp);
                    mDeferredProcesses.RemoveAt(i);
                }
                else if (dp == null || dp.LostReference(holder))
                {
                    mDeferredProcesses.RemoveAt(i);
                }
            }
        }

        private void ScheduleViewHolderDeferredSlideProcess(RecyclerView.ViewHolder holder, ViewHolderDeferredProcess deferredProcess)
        {
            mDeferredProcesses.Add(new WeakReference<ViewHolderDeferredProcess>(deferredProcess));
            holder.ItemView.Post(deferredProcess);
        }

        private bool SlideToSpecifiedPositionInternal(RecyclerView.ViewHolder holder, float amount, bool proportional, bool horizontal, bool shouldAnimate, IInterpolator interpolator, long duration, SwipeFinishInfo swipeFinish)
        {
            View containerView = SwipeableViewHolderUtils.GetSwipeableContainerView(holder);
            if (shouldAnimate)
            {
                shouldAnimate = ViewCompat.IsAttachedToWindow(containerView) && (containerView.Visibility == ViewStates.Visible);
            }

            duration = (shouldAnimate) ? duration : 0;
            if (amount != 0F)
            {
                int width = containerView.Width;
                int height = containerView.Height;
                if (horizontal && (!proportional || width != 0))
                {
                    int translationX;
                    translationX = (int)((proportional ? width * amount : amount) + 0.5F);
                    return AnimateSlideInternalCompat(holder, true, translationX, 0, duration, interpolator, swipeFinish);
                }
                else if (!horizontal && (!proportional || height != 0))
                {
                    int translationY;
                    translationY = (int)((proportional ? height * amount : amount) + 0.5F);
                    return AnimateSlideInternalCompat(holder, false, 0, translationY, duration, interpolator, swipeFinish);
                }
                else
                {
                    if (swipeFinish != null)
                    {
                        throw new InvalidOperationException("Unexpected state in slideToSpecifiedPositionInternal (swipeFinish == null)");
                    }

                    ScheduleViewHolderDeferredSlideProcess(holder, new DeferredSlideProcess(holder, amount, horizontal));
                    return false;
                }
            }
            else
            {
                return AnimateSlideInternalCompat(holder, horizontal, 0, 0, duration, interpolator, swipeFinish);
            }
        }

        private bool SlideToOutsideOfWindowInternal(RecyclerView.ViewHolder holder, int dir, bool shouldAnimate, long duration, SwipeFinishInfo swipeFinish)
        {
            if (!(holder is ISwipeableItemViewHolder))
            {
                return false;
            }

            View containerView = SwipeableViewHolderUtils.GetSwipeableContainerView(holder);
            ViewGroup parent = (ViewGroup)containerView.Parent;
            if (parent == null)
            {
                return false;
            }

            int left = containerView.Left;
            int right = containerView.Right;
            int top = containerView.Top;
            int bottom = containerView.Bottom;
            int width = right - left;
            int height = bottom - top;
            parent.GetWindowVisibleDisplayFrame(mTmpRect);
            int windowWidth = mTmpRect.Width();
            int windowHeight = mTmpRect.Height();
            int translateX = 0;
            int translateY = 0;
            if ((width == 0) || (height == 0))
            {

                // not measured yet or not shown
                switch (dir)
                {
                    case DIR_LEFT:
                        translateX = -windowWidth;
                        break;
                    case DIR_UP:
                        translateY = -windowHeight;
                        break;
                    case DIR_RIGHT:
                        translateX = windowWidth;
                        break;
                    case DIR_DOWN:
                        translateY = windowHeight;
                        break;
                    default:
                        break;
                }

                shouldAnimate = false;
            }
            else
            {
                parent.GetLocationInWindow(mTmpLocation);
                int x = mTmpLocation[0];
                int y = mTmpLocation[1];
                switch (dir)
                {
                    case DIR_LEFT:
                        translateX = -(x + width);
                        break;
                    case DIR_UP:
                        translateY = -(y + height);
                        break;
                    case DIR_RIGHT:
                        translateX = windowWidth - (x - left);
                        break;
                    case DIR_DOWN:
                        translateY = windowHeight - (y - top);
                        break;
                    default:
                        break;
                }
            }

            if (shouldAnimate)
            {
                shouldAnimate = ViewCompat.IsAttachedToWindow(containerView) && (containerView.Visibility == ViewStates.Visible);
            }

            duration = (shouldAnimate) ? duration : 0;
            bool horizontal = (dir == DIR_LEFT || dir == DIR_RIGHT);
            return AnimateSlideInternalCompat(holder, horizontal, translateX, translateY, duration, mSlideToOutsideOfWindowAnimationInterpolator, swipeFinish);
        }

        private bool AnimateSlideInternalCompat(RecyclerView.ViewHolder holder, bool horizontal, int translationX, int translationY, long duration, IInterpolator interpolator, SwipeFinishInfo swipeFinish)
        {
            bool result;
            result = AnimateSlideInternal(holder, horizontal, translationX, translationY, duration, interpolator, swipeFinish);

            // if ((swipeFinish != null) && !result) {
            // NOTE: Have to invoke the onSwipeSlideItemAnimationEnd() method in caller context
            // }
            return result;
        }

        static void SlideInternalCompat(RecyclerView.ViewHolder holder, bool horizontal, int translationX, int translationY)
        {
            SlideInternal(holder, horizontal, translationX, translationY);
        }

        private static void SlideInternal(RecyclerView.ViewHolder holder, bool horizontal, int translationX, int translationY)
        {
            if (!(holder is ISwipeableItemViewHolder))
            {
                return;
            }

            View containerView = SwipeableViewHolderUtils.GetSwipeableContainerView(holder);
            ViewCompat.Animate(containerView).Cancel();
            containerView.TranslationX=(translationX);
            containerView.TranslationY=(translationY);
        }

        private bool AnimateSlideInternal(RecyclerView.ViewHolder holder, bool horizontal, int translationX, int translationY, long duration, IInterpolator interpolator, SwipeFinishInfo swipeFinish)
        {
            if (!(holder is ISwipeableItemViewHolder))
            {
                return false;
            }

            View containerView = SwipeableViewHolderUtils.GetSwipeableContainerView(holder);
            int prevTranslationX = (int)(containerView.TranslationX + 0.5F);
            int prevTranslationY = (int)(containerView.TranslationY + 0.5F);
            EndAnimation(holder);
            int curTranslationX = (int)(containerView.TranslationX + 0.5F);
            int curTranslationY = (int)(containerView.TranslationY + 0.5F);

            //noinspection UnnecessaryLocalVariable
            int toX = translationX;

            //noinspection UnnecessaryLocalVariable
            int toY = translationY;
            if ((duration == 0) || (curTranslationX == toX && curTranslationY == toY) || (Math.Max(Math.Abs(toX - prevTranslationX), Math.Abs(toY - prevTranslationY)) <= mImmediatelySetTranslationThreshold))
            {
                containerView.TranslationX=(toX);
                containerView.TranslationY=(toY);
                return false;
            }

            containerView.TranslationX=(prevTranslationX);
            containerView.TranslationY=(prevTranslationY);
            SlidingAnimatorListenerObject listener = new SlidingAnimatorListenerObject(mAdapter, mActive, holder, toX, toY, duration, horizontal, interpolator, swipeFinish);
            listener.Start();
            return true;
        }

        public virtual void EndAnimation(RecyclerView.ViewHolder holder)
        {
            if (!(holder is ISwipeableItemViewHolder))
            {
                return;
            }

            CancelDeferredProcess(holder);
            View containerView = SwipeableViewHolderUtils.GetSwipeableContainerView(holder);
            ViewCompat.Animate(containerView).Cancel();
            if (mActive.Remove(holder))
            {
                throw new InvalidOperationException("after animation is cancelled, item should not be in the active animation list [slide]");
            }
        }

        public virtual void EndAnimations()
        {
            for (int i = mActive.Count - 1; i >= 0; i--)
            {
                RecyclerView.ViewHolder holder = mActive[i];
                EndAnimation(holder);
            }
        }

        public virtual bool IsRunning(RecyclerView.ViewHolder holder)
        {
            return mActive.Contains(holder);
        }

        public virtual bool IsRunning()
        {
            return mActive.Any();
        }

        public virtual int GetImmediatelySetTranslationThreshold()
        {
            return mImmediatelySetTranslationThreshold;
        }

        public virtual void SetImmediatelySetTranslationThreshold(int threshold)
        {
            mImmediatelySetTranslationThreshold = threshold;
        }

        public virtual int GetSwipeContainerViewTranslationX(RecyclerView.ViewHolder holder)
        {
            View containerView = SwipeableViewHolderUtils.GetSwipeableContainerView(holder);
            return (int)(containerView.TranslationX + 0.5F);
        }

        public virtual int GetSwipeContainerViewTranslationY(RecyclerView.ViewHolder holder)
        {
            View containerView = SwipeableViewHolderUtils.GetSwipeableContainerView(holder);
            return (int)(containerView.TranslationY + 0.5F);
        }

        private abstract class ViewHolderDeferredProcess : Java.Lang.Object, IRunnable
        {
            readonly WeakReference<RecyclerView.ViewHolder> mRefHolder;
            public ViewHolderDeferredProcess(RecyclerView.ViewHolder holder)
            {
                mRefHolder = new WeakReference<RecyclerView.ViewHolder>(holder);
            }

            public virtual void Run()
            {
                if (mRefHolder.TryGetTarget(out RecyclerView.ViewHolder holder))
                {
                    OnProcess(holder);
                }
            }

            public virtual bool LostReference(RecyclerView.ViewHolder holder)
            {
                return !mRefHolder.TryGetTarget(out RecyclerView.ViewHolder holder2);
            }

            public virtual bool HasTargetViewHolder(RecyclerView.ViewHolder holder)
            {
                return !mRefHolder.TryGetTarget(out RecyclerView.ViewHolder holder2);
            }

            protected abstract void OnProcess(RecyclerView.ViewHolder holder);
        }

        private sealed class DeferredSlideProcess : ViewHolderDeferredProcess
        {
            readonly float mPosition;
            readonly bool mHorizontal;
            public DeferredSlideProcess(RecyclerView.ViewHolder holder, float position, bool horizontal) : base(holder)
            {
                mPosition = position;
                mHorizontal = horizontal;
            }

            protected override void OnProcess(RecyclerView.ViewHolder holder)
            {
                View containerView = SwipeableViewHolderUtils.GetSwipeableContainerView(holder);
                if (mHorizontal)
                {
                    int width = containerView.Width;
                    int translationX;
                    translationX = (int)(width * mPosition + 0.5F);
                    SlideInternalCompat(holder, true, translationX, 0);
                }
                else
                {
                    int height = containerView.Height;
                    int translationY;
                    translationY = (int)(height * mPosition + 0.5F);
                    SlideInternalCompat(holder, false, 0, translationY);
                }
            }
        }

        private class SlidingAnimatorListenerObject : Java.Lang.Object, IViewPropertyAnimatorListener, IViewPropertyAnimatorUpdateListener
        {
            private SwipeableItemWrapperAdapter mAdapter;
            private List<RecyclerView.ViewHolder> mActive;
            private RecyclerView.ViewHolder mViewHolder;
            private ViewPropertyAnimatorCompat mAnimator;
            private readonly int mToX;
            private readonly int mToY;
            private readonly long mDuration;
            private readonly bool mHorizontal;
            private readonly SwipeFinishInfo mSwipeFinish;
            private readonly IInterpolator mInterpolator;
            private float mInvSize;
            public SlidingAnimatorListenerObject(SwipeableItemWrapperAdapter adapter, List<RecyclerView.ViewHolder> activeViewHolders, RecyclerView.ViewHolder holder, int toX, int toY, long duration, bool horizontal, IInterpolator interpolator, SwipeFinishInfo swipeFinish)
            {
                mAdapter = adapter;
                mActive = activeViewHolders;
                mViewHolder = holder;
                mToX = toX;
                mToY = toY;
                mHorizontal = horizontal;
                mSwipeFinish = swipeFinish;
                mDuration = duration;
                mInterpolator = interpolator;
            }

            public virtual void Start()
            {
                View containerView = SwipeableViewHolderUtils.GetSwipeableContainerView(mViewHolder);
                mInvSize = (1F / Math.Max(1F, mHorizontal ? containerView.Width : containerView.Height));

                // setup animator
                mAnimator = ViewCompat.Animate(containerView);
                mAnimator.SetDuration(mDuration);
                mAnimator.TranslationX(mToX);
                mAnimator.TranslationY(mToY);
                if (mInterpolator != null)
                {
                    mAnimator.SetInterpolator(mInterpolator);
                }

                mAnimator.SetListener(this);
                mAnimator.SetUpdateListener(this);

                // start
                mActive.Add(mViewHolder);
                mAnimator.Start();
            }

            // setup animator
            // start
            public virtual void OnAnimationUpdate(View view)
            {
                float translation = mHorizontal ? view.TranslationX : view.TranslationY;
                float amount = translation * mInvSize;
                mAdapter.OnUpdateSlideAmount(mViewHolder, mViewHolder.LayoutPosition, amount, true, mHorizontal, false);
            }

            // setup animator
            // start
            public virtual void OnAnimationStart(View view)
            {
            }

            // setup animator
            // start
            public virtual void OnAnimationEnd(View view)
            {
                mAnimator.SetListener(null);

                // [WORKAROUND]
                // Issue 189686: NPE can be occurred when using the ViewPropertyAnimatorCompat
                // https://code.google.com/p/android/issues/detail?id=189686
                if (Build.VERSION.SdkInt >= BuildVersionCodes.Kitkat)
                {
                    InternalHelperKK.ClearViewPropertyAnimatorUpdateListener(view);
                }
                else
                {
                    mAnimator.SetUpdateListener(null);
                }

                view.TranslationX=(mToX);
                view.TranslationY=(mToY);
                mActive.Remove(mViewHolder);

                // [WORKAROUND]
                // issue #152 - bug:Samsung S3 4.1.1(Genymotion) with swipe left
                IViewParent itemParentView = mViewHolder.ItemView.Parent;
                if (itemParentView != null)
                {
                    ViewCompat.PostInvalidateOnAnimation((View)itemParentView);
                }

                if (mSwipeFinish != null)
                {
                    mSwipeFinish.resultAction.SlideAnimationEnd();
                }


                // clean up
                mActive = null;
                mAnimator = null;
                mViewHolder = null;
                mAdapter = null;
            }

            // setup animator
            // start
            // [WORKAROUND]
            // Issue 189686: NPE can be occurred when using the ViewPropertyAnimatorCompat
            // https://code.google.com/p/android/issues/detail?id=189686
            // [WORKAROUND]
            // issue #152 - bug:Samsung S3 4.1.1(Genymotion) with swipe left
            // clean up
            public virtual void OnAnimationCancel(View view)
            {
            }
        }

        private class SwipeFinishInfo
        {
            public readonly int itemPosition;
            public SwipeResultAction resultAction;
            public SwipeFinishInfo(int itemPosition, SwipeResultAction resultAction)
            {
                this.itemPosition = itemPosition;
                this.resultAction = resultAction;
            }

            public virtual void Clear()
            {
                this.resultAction = null;
            }
        }
    }
}