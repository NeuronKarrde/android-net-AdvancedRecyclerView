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
using Android.Util;
using Android.Views;
using AndroidX.Core.View;
using Com.H6ah4i.Android.Widget.Advrecyclerview.Adapter;
using Com.H6ah4i.Android.Widget.Advrecyclerview.Swipeable.Action;
using Com.H6ah4i.Android.Widget.Advrecyclerview.Utils;
using AndroidX.RecyclerView.Widget;

namespace Com.H6ah4i.Android.Widget.Advrecyclerview.Swipeable
{
    public class SwipeableItemWrapperAdapter : SimpleWrapperAdapter
    {
        private static readonly string TAG = "ARVSwipeableWrapper";
        private class Constants : SwipeableItemConstants
        {
        }

        private static readonly int STATE_FLAG_INITIAL_VALUE = -1;
        private static readonly bool LOCAL_LOGV = false;
        private static readonly bool LOCAL_LOGD = false;
        private ISwipeableItemAdapter mSwipeableItemAdapter;
        private RecyclerViewSwipeManager mSwipeManager;
        private long mSwipingItemId = RecyclerView.NoId;
        private bool mCallingSwipeStarted;
        public SwipeableItemWrapperAdapter(RecyclerViewSwipeManager manager, RecyclerView.Adapter adapter) : base(adapter)
        {
            mSwipeableItemAdapter = WrapperAdapterUtils.FindWrappedAdapter<ISwipeableItemAdapter>(adapter);
            if (mSwipeableItemAdapter == null)
            {
                throw new ArgumentException("adapter does not implement SwipeableItemAdapter");
            }

            if (manager == null)
            {
                throw new ArgumentException("manager cannot be null");
            }

            mSwipeManager = manager;
        }

        protected override void OnRelease()
        {
            base.OnRelease();
            mSwipeableItemAdapter = null;
            mSwipeManager = null;
            mSwipingItemId = RecyclerView.NoId;
        }

        public override void OnViewRecycled(RecyclerView.ViewHolder holder, int viewType)
        {
            base.OnViewRecycled(holder, viewType);
            if ((mSwipingItemId != RecyclerView.NoId) && (mSwipingItemId == holder.ItemId))
            {
                mSwipeManager.CancelSwipe();
            }


            // reset SwipeableItemViewHolder state
            if (holder is ISwipeableItemViewHolder)
            {
                if (mSwipeManager != null)
                {
                    mSwipeManager.CancelPendingAnimations(holder);
                }

                ISwipeableItemViewHolder swipeableHolder = (ISwipeableItemViewHolder)holder;

                // reset result and reaction (#262)
                swipeableHolder.SwipeResult = (SwipeableItemConstants.RESULT_NONE);
                swipeableHolder.AfterSwipeReaction = (SwipeableItemConstants.AFTER_SWIPE_REACTION_DEFAULT);
                swipeableHolder.SwipeItemHorizontalSlideAmount = (0);
                swipeableHolder.SwipeItemVerticalSlideAmount = (0);
                swipeableHolder.IsProportionalSwipeAmountModeEnabled = (true);
                View containerView = SwipeableViewHolderUtils.GetSwipeableContainerView(swipeableHolder);
                if (containerView != null)
                {
                    ViewCompat.Animate(containerView).Cancel();
                    containerView.TranslationX=(0F);
                    containerView.TranslationY=(0F);
                }
            }
        }

        // reset SwipeableItemViewHolder state
        // reset result and reaction (#262)
        public override RecyclerView.ViewHolder OnCreateViewHolder(ViewGroup parent, int viewType)
        {
            RecyclerView.ViewHolder holder = base.OnCreateViewHolder(parent, viewType);
            if (holder is ISwipeableItemViewHolder)
            {
                ((ISwipeableItemViewHolder)holder).SwipeStateFlags = (STATE_FLAG_INITIAL_VALUE);
            }

            return holder;
        }

        // reset SwipeableItemViewHolder state
        // reset result and reaction (#262)
        public override void OnBindViewHolder(RecyclerView.ViewHolder holder, int position, IList<Java.Lang.Object> payloads)
        {
            float prevSwipeItemSlideAmount = 0;
            ISwipeableItemViewHolder swipeableHolder = (holder is ISwipeableItemViewHolder) ? (((ISwipeableItemViewHolder)holder)) : null;
            if (swipeableHolder != null)
            {
                prevSwipeItemSlideAmount = GetSwipeItemSlideAmount(((ISwipeableItemViewHolder)holder), SwipeHorizontal());
            }

            if (IsSwiping())
            {
                int flags = Constants.StateFlagSwiping;
                if (holder.ItemId == mSwipingItemId)
                {
                    flags |= Constants.StateFlagIsActive;
                }

                SafeUpdateFlags(holder, flags);
                base.OnBindViewHolder(holder, position, payloads);
            }
            else
            {
                SafeUpdateFlags(holder, 0);
                base.OnBindViewHolder(holder, position, payloads);
            }

            if (swipeableHolder != null)
            {
                float swipeItemSlideAmount = GetSwipeItemSlideAmount(swipeableHolder, SwipeHorizontal());
                bool proportionalAmount = swipeableHolder.IsProportionalSwipeAmountModeEnabled;
                bool isSwiping = mSwipeManager.IsSwiping();
                bool isAnimationRunning = mSwipeManager.IsAnimationRunning(holder);
                if ((prevSwipeItemSlideAmount != swipeItemSlideAmount) || !(isSwiping || isAnimationRunning))
                {
                    mSwipeManager.ApplySlideItem(holder, position, prevSwipeItemSlideAmount, swipeItemSlideAmount, proportionalAmount, SwipeHorizontal(), true, isSwiping);
                }
            }
        }

        // reset SwipeableItemViewHolder state
        // reset result and reaction (#262)
        protected override void OnHandleWrappedAdapterChanged()
        {
            if (IsSwiping() && !mCallingSwipeStarted)
            {
                CancelSwipe();
            }

            base.OnHandleWrappedAdapterChanged();
        }

        // reset SwipeableItemViewHolder state
        // reset result and reaction (#262)
        protected override void OnHandleWrappedAdapterItemRangeChanged(int positionStart, int itemCount)
        {
            base.OnHandleWrappedAdapterItemRangeChanged(positionStart, itemCount);
        }

        // reset SwipeableItemViewHolder state
        // reset result and reaction (#262)
        protected override void OnHandleWrappedAdapterItemRangeChanged(int positionStart, int itemCount, Java.Lang.Object payload)
        {
            base.OnHandleWrappedAdapterItemRangeChanged(positionStart, itemCount, payload);
        }

        // reset SwipeableItemViewHolder state
        // reset result and reaction (#262)
        protected override void OnHandleWrappedAdapterItemRangeInserted(int positionStart, int itemCount)
        {
            if (IsSwiping())
            {
                int pos = mSwipeManager.GetSwipingItemPosition();
                if (pos >= positionStart)
                {
                    mSwipeManager.SyncSwipingItemPosition(pos + itemCount);
                }
            }

            base.OnHandleWrappedAdapterItemRangeInserted(positionStart, itemCount);
        }

        // reset SwipeableItemViewHolder state
        // reset result and reaction (#262)
        protected override void OnHandleWrappedAdapterItemRangeRemoved(int positionStart, int itemCount)
        {
            if (IsSwiping())
            {
                int pos = mSwipeManager.GetSwipingItemPosition();
                if (CheckInRange(pos, positionStart, itemCount))
                {
                    CancelSwipe();
                }
                else if (positionStart < pos)
                {
                    mSwipeManager.SyncSwipingItemPosition(pos - itemCount);
                }
            }

            base.OnHandleWrappedAdapterItemRangeRemoved(positionStart, itemCount);
        }

        // reset SwipeableItemViewHolder state
        // reset result and reaction (#262)
        protected override void OnHandleWrappedAdapterRangeMoved(int fromPosition, int toPosition, int itemCount)
        {
            if (IsSwiping())
            {
                mSwipeManager.SyncSwipingItemPosition();
            }

            base.OnHandleWrappedAdapterRangeMoved(fromPosition, toPosition, itemCount);
        }

        // reset SwipeableItemViewHolder state
        // reset result and reaction (#262)
        private void CancelSwipe()
        {
            if (mSwipeManager != null)
            {
                mSwipeManager.CancelSwipe();
            }
        }

        // reset SwipeableItemViewHolder state
        // reset result and reaction (#262)
        // NOTE: This method is called from RecyclerViewSwipeManager
        /*package*/
        public virtual int GetSwipeReactionType(RecyclerView.ViewHolder holder, int position, int x, int y)
        {
            if (LOCAL_LOGV)
            {
                Log.Verbose(TAG, "getSwipeReactionType(holder = " + holder + ", position = " + position + ", x = " + x + ", y = " + y + ")");
            }

            return mSwipeableItemAdapter.OnGetSwipeReactionType(holder, position, x, y);
        }

        // reset SwipeableItemViewHolder state
        // reset result and reaction (#262)
        // NOTE: This method is called from RecyclerViewSwipeManager
        /*package*/
        // NOTE: This method is called from RecyclerViewSwipeManager
        /*package*/
        public virtual void OnUpdateSlideAmount(RecyclerView.ViewHolder holder, int position, float amount, bool proportionalAmount, bool horizontal, bool isSwiping, int type)
        {
            if (LOCAL_LOGV)
            {
                Log.Verbose(TAG, "onUpdateSlideAmount(holder = " + holder + ", position = " + position + ", amount = " + amount + ", proportionalAmount = " + proportionalAmount + ", horizontal = " + horizontal + ", isSwiping = " + isSwiping + ", type = " + type + ")");
            }

            mSwipeableItemAdapter.OnSetSwipeBackground(holder, position, type);
            OnUpdateSlideAmount(holder, position, amount, proportionalAmount, horizontal, isSwiping);
        }

        // reset SwipeableItemViewHolder state
        // reset result and reaction (#262)
        // NOTE: This method is called from RecyclerViewSwipeManager
        /*package*/
        // NOTE: This method is called from RecyclerViewSwipeManager
        /*package*/
        // NOTE: This method is called from ItemSlidingAnimator
        /*package*/
        public virtual void OnUpdateSlideAmount(RecyclerView.ViewHolder holder, int position, float amount, bool proportionalAmount, bool horizontal, bool isSwiping)
        {
            if (LOCAL_LOGV)
            {
                Log.Verbose(TAG, "onUpdateSlideAmount(holder = " + holder + ", position = " + position + ", amount = " + amount + ", proportionalAmount = " + proportionalAmount + ", horizontal = " + horizontal + ", isSwiping = " + isSwiping + ")");
            }

            ISwipeableItemViewHolder holder2 = (ISwipeableItemViewHolder)holder;
            bool isItemExpectsProportionalAmount = holder2.IsProportionalSwipeAmountModeEnabled;
            float adaptedAmount = RecyclerViewSwipeManager.AdaptAmount(holder2, horizontal, amount, proportionalAmount, isItemExpectsProportionalAmount);
            holder2.OnSlideAmountUpdated((horizontal ? adaptedAmount : 0F), (horizontal ? 0F : adaptedAmount), isSwiping);
        }

        // reset SwipeableItemViewHolder state
        // reset result and reaction (#262)
        // NOTE: This method is called from RecyclerViewSwipeManager
        /*package*/
        // NOTE: This method is called from RecyclerViewSwipeManager
        /*package*/
        // NOTE: This method is called from ItemSlidingAnimator
        /*package*/
        // NOTE: This method is called from RecyclerViewSwipeManager
        /*package*/
        public virtual void OnSwipeItemStarted(RecyclerViewSwipeManager manager, RecyclerView.ViewHolder holder, int position, long id)
        {
            if (LOCAL_LOGD)
            {
                Log.Debug(TAG, "onSwipeItemStarted(holder = " + holder + ", position = " + position + ", id = " + id + ")");
            }

            mSwipingItemId = id;
            mCallingSwipeStarted = true;
            mSwipeableItemAdapter.OnSwipeItemStarted(holder, position);
            mCallingSwipeStarted = false;
        }

        // reset SwipeableItemViewHolder state
        // reset result and reaction (#262)
        // NOTE: This method is called from RecyclerViewSwipeManager
        /*package*/
        // NOTE: This method is called from RecyclerViewSwipeManager
        /*package*/
        // NOTE: This method is called from ItemSlidingAnimator
        /*package*/
        // NOTE: This method is called from RecyclerViewSwipeManager
        /*package*/
        // NOTE: This method is called from RecyclerViewSwipeManager
        /*package*/
        public virtual SwipeResultAction OnSwipeItemFinished(RecyclerView.ViewHolder holder, int position, int result)
        {
            if (LOCAL_LOGD)
            {
                Log.Debug(TAG, "onSwipeItemFinished(holder = " + holder + ", position = " + position + ", result = " + result + ")");
            }

            mSwipingItemId = RecyclerView.NoId;
            return mSwipeableItemAdapter.OnSwipeItem(holder, position, result);
        }

        // reset SwipeableItemViewHolder state
        // reset result and reaction (#262)
        // NOTE: This method is called from RecyclerViewSwipeManager
        /*package*/
        // NOTE: This method is called from RecyclerViewSwipeManager
        /*package*/
        // NOTE: This method is called from ItemSlidingAnimator
        /*package*/
        // NOTE: This method is called from RecyclerViewSwipeManager
        /*package*/
        // NOTE: This method is called from RecyclerViewSwipeManager
        /*package*/
        /*package*/
        public virtual void OnSwipeItemFinished2(RecyclerView.ViewHolder holder, int position, int result, int afterReaction, SwipeResultAction resultAction)
        {
            ((ISwipeableItemViewHolder)holder).SwipeResult = (result);
            ((ISwipeableItemViewHolder)holder).AfterSwipeReaction = (afterReaction);
            if (afterReaction != RecyclerViewSwipeManager.AFTER_SWIPE_REACTION_DO_NOTHING)
            {
                SetSwipeItemSlideAmount(((ISwipeableItemViewHolder)holder), GetSwipeAmountFromAfterReaction(result, afterReaction), SwipeHorizontal());
            }

            resultAction.PerformAction();
            NotifyDataSetChanged();
        }

        // reset SwipeableItemViewHolder state
        // reset result and reaction (#262)
        // NOTE: This method is called from RecyclerViewSwipeManager
        /*package*/
        // NOTE: This method is called from RecyclerViewSwipeManager
        /*package*/
        // NOTE: This method is called from ItemSlidingAnimator
        /*package*/
        // NOTE: This method is called from RecyclerViewSwipeManager
        /*package*/
        // NOTE: This method is called from RecyclerViewSwipeManager
        /*package*/
        /*package*/
        protected virtual bool IsSwiping()
        {
            return (mSwipingItemId != RecyclerView.NoId);
        }

        // reset SwipeableItemViewHolder state
        // reset result and reaction (#262)
        // NOTE: This method is called from RecyclerViewSwipeManager
        /*package*/
        // NOTE: This method is called from RecyclerViewSwipeManager
        /*package*/
        // NOTE: This method is called from ItemSlidingAnimator
        /*package*/
        // NOTE: This method is called from RecyclerViewSwipeManager
        /*package*/
        // NOTE: This method is called from RecyclerViewSwipeManager
        /*package*/
        /*package*/
        private static bool CheckInRange(int pos, int start, int count)
        {
            return (pos >= start) && (pos < (start + count));
        }

        // reset SwipeableItemViewHolder state
        // reset result and reaction (#262)
        // NOTE: This method is called from RecyclerViewSwipeManager
        /*package*/
        // NOTE: This method is called from RecyclerViewSwipeManager
        /*package*/
        // NOTE: This method is called from ItemSlidingAnimator
        /*package*/
        // NOTE: This method is called from RecyclerViewSwipeManager
        /*package*/
        // NOTE: This method is called from RecyclerViewSwipeManager
        /*package*/
        /*package*/
        private bool SwipeHorizontal()
        {
            return mSwipeManager.SwipeHorizontal();
        }

        // reset SwipeableItemViewHolder state
        // reset result and reaction (#262)
        // NOTE: This method is called from RecyclerViewSwipeManager
        /*package*/
        // NOTE: This method is called from RecyclerViewSwipeManager
        /*package*/
        // NOTE: This method is called from ItemSlidingAnimator
        /*package*/
        // NOTE: This method is called from RecyclerViewSwipeManager
        /*package*/
        // NOTE: This method is called from RecyclerViewSwipeManager
        /*package*/
        /*package*/
        private static float GetSwipeItemSlideAmount(ISwipeableItemViewHolder holder, bool horizontal)
        {
            if (horizontal)
            {
                return holder.SwipeItemHorizontalSlideAmount;
            }
            else
            {
                return holder.SwipeItemVerticalSlideAmount;
            }
        }

        // reset SwipeableItemViewHolder state
        // reset result and reaction (#262)
        // NOTE: This method is called from RecyclerViewSwipeManager
        /*package*/
        // NOTE: This method is called from RecyclerViewSwipeManager
        /*package*/
        // NOTE: This method is called from ItemSlidingAnimator
        /*package*/
        // NOTE: This method is called from RecyclerViewSwipeManager
        /*package*/
        // NOTE: This method is called from RecyclerViewSwipeManager
        /*package*/
        /*package*/
        private static void SetSwipeItemSlideAmount(ISwipeableItemViewHolder holder, float amount, bool horizontal)
        {
            if (horizontal)
            {
                holder.SwipeItemHorizontalSlideAmount = (amount);
            }
            else
            {
                holder.SwipeItemVerticalSlideAmount = (amount);
            }
        }

        // reset SwipeableItemViewHolder state
        // reset result and reaction (#262)
        // NOTE: This method is called from RecyclerViewSwipeManager
        /*package*/
        // NOTE: This method is called from RecyclerViewSwipeManager
        /*package*/
        // NOTE: This method is called from ItemSlidingAnimator
        /*package*/
        // NOTE: This method is called from RecyclerViewSwipeManager
        /*package*/
        // NOTE: This method is called from RecyclerViewSwipeManager
        /*package*/
        /*package*/
        private static float GetSwipeAmountFromAfterReaction(int result, int afterReaction)
        {
            switch (afterReaction)
            {
                case RecyclerViewSwipeManager.AFTER_SWIPE_REACTION_MOVE_TO_ORIGIN:
                    return 0F;
                case RecyclerViewSwipeManager.AFTER_SWIPE_REACTION_MOVE_TO_SWIPED_DIRECTION:
                case RecyclerViewSwipeManager.AFTER_SWIPE_REACTION_REMOVE_ITEM:
                    switch (result)
                    {
                        case RecyclerViewSwipeManager.ResultSwipedLeft:
                            return RecyclerViewSwipeManager.OUTSIDE_OF_THE_WINDOW_LEFT;
                        case RecyclerViewSwipeManager.ResultSwipedRight:
                            return RecyclerViewSwipeManager.OUTSIDE_OF_THE_WINDOW_RIGHT;
                        case RecyclerViewSwipeManager.ResultSwipedUp:
                            return RecyclerViewSwipeManager.OUTSIDE_OF_THE_WINDOW_TOP;
                        case RecyclerViewSwipeManager.ResultSwipedDown:
                            return RecyclerViewSwipeManager.OUTSIDE_OF_THE_WINDOW_BOTTOM;
                        default:
                            return 0F;
                            break;
                    }

                default:
                    return 0F;
                    break;
            }
        }

        // reset SwipeableItemViewHolder state
        // reset result and reaction (#262)
        // NOTE: This method is called from RecyclerViewSwipeManager
        /*package*/
        // NOTE: This method is called from RecyclerViewSwipeManager
        /*package*/
        // NOTE: This method is called from ItemSlidingAnimator
        /*package*/
        // NOTE: This method is called from RecyclerViewSwipeManager
        /*package*/
        // NOTE: This method is called from RecyclerViewSwipeManager
        /*package*/
        /*package*/
        private static void SafeUpdateFlags(RecyclerView.ViewHolder holder, int flags)
        {
            if (!(holder is ISwipeableItemViewHolder))
            {
                return;
            }

            ISwipeableItemViewHolder holder2 = (ISwipeableItemViewHolder)holder;
            int curFlags = holder2.SwipeStateFlags;
            int mask = ~Constants.StateFlagIsUpdated;

            // append UPDATED flag
            if ((curFlags == STATE_FLAG_INITIAL_VALUE) || (((curFlags ^ flags) & mask) != 0))
            {
                flags |= Constants.StateFlagIsUpdated;
            }

            ((ISwipeableItemViewHolder)holder).SwipeStateFlags = (flags);
        }
    }
}