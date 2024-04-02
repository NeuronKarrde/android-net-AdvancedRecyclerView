using Com.H6ah4i.Android.Widget.Advrecyclerview.Swipeable.Annotation;

namespace Com.H6ah4i.Android.Widget.Advrecyclerview.Swipeable
{
    /// <summary>
    /// Helper class for decoding {@link SwipeableItemViewHolder#getSwipeStateFlags()} flag values.
    /// </summary>
    public class SwipeableItemState
    {
        private int mFlags;
        public virtual int GetFlags()
        {
            return mFlags;
        }

        public virtual void SetFlags(int flags)
        {
            mFlags = flags;
        }

        /// <summary>
        /// Checks whether the swiping is currently performed.
        /// </summary>
        /// <returns>True if the user is swiping an item, otherwise else.</returns>
        public virtual bool IsSwiping()
        {
            return (mFlags & SwipeableItemConstants.StateFlagSwiping) != 0;
        }

        /// <summary>
        /// Checks whether the item is being dragged.
        /// </summary>
        /// <returns>True if the associated item is being swiped, otherwise false.</returns>
        public virtual bool IsActive()
        {
            return (mFlags & SwipeableItemConstants.StateFlagIsActive) != 0;
        }

        /// <summary>
        /// Checks whether state flags are changed or not.
        /// </summary>
        /// <returns>True if flags are updated, otherwise false.</returns>
        public virtual bool IsUpdated()
        {
            return (mFlags & SwipeableItemConstants.StateFlagIsUpdated) != 0;
        }
    }
}