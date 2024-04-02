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
using Android.Graphics;
using Com.H6ah4i.Android.Widget.Advrecyclerview.Utils;
using AndroidX.RecyclerView.Widget;

namespace Com.H6ah4i.Android.Widget.Advrecyclerview.Draggable
{
    public class DraggingItemInfo
    {
        public readonly int width;
        public readonly int height;
        public readonly long id;
        public readonly int initialItemLeft;
        public readonly int initialItemTop;
        public readonly int grabbedPositionX;
        public readonly int grabbedPositionY;
        public readonly Rect margins;
        public readonly int spanSize;
        public DraggingItemInfo(RecyclerView rv, RecyclerView.ViewHolder vh, int touchX, int touchY)
        {
            width = vh.ItemView.Width;
            height = vh.ItemView.Height;
            id = vh.ItemId;
            initialItemLeft = vh.ItemView.Left;
            initialItemTop = vh.ItemView.Top;
            grabbedPositionX = touchX - initialItemLeft;
            grabbedPositionY = touchY - initialItemTop;
            margins = new Rect();
            CustomRecyclerViewUtils.GetLayoutMargins(vh.ItemView, margins);
            spanSize = CustomRecyclerViewUtils.GetSpanSize(vh);
        }

        private DraggingItemInfo(DraggingItemInfo info, RecyclerView.ViewHolder vh)
        {
            id = info.id;
            width = vh.ItemView.Width;
            height = vh.ItemView.Height;
            margins = new Rect(info.margins);
            spanSize = CustomRecyclerViewUtils.GetSpanSize(vh);
            initialItemLeft = info.initialItemLeft;
            initialItemTop = info.initialItemTop;
            float pcx = info.width * 0.5F;
            float pcy = info.height * 0.5F;
            float cx = width * 0.5F;
            float cy = height * 0.5F;
            float centerOffsetX = info.grabbedPositionX - pcx;
            float centerOffsetY = info.grabbedPositionY - pcy;
            float gpx = cx + centerOffsetX;
            float gpy = cy + centerOffsetY;
            grabbedPositionX = (int)((gpx >= 0 && gpx < width) ? gpx : cx);
            grabbedPositionY = (int)((gpy >= 0 && gpy < height) ? gpy : cy);
        }

        public static DraggingItemInfo CreateWithNewView(DraggingItemInfo info, RecyclerView.ViewHolder vh)
        {
            return new DraggingItemInfo(info, vh);
        }
    }
}