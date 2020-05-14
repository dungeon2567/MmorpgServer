namespace MmorpgServer
{
    public class Wall : GameObject
    {
        public Wall()
        {
            CollisionShape = new Rectangle(Vector2.Zero, new Vector2(1.5, 0.5));

            Solid = true;
        }

        
    }
}