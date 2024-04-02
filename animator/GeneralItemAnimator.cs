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
using AndroidX.Core.View;
using AndroidX.RecyclerView.Widget;
using Com.H6ah4i.Android.Widget.Advrecyclerview.Animator.Impl;
using Java.Lang;
using Math = System.Math;

namespace Com.H6ah4i.Android.Widget.Advrecyclerview.Animator
{
    public abstract class GeneralItemAnimator : BaseItemAnimator
    {
        private static readonly string TAG = "ARVGeneralItemAnimator";
        private bool mDebug;
        private ItemRemoveAnimationManager mRemoveAnimationManager;
        private ItemAddAnimationManager mAddAnimationsManager;
        private ItemChangeAnimationManager mChangeAnimationsManager;
        private ItemMoveAnimationManager mMoveAnimationsManager;
        protected GeneralItemAnimator()
        {
            Setup();
        }

        private void Setup()
        {
            OnSetup();
            if (mRemoveAnimationManager == null || mAddAnimationsManager == null || mChangeAnimationsManager == null || mMoveAnimationsManager == null)
            {
                throw new InvalidOperationException("setup incomplete");
            }
        }

        protected abstract void OnSetup();
        public override void RunPendingAnimations()
        {
            if (!HasPendingAnimations())
            {
                return;
            }

            OnSchedulePendingAnimations();
        }

        public override bool AnimateRemove(RecyclerView.ViewHolder holder)
        {
            if (mDebug)
            {
                Log.Debug(TAG, "animateRemove(id = " + holder.ItemId + ", position = " + holder.LayoutPosition + ")");
            }

            return mRemoveAnimationManager.AddPendingAnimation(holder);
        }

        public override bool AnimateAdd(RecyclerView.ViewHolder holder)
        {
            if (mDebug)
            {
                Log.Debug(TAG, "animateAdd(id = " + holder.ItemId + ", position = " + holder.LayoutPosition + ")");
            }

            return mAddAnimationsManager.AddPendingAnimation(holder);
        }

        public override bool AnimateMove(RecyclerView.ViewHolder holder, int fromX, int fromY, int toX, int toY)
        {
            if (mDebug)
            {
                Log.Debug(TAG, "animateMove(id = " + holder.ItemId + ", position = " + holder.LayoutPosition + ", fromX = " + fromX + ", fromY = " + fromY + ", toX = " + toX + ", toY = " + toY + ")");
            }

            return mMoveAnimationsManager.AddPendingAnimation(holder, fromX, fromY, toX, toY);
        }

        public override bool AnimateChange(RecyclerView.ViewHolder oldHolder, RecyclerView.ViewHolder newHolder, int fromX, int fromY, int toX, int toY)
        {
            if (oldHolder == newHolder)
            {

                // NOTE: This condition can be occurred since v23.1.0.
                // Don't know how to run change animations when the same view holder is re-used.
                // run a move animation to handle position changes.
                return mMoveAnimationsManager.AddPendingAnimation(oldHolder, fromX, fromY, toX, toY);
            }

            if (mDebug)
            {
                string oldId = (oldHolder != null) ? Long.ToString(oldHolder.ItemId) : "-";
                string oldPosition = (oldHolder != null) ? Long.ToString(oldHolder.LayoutPosition) : "-";
                string newId = (newHolder != null) ? Long.ToString(newHolder.ItemId) : "-";
                string newPosition = (newHolder != null) ? Long.ToString(newHolder.LayoutPosition) : "-";
                Log.Debug(TAG, "animateChange(old.id = " + oldId + ", old.position = " + oldPosition + ", new.id = " + newId + ", new.position = " + newPosition + ", fromX = " + fromX + ", fromY = " + fromY + ", toX = " + toX + ", toY = " + toY + ")");
            }

            return mChangeAnimationsManager.AddPendingAnimation(oldHolder, newHolder, fromX, fromY, toX, toY);
        }

        // NOTE: This condition can be occurred since v23.1.0.
        // Don't know how to run change animations when the same view holder is re-used.
        // run a move animation to handle position changes.
        protected virtual void CancelAnimations(RecyclerView.ViewHolder item)
        {
            ViewCompat.Animate(item.ItemView).Cancel();
        }

        // NOTE: This condition can be occurred since v23.1.0.
        // Don't know how to run change animations when the same view holder is re-used.
        // run a move animation to handle position changes.
        public override void EndAnimation(RecyclerView.ViewHolder item)
        {

            // this will trigger end callback which should set properties to their target values.
            CancelAnimations(item);
            mMoveAnimationsManager.EndPendingAnimations(item);
            mChangeAnimationsManager.EndPendingAnimations(item);
            mRemoveAnimationManager.EndPendingAnimations(item);
            mAddAnimationsManager.EndPendingAnimations(item);
            mMoveAnimationsManager.EndDeferredReadyAnimations(item);
            mChangeAnimationsManager.EndDeferredReadyAnimations(item);
            mRemoveAnimationManager.EndDeferredReadyAnimations(item);
            mAddAnimationsManager.EndDeferredReadyAnimations(item);

            // animations should be ended by the cancel above.
            if (mRemoveAnimationManager.RemoveFromActive(item) && mDebug)
            {
                throw new InvalidOperationException("after animation is cancelled, item should not be in the active animation list [remove]");
            }

            if (mAddAnimationsManager.RemoveFromActive(item) && mDebug)
            {
                throw new InvalidOperationException("after animation is cancelled, item should not be in the active animation list [add]");
            }

            if (mChangeAnimationsManager.RemoveFromActive(item) && mDebug)
            {
                throw new InvalidOperationException("after animation is cancelled, item should not be in the active animation list [change]");
            }

            if (mMoveAnimationsManager.RemoveFromActive(item) && mDebug)
            {
                throw new InvalidOperationException("after animation is cancelled, item should not be in the active animation list [move]");
            }

            DispatchFinishedWhenDone();
        }

        // NOTE: This condition can be occurred since v23.1.0.
        // Don't know how to run change animations when the same view holder is re-used.
        // run a move animation to handle position changes.
        // this will trigger end callback which should set properties to their target values.
        // animations should be ended by the cancel above.
        public override bool IsRunning => mRemoveAnimationManager.IsRunning() || mAddAnimationsManager.IsRunning() || mChangeAnimationsManager.IsRunning() || mMoveAnimationsManager.IsRunning();
        

        // NOTE: This condition can be occurred since v23.1.0.
        // Don't know how to run change animations when the same view holder is re-used.
        // run a move animation to handle position changes.
        // this will trigger end callback which should set properties to their target values.
        // animations should be ended by the cancel above.
        public override void EndAnimations()
        {

            // end all pending animations
            mMoveAnimationsManager.EndAllPendingAnimations();
            mRemoveAnimationManager.EndAllPendingAnimations();
            mAddAnimationsManager.EndAllPendingAnimations();
            mChangeAnimationsManager.EndAllPendingAnimations();
            if (!IsRunning)
            {
                return;
            }


            // end all deferred animations
            mMoveAnimationsManager.EndAllDeferredReadyAnimations();
            mAddAnimationsManager.EndAllDeferredReadyAnimations();
            mChangeAnimationsManager.EndAllDeferredReadyAnimations();

            // cancel all started animations
            mRemoveAnimationManager.CancelAllStartedAnimations();
            mMoveAnimationsManager.CancelAllStartedAnimations();
            mAddAnimationsManager.CancelAllStartedAnimations();
            mChangeAnimationsManager.CancelAllStartedAnimations();
            DispatchAnimationsFinished();
        }

        // NOTE: This condition can be occurred since v23.1.0.
        // Don't know how to run change animations when the same view holder is re-used.
        // run a move animation to handle position changes.
        // this will trigger end callback which should set properties to their target values.
        // animations should be ended by the cancel above.
        // end all pending animations
        // end all deferred animations
        // cancel all started animations
        public override bool DebugLogEnabled()
        {
            return mDebug;
        }

        // NOTE: This condition can be occurred since v23.1.0.
        // Don't know how to run change animations when the same view holder is re-used.
        // run a move animation to handle position changes.
        // this will trigger end callback which should set properties to their target values.
        // animations should be ended by the cancel above.
        // end all pending animations
        // end all deferred animations
        // cancel all started animations
        public override bool DispatchFinishedWhenDone()
        {
            if (mDebug && !IsRunning)
            {
                Log.Debug(TAG, "dispatchFinishedWhenDone()");
            }

            return base.DispatchFinishedWhenDone();
        }

        // NOTE: This condition can be occurred since v23.1.0.
        // Don't know how to run change animations when the same view holder is re-used.
        // run a move animation to handle position changes.
        // this will trigger end callback which should set properties to their target values.
        // animations should be ended by the cancel above.
        // end all pending animations
        // end all deferred animations
        // cancel all started animations
        protected virtual bool HasPendingAnimations()
        {
            return (mRemoveAnimationManager.HasPending() || mMoveAnimationsManager.HasPending() || mChangeAnimationsManager.HasPending() || mAddAnimationsManager.HasPending());
        }

        // NOTE: This condition can be occurred since v23.1.0.
        // Don't know how to run change animations when the same view holder is re-used.
        // run a move animation to handle position changes.
        // this will trigger end callback which should set properties to their target values.
        // animations should be ended by the cancel above.
        // end all pending animations
        // end all deferred animations
        // cancel all started animations
        protected virtual ItemRemoveAnimationManager GetRemoveAnimationManager()
        {
            return mRemoveAnimationManager;
        }

        // NOTE: This condition can be occurred since v23.1.0.
        // Don't know how to run change animations when the same view holder is re-used.
        // run a move animation to handle position changes.
        // this will trigger end callback which should set properties to their target values.
        // animations should be ended by the cancel above.
        // end all pending animations
        // end all deferred animations
        // cancel all started animations
        protected virtual void SetItemRemoveAnimationManager(ItemRemoveAnimationManager removeAnimationManager)
        {
            mRemoveAnimationManager = removeAnimationManager;
        }

        // NOTE: This condition can be occurred since v23.1.0.
        // Don't know how to run change animations when the same view holder is re-used.
        // run a move animation to handle position changes.
        // this will trigger end callback which should set properties to their target values.
        // animations should be ended by the cancel above.
        // end all pending animations
        // end all deferred animations
        // cancel all started animations
        protected virtual ItemAddAnimationManager GetItemAddAnimationsManager()
        {
            return mAddAnimationsManager;
        }

        // NOTE: This condition can be occurred since v23.1.0.
        // Don't know how to run change animations when the same view holder is re-used.
        // run a move animation to handle position changes.
        // this will trigger end callback which should set properties to their target values.
        // animations should be ended by the cancel above.
        // end all pending animations
        // end all deferred animations
        // cancel all started animations
        protected virtual void SetItemAddAnimationsManager(ItemAddAnimationManager addAnimationsManager)
        {
            mAddAnimationsManager = addAnimationsManager;
        }

        // NOTE: This condition can be occurred since v23.1.0.
        // Don't know how to run change animations when the same view holder is re-used.
        // run a move animation to handle position changes.
        // this will trigger end callback which should set properties to their target values.
        // animations should be ended by the cancel above.
        // end all pending animations
        // end all deferred animations
        // cancel all started animations
        protected virtual ItemChangeAnimationManager GetItemChangeAnimationsManager()
        {
            return mChangeAnimationsManager;
        }

        // NOTE: This condition can be occurred since v23.1.0.
        // Don't know how to run change animations when the same view holder is re-used.
        // run a move animation to handle position changes.
        // this will trigger end callback which should set properties to their target values.
        // animations should be ended by the cancel above.
        // end all pending animations
        // end all deferred animations
        // cancel all started animations
        protected virtual void SetItemChangeAnimationsManager(ItemChangeAnimationManager changeAnimationsManager)
        {
            mChangeAnimationsManager = changeAnimationsManager;
        }

        // NOTE: This condition can be occurred since v23.1.0.
        // Don't know how to run change animations when the same view holder is re-used.
        // run a move animation to handle position changes.
        // this will trigger end callback which should set properties to their target values.
        // animations should be ended by the cancel above.
        // end all pending animations
        // end all deferred animations
        // cancel all started animations
        protected virtual ItemMoveAnimationManager GetItemMoveAnimationsManager()
        {
            return mMoveAnimationsManager;
        }

        // NOTE: This condition can be occurred since v23.1.0.
        // Don't know how to run change animations when the same view holder is re-used.
        // run a move animation to handle position changes.
        // this will trigger end callback which should set properties to their target values.
        // animations should be ended by the cancel above.
        // end all pending animations
        // end all deferred animations
        // cancel all started animations
        protected virtual void SetItemMoveAnimationsManager(ItemMoveAnimationManager moveAnimationsManager)
        {
            mMoveAnimationsManager = moveAnimationsManager;
        }

        // NOTE: This condition can be occurred since v23.1.0.
        // Don't know how to run change animations when the same view holder is re-used.
        // run a move animation to handle position changes.
        // this will trigger end callback which should set properties to their target values.
        // animations should be ended by the cancel above.
        // end all pending animations
        // end all deferred animations
        // cancel all started animations
        public virtual bool IsDebug()
        {
            return mDebug;
        }

        // NOTE: This condition can be occurred since v23.1.0.
        // Don't know how to run change animations when the same view holder is re-used.
        // run a move animation to handle position changes.
        // this will trigger end callback which should set properties to their target values.
        // animations should be ended by the cancel above.
        // end all pending animations
        // end all deferred animations
        // cancel all started animations
        public virtual void SetDebug(bool debug)
        {
            mDebug = debug;
        }

        // NOTE: This condition can be occurred since v23.1.0.
        // Don't know how to run change animations when the same view holder is re-used.
        // run a move animation to handle position changes.
        // this will trigger end callback which should set properties to their target values.
        // animations should be ended by the cancel above.
        // end all pending animations
        // end all deferred animations
        // cancel all started animations
        /// <summary>
        /// Schedule order and timing of pending animations.
        /// Override this method to custom animation order.
        /// </summary>
        protected virtual void OnSchedulePendingAnimations()
        {
            SchedulePendingAnimationsByDefaultRule();
        }

        // NOTE: This condition can be occurred since v23.1.0.
        // Don't know how to run change animations when the same view holder is re-used.
        // run a move animation to handle position changes.
        // this will trigger end callback which should set properties to their target values.
        // animations should be ended by the cancel above.
        // end all pending animations
        // end all deferred animations
        // cancel all started animations
        /// <summary>
        /// Schedule order and timing of pending animations.
        /// Override this method to custom animation order.
        /// </summary>
        protected virtual void SchedulePendingAnimationsByDefaultRule()
        {
            bool removalsPending = mRemoveAnimationManager.HasPending();
            bool movesPending = mMoveAnimationsManager.HasPending();
            bool changesPending = mChangeAnimationsManager.HasPending();
            bool additionsPending = mAddAnimationsManager.HasPending();
            long removeDuration = removalsPending ? RemoveDuration : 0;
            long moveDuration = movesPending ? MoveDuration : 0;
            long changeDuration = changesPending ? ChangeDuration : 0;
            if (removalsPending)
            {
                mRemoveAnimationManager.RunPendingAnimations(false, 0);
            }

            if (movesPending)
            {
                bool deferred = removalsPending;
                long deferredDelay = removeDuration;
                mMoveAnimationsManager.RunPendingAnimations(deferred, deferredDelay);
            }

            if (changesPending)
            {
                bool deferred = removalsPending;
                long deferredDelay = removeDuration;
                mChangeAnimationsManager.RunPendingAnimations(deferred, deferredDelay);
            }

            if (additionsPending)
            {
                bool deferred = (removalsPending || movesPending || changesPending);
                long totalDelay = removeDuration + Math.Max(moveDuration, changeDuration);
                long deferredDelay = (deferred) ? totalDelay : 0;
                mAddAnimationsManager.RunPendingAnimations(deferred, deferredDelay);
            }
        }
    }
}