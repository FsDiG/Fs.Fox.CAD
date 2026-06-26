using System;
using StraightSkeletonNet.Circular;
using StraightSkeletonNet.Primitives;

namespace StraightSkeletonNet.Events
{
    internal class VerticesplitEvent : SplitEvent
    {
        public VerticesplitEvent(Vector2d point, double distance, Vertex parent) :
            base(point, distance, parent, null)
        {
        }

        public override String ToString()
        {
            return "VerticesplitEvent [V=" + V + ", Parent=" +
                   (Parent != null ? Parent.Point.ToString() : "null")
                   + ", Distance=" + Distance + "]";
        }
    }
}