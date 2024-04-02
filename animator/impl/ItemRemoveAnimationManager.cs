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
    public abstract class ItemRemoveAnimationManager : BaseItemAnimationManager<RemoveAnimationInfo>
    {
        private static readonly string TAG = "ARVItemRemoveAnimMgr";
        public ItemRemoveAnimationManager(BaseItemAnimator itemAnimator) : base(itemAnimator)
        {
        }

        public override long GetDuration()
        {
            return mItemAnimator.RemoveDuration;
        }

        public override void SetDuration(long duration)
        {
            mItemAnimator.RemoveDuration = duration;
        }

        public override void DispatchStarting(RemoveAnimationInfo info, RecyclerView.ViewHolder item)
        {
            if (DebugLogEnabled())
            {
                Log.Debug(TAG, "dispatchRemoveStarting(" + item + ")");
            }

            mItemAnimator.DispatchRemoveStarting(item);
        }

        public override void DispatchFinished(RemoveAnimationInfo info, RecyclerView.ViewHolder item)
        {
            if (DebugLogEnabled())
            {
                Log.Debug(TAG, "dispatchRemoveFinished(" + item + ")");
            }

            mItemAnimator.DispatchRemoveFinished(item);
        }

        protected override bool EndNotStartedAnimation(RemoveAnimationInfo info, RecyclerView.ViewHolder item)
        {
            if ((info.holder != null) && ((item == null) || (info.holder == item)))
            {
                OnAnimationEndedBeforeStarted(info, info.holder);
                DispatchFinished(info, info.holder);
                info.Clear(info.holder);
                return true;
            }
            else
            {
                return false;
            }
        }

        public abstract bool AddPendingAnimation(RecyclerView.ViewHolder holder);
    }
}