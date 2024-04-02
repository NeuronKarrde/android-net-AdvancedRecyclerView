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
namespace Com.H6ah4i.Android.Widget.Advrecyclerview.Swipeable
{
    class SwipeReactionUtils
    {
        public static int ExtractLeftReaction(int type)
        {
            return ((type >>> InternalConstants.BIT_SHIFT_AMOUNT_LEFT) & InternalConstants.REACTION_CAPABILITY_MASK);
        }

        public static int ExtractUpReaction(int type)
        {
            return ((type >>> InternalConstants.BIT_SHIFT_AMOUNT_UP) & InternalConstants.REACTION_CAPABILITY_MASK);
        }

        public static int ExtractRightReaction(int type)
        {
            return ((type >>> InternalConstants.BIT_SHIFT_AMOUNT_RIGHT) & InternalConstants.REACTION_CAPABILITY_MASK);
        }

        public static int ExtractDownReaction(int type)
        {
            return ((type >>> InternalConstants.BIT_SHIFT_AMOUNT_DOWN) & InternalConstants.REACTION_CAPABILITY_MASK);
        }

        public static bool CanSwipeLeft(int reactionType)
        {
            return (ExtractLeftReaction(reactionType) == InternalConstants.REACTION_CAN_SWIPE);
        }

        public static bool CanSwipeUp(int reactionType)
        {
            return (ExtractUpReaction(reactionType) == InternalConstants.REACTION_CAN_SWIPE);
        }

        public static bool CanSwipeRight(int reactionType)
        {
            return (ExtractRightReaction(reactionType) == InternalConstants.REACTION_CAN_SWIPE);
        }

        public static bool CanSwipeDown(int reactionType)
        {
            return (ExtractDownReaction(reactionType) == InternalConstants.REACTION_CAN_SWIPE);
        }
    }
}