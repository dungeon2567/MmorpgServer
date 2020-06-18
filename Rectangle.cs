using System;
namespace MmorpgServer
{
    public class Rectangle : Shape
    {
        public override Vector2 HalfSize
        {
            get;
            set;
        }
        public Rectangle(in Vector2 position, in Vector2 halfSize) : base(in position)
        {
            this.HalfSize = halfSize;
        }

        public double X1
        {
            get
            {
                return Position.X - HalfSize.X;
            }
        }

        public double X2
        {
            get
            {
                return Position.X + HalfSize.X;
            }
        }

        public double y1
        {
            get
            {
                return Position.Y - HalfSize.Y;
            }
        }

        public double Y2
        {
            get
            {
                return Position.Y + HalfSize.Y;
            }
        }

        public override void ResolveCollision(Circle other, out CollisionResult result)
        {
            other.ResolveCollision(this, out result);

            result.Normal.X = -result.Normal.X;
            result.Normal.Y = -result.Normal.Y;
        }

        public override void ResolveCollision(Shape other, out CollisionResult result)
        {
            other.ResolveCollision(this, out result);

            result.Normal.X = -result.Normal.X;
            result.Normal.Y = -result.Normal.Y;
        }

        public override void ResolveCollision(Rectangle other, out CollisionResult result)
        {
            throw new NotImplementedException();
        }

        public override bool IntersectsWith(AreaOfInterest areaOfInterest)
        {
            var nearestX = Math.Max(this.Position.X, Math.Min(areaOfInterest.Position.X, areaOfInterest.Position.X + this.HalfSize.X));
            var nearestY = Math.Max(this.Position.Y, Math.Min(areaOfInterest.Position.Y, areaOfInterest.Position.Y + this.HalfSize.Y));

            var dist = new Vector2(areaOfInterest.Position.X - nearestX, areaOfInterest.Position.Y - nearestY);

            var sqrLength = dist.X * dist.X + dist.Y * dist.Y;

            return areaOfInterest.Radius * areaOfInterest.Radius > sqrLength;
        }

        public override BoundingBox GetClusters()
        {
            return new BoundingBox(new Vector2i(
                (Int32)Math.Floor((Position.X - HalfSize.X) / 3),
                (Int32)Math.Floor((Position.Y - HalfSize.Y) / 3)
            ), new Vector2i(
                (Int32)Math.Ceiling((Position.X + HalfSize.X) / 3),
                (Int32)Math.Ceiling((Position.Y + HalfSize.Y) / 3)
            ));
        }

        public override double Raycast(in Ray ray)
        {
            double tx1 = (X1 - ray.Position.X) * ray.InvDirection.X;
            double tx2 = (X2 - ray.Position.X) * ray.InvDirection.X;

            double tmin = Math.Min(tx1, tx2);
            double tmax = Math.Max(tx1, tx2);

            double ty1 = (y1 - ray.Position.Y) * ray.InvDirection.Y;
            double ty2 = (Y2 - ray.Position.Y) * ray.InvDirection.Y;

            tmin = Math.Max(tmin, Math.Min(ty1, ty2));
            tmax = Math.Min(tmax,  Math.Max(ty1, ty2));

            return tmax >= tmin ? tmin : Double.PositiveInfinity;
        }
    }
}