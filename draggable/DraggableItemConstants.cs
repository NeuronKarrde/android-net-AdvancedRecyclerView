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
namespace Com.H6ah4i.Android.Widget.Advrecyclerview.Draggable
{
    [Flags]
    public enum DraggableItemConstants : int
    {
        /// <summary>
        /// State flag for the {@link DraggableItemViewHolder#setDragStateFlags(int)} and {@link DraggableItemViewHolder#getDragStateFlags()} methods.
        /// Indicates that currently performing dragging.
        /// </summary>
        STATE_FLAG_DRAGGING = 0,
        /// <summary>
        /// State flag for the {@link DraggableItemViewHolder#setDragStateFlags(int)} and {@link DraggableItemViewHolder#getDragStateFlags()} methods.
        /// Indicates that this item is being dragged.
        /// </summary>
        STATE_FLAG_IS_ACTIVE = 1,
        /// <summary>
        /// State flag for the {@link DraggableItemViewHolder#setDragStateFlags(int)} and {@link DraggableItemViewHolder#getDragStateFlags()} methods.
        /// Indicates that this item is in the range of drag-sortable items
        /// </summary>
        STATE_FLAG_IS_IN_RANGE = 2,
        /// <summary>
        /// State flag for the {@link DraggableItemViewHolder#setDragStateFlags(int)} and {@link DraggableItemViewHolder#getDragStateFlags()} methods.
        /// If this flag is set, some other flags are changed and require to apply.
        /// </summary>
        STATE_FLAG_IS_UPDATED = 4 // ---
    }
}