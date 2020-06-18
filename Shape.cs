using System;

namespace MmorpgServer
{
    public abstract class Shape
    {
        public Vector2 Position;

        public Double Rotation;

        public Shape(in Vector2 position)
        {
            this.Position = position;
            this.Rotation = 0;
        }

        public virtual Vector2 HalfSize
        {
            get
            {
                throw new NotSupportedException();
            }
            set
            {
                throw new NotSupportedException();
            }
        }

        public virtual Double Radius
        {
            get
            {
                throw new NotSupportedException();
            }
            set
            {
                throw new NotSupportedException();
            }
        }

        public virtual Vector2 GetPointOnDirection(in Vector2 direction)
        {
            throw new NotImplementedException();
        }

        public abstract void ResolveCollision(Shape other, out CollisionResult result);

        public abstract void ResolveCollision(Circle other, out CollisionResult result);

        public abstract void ResolveCollision(Rectangle other, out CollisionResult result);

        public abstract bool IntersectsWith(AreaOfInterest areaOfInterest);

        public virtual double Raycast(in Ray ray)
        {
            return Double.PositiveInfinity;
        }

        public abstract BoundingBox GetClusters();
    }
}