/*
 *    Copyright (C) 2016 Haruki Hasegawa
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
using Android.Views.Animations;

namespace Com.H6ah4i.Android.Widget.Advrecyclerview.Draggable
{
    class DraggingItemEffectsInfo
    {
        public int durationMillis;
        public float scale = 1F;
        public float rotation = 0F;
        public float alpha = 1F;
        public IInterpolator scaleInterpolator = null;
        public IInterpolator rotationInterpolator = null;
        public IInterpolator alphaInterpolator = null;
    }
}