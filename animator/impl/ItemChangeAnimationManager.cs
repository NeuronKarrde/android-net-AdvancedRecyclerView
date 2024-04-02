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
using AndroidX.RecyclerView.Widget;

namespace Com.H6ah4i.Android.Widget.Advrecyclerview.Animator.Impl
{
    public abstract class ItemChangeAnimationManager : BaseItemAnimationManager<ChangeAnimationInfo>
    {
        private static readonly string TAG = "ARVItemChangeAnimMgr";
        public ItemChangeAnimationManager(BaseItemAnimator itemAnimator) : base(itemAnimator)
        {
        }

        public override void DispatchStarting(ChangeAnimationInfo info, RecyclerView.ViewHolder item)
        {
            if (DebugLogEnabled())
            {
                Log.Debug(TAG, "dispatchChangeStarting(" + item + ")");
            }

            mItemAnimator.DispatchChangeStarting(item, (item == info.oldHolder));
        }

        public override void DispatchFinished(ChangeAnimationInfo info, RecyclerView.ViewHolder item)
        {
            if (DebugLogEnabled())
            {
                Log.Debug(TAG, "dispatchChangeFinished(" + item + ")");
            }

            mItemAnimator.DispatchChangeFinished(item, (item == info.oldHolder));
        }

        public override long GetDuration()
        {
            return mItemAnimator.ChangeDuration;
        }

        public override void SetDuration(long duration)
        {
            mItemAnimator.ChangeDuration = duration;
        }

        protected override void OnCreateAnimation(ChangeAnimationInfo info)
        {
            if (info.oldHolder != null)
            {
                OnCreateChangeAnimationForOldItem(info);
            }

            if (info.newHolder != null)
            {
                OnCreateChangeAnimationForNewItem(info);
            }
        }

        protected override bool EndNotStartedAnimation(ChangeAnimationInfo info, RecyclerView.ViewHolder item)
        {
            if ((info.oldHolder != null) && ((item == null) || (info.oldHolder == item)))
            {
                OnAnimationEndedBeforeStarted(info, info.oldHolder);
                DispatchFinished(info, info.oldHolder);
                info.Clear(info.oldHolder);
            }

            if ((info.newHolder != null) && ((item == null) || (info.newHolder == item)))
            {
                OnAnimationEndedBeforeStarted(info, info.newHolder);
                DispatchFinished(info, info.newHolder);
                info.Clear(info.newHolder);
            }

            return (info.oldHolder == null && info.newHolder == null);
        }

        protected abstract void OnCreateChangeAnimationForNewItem(ChangeAnimationInfo info);
        protected abstract void OnCreateChangeAnimationForOldItem(ChangeAnimationInfo info);
        public abstract bool AddPendingAnimation(RecyclerView.ViewHolder oldHolder, RecyclerView.ViewHolder newHolder, int fromX, int fromY, int toX, int toY);
    }
}