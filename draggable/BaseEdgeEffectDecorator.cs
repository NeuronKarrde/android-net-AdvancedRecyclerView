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
using Android.Widget;
using AndroidX.Core.View;
using AndroidX.Core.Widget;
using AndroidX.RecyclerView.Widget;

namespace Com.H6ah4i.Android.Widget.Advrecyclerview.Draggable
{
    abstract class BaseEdgeEffectDecorator : RecyclerView.ItemDecoration
    {
        private RecyclerView mRecyclerView;
        private EdgeEffect mGlow1;
        private EdgeEffect mGlow2;
        private bool mStarted;
        private int mGlow1Dir;
        private int mGlow2Dir;
        protected const int EDGE_LEFT = 0;
        protected const int EDGE_TOP = 1;
        protected const int EDGE_RIGHT = 2;
        protected const int EDGE_BOTTOM = 3;
        public BaseEdgeEffectDecorator(RecyclerView recyclerView)
        {
            mRecyclerView = recyclerView;
        }

        protected abstract int GetEdgeDirection(int no);
        public override void OnDrawOver(Canvas c, RecyclerView parent, RecyclerView.State state)
        {
            bool needsInvalidate = false;
            if (mGlow1 != null)
            {
                needsInvalidate |= DrawGlow(c, parent, mGlow1Dir, mGlow1);
            }

            if (mGlow2 != null)
            {
                needsInvalidate |= DrawGlow(c, parent, mGlow2Dir, mGlow2);
            }

            if (needsInvalidate)
            {
                ViewCompat.PostInvalidateOnAnimation(parent);
            }
        }

        private static bool DrawGlow(Canvas c, RecyclerView parent, int dir, EdgeEffect edge)
        {
            if (edge.IsFinished)
            {
                return false;
            }

            int restore = c.Save();
            bool clipToPadding = GetClipToPadding(parent);
            switch (dir)
            {
                case EDGE_TOP:
                    if (clipToPadding)
                    {
                        c.Translate(parent.PaddingLeft, parent.PaddingTop);
                    }

                    break;
                case EDGE_BOTTOM:
                    c.Rotate(180);
                    if (clipToPadding)
                    {
                        c.Translate(-parent.Width + parent.PaddingRight, -parent.Height + parent.PaddingBottom);
                    }
                    else
                    {
                        c.Translate(-parent.Width, -parent.Height);
                    }

                    break;
                case EDGE_LEFT:
                    c.Rotate(-90);
                    if (clipToPadding)
                    {
                        c.Translate(-parent.Height + parent.PaddingTop, parent.PaddingLeft);
                    }
                    else
                    {
                        c.Translate(-parent.Height, 0);
                    }

                    break;
                case EDGE_RIGHT:
                    c.Rotate(90);
                    if (clipToPadding)
                    {
                        c.Translate(parent.PaddingTop, -parent.Width + parent.PaddingRight);
                    }
                    else
                    {
                        c.Translate(0, -parent.Width);
                    }

                    break;
            }

            bool needsInvalidate = edge.Draw(c);
            c.RestoreToCount(restore);
            return needsInvalidate;
        }

        public virtual void Start()
        {
            if (mStarted)
            {
                return;
            }

            mGlow1Dir = GetEdgeDirection(0);
            mGlow2Dir = GetEdgeDirection(1);
            mRecyclerView.AddItemDecoration(this);
            mStarted = true;
        }

        public virtual void Finish()
        {
            if (mStarted)
            {
                mRecyclerView.RemoveItemDecoration(this);
            }

            ReleaseBothGlows();
            mRecyclerView = null;
            mStarted = false;
        }

        public virtual void PullFirstEdge(float deltaDistance)
        {
            EnsureGlow1(mRecyclerView);
            EdgeEffectCompat.OnPull(mGlow1, deltaDistance, 0.5F);
            ViewCompat.PostInvalidateOnAnimation(mRecyclerView);
        }

        public virtual void PullSecondEdge(float deltaDistance)
        {
            EnsureGlow2(mRecyclerView);
            EdgeEffectCompat.OnPull(mGlow2, deltaDistance, 0.5F);
            ViewCompat.PostInvalidateOnAnimation(mRecyclerView);
        }

        public virtual void ReleaseBothGlows()
        {
            bool needsInvalidate = false;
            if (mGlow1 != null)
            {
                mGlow1.OnRelease();

                //noinspection ConstantConditions
                needsInvalidate |= mGlow1.IsFinished;
            }

            if (mGlow2 != null)
            {
                mGlow2.OnRelease();
                needsInvalidate |= mGlow2.IsFinished;
            }

            if (needsInvalidate)
            {
                ViewCompat.PostInvalidateOnAnimation(mRecyclerView);
            }
        }

        //noinspection ConstantConditions
        private void EnsureGlow1(RecyclerView rv)
        {
            if (mGlow1 == null)
            {
                mGlow1 = new EdgeEffect(rv.Context);
            }

            UpdateGlowSize(rv, mGlow1, mGlow1Dir);
        }

        //noinspection ConstantConditions
        private void EnsureGlow2(RecyclerView rv)
        {
            if (mGlow2 == null)
            {
                mGlow2 = new EdgeEffect(rv.Context);
            }

            UpdateGlowSize(rv, mGlow2, mGlow2Dir);
        }

        //noinspection ConstantConditions
        private static void UpdateGlowSize(RecyclerView rv, EdgeEffect glow, int dir)
        {
            int width = rv.MeasuredWidth;
            int height = rv.MeasuredHeight;
            if (GetClipToPadding(rv))
            {
                width -= rv.PaddingLeft + rv.PaddingRight;
                height -= rv.PaddingTop + rv.PaddingBottom;
            }

            width = Math.Max(0, width);
            height = Math.Max(0, height);
            if (dir == EDGE_LEFT || dir == EDGE_RIGHT)
            {
                int t = width;

                //noinspection SuspiciousNameCombination
                width = height;
                height = t;
            }

            glow.SetSize(width, height);
        }

        //noinspection ConstantConditions
        //noinspection SuspiciousNameCombination
        private static bool GetClipToPadding(RecyclerView rv)
        {
            return rv.GetLayoutManager().ClipToPadding;
        }

        //noinspection ConstantConditions
        //noinspection SuspiciousNameCombination
        public virtual void ReorderToTop()
        {
            if (mStarted)
            {
                mRecyclerView.RemoveItemDecoration(this);
                mRecyclerView.AddItemDecoration(this);
            }
        }
    }
}