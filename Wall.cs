namespace MmorpgServer
{
    public class Wall : Entity
    {
        public Wall()
        {
            CollisionShape = new Rectangle(Vector2.Zero, new Vector2(1, 1));

            Solid = true;
        }

        public Vector2 HalfSize
        {
            get
            {
                return CollisionShape.HalfSize;
            }
            set
            {
                CollisionShape.HalfSize = value;
            }
        }
    }
}