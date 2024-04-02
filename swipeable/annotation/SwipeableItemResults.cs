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
using Com.H6ah4i.Android.Widget.Advrecyclerview.Swipeable;

namespace Com.H6ah4i.Android.Widget.Advrecyclerview.Swipeable.Annotation
{
  public enum SwipeableItemResults
  {
       RESULT_NONE = SwipeableItemConstants.RESULT_NONE,
       RESULT_CANCELED = SwipeableItemConstants.RESULT_CANCELED,
       RESULT_SWIPED_LEFT = SwipeableItemConstants.ResultSwipedLeft,
       RESULT_SWIPED_UP = SwipeableItemConstants.ResultSwipedUp,
       RESULT_SWIPED_RIGHT = SwipeableItemConstants.ResultSwipedRight,
       RESULT_SWIPED_DOWN = SwipeableItemConstants.ResultSwipedDown
  }
}