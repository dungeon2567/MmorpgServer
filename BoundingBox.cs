namespace MmorpgServer
{
    public struct BoundingBox
    {
        public Vector2i From;
        public Vector2i To;

        public BoundingBox(in Vector2i from, in Vector2i to)
        {
            this.From = from;
            this.To = to;
        }

        public bool Contains(in Vector2i p)
        {
            return p.X >= this.From.X && p.X <= this.To.X && p.Y >= this.From.Y && p.Y <= this.To.Y;
        }

        public static bool operator ==(in BoundingBox self, in BoundingBox other)
        {
            return self.From == other.From && self.To == other.To;
        }

        public static bool operator !=(in BoundingBox self, in BoundingBox other)
        {
            return self.From != other.From || self.To != other.To;
        }
    }
}