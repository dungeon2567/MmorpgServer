namespace MmorpgServer
{
    public class CollisionResult
    {
        public double Penetration;

        public Vector2 Normal;

        public CollisionResult(double penetration, Vector2 normal)
        {
            this.Penetration = penetration;
            this.Normal = normal;
        }

        public override string ToString()
        {
            return "CollisionResult(" + this.Penetration + ", " + this.Normal + ")";
        }
    }
}