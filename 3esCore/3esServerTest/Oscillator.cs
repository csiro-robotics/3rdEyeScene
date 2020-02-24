using System;
using Tes.Maths;

namespace Tes
{
  class Oscillator : ShapeMover
  {
    public Vector3 ReferencePos { get; set; }
    public Vector3 Axis { get; set; }
    public float Amplitude { get; set; }
    public float Period { get; set; }

    public Oscillator(Shapes.Shape shape, float amplitude = 1.0f, float period = 5.0f)
      : this(shape, amplitude, period, Vector3.AxisZ)
    {
    }

    public Oscillator(Shapes.Shape shape, float amplitude, float period, Vector3 axis)
      : base(shape)
    {
      ReferencePos = (shape != null) ? shape.Position : Vector3.Zero;
      Axis = axis;
      Amplitude = amplitude;
      Period = period;
    }

    public override void Reset()
    {
      ReferencePos = (Shape != null) ? Shape.Position : Vector3.Zero;
    }


    public override void Update(float time, float dt)
    {
      Vector3 pos = ReferencePos + Amplitude * (float)Math.Sin(time) * Axis;
      Shape.Position = pos;
    }
  }
}
