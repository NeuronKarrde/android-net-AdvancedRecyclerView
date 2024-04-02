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
    public abstract class ItemMoveAnimationManager : BaseItemAnimationManager<MoveAnimationInfo>
    {
        public static readonly string TAG = "ARVItemMoveAnimMgr";
        public ItemMoveAnimationManager(BaseItemAnimator itemAnimator) : base(itemAnimator)
        {
        }

        public override long GetDuration()
        {
            return mItemAnimator.MoveDuration;
        }

        public override void SetDuration(long duration)
        {
            mItemAnimator.MoveDuration = duration;
        }

        public override void DispatchStarting(MoveAnimationInfo info, RecyclerView.ViewHolder item)
        {
            if (DebugLogEnabled())
            {
                Log.Debug(TAG, "dispatchMoveStarting(" + item + ")");
            }

            mItemAnimator.DispatchMoveStarting(item);
        }

        public override void DispatchFinished(MoveAnimationInfo info, RecyclerView.ViewHolder item)
        {
            if (DebugLogEnabled())
            {
                Log.Debug(TAG, "dispatchMoveFinished(" + item + ")");
            }

            mItemAnimator.DispatchMoveFinished(item);
        }

        protected override bool EndNotStartedAnimation(MoveAnimationInfo info, RecyclerView.ViewHolder item)
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

        public abstract bool AddPendingAnimation(RecyclerView.ViewHolder item, int fromX, int fromY, int toX, int toY);
    }
}