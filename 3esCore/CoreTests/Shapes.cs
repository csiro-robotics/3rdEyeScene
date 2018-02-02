// Copyright (c) CSIRO 2018
// Commonwealth Scientific and Industrial Research Organisation (CSIRO)
// ABN 41 687 119 230
//
// author Kazys Stepanas
//
using NUnit.Framework;
using Tes.Maths;
using Tes.Shapes;

namespace Tes.CoreTests
{
  [TestFixture()]
  public class Shapes
  {
    [TestCase]
    public void ArrowTest()
    {
      ShapeTestFramework framework = new ShapeTestFramework();
      framework.TestShape<Arrow>(new Arrow());
      framework.TestShape<Arrow>(new Arrow(42));
      framework.TestShape<Arrow>(new Arrow(42, 1));
      framework.TestShape<Arrow>(new Arrow(42, new Vector3(1.2f, 2.3f, 3.4f), new Vector3(0, 0, 0), 0.05f));
      framework.TestShape<Arrow>(new Arrow(42, new Vector3(1.2f, 2.3f, 3.4f), new Vector3(1, 1, 1).Normalised, 2.0f, 0.05f));
      framework.TestShape<Arrow>(new Arrow(42, 1, new Vector3(1.2f, 2.3f, 3.4f), new Vector3(0, 0, 0), 0.05f));
      framework.TestShape<Arrow>(new Arrow(42, 1, new Vector3(1.2f, 2.3f, 3.4f), new Vector3(1, 1, 1).Normalised, 2.0f, 0.05f));
    }
  }
}
