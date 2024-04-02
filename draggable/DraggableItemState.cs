using Com.H6ah4i.Android.Widget.Advrecyclerview.Draggable.Annotation;

namespace Com.H6ah4i.Android.Widget.Advrecyclerview.Draggable
{
    /// <summary>
    /// Helper class for decoding {@link DraggableItemViewHolder#getDragStateFlags()} flag values.
    /// </summary>
    public class DraggableItemState
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
        /// Checks whether the dragging is currently performed.
        /// </summary>
        /// <returns>True if the user is dragging an item, otherwise else.</returns>
        public virtual bool IsDragging()
        {
            return (mFlags & (int)DraggableItemConstants.STATE_FLAG_DRAGGING) != 0;
        }

        /// <summary>
        /// Checks whether the item is being dragged.
        /// </summary>
        /// <returns>True if the associated item is being dragged, otherwise false.</returns>
        public virtual bool IsActive()
        {
            return (mFlags & (int)DraggableItemConstants.STATE_FLAG_IS_ACTIVE) != 0;
        }

        /// <summary>
        /// Checks whether the item is in range of drag-sortable items.
        /// </summary>
        /// <returns>True if the associated item is in range, otherwise false.</returns>
        public virtual bool IsInRange()
        {
            return (mFlags & (int)DraggableItemConstants.STATE_FLAG_IS_IN_RANGE) != 0;
        }

        /// <summary>
        /// Checks whether state flags are changed or not.
        /// </summary>
        /// <returns>True if flags are updated, otherwise false.</returns>
        public virtual bool IsUpdated()
        {
            return (mFlags & (int)DraggableItemConstants.STATE_FLAG_IS_UPDATED) != 0;
        }
    }
}