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
using Com.H6ah4i.Android.Widget.Advrecyclerview.Swipeable.Annotation;
using AndroidX.RecyclerView.Widget;

namespace Com.H6ah4i.Android.Widget.Advrecyclerview.Expandable
{
    public interface IBaseExpandableSwipeableItemAdapter
    {
        int OnGetGroupItemSwipeReactionType(RecyclerView.ViewHolder holder, int groupPosition, int x, int y);
        int OnGetChildItemSwipeReactionType(RecyclerView.ViewHolder holder, int groupPosition, int childPosition, int x, int y);
        void OnSwipeGroupItemStarted(RecyclerView.ViewHolder holder, int groupPosition);
        void OnSwipeChildItemStarted(RecyclerView.ViewHolder holder, int groupPosition, int childPosition);
        void OnSetGroupItemSwipeBackground(RecyclerView.ViewHolder holder, int groupPosition, int type);
        void OnSetChildItemSwipeBackground(RecyclerView.ViewHolder holder, int groupPosition, int childPosition, int type);
    }
}