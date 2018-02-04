// Copyright (c) CSIRO 2018
// Commonwealth Scientific and Industrial Research Organisation (CSIRO)
// ABN 41 687 119 230
//
// author Kazys Stepanas
//
using NUnit.Framework;
using System;
using System.Collections.Generic;
using Tes.Maths;
using Tes.Shapes;

namespace Tes.CoreTests
{
  [TestFixture()]
  public class Shapes
  {
    public static void ValidateText2D(Shape shapeArg, Shape referenceArg, Dictionary<ulong, Resource> resources)
    {
      ShapeTestFramework.ValidateShape(shapeArg, referenceArg, resources);
      Text2D shape = (Text2D)shapeArg;
      Text2D reference = (Text2D)referenceArg;
      Assert.AreEqual(reference.Text, shape.Text);
    }

    public static void ValidateText3D(Shape shapeArg, Shape referenceArg, Dictionary<ulong, Resource> resources)
    {
      ShapeTestFramework.ValidateShape(shapeArg, referenceArg, resources);
      Text3D shape = (Text3D)shapeArg;
      Text3D reference = (Text3D)referenceArg;
      Assert.AreEqual(reference.Text, shape.Text);
    }

    [TestCase]
    public void ArrowTest()
    {
      ShapeTestFramework.CreateShapeFunction create = () => { return new Arrow(); };
      ShapeTestFramework.TestShape(new Arrow(), create);
      ShapeTestFramework.TestShape(new Arrow(42), create);
      ShapeTestFramework.TestShape(new Arrow(42, 1), create);
      ShapeTestFramework.TestShape(new Arrow(42, new Vector3(1.2f, 2.3f, 3.4f), new Vector3(0, 0, 0), 0.05f), create);
      ShapeTestFramework.TestShape(new Arrow(42, 1, new Vector3(1.2f, 2.3f, 3.4f), new Vector3(0, 0, 0), 0.05f), create);
      ShapeTestFramework.TestShape(new Arrow(42, new Vector3(1.2f, 2.3f, 3.4f), new Vector3(1, 1, 1).Normalised, 2.0f, 0.05f), create);
      ShapeTestFramework.TestShape(new Arrow(42, 1, new Vector3(1.2f, 2.3f, 3.4f), new Vector3(1, 1, 1).Normalised, 2.0f, 0.05f), create);
    }

    [TestCase]
    public void BoxTest()
    {
      ShapeTestFramework.CreateShapeFunction create = () => { return new Box(); };
      ShapeTestFramework.TestShape(new Box(), create);
      ShapeTestFramework.TestShape(new Box(42), create);
      ShapeTestFramework.TestShape(new Box(42, 1), create);
      ShapeTestFramework.TestShape(new Box(42, new Vector3(1.2f, 2.3f, 3.4f), new Vector3(0.7f, 1.2f, 3.0f)), create);
      ShapeTestFramework.TestShape(new Box(42, 1, new Vector3(1.2f, 2.3f, 3.4f), new Vector3(0.7f, 1.2f, 3.0f)), create);
      ShapeTestFramework.TestShape(new Box(42, new Vector3(1.2f, 2.3f, 3.4f), new Vector3(0.7f, 1.2f, 3.0f), new Quaternion(Vector3.One.Normalised, 18.0f * (float)Math.PI / 180.0f)), create);
      ShapeTestFramework.TestShape(new Box(42, 1, new Vector3(1.2f, 2.3f, 3.4f), new Vector3(0.7f, 1.2f, 3.0f), new Quaternion(Vector3.One.Normalised, 18.0f * (float)Math.PI / 180.0f)), create);
    }

    [TestCase]
    public void CapsuleTest()
    {
      ShapeTestFramework.CreateShapeFunction create = () => { return new Capsule(); };
      ShapeTestFramework.TestShape(new Capsule(), create);
      ShapeTestFramework.TestShape(new Capsule(42), create);
      ShapeTestFramework.TestShape(new Capsule(42, 1), create);
      ShapeTestFramework.TestShape(new Capsule(42, new Vector3(1.2f, 2.3f, 3.4f), new Vector3(1, 1, 1).Normalised, 2.2f, 0.25f), create);
      ShapeTestFramework.TestShape(new Capsule(42, 1, new Vector3(1.2f, 2.3f, 3.4f), new Vector3(1, 1, 1).Normalised, 2.2f, 0.25f), create);
      ShapeTestFramework.TestShape(new Capsule(42, new Vector3(-1.0f, -0.5f, -0.25f), new Vector3(0.25f, 0.25f, 1.0f).Normalised, 0.15f), create);
      ShapeTestFramework.TestShape(new Capsule(42, 1, new Vector3(-1.0f, -0.5f, -0.25f), new Vector3(0.25f, 0.25f, 1.0f).Normalised, 0.15f), create);
    }

    [TestCase]
    public void CylinderTest()
    {
      ShapeTestFramework.CreateShapeFunction create = () => { return new Cylinder(); };
      ShapeTestFramework.TestShape(new Cylinder(), create);
      ShapeTestFramework.TestShape(new Cylinder(42), create);
      ShapeTestFramework.TestShape(new Cylinder(42, 1), create);
      ShapeTestFramework.TestShape(new Cylinder(42, new Vector3(1.2f, 2.3f, 3.4f), new Vector3(1, 1, 1).Normalised, 2.2f, 0.25f), create);
      ShapeTestFramework.TestShape(new Cylinder(42, 1, new Vector3(1.2f, 2.3f, 3.4f), new Vector3(1, 1, 1).Normalised, 2.2f, 0.25f), create);
      ShapeTestFramework.TestShape(new Cylinder(42, new Vector3(-1.0f, -0.5f, -0.25f), new Vector3(0.25f, 0.25f, 1.0f).Normalised, 0.15f), create);
      ShapeTestFramework.TestShape(new Cylinder(42, 1, new Vector3(-1.0f, -0.5f, -0.25f), new Vector3(0.25f, 0.25f, 1.0f).Normalised, 0.15f), create);
    }

    [TestCase]
    public void ConeTest()
    {
      ShapeTestFramework.CreateShapeFunction create = () => { return new Cone(); };
      ShapeTestFramework.TestShape(new Cone(), create);
      ShapeTestFramework.TestShape(new Cone(42), create);
      ShapeTestFramework.TestShape(new Cone(42, 1), create);
      ShapeTestFramework.TestShape(new Cone(42, new Vector3(1.2f, 2.3f, 3.4f), new Vector3(1, 1, 1).Normalised, 2.0f, 12.0f * (float)Math.PI / 180.0f), create);
      ShapeTestFramework.TestShape(new Cone(42, 1, new Vector3(1.2f, 2.3f, 3.4f), new Vector3(1, 1, 1).Normalised, 2.0f, 12.0f * (float)Math.PI / 180.0f), create);
      ShapeTestFramework.TestShape(new Cone(42, new Vector3(1.2f, 2.3f, 3.4f), new Vector3(0, 0, 0), 1.05f), create);
      ShapeTestFramework.TestShape(new Cone(42, 1, new Vector3(1.2f, 2.3f, 3.4f), new Vector3(0, 0, 0), 1.05f), create);
    }

    [TestCase]
    public void PlaneTest()
    {
      ShapeTestFramework.CreateShapeFunction create = () => { return new Plane(); };
      ShapeTestFramework.TestShape(new Plane(), create);
      ShapeTestFramework.TestShape(new Plane(42), create);
      ShapeTestFramework.TestShape(new Plane(42, 1), create);
      ShapeTestFramework.TestShape(new Plane(42, new Vector3(1.2f, 2.3f, 3.4f), new Vector3(0.7f, 1.2f, 3.0f), 3.2f, 0.75f), create);
      ShapeTestFramework.TestShape(new Plane(42, 1, new Vector3(1.2f, 2.3f, 3.4f), new Vector3(0.7f, 1.2f, 3.0f), 3.2f, 0.75f), create);
      ShapeTestFramework.TestShape(new Plane(42, 1, new Vector3(1.2f, 2.3f, 3.4f), new Vector3(0.7f, 1.2f, 3.0f)), create);
    }

    [TestCase]
    public void SphereTest()
    {
      ShapeTestFramework.CreateShapeFunction create = () => { return new Sphere(); };
      ShapeTestFramework.TestShape(new Sphere(), create);
      ShapeTestFramework.TestShape(new Sphere(42), create);
      ShapeTestFramework.TestShape(new Sphere(42, 1), create);
      ShapeTestFramework.TestShape(new Sphere(42, new Vector3(1.2f, 2.3f, 3.4f), 0.75f), create);
      ShapeTestFramework.TestShape(new Sphere(42, 1, new Vector3(1.2f, 2.3f, 3.4f), 0.75f), create);
    }

    [TestCase]
    public void StarTest()
    {
      ShapeTestFramework.CreateShapeFunction create = () => { return new Star(); };
      ShapeTestFramework.TestShape(new Star(), create);
      ShapeTestFramework.TestShape(new Star(42), create);
      ShapeTestFramework.TestShape(new Star(42, 1), create);
      ShapeTestFramework.TestShape(new Star(42, new Vector3(1.2f, 2.3f, 3.4f), 0.75f), create);
      ShapeTestFramework.TestShape(new Star(42, 1, new Vector3(1.2f, 2.3f, 3.4f), 0.75f), create);
    }

    [TestCase]
    public void Text2DTest()
    {
      ShapeTestFramework.CreateShapeFunction create = () => { return new Text2D(); };
      ShapeTestFramework.TestShape(new Text2D(), create);
      ShapeTestFramework.TestShape(new Text2D("Seven and a half..."), create, ValidateText2D);
      ShapeTestFramework.TestShape(new Text2D("The answer is...", 42), create, ValidateText2D);
      ShapeTestFramework.TestShape(new Text2D("The answer is...", 42, 1), create, ValidateText2D);
      ShapeTestFramework.TestShape(new Text2D("Seven and a half...", new Vector3(1.2f, 2.3f, 3.4f)) { InWorldSpace = true }, create, ValidateText2D);
      ShapeTestFramework.TestShape(new Text2D("The answer is...", 42, new Vector3(1.2f, 2.3f, 3.4f)) { InWorldSpace = false }, create, ValidateText2D);
      ShapeTestFramework.TestShape(new Text2D("The answer is...", 42, 1, new Vector3(1.2f, 2.3f, 3.4f)), create, ValidateText2D);
    }

    [TestCase]
    public void Text3DTest()
    {
      ShapeTestFramework.CreateShapeFunction create = () => { return new Text3D(); };
      ShapeTestFramework.TestShape(new Text3D(), create, ValidateText3D);
      ShapeTestFramework.TestShape(new Text3D("Seven and a half..."), create, ValidateText3D);
      ShapeTestFramework.TestShape(new Text3D("The answer is...", 42), create, ValidateText3D);
      ShapeTestFramework.TestShape(new Text3D("The answer is...", 42, 1), create, ValidateText3D);
      ShapeTestFramework.TestShape(new Text3D("Seven and a half...", new Vector3(1.2f, 2.3f, 3.4f)) { ScreenFacing = true }, create, ValidateText3D);
      ShapeTestFramework.TestShape(new Text3D("The answer is...", 42, new Vector3(1.2f, 2.3f, 3.4f)) { ScreenFacing = false }, create, ValidateText3D);
      ShapeTestFramework.TestShape(new Text3D("The answer is...", 42, 1, new Vector3(1.2f, 2.3f, 3.4f)) { Facing = Vector3.One.Normalised }, create, ValidateText3D);
    }
  }
}
