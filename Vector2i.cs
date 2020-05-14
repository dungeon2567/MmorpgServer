using System;

namespace MmorpgServer
{
    public struct Vector2i : IEquatable<Vector2i>
    {
        public Int32 X;
        public Int32 Y;

        public Vector2i(Int32 x, Int32 y)
        {
            X = x;
            Y = y;
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return (this.X.GetHashCode() * 397) ^ this.Y.GetHashCode();
            }
        }

        public override bool Equals(object obj)
        {
            if (!(obj is Vector2i))
            {
                return false;
            }

            return this.Equals((Vector2i)obj);
        }

        public bool Equals(Vector2i other)
        {
            return
                X == other.X &&
                Y == other.Y;
        }

        public static bool operator ==(in Vector2i self, in Vector2i other)
        {
            return self.X == other.X && self.Y == other.Y;
        }

        public static bool operator !=(in Vector2i self, in Vector2i other)
        {
            return self.X != other.X || self.Y != other.Y;
        }
    }
}