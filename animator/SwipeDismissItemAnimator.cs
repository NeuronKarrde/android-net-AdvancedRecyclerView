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
using Android.Views;
using Android.Views.Animations;
using Com.H6ah4i.Android.Widget.Advrecyclerview.Animator.Impl;
using Com.H6ah4i.Android.Widget.Advrecyclerview.Swipeable;
using AndroidX.Core.View;
using AndroidX.RecyclerView.Widget;

namespace Com.H6ah4i.Android.Widget.Advrecyclerview.Animator
{
    public class SwipeDismissItemAnimator : DraggableItemAnimator
    {
        public static readonly IInterpolator MOVE_INTERPOLATOR = new AccelerateDecelerateInterpolator();
        protected override void OnSetup()
        {
            SetItemAddAnimationsManager(new DefaultItemAddAnimationManager(this));
            SetItemRemoveAnimationManager(new SwipeDismissItemRemoveAnimationManager(this));
            SetItemChangeAnimationsManager(new DefaultItemChangeAnimationManager(this));
            SetItemMoveAnimationsManager(new DefaultItemMoveAnimationManager(this));
            RemoveDuration = (150);
            MoveDuration = (150);
        }

        /// <summary>
        /// Item Animation manager for REMOVE operation  (Same behavior as DefaultItemAnimator class)
        /// </summary>
        protected class SwipeDismissItemRemoveAnimationManager : ItemRemoveAnimationManager
        {
            protected static readonly IInterpolator DEFAULT_INTERPOLATOR = new AccelerateDecelerateInterpolator();
            public SwipeDismissItemRemoveAnimationManager(BaseItemAnimator itemAnimator) : base(itemAnimator)
            {
            }

            protected override void OnCreateAnimation(RemoveAnimationInfo info)
            {
                ViewPropertyAnimatorCompat animator;
                if (IsSwipeDismissed(info.holder))
                {
                    View view = info.holder.ItemView;
                    animator = ViewCompat.Animate(view);
                    animator.SetDuration(GetDuration());
                }
                else
                {
                    View view = info.holder.ItemView;
                    animator = ViewCompat.Animate(view);
                    animator.SetDuration(GetDuration());
                    animator.SetInterpolator(DEFAULT_INTERPOLATOR);
                    animator.Alpha(0);
                }

                StartActiveItemAnimation(info, info.holder, animator);
            }

            protected static bool IsSwipeDismissed(RecyclerView.ViewHolder item)
            {
                if (!(item is ISwipeableItemViewHolder))
                {
                    return false;
                }

                ISwipeableItemViewHolder item2 = (ISwipeableItemViewHolder)item;
                int result = item2.SwipeResult;
                int reaction = item2.AfterSwipeReaction;
                
                return ((result == RecyclerViewSwipeManager.ResultSwipedLeft) || (result == RecyclerViewSwipeManager.ResultSwipedUp) || (result == RecyclerViewSwipeManager.ResultSwipedRight) || (result == RecyclerViewSwipeManager.ResultSwipedDown)) && (reaction == RecyclerViewSwipeManager.AFTER_SWIPE_REACTION_REMOVE_ITEM);
            }

            protected static bool IsSwipeDismissed(RemoveAnimationInfo info)
            {
                return (info is SwipeDismissRemoveAnimationInfo);
            }

            protected override void OnAnimationEndedSuccessfully(RemoveAnimationInfo info, RecyclerView.ViewHolder item)
            {
                View view = item.ItemView;
                if (IsSwipeDismissed(info))
                {
                    view.TranslationX = (0);
                    view.TranslationY = (0);
                }
                else
                {
                    view.Alpha = (1);
                }
            }

            protected override void OnAnimationEndedBeforeStarted(RemoveAnimationInfo info, RecyclerView.ViewHolder item)
            {
                View view = item.ItemView;
                if (IsSwipeDismissed(info))
                {
                    view.TranslationX = (0);
                    view.TranslationY = (0);
                }
                else
                {
                    view.Alpha = (1);
                }
            }

            protected override void OnAnimationCancel(RemoveAnimationInfo info, RecyclerView.ViewHolder item)
            {
            }

            public override bool AddPendingAnimation(RecyclerView.ViewHolder holder)
            {
                if (IsSwipeDismissed(holder))
                {
                    View itemView = holder.ItemView;
                    int prevItemX = (int)(itemView.TranslationX + 0.5F);
                    int prevItemY = (int)(itemView.TranslationY + 0.5F);
                    EndAnimation(holder);
                    itemView.TranslationX = (prevItemX);
                    itemView.TranslationY = (prevItemY);
                    EnqueuePendingAnimationInfo(new SwipeDismissRemoveAnimationInfo(holder));
                    return true;
                }
                else
                {
                    EndAnimation(holder);
                    EnqueuePendingAnimationInfo(new RemoveAnimationInfo(holder));
                    return true;
                }
            }
        }

        /// <summary>
        /// Item Animation manager for REMOVE operation  (Same behavior as DefaultItemAnimator class)
        /// </summary>
        protected class SwipeDismissRemoveAnimationInfo : RemoveAnimationInfo
        {
            public SwipeDismissRemoveAnimationInfo(RecyclerView.ViewHolder holder) : base(holder)
            {
            }
        }
    }
}