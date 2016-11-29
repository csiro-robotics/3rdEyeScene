using System;
using Tes;

namespace Tes
{
  abstract class ShapeMover
  {
    protected ShapeMover(Shapes.Shape shape)
    {
      Shape = shape;
    }

    public Shapes.Shape Shape { get; protected set; }

    public abstract void Reset();

    public abstract void Update(float time, float dt);
  }
}
