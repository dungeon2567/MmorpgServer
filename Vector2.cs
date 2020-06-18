using System.Xml.Serialization;
using System;
using System.Xml;
namespace MmorpgServer
{
    [Serializable]
    public struct Vector2
    {
        public static readonly Vector2 Deg90 = new Vector2(0, 1);
        public static readonly Vector2 Deg270 = new Vector2(0, -1);

        public double X;
        public double Y;

        public Vector2(double X, double Y)
        {
            this.X = X;
            this.Y = Y;
        }

        [XmlIgnore]
        public double Length
        {
            get
            {
                return Math.Sqrt(X * X + Y * Y);
            }
        }

        public void Normalize()
        {
            Double length = Length;

            length = length == 0 ? 1 : length;

            X = X / length;
            Y = Y / length;
        }

        [XmlIgnore]
        public double SqrLength
        {
            get
            {
                return X * X + Y * Y;
            }
        }
        public static readonly Vector2 Zero = new Vector2(0, 0);

        public static Vector2 operator -(in Vector2 a) => new Vector2(-a.X, -a.Y);

        public static Vector2 operator -(in Vector2 a, in Vector2 b) => new Vector2(a.X - b.X, a.Y - b.Y);

        public static Vector2 operator +(in Vector2 a, in Vector2 b) => new Vector2(a.X + b.X, a.Y + b.Y);

        public static Vector2 operator *(in Vector2 a, double b) => new Vector2(a.X * b, a.Y * b);

        public static Vector2 operator /(double a, in Vector2 b) => new Vector2(a / b.X, a / b.Y);

        public static Vector2 operator *(in Vector2 a, in Vector2 b)
        {
            return new Vector2(a.X * b.X - a.Y * b.Y, a.X * b.Y + b.X * a.Y);
        }

        public override string ToString()
        {
            return "Vector2(" + X + "," + Y + ")";
        }

        const Double Epsilon = 0.1 * 0.1;

        public static bool operator ==(in Vector2 self, in Vector2 other)
        {
            var dist = self - other;

            var sqrDist = (dist.X * dist.X) + (dist.Y * dist.Y);

            return sqrDist < Epsilon;
        }

        public static bool operator !=(in Vector2 self, in Vector2 other)
        {
            var dist = self - other;

            var sqrDist = (dist.X * dist.X) + (dist.Y * dist.Y);

            return sqrDist >= Epsilon;
        }
    }
}