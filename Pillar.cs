namespace MmorpgServer
{
    public class Pillar : Entity
    {
        public Pillar()
        {
            CollisionShape = new Circle(Vector2.Zero, 0.75);

            Solid = true;
        } 

        public double Radius
        {
            get
            {
                return CollisionShape.Radius;
            }
            set
            {
                CollisionShape.Radius = value;
            }
        }
    }
}