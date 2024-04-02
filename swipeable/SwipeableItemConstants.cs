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
using Android.Annotation;
using AndroidX.RecyclerView.Widget;

namespace Com.H6ah4i.Android.Widget.Advrecyclerview.Swipeable
{
    public abstract class SwipeableItemConstants
    {
        public const int StateFlagSwiping = (1 << 0);
        public const int StateFlagIsActive = (1 << 1);
        public const int StateFlagIsUpdated = (1 << 31);
        public const int RESULT_NONE = 0;
        public const int RESULT_CANCELED = 1;
        public const int ResultSwipedLeft = 2;
        public const int ResultSwipedUp = 3;
        public const int ResultSwipedRight = 4;
        public const int ResultSwipedDown = 5;
        public const int ReactionCanNotSwipeAny = 0;
        public static int REACTION_CAN_NOT_SWIPE_LEFT = (InternalConstants.REACTION_CAN_NOT_SWIPE << InternalConstants.BIT_SHIFT_AMOUNT_LEFT);
        
        public static int REACTION_CAN_NOT_SWIPE_LEFT_WITH_RUBBER_BAND_EFFECT = (InternalConstants.REACTION_CAN_NOT_SWIPE_WITH_RUBBER_BAND_EFFECT << InternalConstants.BIT_SHIFT_AMOUNT_LEFT);
       
        public static int REACTION_CAN_SWIPE_LEFT = (InternalConstants.REACTION_CAN_SWIPE << InternalConstants.BIT_SHIFT_AMOUNT_LEFT);
      
        public static int REACTION_MASK_START_SWIPE_LEFT = (InternalConstants.REACTION_MASK_START_SWIPE << InternalConstants.BIT_SHIFT_AMOUNT_LEFT);
     
        public static int REACTION_CAN_NOT_SWIPE_UP = (InternalConstants.REACTION_CAN_NOT_SWIPE << InternalConstants.BIT_SHIFT_AMOUNT_UP);
       
        public static int REACTION_CAN_NOT_SWIPE_UP_WITH_RUBBER_BAND_EFFECT = (InternalConstants.REACTION_CAN_NOT_SWIPE_WITH_RUBBER_BAND_EFFECT << InternalConstants.BIT_SHIFT_AMOUNT_UP);
       
        public static int REACTION_CAN_SWIPE_UP = (InternalConstants.REACTION_CAN_SWIPE << InternalConstants.BIT_SHIFT_AMOUNT_UP);
      
        public static int REACTION_MASK_START_SWIPE_UP = (InternalConstants.REACTION_MASK_START_SWIPE << InternalConstants.BIT_SHIFT_AMOUNT_UP);
       
        public static int REACTION_CAN_NOT_SWIPE_RIGHT = (InternalConstants.REACTION_CAN_NOT_SWIPE << InternalConstants.BIT_SHIFT_AMOUNT_RIGHT);
       
        public static int REACTION_CAN_NOT_SWIPE_RIGHT_WITH_RUBBER_BAND_EFFECT = (InternalConstants.REACTION_CAN_NOT_SWIPE_WITH_RUBBER_BAND_EFFECT << InternalConstants.BIT_SHIFT_AMOUNT_RIGHT);
       
        public static int REACTION_CAN_SWIPE_RIGHT = (InternalConstants.REACTION_CAN_SWIPE << InternalConstants.BIT_SHIFT_AMOUNT_RIGHT);
       
        public static int REACTION_MASK_START_SWIPE_RIGHT = (InternalConstants.REACTION_MASK_START_SWIPE << InternalConstants.BIT_SHIFT_AMOUNT_RIGHT);
        public static int REACTION_CAN_NOT_SWIPE_DOWN = (InternalConstants.REACTION_CAN_NOT_SWIPE << InternalConstants.BIT_SHIFT_AMOUNT_DOWN);
       
        public static int REACTION_CAN_NOT_SWIPE_DOWN_WITH_RUBBER_BAND_EFFECT = (InternalConstants.REACTION_CAN_NOT_SWIPE_WITH_RUBBER_BAND_EFFECT << InternalConstants.BIT_SHIFT_AMOUNT_DOWN);
       
        public static int REACTION_CAN_SWIPE_DOWN = (InternalConstants.REACTION_CAN_SWIPE << InternalConstants.BIT_SHIFT_AMOUNT_DOWN);
        
        public static int REACTION_MASK_START_SWIPE_DOWN = (InternalConstants.REACTION_MASK_START_SWIPE << InternalConstants.BIT_SHIFT_AMOUNT_DOWN);
        
        public static int REACTION_START_SWIPE_ON_LONG_PRESS = InternalConstants.REACTION_START_SWIPE_ON_LONG_PRESS;
        
        public static int REACTION_CAN_NOT_SWIPE_BOTH_H => REACTION_CAN_NOT_SWIPE_LEFT | REACTION_CAN_NOT_SWIPE_RIGHT;
        
        public static int REACTION_CAN_NOT_SWIPE_BOTH_H_WITH_RUBBER_BAND_EFFECT => REACTION_CAN_NOT_SWIPE_LEFT_WITH_RUBBER_BAND_EFFECT | REACTION_CAN_NOT_SWIPE_RIGHT_WITH_RUBBER_BAND_EFFECT;
        
        public static int REACTION_CAN_SWIPE_BOTH_H => REACTION_CAN_SWIPE_LEFT | REACTION_CAN_SWIPE_RIGHT;
        
        public static int REACTION_CAN_NOT_SWIPE_BOTH_V => REACTION_CAN_NOT_SWIPE_UP | REACTION_CAN_NOT_SWIPE_DOWN;
        
        public static int REACTION_CAN_NOT_SWIPE_BOTH_V_WITH_RUBBER_BAND_EFFECT => REACTION_CAN_NOT_SWIPE_UP_WITH_RUBBER_BAND_EFFECT | REACTION_CAN_NOT_SWIPE_DOWN_WITH_RUBBER_BAND_EFFECT;
        
        public static int REACTION_CAN_SWIPE_BOTH_V => REACTION_CAN_SWIPE_UP | REACTION_CAN_SWIPE_DOWN;
        public const int DrawableSwipeNeutralBackground = 0;
        public const int DrawableSwipeLeftBackground = 1;
        public const int DrawableSwipeUpBackground = 2;
        public const int DrawableSwipeRightBackground = 3;
        public const int DrawableSwipeDownBackground = 4;
        public const int AFTER_SWIPE_REACTION_MOVE_TO_ORIGIN = 0;
        public const int AFTER_SWIPE_REACTION_REMOVE_ITEM = 1;
        public const int AFTER_SWIPE_REACTION_MOVE_TO_SWIPED_DIRECTION = 2;
        public const int AFTER_SWIPE_REACTION_DO_NOTHING = 3;
        public static int AFTER_SWIPE_REACTION_DEFAULT = AFTER_SWIPE_REACTION_MOVE_TO_ORIGIN;
        public const float OUTSIDE_OF_THE_WINDOW_LEFT = -((1 << 16) + 0);
        public const float OUTSIDE_OF_THE_WINDOW_TOP = -((1 << 16) + 1);
        public const float OUTSIDE_OF_THE_WINDOW_RIGHT = ((1 << 16) + 0);
        public const float OUTSIDE_OF_THE_WINDOW_BOTTOM = ((1 << 16) + 1); // ---
    }
}