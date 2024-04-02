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
using Com.H6ah4i.Android.Widget.Advrecyclerview.Adapter;
using Java.Util;
using AndroidX.RecyclerView.Widget;

namespace Com.H6ah4i.Android.Widget.Advrecyclerview.Utils
{
    public class WrapperAdapterUtils
    {
        private WrapperAdapterUtils()
        {
        }

        public static T FindWrappedAdapter<T>(RecyclerView.Adapter adapter) where T : class
        {
            if (adapter is T castedAdapter)
            {
                return castedAdapter;
            }
            else if (adapter is SimpleWrapperAdapter simpleWrapperAdapter)
            {
                RecyclerView.Adapter wrappedAdapter = simpleWrapperAdapter.GetWrappedAdapter();
                return FindWrappedAdapter<T>(wrappedAdapter);
            }
            else
            {
                return null;
            }
        }

        public static T FindWrappedAdapter<T>(RecyclerView.Adapter originAdapter, int position) where T : class
        {
            AdapterPath path = new AdapterPath();
            int wrappedPosition = UnwrapPosition(originAdapter, null, null, position, path);

            if (wrappedPosition == RecyclerView.NoPosition)
            {
                return null;
            }

            foreach (AdapterPathSegment segment in path.Segments())
            {
                if (segment.adapter is T castedAdapter)
                {
                    return castedAdapter;
                }
            }

            return null;
        }

        public static RecyclerView.Adapter ReleaseAll(RecyclerView.Adapter adapter)
        {
            return ReleaseCyclically(adapter);
        }

        private static RecyclerView.Adapter ReleaseCyclically(RecyclerView.Adapter adapter)
        {
            if (!(adapter is IWrapperAdapter))
            {
                return adapter;
            }

            IWrapperAdapter wrapperAdapter = (IWrapperAdapter)adapter;
            List<RecyclerView.Adapter> wrappedAdapters = new List<RecyclerView.Adapter>();
            wrapperAdapter.GetWrappedAdapters(wrappedAdapters);
            wrapperAdapter.Release();
            for (int i = wrappedAdapters.Count - 1; i >= 0; i--)
            {
                RecyclerView.Adapter wrappedAdapter = wrappedAdapters[i];
                ReleaseCyclically(wrappedAdapter);
            }

            wrappedAdapters.Clear();
            return adapter;
        }

        public static int UnwrapPosition(RecyclerView.Adapter originAdapter, int position)
        {
            return UnwrapPosition(originAdapter, null, position);
        }

        public static int UnwrapPosition(RecyclerView.Adapter originAdapter, RecyclerView.Adapter targetAdapter, int position)
        {
            return UnwrapPosition(originAdapter, targetAdapter, null, position, null);
        }

        public static int UnwrapPosition(RecyclerView.Adapter originAdapter, RecyclerView.Adapter targetAdapter, object targetAdapterTag, int position)
        {
            return UnwrapPosition(originAdapter, targetAdapter, targetAdapterTag, position, null);
        }

        public static int UnwrapPosition(RecyclerView.Adapter originAdapter, AdapterPathSegment targetAdapterPathSegment, int originPosition, AdapterPath destPath)
        {
            return UnwrapPosition(originAdapter, targetAdapterPathSegment.adapter, targetAdapterPathSegment.tag, originPosition, destPath);
        }

        public static int UnwrapPosition(RecyclerView.Adapter originAdapter, RecyclerView.Adapter targetAdapter, object targetAdapterTag, int originPosition, AdapterPath destPath)
        {
            RecyclerView.Adapter wrapper = originAdapter;
            int wrappedPosition = originPosition;
            UnwrapPositionResult tmpResult = new UnwrapPositionResult();
            object wrappedAdapterTag = null;
            if (destPath != null)
            {
                destPath.Clear();
            }

            if (wrapper == null)
            {
                return RecyclerView.NoPosition;
            }

            if (destPath != null)
            {
                destPath.Append(new AdapterPathSegment(originAdapter, null));
            }

            do
            {
                if (wrappedPosition == RecyclerView.NoPosition)
                {
                    break;
                }

                if (wrapper == targetAdapter)
                {
                    break;
                }

                if (!(wrapper is IWrapperAdapter))
                {
                    if (targetAdapter != null)
                    {
                        wrappedPosition = RecyclerView.NoPosition;
                    }

                    break;
                }

                IWrapperAdapter wrapperParentAdapter = (IWrapperAdapter)wrapper;
                tmpResult.Clear();
                wrapperParentAdapter.UnwrapPosition(tmpResult, wrappedPosition);
                wrappedPosition = tmpResult.position;
                wrappedAdapterTag = tmpResult.tag;
                if (tmpResult.IsValid())
                {
                    if (destPath != null)
                    {
                        destPath.Append(tmpResult);
                    }
                }

                wrapper = tmpResult.adapter;
            }
            while (wrapper != null);
            if (targetAdapter != null && wrapper != targetAdapter)
            {
                wrappedPosition = RecyclerView.NoPosition;
            }

            if (targetAdapterTag != null && (wrappedAdapterTag != targetAdapterTag))
            {
                wrappedPosition = RecyclerView.NoPosition;
            }

            if (wrappedPosition == RecyclerView.NoPosition && destPath != null)
            {
                destPath.Clear();
            }

            return wrappedPosition;
        }

        public static int WrapPosition(AdapterPath path, RecyclerView.Adapter originAdapter, RecyclerView.Adapter targetAdapter, int position)
        {
            IList<AdapterPathSegment> segments = path.Segments();
            int nSegments = segments.Count;
            int originSegmentIndex = (originAdapter == null) ? nSegments - 1 : -1;
            int targetSegmentIndex = (targetAdapter == null) ? 0 : -1;
            if (originAdapter != null || targetAdapter != null)
            {
                for (int i = 0; i < nSegments; i++)
                {
                    AdapterPathSegment segment = segments[i];
                    if (originAdapter != null && segment.adapter == originAdapter)
                    {
                        originSegmentIndex = i;
                    }

                    if (targetAdapter != null && segment.adapter == targetAdapter)
                    {
                        targetSegmentIndex = i;
                    }
                }
            }

            if (!((originSegmentIndex != -1) && (targetSegmentIndex != -1) && (targetSegmentIndex <= originSegmentIndex)))
            {
                return RecyclerView.NoPosition;
            }

            return WrapPosition(path, originSegmentIndex, targetSegmentIndex, position);
        }

        public static int WrapPosition(AdapterPath path, int originSegmentIndex, int targetSegmentIndex, int position)
        {
            IList<AdapterPathSegment> segments = path.Segments();
            int wrappedPosition = position;
            for (int i = originSegmentIndex; i > targetSegmentIndex; i--)
            {
                AdapterPathSegment segment = segments[i];
                AdapterPathSegment parentSegment = segments[i - 1];
                int prevWrappedPosition = wrappedPosition;
                wrappedPosition = ((IWrapperAdapter)parentSegment.adapter).WrapPosition(segment, wrappedPosition);
                if (wrappedPosition == RecyclerView.NoPosition)
                {
                    break;
                }
            }

            return wrappedPosition;
        }
    }
}