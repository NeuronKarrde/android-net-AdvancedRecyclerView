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
using Com.H6ah4i.Android.Widget.Advrecyclerview.Utils;
using AndroidX.Core.View;
using AndroidX.RecyclerView.Widget;

namespace Com.H6ah4i.Android.Widget.Advrecyclerview.Draggable
{
    class SwapTargetItemOperator : BaseDraggableItemDecorator
    {
        private static readonly string TAG = "SwapTargetItemOperator";
        private RecyclerView.ViewHolder mSwapTargetItem;
        private IInterpolator mSwapTargetTranslationInterpolator;
        private int mTranslationX;
        private int mTranslationY;
        private readonly Rect mSwapTargetDecorationOffsets = new Rect();
        private readonly Rect mSwapTargetItemMargins = new Rect();
        private readonly Rect mDraggingItemDecorationOffsets = new Rect();
        private bool mStarted;
        private float mReqTranslationPhase;
        private float mCurTranslationPhase;
        private DraggingItemInfo mDraggingItemInfo;
        private bool mSwapTargetItemChanged;
        private readonly IViewPropertyAnimatorListener RESET_TRANSLATION_LISTENER;

        static SwapTargetItemOperator()
        {
            
        }
        private sealed class AnonymousViewPropertyAnimatorListener : Java.Lang.Object, IViewPropertyAnimatorListener
        {
            public AnonymousViewPropertyAnimatorListener(SwapTargetItemOperator parent)
            {
                this.parent = parent;
            }

            private readonly SwapTargetItemOperator parent;
            public void OnAnimationStart(View view)
            {
            }

            public void OnAnimationEnd(View view)
            {
                ViewPropertyAnimatorCompat animator = ViewCompat.Animate(view);
                animator.SetListener(null);
                view.TranslationX=(0);
                view.TranslationY=(0);
            }

            public void OnAnimationCancel(View view)
            {
            }
        }

        public SwapTargetItemOperator(RecyclerView recyclerView, RecyclerView.ViewHolder draggingItem, DraggingItemInfo draggingItemInfo) : base(recyclerView, draggingItem)
        {
            RESET_TRANSLATION_LISTENER = new AnonymousViewPropertyAnimatorListener(this);
            mDraggingItemInfo = draggingItemInfo;
            CustomRecyclerViewUtils.GetDecorationOffsets(mRecyclerView.GetLayoutManager(), mDraggingItemViewHolder.ItemView, mDraggingItemDecorationOffsets);
        }

        private static float CalculateCurrentTranslationPhase(float cur, float req)
        {
            float A = 0.3F;
            float B = 0.01F;
            float tmp = (cur * (1F - A)) + (req * A);
            return (Math.Abs(tmp - req) < B) ? req : tmp;
        }

        public virtual void SetSwapTargetTranslationInterpolator(IInterpolator interpolator)
        {
            mSwapTargetTranslationInterpolator = interpolator;
        }

        public virtual void SetSwapTargetItem(RecyclerView.ViewHolder swapTargetItem)
        {
            if (mSwapTargetItem == swapTargetItem)
            {
                return;
            }


            // reset Y-translation if the swap target has been changed
            if (mSwapTargetItem != null)
            {
                ViewPropertyAnimatorCompat animator = ViewCompat.Animate(mSwapTargetItem.ItemView);
                animator.Cancel();
                animator.SetDuration(10).TranslationX(0).TranslationY(0).SetListener(RESET_TRANSLATION_LISTENER).Start();
            }

            mSwapTargetItem = swapTargetItem;
            if (mSwapTargetItem != null)
            {
                ViewPropertyAnimatorCompat animator = ViewCompat.Animate(mSwapTargetItem.ItemView);
                animator.Cancel();
            }

            mSwapTargetItemChanged = true;
        }

        // reset Y-translation if the swap target has been changed
        public override void OnDraw(Canvas c, RecyclerView parent, RecyclerView.State state)
        {
            RecyclerView.ViewHolder draggingItem = mDraggingItemViewHolder;
            RecyclerView.ViewHolder swapTargetItem = mSwapTargetItem;
            if (draggingItem == null || swapTargetItem == null || draggingItem.ItemId != mDraggingItemInfo.id)
            {
                return;
            }

            mReqTranslationPhase = CalculateTranslationPhase(draggingItem, swapTargetItem);
            if (mSwapTargetItemChanged)
            {
                mSwapTargetItemChanged = false;
                mCurTranslationPhase = mReqTranslationPhase;
            }
            else
            {

                // interpolate to make it moves smoothly
                mCurTranslationPhase = CalculateCurrentTranslationPhase(mCurTranslationPhase, mReqTranslationPhase);
            }

            UpdateSwapTargetTranslation(draggingItem, swapTargetItem, mCurTranslationPhase);
        }

        // reset Y-translation if the swap target has been changed
        // interpolate to make it moves smoothly
        private float CalculateTranslationPhase(RecyclerView.ViewHolder draggingItem, RecyclerView.ViewHolder swapTargetItem)
        {
            View swapItemView = swapTargetItem.ItemView;
            int pos1 = draggingItem.LayoutPosition;
            int pos2 = swapTargetItem.LayoutPosition;
            CustomRecyclerViewUtils.GetDecorationOffsets(mRecyclerView.GetLayoutManager(), swapItemView, mSwapTargetDecorationOffsets);
            CustomRecyclerViewUtils.GetLayoutMargins(swapItemView, mSwapTargetItemMargins);
            Rect m2 = mSwapTargetItemMargins;
            Rect d2 = mSwapTargetDecorationOffsets;
            int h2 = swapItemView.Height + m2.Top + m2.Bottom + d2.Top + d2.Bottom;
            int w2 = swapItemView.Width + m2.Left + m2.Right + d2.Left + d2.Right;
            float offsetXPx = draggingItem.ItemView.Left - mTranslationX; // == -(draggingItem.itemView.TranslationY
            float phaseX = (w2 != 0) ? (offsetXPx / w2) : 0F;
            float offsetYPx = draggingItem.ItemView.Top - mTranslationY; // == -(draggingItem.itemView.TranslationY
            float phaseY = (h2 != 0) ? (offsetYPx / h2) : 0F;
            float translationPhase = 0F;
            int orientation = CustomRecyclerViewUtils.GetOrientation(mRecyclerView);
            if (orientation == CustomRecyclerViewUtils.ORIENTATION_VERTICAL)
            {
                if (pos1 > pos2)
                {

                    // dragging item moving to upward
                    translationPhase = phaseY;
                }
                else
                {

                    // dragging item moving to downward
                    translationPhase = 1F + phaseY;
                }
            }
            else if (orientation == CustomRecyclerViewUtils.ORIENTATION_HORIZONTAL)
            {
                if (pos1 > pos2)
                {

                    // dragging item moving to left
                    translationPhase = phaseX;
                }
                else
                {

                    // dragging item moving to right
                    translationPhase = 1F + phaseX;
                }
            }

            return Math.Min(Math.Max(translationPhase, 0F), 1F);
        }

        // reset Y-translation if the swap target has been changed
        // interpolate to make it moves smoothly
        // == -(draggingItem.itemView.TranslationY
        // == -(draggingItem.itemView.TranslationY
        // dragging item moving to upward
        // dragging item moving to downward
        // dragging item moving to left
        // dragging item moving to right
        private void UpdateSwapTargetTranslation(RecyclerView.ViewHolder draggingItem, RecyclerView.ViewHolder swapTargetItem, float translationPhase)
        {
            View swapItemView = swapTargetItem.ItemView;
            int pos1 = draggingItem.LayoutPosition;
            int pos2 = swapTargetItem.LayoutPosition;
            Rect m1 = mDraggingItemInfo.margins;
            Rect d1 = mDraggingItemDecorationOffsets;
            int h1 = mDraggingItemInfo.height + m1.Top + m1.Bottom + d1.Top + d1.Bottom;
            int w1 = mDraggingItemInfo.width + m1.Left + m1.Right + d1.Left + d1.Right;
            if (mSwapTargetTranslationInterpolator != null)
            {
                translationPhase = mSwapTargetTranslationInterpolator.GetInterpolation(translationPhase);
            }

            switch (CustomRecyclerViewUtils.GetOrientation(mRecyclerView))
            {
                case CustomRecyclerViewUtils.ORIENTATION_VERTICAL:
                    if (pos1 > pos2)
                    {

                        // dragging item moving to upward
                        swapItemView.TranslationY=(translationPhase * h1);
                    }
                    else
                    {

                        // dragging item moving to downward
                        swapItemView.TranslationY=((translationPhase - 1F) * h1);
                    }

                    break;
                case CustomRecyclerViewUtils.ORIENTATION_HORIZONTAL:
                    if (pos1 > pos2)
                    {

                        // dragging item moving to left
                        swapItemView.TranslationX=(translationPhase * w1);
                    }
                    else
                    {

                        // dragging item moving to right
                        swapItemView.TranslationX=((translationPhase - 1F) * w1);
                    }

                    break;
            }
        }

        // reset Y-translation if the swap target has been changed
        // interpolate to make it moves smoothly
        // == -(draggingItem.itemView.TranslationY
        // == -(draggingItem.itemView.TranslationY
        // dragging item moving to upward
        // dragging item moving to downward
        // dragging item moving to left
        // dragging item moving to right
        // dragging item moving to upward
        // dragging item moving to downward
        // dragging item moving to left
        // dragging item moving to right
        public virtual void Start()
        {
            if (mStarted)
            {
                return;
            }

            mRecyclerView.AddItemDecoration(this, 0);
            mStarted = true;
        }

        // reset Y-translation if the swap target has been changed
        // interpolate to make it moves smoothly
        // == -(draggingItem.itemView.TranslationY
        // == -(draggingItem.itemView.TranslationY
        // dragging item moving to upward
        // dragging item moving to downward
        // dragging item moving to left
        // dragging item moving to right
        // dragging item moving to upward
        // dragging item moving to downward
        // dragging item moving to left
        // dragging item moving to right
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
            if (mSwapTargetItem != null)
            {

                // return to default position
                UpdateSwapTargetTranslation(mDraggingItemViewHolder, mSwapTargetItem, mCurTranslationPhase);
                MoveToDefaultPosition(mSwapTargetItem.ItemView, 1F, 1F, 0F, 1F, animate);
                mSwapTargetItem = null;
            }

            mDraggingItemViewHolder = null;
            mTranslationX = 0;
            mTranslationY = 0;
            mCurTranslationPhase = 0F;
            mReqTranslationPhase = 0F;
            mStarted = false;
            mDraggingItemInfo = null;
        }

        // reset Y-translation if the swap target has been changed
        // interpolate to make it moves smoothly
        // == -(draggingItem.itemView.TranslationY
        // == -(draggingItem.itemView.TranslationY
        // dragging item moving to upward
        // dragging item moving to downward
        // dragging item moving to left
        // dragging item moving to right
        // dragging item moving to upward
        // dragging item moving to downward
        // dragging item moving to left
        // dragging item moving to right
        // return to default position
        public virtual void Update(int translationX, int translationY)
        {
            mTranslationX = translationX;
            mTranslationY = translationY;
        }

        // reset Y-translation if the swap target has been changed
        // interpolate to make it moves smoothly
        // == -(draggingItem.itemView.TranslationY
        // == -(draggingItem.itemView.TranslationY
        // dragging item moving to upward
        // dragging item moving to downward
        // dragging item moving to left
        // dragging item moving to right
        // dragging item moving to upward
        // dragging item moving to downward
        // dragging item moving to left
        // dragging item moving to right
        // return to default position
        public virtual void OnItemViewRecycled(RecyclerView.ViewHolder holder)
        {
            if (holder == mSwapTargetItem)
            {
                SetSwapTargetItem(null);
            }
        }
    }
}