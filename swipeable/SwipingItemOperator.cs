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
using Android.Views;
using Android.Views.Animations;
using AndroidX.RecyclerView.Widget;

namespace Com.H6ah4i.Android.Widget.Advrecyclerview.Swipeable
{
    class SwipingItemOperator
    {
        private const string TAG = "SwipingItemOperator";
        private const int REACTION_CAN_NOT_SWIPE = InternalConstants.REACTION_CAN_NOT_SWIPE;
        private const int REACTION_CAN_NOT_SWIPE_WITH_RUBBER_BAND_EFFECT = InternalConstants.REACTION_CAN_NOT_SWIPE_WITH_RUBBER_BAND_EFFECT;
        private const int REACTION_CAN_SWIPE = InternalConstants.REACTION_CAN_SWIPE;
        private const float RUBBER_BAND_LIMIT = 0.15F;
        private const int MIN_GRABBING_AREA_SIZE = 48;
        private readonly IInterpolator RUBBER_BAND_INTERPOLATOR = new RubberBandInterpolator(RUBBER_BAND_LIMIT);
        private RecyclerViewSwipeManager mSwipeManager;
        private RecyclerView.ViewHolder mSwipingItem;
        private View mSwipingItemContainerView;
        private int mLeftSwipeReactionType;
        private int mUpSwipeReactionType;
        private int mRightSwipeReactionType;
        private int mDownSwipeReactionType;
        private int mSwipingItemWidth;
        private readonly int mSwipingItemHeight;
        private float mInvSwipingItemWidth;
        private float mInvSwipingItemHeight;
        private int mSwipeDistanceX;
        private int mSwipeDistanceY;
        private float mPrevTranslateAmount;
        private int mInitialTranslateAmountX;
        private int mInitialTranslateAmountY;
        private readonly bool mSwipeHorizontal;
        public SwipingItemOperator(RecyclerViewSwipeManager manager, RecyclerView.ViewHolder swipingItem, int swipeReactionType, bool swipeHorizontal)
        {
            mSwipeManager = manager;
            mSwipingItem = swipingItem;
            mLeftSwipeReactionType = SwipeReactionUtils.ExtractLeftReaction(swipeReactionType);
            mUpSwipeReactionType = SwipeReactionUtils.ExtractUpReaction(swipeReactionType);
            mRightSwipeReactionType = SwipeReactionUtils.ExtractRightReaction(swipeReactionType);
            mDownSwipeReactionType = SwipeReactionUtils.ExtractDownReaction(swipeReactionType);
            mSwipeHorizontal = swipeHorizontal;
            mSwipingItemContainerView = SwipeableViewHolderUtils.GetSwipeableContainerView(swipingItem);
            mSwipingItemWidth = mSwipingItemContainerView.Width;
            mSwipingItemHeight = mSwipingItemContainerView.Height;
            mInvSwipingItemWidth = CalcInv(mSwipingItemWidth);
            mInvSwipingItemHeight = CalcInv(mSwipingItemHeight);
        }

        public virtual void Start()
        {
            float density = mSwipingItem.ItemView.Resources.DisplayMetrics.Density;
            int maxAmountH = Math.Max(0, mSwipingItemWidth - (int)(density * MIN_GRABBING_AREA_SIZE));
            int maxAmountV = Math.Max(0, mSwipingItemHeight - (int)(density * MIN_GRABBING_AREA_SIZE));
            mInitialTranslateAmountX = Clip(mSwipeManager.GetSwipeContainerViewTranslationX(mSwipingItem), -maxAmountH, maxAmountH);
            mInitialTranslateAmountY = Clip(mSwipeManager.GetSwipeContainerViewTranslationY(mSwipingItem), -maxAmountV, maxAmountV);
        }

        public virtual void Finish()
        {
            mSwipeManager = null;
            mSwipingItem = null;
            mSwipeDistanceX = 0;
            mSwipeDistanceY = 0;
            mSwipingItemWidth = 0;
            mInvSwipingItemWidth = 0;
            mInvSwipingItemHeight = 0;
            mLeftSwipeReactionType = REACTION_CAN_NOT_SWIPE;
            mUpSwipeReactionType = REACTION_CAN_NOT_SWIPE;
            mRightSwipeReactionType = REACTION_CAN_NOT_SWIPE;
            mDownSwipeReactionType = REACTION_CAN_NOT_SWIPE;
            mPrevTranslateAmount = 0;
            mInitialTranslateAmountX = 0;
            mInitialTranslateAmountY = 0;
            mSwipingItemContainerView = null;
        }

        public virtual void Update(int itemPosition, int swipeDistanceX, int swipeDistanceY)
        {
            if ((mSwipeDistanceX == swipeDistanceX) && (mSwipeDistanceY == swipeDistanceY))
            {
                return;
            }

            mSwipeDistanceX = swipeDistanceX;
            mSwipeDistanceY = swipeDistanceY;
            int distance = (mSwipeHorizontal) ? (mSwipeDistanceX + mInitialTranslateAmountX) : (mSwipeDistanceY + mInitialTranslateAmountY);
            int itemSize = (mSwipeHorizontal) ? mSwipingItemWidth : mSwipingItemHeight;
            float invItemSize = (mSwipeHorizontal) ? mInvSwipingItemWidth : mInvSwipingItemHeight;
            int reactionType;
            if (mSwipeHorizontal)
            {
                reactionType = (distance > 0) ? mRightSwipeReactionType : mLeftSwipeReactionType;
            }
            else
            {
                reactionType = (distance > 0) ? mDownSwipeReactionType : mUpSwipeReactionType;
            }

            float translateAmount = 0;
            switch (reactionType)
            {
                case REACTION_CAN_NOT_SWIPE:
                    break;
                case REACTION_CAN_NOT_SWIPE_WITH_RUBBER_BAND_EFFECT:
                    float proportion = Math.Min(Math.Abs(distance), itemSize) * invItemSize;
                    translateAmount = Math.Sign(distance) * RUBBER_BAND_INTERPOLATOR.GetInterpolation(proportion);
                    break;
                case REACTION_CAN_SWIPE:
                    translateAmount = Math.Min(Math.Max((distance * invItemSize), -1F), 1F);
                    break;
            }

            mSwipeManager.ApplySlideItem(mSwipingItem, itemPosition, mPrevTranslateAmount, translateAmount, true, mSwipeHorizontal, false, true);
            mPrevTranslateAmount = translateAmount;
        }

        private static float CalcInv(int value)
        {
            return (value != 0) ? (1F / value) : 0F;
        }

        private static int Clip(int v, int min, int max)
        {
            return Math.Min(Math.Max(v, min), max);
        }
    }
}