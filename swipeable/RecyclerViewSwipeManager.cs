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
using Android.Graphics;
using Android.OS;
using Android.Util;
using Android.Views;
using Com.H6ah4i.Android.Widget.Advrecyclerview.Adapter;
using Com.H6ah4i.Android.Widget.Advrecyclerview.Animator;
using Com.H6ah4i.Android.Widget.Advrecyclerview.Swipeable.Action;
using Com.H6ah4i.Android.Widget.Advrecyclerview.Utils;
using AndroidX.RecyclerView.Widget;

namespace Com.H6ah4i.Android.Widget.Advrecyclerview.Swipeable
{
    public class RecyclerViewSwipeManager : SwipeableItemConstants
    {
        private static readonly string TAG = "ARVSwipeManager";
        public interface IOnItemSwipeEventListener
        {
            void OnItemSwipeStarted(int position);
            void OnItemSwipeFinished(int position, int result, int afterSwipeReaction);
        }
        private static readonly int MIN_DISTANCE_TOUCH_SLOP_MUL = 5;
        private static readonly int SLIDE_ITEM_IMMEDIATELY_SET_TRANSLATION_THRESHOLD_DP = 8;
        private static readonly bool LOCAL_LOGV = false;
        private static readonly bool LOCAL_LOGD = false;
        private RecyclerView.IOnItemTouchListener mInternalUseOnItemTouchListener;
        private RecyclerView mRecyclerView;
        private long mReturnToDefaultPositionAnimationDuration = 300;
        private long mMoveToSpecifiedPositionAnimationDuration = 200;
        private long mMoveToOutsideWindowAnimationDuration = 200;
        private int mTouchSlop;
        private int mMinFlingVelocity;
        private int mMaxFlingVelocity;
        private int mSwipeThresholdDistance;
        private int mInitialTouchX;
        private int mInitialTouchY;
        private long mCheckingTouchSlop = RecyclerView.NoId;
        private bool mSwipeHorizontal;
        private ItemSlidingAnimator mItemSlideAnimator;
        private SwipeableItemWrapperAdapter mWrapperAdapter;
        private RecyclerView.ViewHolder mSwipingItem;
        private int mSwipingItemPosition = RecyclerView.NoPosition;
        private long mSwipingItemId = RecyclerView.NoId;
        private readonly Rect mSwipingItemMargins = new Rect();
        private int mTouchedItemOffsetX;
        private int mTouchedItemOffsetY;
        private int mLastTouchX;
        private int mLastTouchY;
        private int mSwipingItemReactionType;
        private VelocityTracker mVelocityTracker;
        private SwipingItemOperator mSwipingItemOperator;
        private IOnItemSwipeEventListener mItemSwipeEventListener;
        private InternalHandler mHandler;
        private int mLongPressTimeout;
        public RecyclerViewSwipeManager()
        {
            mInternalUseOnItemTouchListener = new AnonymousOnItemTouchListener(this);
            mVelocityTracker = VelocityTracker.Obtain();
            mLongPressTimeout = ViewConfiguration.LongPressTimeout;
        }

        private sealed class AnonymousOnItemTouchListener : Java.Lang.Object, RecyclerView.IOnItemTouchListener
        {
            public AnonymousOnItemTouchListener(RecyclerViewSwipeManager parent)
            {
                this.parent = parent;
            }

            private readonly RecyclerViewSwipeManager parent;
            public bool OnInterceptTouchEvent(RecyclerView rv, MotionEvent e)
            {
                return this.parent.OnInterceptTouchEvent(rv, e);
            }

            public void OnTouchEvent(RecyclerView rv, MotionEvent e)
            {
                this.parent.OnTouchEvent(rv, e);
            }

            public void OnRequestDisallowInterceptTouchEvent(bool disallowIntercept)
            {
                this.parent.OnRequestDisallowInterceptTouchEvent(disallowIntercept);
            }
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

            mWrapperAdapter = new SwipeableItemWrapperAdapter(this, adapter);
            return mWrapperAdapter;
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

            int layoutOrientation = CustomRecyclerViewUtils.GetOrientation(rv);
            if (layoutOrientation == CustomRecyclerViewUtils.ORIENTATION_UNKNOWN)
            {
                throw new InvalidOperationException("failed to determine layout orientation");
            }

            mRecyclerView = rv;
            mRecyclerView.AddOnItemTouchListener(mInternalUseOnItemTouchListener);
            ViewConfiguration vc = ViewConfiguration.Get(rv.Context);
            mTouchSlop = vc.ScaledTouchSlop;
            mMinFlingVelocity = vc.ScaledMinimumFlingVelocity;
            mMaxFlingVelocity = vc.ScaledMaximumFlingVelocity;
            mSwipeThresholdDistance = mTouchSlop * MIN_DISTANCE_TOUCH_SLOP_MUL;
            mItemSlideAnimator = new ItemSlidingAnimator(mWrapperAdapter);
            mItemSlideAnimator.SetImmediatelySetTranslationThreshold((int)(rv.Resources.DisplayMetrics.Density * SLIDE_ITEM_IMMEDIATELY_SET_TRANSLATION_THRESHOLD_DP + 0.5F));
            mSwipeHorizontal = (layoutOrientation == CustomRecyclerViewUtils.ORIENTATION_VERTICAL);
            mHandler = new InternalHandler(this);
        }
        public virtual void Release()
        {
            CancelSwipe(true);
            if (mHandler != null)
            {
                mHandler.Release();
                mHandler = null;
            }

            if (mRecyclerView != null && mInternalUseOnItemTouchListener != null)
            {
                mRecyclerView.RemoveOnItemTouchListener(mInternalUseOnItemTouchListener);
            }

            mInternalUseOnItemTouchListener = null;
            if (mVelocityTracker != null)
            {
                mVelocityTracker.Recycle();
                mVelocityTracker = null;
            }

            if (mItemSlideAnimator != null)
            {
                mItemSlideAnimator.EndAnimations();
                mItemSlideAnimator = null;
            }

            mWrapperAdapter = null;
            mRecyclerView = null;
        }
        public virtual bool IsSwiping()
        {
            return (mSwipingItem != null) && (!mHandler.IsCancelSwipeRequested());
        }
        public virtual void SetLongPressTimeout(int longPressTimeout)
        {
            mLongPressTimeout = longPressTimeout;
        }
        public virtual void SetSwipeThresholdDistance(int distanceInPixels)
        {
            mSwipeThresholdDistance = Math.Max(distanceInPixels, mTouchSlop);
        }
        public virtual int GetSwipeThresholdDistance()
        {
            return mSwipeThresholdDistance;
        }
        public virtual bool OnInterceptTouchEvent(RecyclerView rv, MotionEvent e)
        {
            var action = e.ActionMasked;
            if (LOCAL_LOGV)
            {
                Log.Verbose(TAG, "onInterceptTouchEvent() action = " + action);
            }

            switch (action)
            {
                case MotionEventActions.Up:
                case MotionEventActions.Cancel:
                    if (HandleActionUpOrCancel(e, true))
                    {
                        return true;
                    }

                    break;
                case MotionEventActions.Down:
                    if (!IsSwiping())
                    {
                        HandleActionDown(rv, e);
                    }

                    break;
                case MotionEventActions.Move:
                    if (IsSwiping())
                    {
                        HandleActionMoveWhileSwiping(e);
                        return true;
                    }
                    else
                    {
                        if (HandleActionMoveWhileNotSwiping(rv, e))
                        {
                            return true;
                        }
                    }

                    break;
            }

            return false;
        }
        public virtual void OnTouchEvent(RecyclerView rv, MotionEvent e)
        {
            var action = e.ActionMasked;
            if (LOCAL_LOGV)
            {
                Log.Verbose(TAG, "onTouchEvent() action = " + action);
            }

            if (!IsSwiping())
            {
                return;
            }

            switch (action)
            {
                case MotionEventActions.Up:
                case MotionEventActions.Cancel:
                    HandleActionUpOrCancel(e, true);
                    break;
                case MotionEventActions.Move:
                    HandleActionMoveWhileSwiping(e);
                    break;
            }
        }
        public virtual void OnRequestDisallowInterceptTouchEvent(bool disallowIntercept)
        {
            if (disallowIntercept)
            {
                CancelSwipe(true);
            }
        }
        private bool HandleActionDown(RecyclerView rv, MotionEvent e)
        {
            RecyclerView.ViewHolder holder = CustomRecyclerViewUtils.FindChildViewHolderUnderWithTranslation(rv, e.GetX(), e.GetY());
            if (!(holder is ISwipeableItemViewHolder))
            {
                return false;
            }

            int wrappedItemPosition = GetWrappedItemPosition(holder);
            if (!(wrappedItemPosition >= 0 && wrappedItemPosition < mWrapperAdapter.ItemCount))
            {
                return false;
            }

            long wrappedAdapterItemId = ItemIdComposer.ExtractWrappedIdPart(mWrapperAdapter.GetItemId(wrappedItemPosition));
            long wrappedItemId = ItemIdComposer.ExtractWrappedIdPart(holder.ItemId);
            if (wrappedItemId != wrappedAdapterItemId)
            {
                return false;
            }

            int touchX = (int)(e.GetX() + 0.5F);
            int touchY = (int)(e.GetY() + 0.5F);
            View view = holder.ItemView;
            int translateX = (int)(view.TranslationX + 0.5F);
            int translateY = (int)(view.TranslationY + 0.5F);
            int viewX = touchX - (view.Left + translateX);
            int viewY = touchY - (view.Top + translateY);
            int reactionType = mWrapperAdapter.GetSwipeReactionType(holder, wrappedItemPosition, viewX, viewY);
            if (reactionType == 0)
            {
                return false;
            }

            mInitialTouchX = touchX;
            mInitialTouchY = touchY;
            mCheckingTouchSlop = holder.ItemId;
            mSwipingItemReactionType = reactionType;
            if ((reactionType & REACTION_START_SWIPE_ON_LONG_PRESS) != 0)
            {
                mHandler.StartLongPressDetection(e, mLongPressTimeout);
            }

            return true;
        }
        private bool HandleActionUpOrCancel(MotionEvent e, bool invokeFinish)
        {
            var action = MotionEventActions.Cancel;
            if (e != null)
            {
                action = e.ActionMasked;
                mLastTouchX = (int)(e.GetX() + 0.5F);
                mLastTouchY = (int)(e.GetY() + 0.5F);
            }

            if (IsSwiping())
            {
                if (invokeFinish)
                {
                    HandleActionUpOrCancelWhileSwiping(action);
                }

                return true;
            }
            else
            {
                HandleActionUpOrCancelWhileNotSwiping();
                return false;
            }
        }
        private void HandleActionUpOrCancelWhileNotSwiping()
        {
            if (mHandler != null)
            {
                mHandler.CancelLongPressDetection();
            }

            mCheckingTouchSlop = RecyclerView.NoId;
            mSwipingItemReactionType = 0;
        }
        private void HandleActionUpOrCancelWhileSwiping(MotionEventActions action)
        {
            int result = RESULT_CANCELED;
            if (action == MotionEventActions.Up)
            {
                float swipeThresholdDistanceCoeff = 0.8F;
                float swipeThresholdVelocity = mMinFlingVelocity;
                bool horizontal = mSwipeHorizontal;
                ISwipeableItemViewHolder holder = (ISwipeableItemViewHolder)mSwipingItem;
                View containerView = SwipeableViewHolderUtils.GetSwipeableContainerView(holder);
                int containerSize = (horizontal) ? containerView.Width : containerView.Height;
                float distance = (horizontal) ? (mLastTouchX - mInitialTouchX) : (mLastTouchY - mInitialTouchY);
                float absDistance = Math.Abs(distance);
                bool canSwipeNegativeDir = (horizontal) ? SwipeReactionUtils.CanSwipeLeft(mSwipingItemReactionType) : SwipeReactionUtils.CanSwipeUp(mSwipingItemReactionType);
                bool canSwipePositiveDir = (horizontal) ? SwipeReactionUtils.CanSwipeRight(mSwipingItemReactionType) : SwipeReactionUtils.CanSwipeDown(mSwipingItemReactionType);
                bool proportional = holder.IsProportionalSwipeAmountModeEnabled;
                float negativeDirLimit = (horizontal) ? holder.MaxLeftSwipeAmount : holder.MaxUpSwipeAmount;
                float positiveDirLimit = (horizontal) ? holder.MaxRightSwipeAmount : holder.MaxDownSwipeAmount;
                negativeDirLimit = AdaptAmount(holder, horizontal, negativeDirLimit, proportional, false);
                positiveDirLimit = AdaptAmount(holder, horizontal, positiveDirLimit, proportional, false);
                if (IsSpecialSwipeAmountValue(negativeDirLimit))
                {
                    negativeDirLimit = -containerSize;
                }

                if (IsSpecialSwipeAmountValue(positiveDirLimit))
                {
                    positiveDirLimit = containerSize;
                }

                mVelocityTracker.ComputeCurrentVelocity(1000, mMaxFlingVelocity);
                float velocity = (horizontal) ? mVelocityTracker.XVelocity : mVelocityTracker.YVelocity;
                float absVelocity = Math.Abs(velocity);
                bool swiped = false;
                bool positiveDir = false;
                if (absDistance > mSwipeThresholdDistance)
                {
                    if (absVelocity >= swipeThresholdVelocity)
                    {
                        if ((velocity * distance) >= 0)
                        {
                            swiped = true;
                            positiveDir = (velocity > 0);
                        }
                    }
                    else if ((distance < 0) && (distance <= negativeDirLimit * swipeThresholdDistanceCoeff))
                    {
                        swiped = true;
                        positiveDir = false;
                    }
                    else if ((distance > 0) && (distance >= positiveDirLimit * swipeThresholdDistanceCoeff))
                    {
                        swiped = true;
                        positiveDir = true;
                    }
                }

                if (swiped)
                {
                    if (!positiveDir && canSwipeNegativeDir)
                    {
                        result = (horizontal) ? ResultSwipedLeft : ResultSwipedUp;
                    }
                    else if (positiveDir && canSwipePositiveDir)
                    {
                        result = (horizontal) ? ResultSwipedRight : ResultSwipedDown;
                    }
                }
            }

            if (LOCAL_LOGD)
            {
                Log.Debug(TAG, "swiping finished  --- result = " + result);
            }

            FinishSwiping(result);
        }
        private bool HandleActionMoveWhileNotSwiping(RecyclerView rv, MotionEvent e)
        {
            if (mCheckingTouchSlop == RecyclerView.NoId)
            {
                return false;
            }

            int dx = (int)(e.GetX() + 0.5F) - mInitialTouchX;
            int dy = (int)(e.GetY() + 0.5F) - mInitialTouchY;
            int scrollAxisDelta;
            int swipeAxisDelta;
            if (mSwipeHorizontal)
            {
                scrollAxisDelta = dy;
                swipeAxisDelta = dx;
            }
            else
            {
                scrollAxisDelta = dx;
                swipeAxisDelta = dy;
            }

            if (Math.Abs(scrollAxisDelta) > mTouchSlop)
            {
                mCheckingTouchSlop = RecyclerView.NoId;
                return false;
            }

            if (Math.Abs(swipeAxisDelta) <= mTouchSlop)
            {
                return false;
            }
            bool dirMasked;
            if (mSwipeHorizontal)
            {
                if (swipeAxisDelta < 0)
                {
                    dirMasked = ((mSwipingItemReactionType & REACTION_MASK_START_SWIPE_LEFT) != 0);
                }
                else
                {
                    dirMasked = ((mSwipingItemReactionType & REACTION_MASK_START_SWIPE_RIGHT) != 0);
                }
            }
            else
            {
                if (swipeAxisDelta < 0)
                {
                    dirMasked = ((mSwipingItemReactionType & REACTION_MASK_START_SWIPE_UP) != 0);
                }
                else
                {
                    dirMasked = ((mSwipingItemReactionType & REACTION_MASK_START_SWIPE_DOWN) != 0);
                }
            }

            if (dirMasked)
            {
                mCheckingTouchSlop = RecyclerView.NoId;
                return false;
            }

            RecyclerView.ViewHolder holder = CustomRecyclerViewUtils.FindChildViewHolderUnderWithTranslation(rv, e.GetX(), e.GetY());
            if (holder == null || holder.ItemId != mCheckingTouchSlop)
            {
                mCheckingTouchSlop = RecyclerView.NoId;
                return false;
            }

            return CheckConditionAndStartSwiping(e, holder);
        }
        private void HandleActionMoveWhileSwiping(MotionEvent e)
        {
            mLastTouchX = (int)(e.GetX() + 0.5F);
            mLastTouchY = (int)(e.GetY() + 0.5F);
            mVelocityTracker.AddMovement(e);
            int swipeDistanceX = mLastTouchX - mTouchedItemOffsetX;
            int swipeDistanceY = mLastTouchY - mTouchedItemOffsetY;
            int swipingItemPosition = GetSwipingItemPosition();
            mSwipingItemOperator.Update(swipingItemPosition, swipeDistanceX, swipeDistanceY);
        }
        private bool CheckConditionAndStartSwiping(MotionEvent e, RecyclerView.ViewHolder holder)
        {
            int wrappedItemPosition = GetWrappedItemPosition(holder);
            if (wrappedItemPosition == RecyclerView.NoPosition)
            {
                return false;
            }

            StartSwiping(e, holder, wrappedItemPosition);
            return true;
        }
        private void StartSwiping(MotionEvent e, RecyclerView.ViewHolder holder, int itemPosition)
        {
            if (LOCAL_LOGD)
            {
                Log.Debug(TAG, "swiping started");
            }

            mHandler.CancelLongPressDetection();
            mSwipingItem = holder;
            mSwipingItemPosition = itemPosition;
            mSwipingItemId = mWrapperAdapter.GetItemId(itemPosition);
            mLastTouchX = (int)(e.GetX() + 0.5F);
            mLastTouchY = (int)(e.GetY() + 0.5F);
            mTouchedItemOffsetX = mLastTouchX;
            mTouchedItemOffsetY = mLastTouchY;
            mCheckingTouchSlop = RecyclerView.NoId;
            CustomRecyclerViewUtils.GetLayoutMargins(holder.ItemView, mSwipingItemMargins);
            mSwipingItemOperator = new SwipingItemOperator(this, mSwipingItem, mSwipingItemReactionType, mSwipeHorizontal);
            mSwipingItemOperator.Start();
            mVelocityTracker.Clear();
            mVelocityTracker.AddMovement(e);
            mRecyclerView.Parent.RequestDisallowInterceptTouchEvent(true);
            if (mItemSwipeEventListener != null)
            {
                mItemSwipeEventListener.OnItemSwipeStarted(itemPosition);
            }
            mWrapperAdapter.OnSwipeItemStarted(this, holder, itemPosition, mSwipingItemId);
        }
        private void FinishSwiping(int result)
        {
            RecyclerView.ViewHolder swipingItem = mSwipingItem;
            if (swipingItem == null)
            {
                return;
            }
            mHandler.RemoveDeferredCancelSwipeRequest();
            mHandler.CancelLongPressDetection();
            if (mRecyclerView != null && mRecyclerView.Parent != null)
            {
                mRecyclerView.Parent.RequestDisallowInterceptTouchEvent(false);
            }

            int itemPosition = GetSwipingItemPosition();
            mVelocityTracker.Clear();
            mSwipingItem = null;
            mSwipingItemPosition = RecyclerView.NoPosition;
            mSwipingItemId = RecyclerView.NoId;
            mLastTouchX = 0;
            mLastTouchY = 0;
            mInitialTouchX = 0;
            mTouchedItemOffsetX = 0;
            mTouchedItemOffsetY = 0;
            mCheckingTouchSlop = RecyclerView.NoId;
            mSwipingItemReactionType = 0;
            if (mSwipingItemOperator != null)
            {
                mSwipingItemOperator.Finish();
                mSwipingItemOperator = null;
            }

            int slideDir = ResultCodeToSlideDirection(result);
            SwipeResultAction resultAction = null;
            if (mWrapperAdapter != null)
            {
                resultAction = mWrapperAdapter.OnSwipeItemFinished(swipingItem, itemPosition, result);
            }

            if (resultAction == null)
            {
                resultAction = new SwipeResultActionDefault();
            }

            int afterReaction = resultAction.GetResultActionType();
            VerifyAfterReaction(result, afterReaction);
            bool slideAnimated = false;
            switch (afterReaction)
            {
                case AFTER_SWIPE_REACTION_MOVE_TO_ORIGIN:
                    slideAnimated = mItemSlideAnimator.FinishSwipeSlideToDefaultPosition(swipingItem, mSwipeHorizontal, true, mReturnToDefaultPositionAnimationDuration, itemPosition, resultAction);
                    break;
                case AFTER_SWIPE_REACTION_MOVE_TO_SWIPED_DIRECTION:
                    slideAnimated = mItemSlideAnimator.FinishSwipeSlideToOutsideOfWindow(swipingItem, slideDir, true, mMoveToOutsideWindowAnimationDuration, itemPosition, resultAction);
                    break;
                case AFTER_SWIPE_REACTION_REMOVE_ITEM:
                {
                    RecyclerView.ItemAnimator itemAnimator = mRecyclerView.GetItemAnimator();
                    long removeAnimationDuration = (itemAnimator != null) ? itemAnimator.RemoveDuration : 0;
                    long moveAnimationDuration = (itemAnimator != null) ? itemAnimator.MoveDuration : 0;
                    RemovingItemDecorator decorator = new RemovingItemDecorator(mRecyclerView, swipingItem, result, removeAnimationDuration, moveAnimationDuration);
                    decorator.SetMoveAnimationInterpolator(SwipeDismissItemAnimator.MOVE_INTERPOLATOR);
                    decorator.Start();
                    slideAnimated = mItemSlideAnimator.FinishSwipeSlideToOutsideOfWindow(swipingItem, slideDir, true, removeAnimationDuration, itemPosition, resultAction);
                }

                    break;
                case AFTER_SWIPE_REACTION_DO_NOTHING:
                    break;
                default:
                    throw new InvalidOperationException("Unknown after reaction type: " + afterReaction);
                    break;
            }

            if (mWrapperAdapter != null)
            {
                mWrapperAdapter.OnSwipeItemFinished2(swipingItem, itemPosition, result, afterReaction, resultAction);
            }
            if (mItemSwipeEventListener != null)
            {
                mItemSwipeEventListener.OnItemSwipeFinished(itemPosition, result, afterReaction);
            }
            if (!slideAnimated)
            {
                resultAction.SlideAnimationEnd();
            }
        }
        private static void VerifyAfterReaction(int result, int afterReaction)
        {
            if ((afterReaction == AFTER_SWIPE_REACTION_MOVE_TO_SWIPED_DIRECTION) || (afterReaction == AFTER_SWIPE_REACTION_REMOVE_ITEM))
            {
                switch (result)
                {
                    case ResultSwipedLeft:
                    case ResultSwipedUp:
                    case ResultSwipedRight:
                    case ResultSwipedDown:
                        break;
                    default:
                        throw new InvalidOperationException("Unexpected after reaction has been requested: result = " + result + ", afterReaction = " + afterReaction);
                        break;
                }
            }
        }
        private static int ResultCodeToSlideDirection(int result)
        {
            switch (result)
            {
                case ResultSwipedLeft:
                    return ItemSlidingAnimator.DIR_LEFT;
                case ResultSwipedUp:
                    return ItemSlidingAnimator.DIR_UP;
                case ResultSwipedRight:
                    return ItemSlidingAnimator.DIR_RIGHT;
                case ResultSwipedDown:
                    return ItemSlidingAnimator.DIR_DOWN;
                default:
                    return ItemSlidingAnimator.DIR_LEFT;
                    break;
            }
        }
        static int GetItemPosition(RecyclerView.Adapter adapter, long itemId, int itemPositionGuess)
        {
            if (adapter == null)
                return RecyclerView.NoPosition;
            int itemCount = adapter.ItemCount;
            if (itemPositionGuess >= 0 && itemPositionGuess < itemCount)
            {
                if (adapter.GetItemId(itemPositionGuess) == itemId)
                    return itemPositionGuess;
            }

            for (int i = 0; i < itemCount; i++)
            {
                if (adapter.GetItemId(i) == itemId)
                    return i;
            }

            return RecyclerView.NoPosition;
        }
        public virtual void CancelSwipe()
        {
            CancelSwipe(false);
        }
        public virtual bool PerformFakeSwipe(RecyclerView.ViewHolder holder, int result)
        {
            if (!(holder is ISwipeableItemViewHolder))
            {
                return false;
            }

            if (IsSwiping())
            {
                return false;
            }

            switch (result)
            {
                case ResultSwipedLeft:
                case ResultSwipedRight:
                    if (!mSwipeHorizontal)
                    {
                        return false;
                    }

                    break;
                case ResultSwipedUp:
                case ResultSwipedDown:
                    if (mSwipeHorizontal)
                    {
                        return false;
                    }

                    break;
                case RESULT_CANCELED:
                    break;
                default:
                    return false;
                    break;
            }

            int wrappedItemPosition = GetWrappedItemPosition(holder);
            if (wrappedItemPosition == RecyclerView.NoPosition)
            {
                return false;
            }

            MotionEvent fakeMotionEvent = MotionEvent.Obtain(SystemClock.UptimeMillis(), SystemClock.UptimeMillis(), MotionEventActions.Down, 0, 0, 0);
            StartSwiping(fakeMotionEvent, holder, wrappedItemPosition);
            fakeMotionEvent.Recycle();
            int direction = 0;
            if (result == ResultSwipedLeft || result == ResultSwipedUp)
            {
                direction = -1;
            }
            else if (result == ResultSwipedRight || result == ResultSwipedDown)
            {
                direction = 1;
            }

            ApplySlideItem(holder, wrappedItemPosition, 0, direction, false, mSwipeHorizontal, false, true);
            FinishSwiping(result);
            return true;
        }
        public virtual void CancelSwipe(bool immediately)
        {
            HandleActionUpOrCancel(null, false);
            if (immediately)
            {
                FinishSwiping(RESULT_CANCELED);
            }
            else
            {
                if (IsSwiping())
                {
                    mHandler.RequestDeferredCancelSwipe();
                }
            }
        }
        public virtual bool IsAnimationRunning(RecyclerView.ViewHolder item)
        {
            return (mItemSlideAnimator != null) && (mItemSlideAnimator.IsRunning(item));
        }
        private void SlideItem(RecyclerView.ViewHolder holder, float amount, bool proportionalAmount, bool horizontal, bool shouldAnimate)
        {
            if (amount == OUTSIDE_OF_THE_WINDOW_LEFT)
            {
                mItemSlideAnimator.SlideToOutsideOfWindow(holder, ItemSlidingAnimator.DIR_LEFT, shouldAnimate, mMoveToOutsideWindowAnimationDuration);
            }
            else if (amount == OUTSIDE_OF_THE_WINDOW_TOP)
            {
                mItemSlideAnimator.SlideToOutsideOfWindow(holder, ItemSlidingAnimator.DIR_UP, shouldAnimate, mMoveToOutsideWindowAnimationDuration);
            }
            else if (amount == OUTSIDE_OF_THE_WINDOW_RIGHT)
            {
                mItemSlideAnimator.SlideToOutsideOfWindow(holder, ItemSlidingAnimator.DIR_RIGHT, shouldAnimate, mMoveToOutsideWindowAnimationDuration);
            }
            else if (amount == OUTSIDE_OF_THE_WINDOW_BOTTOM)
            {
                mItemSlideAnimator.SlideToOutsideOfWindow(holder, ItemSlidingAnimator.DIR_DOWN, shouldAnimate, mMoveToOutsideWindowAnimationDuration);
            }
            else if (amount == 0F)
            {
                mItemSlideAnimator.SlideToDefaultPosition(holder, horizontal, shouldAnimate, mReturnToDefaultPositionAnimationDuration);
            }
            else
            {
                mItemSlideAnimator.SlideToSpecifiedPosition(holder, amount, proportionalAmount, horizontal, shouldAnimate, mMoveToSpecifiedPositionAnimationDuration);
            }
        }
        private int GetWrappedItemPosition(RecyclerView.ViewHolder holder)
        {
            RecyclerView.Adapter rootAdapter = mRecyclerView.GetAdapter();
            int rootItemPosition = CustomRecyclerViewUtils.GetSynchronizedPosition(holder);
            return WrapperAdapterUtils.UnwrapPosition(rootAdapter, mWrapperAdapter, rootItemPosition);
        }
        public virtual long GetReturnToDefaultPositionAnimationDuration()
        {
            return mReturnToDefaultPositionAnimationDuration;
        }
        public virtual void SetReturnToDefaultPositionAnimationDuration(long duration)
        {
            mReturnToDefaultPositionAnimationDuration = duration;
        }
        public virtual long GetMoveToSpecifiedPositionAnimationDuration()
        {
            return mMoveToSpecifiedPositionAnimationDuration;
        }
        public virtual void SetMoveToSpecifiedPositionAnimationDuration(long duration)
        {
            mMoveToSpecifiedPositionAnimationDuration = duration;
        }
        public virtual long GetMoveToOutsideWindowAnimationDuration()
        {
            return mMoveToOutsideWindowAnimationDuration;
        }
        public virtual void SetMoveToOutsideWindowAnimationDuration(long duration)
        {
            mMoveToOutsideWindowAnimationDuration = duration;
        }
        public virtual IOnItemSwipeEventListener GetOnItemSwipeEventListener()
        {
            return mItemSwipeEventListener;
        }
        public virtual void SetOnItemSwipeEventListener(IOnItemSwipeEventListener listener)
        {
            mItemSwipeEventListener = listener;
        }
        public virtual bool SwipeHorizontal()
        {
            return mSwipeHorizontal;
        }
        public virtual void ApplySlideItem(RecyclerView.ViewHolder holder, int itemPosition, float prevAmount, float amount, bool proportionalAmount, bool horizontal, bool shouldAnimate, bool isSwiping)
        {
            ISwipeableItemViewHolder holder2 = (ISwipeableItemViewHolder)holder;
            View containerView = SwipeableViewHolderUtils.GetSwipeableContainerView(holder2);
            if (containerView == null)
            {
                return;
            }

            int reqBackgroundType;
            if (amount == 0F)
            {
                if (prevAmount == 0F)
                {
                    reqBackgroundType = DrawableSwipeNeutralBackground;
                }
                else
                {
                    reqBackgroundType = DetermineBackgroundType(prevAmount, horizontal);
                }
            }
            else
            {
                reqBackgroundType = DetermineBackgroundType(amount, horizontal);
            }

            float adjustedAmount = amount;
            if (amount != 0F)
            {
                bool isLimitProportional = holder2.IsProportionalSwipeAmountModeEnabled;
                float minLimit = horizontal ? holder2.MaxLeftSwipeAmount : holder2.MaxUpSwipeAmount;
                float maxLimit = horizontal ? holder2.MaxRightSwipeAmount : holder2.MaxDownSwipeAmount;
                minLimit = AdaptAmount(holder2, horizontal, minLimit, isLimitProportional, proportionalAmount);
                maxLimit = AdaptAmount(holder2, horizontal, maxLimit, isLimitProportional, proportionalAmount);
                adjustedAmount = Math.Max(adjustedAmount, minLimit);
                adjustedAmount = Math.Min(adjustedAmount, maxLimit);
            }

            SlideItem(holder, adjustedAmount, proportionalAmount, horizontal, shouldAnimate);
            mWrapperAdapter.OnUpdateSlideAmount(holder, itemPosition, amount, proportionalAmount, horizontal, isSwiping, reqBackgroundType);
        }
        private static int DetermineBackgroundType(float amount, bool horizontal)
        {
            if (horizontal)
            {
                return (amount < 0) ? DrawableSwipeLeftBackground : DrawableSwipeRightBackground;
            }
            else
            {
                return (amount < 0) ? DrawableSwipeUpBackground : DrawableSwipeDownBackground;
            }
        }
        public virtual void CancelPendingAnimations(RecyclerView.ViewHolder holder)
        {
            if (mItemSlideAnimator != null)
            {
                mItemSlideAnimator.EndAnimation(holder);
            }
        }
        public virtual int GetSwipeContainerViewTranslationX(RecyclerView.ViewHolder holder)
        {
            return mItemSlideAnimator.GetSwipeContainerViewTranslationX(holder);
        }
        public virtual int GetSwipeContainerViewTranslationY(RecyclerView.ViewHolder holder)
        {
            return mItemSlideAnimator.GetSwipeContainerViewTranslationY(holder);
        }
        public virtual void HandleOnLongPress(MotionEvent e)
        {
            RecyclerView.ViewHolder holder = mRecyclerView.FindViewHolderForItemId(mCheckingTouchSlop);
            if (holder != null)
            {
                CheckConditionAndStartSwiping(e, holder);
            }
        }
        public virtual int GetSwipingItemPosition()
        {
            return mSwipingItemPosition;
        }
        public virtual int SyncSwipingItemPosition()
        {
            return SyncSwipingItemPosition(mSwipingItemPosition);
        }
        public virtual int SyncSwipingItemPosition(int positionGuess)
        {
            mSwipingItemPosition = GetItemPosition(mWrapperAdapter, mSwipingItemId, positionGuess);
            return mSwipingItemPosition;
        }
        public static float AdaptAmount(ISwipeableItemViewHolder holder, bool horizontal, float srcAmount, bool isSrcProportional, bool isDestProportional)
        {
            float destAmount = srcAmount;
            if ((isSrcProportional ^ isDestProportional) && (srcAmount != 0F) && !IsSpecialSwipeAmountValue(srcAmount))
            {
                View v = SwipeableViewHolderUtils.GetSwipeableContainerView(holder);
                float d = (horizontal) ? v.Width : v.Height;
                if (isDestProportional)
                {
                    d = (d != 0) ? (1 / d) : 0;
                }

                destAmount *= d;
            }

            return destAmount;
        }
        private static bool IsSpecialSwipeAmountValue(float amount)
        {
            return (amount == SwipeableItemConstants.OUTSIDE_OF_THE_WINDOW_LEFT) || (amount == SwipeableItemConstants.OUTSIDE_OF_THE_WINDOW_RIGHT) || (amount == SwipeableItemConstants.OUTSIDE_OF_THE_WINDOW_TOP) || (amount == SwipeableItemConstants.OUTSIDE_OF_THE_WINDOW_BOTTOM);
        }
        private class InternalHandler : Handler
        {
            private const int MSG_LONGPRESS = 1;
            private const int MSG_DEFERRED_CANCEL_SWIPE = 2;
            private RecyclerViewSwipeManager mHolder;
            private MotionEvent mDownMotionEvent;
            public InternalHandler(RecyclerViewSwipeManager holder)
            {
                mHolder = holder;
            }

            public virtual void Release()
            {
                RemoveCallbacksAndMessages(null);
                mHolder = null;
            }

            public override void HandleMessage(Message msg)
            {
                switch (msg.What)
                {
                    case MSG_LONGPRESS:
                        mHolder.HandleOnLongPress(mDownMotionEvent);
                        break;
                    case MSG_DEFERRED_CANCEL_SWIPE:
                        mHolder.CancelSwipe(true);
                        break;
                }
            }

            public virtual void StartLongPressDetection(MotionEvent e, int timeout)
            {
                CancelLongPressDetection();
                mDownMotionEvent = MotionEvent.Obtain(e);
                SendEmptyMessageAtTime(MSG_LONGPRESS, e.DownTime + timeout);
            }

            public virtual void CancelLongPressDetection()
            {
                RemoveMessages(MSG_LONGPRESS);
                if (mDownMotionEvent != null)
                {
                    mDownMotionEvent.Recycle();
                    mDownMotionEvent = null;
                }
            }

            public virtual void RemoveDeferredCancelSwipeRequest()
            {
                RemoveMessages(MSG_DEFERRED_CANCEL_SWIPE);
            }

            public virtual void RequestDeferredCancelSwipe()
            {
                if (IsCancelSwipeRequested())
                {
                    return;
                }

                SendEmptyMessage(MSG_DEFERRED_CANCEL_SWIPE);
            }

            public virtual bool IsCancelSwipeRequested()
            {
                return HasMessages(MSG_DEFERRED_CANCEL_SWIPE);
            }
        }
    }
}