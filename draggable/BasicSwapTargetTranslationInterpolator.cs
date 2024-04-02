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
using Android.Views.Animations;

namespace Com.H6ah4i.Android.Widget.Advrecyclerview.Draggable
{
    public class BasicSwapTargetTranslationInterpolator : Java.Lang.Object, IInterpolator
    {
        private readonly float mThreshold;
        private readonly float mHalfValidRange;
        private readonly float mInvValidRange;
        public BasicSwapTargetTranslationInterpolator() : this(0.3F)
        {
        }

        public BasicSwapTargetTranslationInterpolator(float threshold)
        {
            if (!(threshold >= 0 && threshold < 0.5F))
            {
                throw new ArgumentException("Invalid threshold range: " + threshold);
            }

            float validRange = 1F - 2 * threshold;
            mThreshold = threshold;
            mHalfValidRange = validRange * 0.5F;
            mInvValidRange = 1F / validRange;
        }

        public virtual float GetInterpolation(float input)
        {
            if (Math.Abs(input - 0.5F) < mHalfValidRange)
            {
                return (input - mThreshold) * mInvValidRange;
            }
            else
            {
                return (input < 0.5F) ? 0F : 1F;
            }
        }
    }
}