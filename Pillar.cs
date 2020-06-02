namespace MmorpgServer
{
    public class Pillar : GameObject
    {
        public Pillar()
        {
            CollisionShape = new Circle(Vector2.Zero, 0.75);

            Solid = true;
        } 
    }
}