using System;
namespace MmorpgServer
{
    public class Circle : Shape
    {

        public Circle(in Vector2 position, Double radius) : base(in position)
        {
            this.Radius = radius;
        }

        public override double Radius {
            get;
            set;
        }

        public override Vector2 GetPointOnDirection(in Vector2 direction){
            return Position + (direction * Radius);
        }

        public override void ResolveCollision(Rectangle other, out CollisionResult result)
        {
            var nearestX = Math.Max(other.Position.X - other.HalfSize.X, Math.Min(this.Position.X, other.Position.X + other.HalfSize.X));
            var nearestY = Math.Max(other.Position.Y - other.HalfSize.Y, Math.Min(this.Position.Y, other.Position.Y + other.HalfSize.Y));

            var dist = new Vector2(this.Position.X - nearestX, this.Position.Y - nearestY);

            var sqrLength = dist.X * dist.X + dist.Y * dist.Y;

            if (this.Radius * this.Radius < sqrLength)
            {
                result = new CollisionResult(0, Vector2.Zero);
            }
            else
            {
                var length = Math.Sqrt(sqrLength);

                var penetrationDepth = this.Radius - length;

                result = new CollisionResult(penetrationDepth, new Vector2(dist.X / length, dist.Y / length));
            }
        }

        public override void ResolveCollision(Circle other, out CollisionResult result)
        {
            var distance = this.Position - other.Position;

            var radiiSum = this.Radius + other.Radius;

            var squareDist = distance.X * distance.X + distance.Y * distance.Y;

            if (squareDist > radiiSum * radiiSum)
            {
                result = new CollisionResult(0, Vector2.Zero);
            }
            else
            {
                var length = Math.Sqrt(squareDist);

                if (length == 0)
                    length = 1;

                var normal = new Vector2(distance.X / length, distance.Y / length);

                var penetrationDepth = radiiSum - length;

                result = new CollisionResult(penetrationDepth, normal);
            }
        }
        public override void ResolveCollision(Shape other, out CollisionResult result)
        {
            other.ResolveCollision(this, out result);

            result.Normal.X = -result.Normal.X;
            result.Normal.Y = -result.Normal.Y;
        }

        public override bool IntersectsWith(AreaOfInterest areaOfInterest)
        {
            var distance = this.Position - areaOfInterest.Position;

            var radiiSum = this.Radius + areaOfInterest.Radius;

            var squareDist = distance.X * distance.X + distance.Y * distance.Y;

            return squareDist < radiiSum * radiiSum;
        }

        public override BoundingBox GetClusters()
        {
            return new BoundingBox(new Vector2i(
                (Int32)Math.Floor((Position.X - Radius) / 3),
                (Int32)Math.Floor((Position.Y - Radius) / 3)
            ), new Vector2i(
                (Int32)Math.Ceiling((Position.X + Radius) / 3),
                (Int32)Math.Ceiling((Position.Y + Radius) / 3)
            ));
        }
    }
}