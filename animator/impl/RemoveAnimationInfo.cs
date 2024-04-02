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
using AndroidX.RecyclerView.Widget;

namespace Com.H6ah4i.Android.Widget.Advrecyclerview.Animator.Impl
{
    public class RemoveAnimationInfo : ItemAnimationInfo
    {
        public RecyclerView.ViewHolder holder;
        public RemoveAnimationInfo(RecyclerView.ViewHolder holder)
        {
            this.holder = holder;
        }

        public override RecyclerView.ViewHolder GetAvailableViewHolder()
        {
            return holder;
        }

        public override void Clear(RecyclerView.ViewHolder item)
        {
            if (holder == item)
            {
                holder = null;
            }
        }

        public override string ToString()
        {
            return "RemoveAnimationInfo{" + "holder=" + holder + '}';
        }
    }
}