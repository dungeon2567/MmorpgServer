using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MmorpgServer
{
    public struct Ray
    {
        public readonly Vector2 Position;
        public readonly Vector2 InvDirection;

        public Ray(in Vector2 position, in Vector2 direction)
        {
            Position = position;
            InvDirection = 1 / direction;
        }
    }
}
