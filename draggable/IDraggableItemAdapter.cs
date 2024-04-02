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

namespace Com.H6ah4i.Android.Widget.Advrecyclerview.Draggable
{
    public interface IDraggableItemAdapter
    {
        bool OnCheckCanStartDrag(RecyclerView.ViewHolder holder, int position, int x, int y);
        ItemDraggableRange OnGetItemDraggableRange(RecyclerView.ViewHolder holder, int position);
        void OnMoveItem(int fromPosition, int toPosition);
        bool OnCheckCanDrop(int draggingPosition, int dropPosition);
        void OnItemDragStarted(int position);
        void OnItemDragFinished(int fromPosition, int toPosition, bool result);
    }
}