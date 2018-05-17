//
// author Kazys Stepanas
//
using NUnit.Framework;
using System;
using System.Collections.Generic;
using Tes.Maths;
using Tes.Shapes;
using Tes.TestSupport;

namespace Tes.CoreTests
{
  [TestFixture()]
  public class Shapes
  {
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

    public static void ValidateText2D(Shape shapeArg, Shape referenceArg, Dictionary<ulong, Resource> resources)
    {
      ShapeTestFramework.ValidateShape(shapeArg, referenceArg, resources);
      Text2D shape = (Text2D)shapeArg;
      Text2D reference = (Text2D)referenceArg;
      Assert.AreEqual(reference.Text, shape.Text);
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
    public static void ValidateText3D(Shape shapeArg, Shape referenceArg, Dictionary<ulong, Resource> resources)
    {
      ShapeTestFramework.ValidateShape(shapeArg, referenceArg, resources);
      Text3D shape = (Text3D)shapeArg;
      Text3D reference = (Text3D)referenceArg;
      Assert.AreEqual(reference.Text, shape.Text);
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

    public static void ValidateMeshShape(Shape shapeArg, Shape referenceArg, Dictionary<ulong, Resource> resources)
    {
      ShapeTestFramework.ValidateShape(shapeArg, referenceArg, resources);
      MeshShape shape = (MeshShape)shapeArg;
      MeshShape reference = (MeshShape)referenceArg;

      Assert.AreEqual(shape.DrawType, reference.DrawType);
      if (reference.Vertices != null)
      {
        Assert.NotNull(shape.Vertices);
        Assert.AreEqual(reference.Vertices.Length, shape.Vertices.Length);
        bool verticesMatch = true;
        for (int i = 0; i < shape.Vertices.Length; ++i)
        {
          if (reference.Vertices[i] != shape.Vertices[i])
          {
            verticesMatch = false;
            TestContext.WriteLine("vertex mismatch [{0}] : ({1},{2},{3}) != ({4},{5},{6})",
                                  i, reference.Vertices[i].X, reference.Normals[i].Y, reference.Normals[i].Z,
                                  shape.Normals[i].X, shape.Normals[i].Y, shape.Normals[i].Z);
          }
        }

        Assert.IsTrue(verticesMatch);
      }

      if (reference.Normals != null)
      {
        Assert.NotNull(shape.Normals);
        Assert.AreEqual(reference.Normals.Length, shape.Normals.Length);
        bool normalsMatch = true;
        for (int i = 0; i < shape.Normals.Length; ++i)
        {
          if (reference.Normals[i].X != shape.Normals[i].X ||
              reference.Normals[i].Y != shape.Normals[i].Y ||
              reference.Normals[i].Z != shape.Normals[i].Z)
          {
            normalsMatch = false;
            TestContext.WriteLine("normal mismatch [{0}] : ({1},{2},{3}) != ({4},{5},{6})",
                                  i, reference.Normals[i].X, reference.Normals[i].Y, reference.Normals[i].Z,
                                  shape.Normals[i].X, shape.Normals[i].Y, shape.Normals[i].Z);
          }
        }

        Assert.IsTrue(normalsMatch);
      }

      if (reference.Colours != null)
      {
        Assert.NotNull(shape.Colours);
        Assert.AreEqual(reference.Colours.Length, shape.Colours.Length);
        bool coloursMatch = true;
        for (int i = 0; i < shape.Colours.Length; ++i)
        {
          if (reference.Colours[i] != shape.Colours[i])
          {
            TestContext.WriteLine("colour mismatch [{0}] : 0x{1} != 0x{2}",
                                  i, reference.Colours[i].ToString("x"), shape.Colours[i].ToString("x"));
            coloursMatch = false;
          }
        }

        Assert.IsTrue(coloursMatch);
      }

      if (reference.Indices != null)
      {
        Assert.NotNull(shape.Indices);
        Assert.AreEqual(reference.Indices.Length, shape.Indices.Length);
        bool indicesMatch = true;
        for (int i = 0; i < shape.Indices.Length; ++i)
        {
          if (reference.Indices[i] != shape.Indices[i])
          {
            TestContext.WriteLine("index mismatch [{0}] : {1} != {2}",
                                  i, reference.Indices[i], shape.Indices[i]);
            indicesMatch = false;
          }
        }

        Assert.IsTrue(indicesMatch);
      }
    }

    [TestCase]
    public void TestMeshShape()
    {
      List<Vector3> vertices = new List<Vector3>();
      List<int> indices = new List<int>();
      List<Vector3> normals = new List<Vector3>();
      uint[] colours;
      Common.MakeHiResSphere(vertices, indices, normals);

      colours = new uint[vertices.Count];
      for (int i = 0; i < colours.Length; ++i)
      {
        colours[i] = Colour.Cycle(i).Value;
      }

      ShapeTestFramework.CreateShapeFunction create = () => { return new MeshShape(); };

      // Validate all constructors, though not necessarily all default argument settings.
      ShapeTestFramework.TestShape(new MeshShape(Net.MeshDrawType.Points, vertices.ToArray()), create, ValidateMeshShape);
      ShapeTestFramework.TestShape(new MeshShape(Net.MeshDrawType.Points, vertices.ToArray(), new Vector3(1.2f, 2.3f, 3.4f)), create, ValidateMeshShape);
      ShapeTestFramework.TestShape(new MeshShape(Net.MeshDrawType.Points, vertices.ToArray(), new Vector3(1.2f, 2.3f, 3.4f), new Quaternion(Vector3.One.Normalised, 15.0f * (float)Math.PI / 180.0f)), create, ValidateMeshShape);
      ShapeTestFramework.TestShape(new MeshShape(Net.MeshDrawType.Points, vertices.ToArray(), new Vector3(1.2f, 2.3f, 3.4f), new Quaternion(Vector3.One.Normalised, 15.0f * (float)Math.PI / 180.0f), new Vector3(1, 2, 3)), create, ValidateMeshShape);
      ShapeTestFramework.TestShape(new MeshShape(Net.MeshDrawType.Points, vertices.ToArray(), 42, 1), create, ValidateMeshShape);
      ShapeTestFramework.TestShape(new MeshShape(Net.MeshDrawType.Points, vertices.ToArray(), 41, new Vector3(1.2f, 2.3f, 3.4f)), create, ValidateMeshShape);
      ShapeTestFramework.TestShape(new MeshShape(Net.MeshDrawType.Points, vertices.ToArray(), 42, new Vector3(1.2f, 2.3f, 3.4f), new Quaternion(Vector3.One.Normalised, 15.0f * (float)Math.PI / 180.0f), new Vector3(1, 2, 3)), create, ValidateMeshShape);
      ShapeTestFramework.TestShape(new MeshShape(Net.MeshDrawType.Points, vertices.ToArray(), 41, 1, new Vector3(1.2f, 2.3f, 3.4f)), create, ValidateMeshShape);
      ShapeTestFramework.TestShape(new MeshShape(Net.MeshDrawType.Points, vertices.ToArray(), 42, 1, new Vector3(1.2f, 2.3f, 3.4f), new Quaternion(Vector3.One.Normalised, 15.0f * (float)Math.PI / 180.0f), new Vector3(1, 2, 3)), create, ValidateMeshShape);

      ShapeTestFramework.TestShape(new MeshShape(Net.MeshDrawType.Triangles, vertices.ToArray(), indices.ToArray()), create, ValidateMeshShape);
      ShapeTestFramework.TestShape(new MeshShape(Net.MeshDrawType.Triangles, vertices.ToArray(), indices.ToArray(), new Vector3(1.2f, 2.3f, 3.4f)), create, ValidateMeshShape);
      ShapeTestFramework.TestShape(new MeshShape(Net.MeshDrawType.Triangles, vertices.ToArray(), indices.ToArray(), new Vector3(1.2f, 2.3f, 3.4f), new Quaternion(Vector3.One.Normalised, 15.0f * (float)Math.PI / 180.0f)), create, ValidateMeshShape);
      ShapeTestFramework.TestShape(new MeshShape(Net.MeshDrawType.Triangles, vertices.ToArray(), indices.ToArray(), new Vector3(1.2f, 2.3f, 3.4f), new Quaternion(Vector3.One.Normalised, 15.0f * (float)Math.PI / 180.0f), new Vector3(1, 2, 3)), create, ValidateMeshShape);
      ShapeTestFramework.TestShape(new MeshShape(Net.MeshDrawType.Triangles, vertices.ToArray(), indices.ToArray(), 42, 1), create, ValidateMeshShape);
      ShapeTestFramework.TestShape(new MeshShape(Net.MeshDrawType.Triangles, vertices.ToArray(), indices.ToArray(), 42, new Vector3(1.2f, 2.3f, 3.4f)), create, ValidateMeshShape);
      ShapeTestFramework.TestShape(new MeshShape(Net.MeshDrawType.Triangles, vertices.ToArray(), indices.ToArray(), 42, new Vector3(1.2f, 2.3f, 3.4f), new Quaternion(Vector3.One.Normalised, 15.0f * (float)Math.PI / 180.0f)), create, ValidateMeshShape);
      ShapeTestFramework.TestShape(new MeshShape(Net.MeshDrawType.Triangles, vertices.ToArray(), indices.ToArray(), 42, new Vector3(1.2f, 2.3f, 3.4f), new Quaternion(Vector3.One.Normalised, 15.0f * (float)Math.PI / 180.0f), new Vector3(1, 2, 3)), create, ValidateMeshShape);
      ShapeTestFramework.TestShape(new MeshShape(Net.MeshDrawType.Triangles, vertices.ToArray(), indices.ToArray(), 42, 1, new Vector3(1.2f, 2.3f, 3.4f)), create, ValidateMeshShape);
      ShapeTestFramework.TestShape(new MeshShape(Net.MeshDrawType.Triangles, vertices.ToArray(), indices.ToArray(), 42, 1, new Vector3(1.2f, 2.3f, 3.4f), new Quaternion(Vector3.One.Normalised, 15.0f * (float)Math.PI / 180.0f)), create, ValidateMeshShape);
      ShapeTestFramework.TestShape(new MeshShape(Net.MeshDrawType.Triangles, vertices.ToArray(), indices.ToArray(), 42, 1, new Vector3(1.2f, 2.3f, 3.4f), new Quaternion(Vector3.One.Normalised, 15.0f * (float)Math.PI / 180.0f), new Vector3(1, 2, 3)), create, ValidateMeshShape);

      // Validate with normals.
      ShapeTestFramework.TestShape(new MeshShape(Net.MeshDrawType.Triangles, vertices.ToArray(), indices.ToArray()) { Normals = normals.ToArray() }, create, ValidateMeshShape);
      ShapeTestFramework.TestShape(new MeshShape(Net.MeshDrawType.Voxels, vertices.ToArray()).SetUniformNormal(Vector3.One), create, ValidateMeshShape);

      // Try with colours.
      ShapeTestFramework.TestShape(new MeshShape(Net.MeshDrawType.Points, vertices.ToArray()) { Colours = colours }, create, ValidateMeshShape);

      // And one with the lot.
      ShapeTestFramework.TestShape(new MeshShape(Net.MeshDrawType.Triangles, vertices.ToArray(), indices.ToArray())
      { Normals = normals.ToArray(), Colours = colours }, create, ValidateMeshShape);
    }

    public static void ValidateMesh(MeshResource mesh, MeshResource reference)
    {
      Assert.AreEqual(reference.ID, mesh.ID);
      Assert.AreEqual(reference.TypeID, mesh.TypeID);
      Assert.AreEqual(reference.Transform, mesh.Transform);
      Assert.AreEqual(reference.Tint, mesh.Tint);
      Assert.AreEqual(reference.DrawType, mesh.DrawType);
      Assert.AreEqual(reference.IndexSize, mesh.IndexSize);
      Assert.AreEqual(reference.VertexCount(), mesh.VertexCount());
      Assert.AreEqual(reference.IndexCount(), mesh.IndexCount());

      if (mesh.VertexCount() > 0)
      {
        Assert.IsNotNull(reference.Vertices());
        Assert.IsNotNull(mesh.Vertices());

        Vector3 refv, meshv;
        for (int i = 0; i < mesh.VertexCount(); ++i)
        {
          refv = reference.Vertices()[i];
          meshv = mesh.Vertices()[i];
          Assert.Multiple(() =>
          {
            Assert.AreEqual(refv.X, meshv.X);
            Assert.AreEqual(refv.Y, meshv.Y);
            Assert.AreEqual(refv.Z, meshv.Z);
          });
        }
      }

      if (mesh.IndexCount() > 0)
      {
        if (reference.IndexSize == 2)
        {
          Assert.IsNotNull(mesh.Indices2());

          for (int i = 0; i < mesh.IndexCount(); ++i)
          {
            Assert.AreEqual(reference.Indices2()[i], mesh.Indices2()[i]);
          }
        }
        else
        {
          Assert.IsNotNull(reference.Indices4());
          Assert.IsNotNull(mesh.Indices4());

          for (int i = 0; i < mesh.IndexCount(); ++i)
          {
            Assert.AreEqual(reference.Indices4()[i], mesh.Indices4()[i]);
          }
        }
      }

      if (mesh.Normals() != null)
      {
        Assert.IsNotNull(mesh.Normals());

        Vector3 refn, meshn;
        for (int i = 0; i < mesh.VertexCount(); ++i)
        {
          refn = reference.Normals()[i];
          meshn = mesh.Normals()[i];
          Assert.Multiple(() =>
          {
            Assert.AreEqual(refn.X, meshn.X);
            Assert.AreEqual(refn.Y, meshn.Y);
            Assert.AreEqual(refn.Z, meshn.Z);
          });
        }
      }

      if (mesh.Colours() != null)
      {
        Assert.IsNotNull(mesh.Colours());

        for (int i = 0; i < mesh.VertexCount(); ++i)
        {
          Assert.AreEqual(reference.Colours()[i], mesh.Colours()[i]);
        }
      }

      if (mesh.UVs() != null)
      {
        Assert.IsNotNull(mesh.UVs());

        Vector2 refuv, meshuv;
        for (int i = 0; i < mesh.VertexCount(); ++i)
        {
          refuv = reference.UVs()[i];
          meshuv = mesh.UVs()[i];
          Assert.Multiple(() =>
          {
            Assert.AreEqual(refuv.X, meshuv.X);
            Assert.AreEqual(refuv.Y, meshuv.Y);
          });
        }
      }
    }

    void ValidateMeshSetShape(Shape shapeArg, Shape referenceArg, Dictionary<ulong, Resource> resources)
    {
      ShapeTestFramework.ValidateShape(shapeArg, referenceArg, resources);
      MeshSet shape = (MeshSet)shapeArg;
      MeshSet reference = (MeshSet)referenceArg;

      Assert.AreEqual(reference.PartCount, shape.PartCount);

      Net.ObjectAttributes attrRef = new Net.ObjectAttributes();
      Net.ObjectAttributes attrMesh = new Net.ObjectAttributes();
      for (int i = 0; i < shape.PartCount; ++i)
      {
        // Transforms may not be exactly equal. We need to convert to PRS components and compare approximate equality
        // there as this is what the transfer does.
        attrRef.SetFromTransform(reference.PartTransformAt(i));
        attrMesh.SetFromTransform(shape.PartTransformAt(i));

        Assert.Multiple(() =>
        {
          Assert.AreEqual(attrRef.X, attrMesh.X, 1e-3f);
          Assert.AreEqual(attrRef.Y, attrMesh.Y, 1e-3f);
          Assert.AreEqual(attrRef.Z, attrMesh.Z, 1e-3f);
        });

        Assert.Multiple(() =>
        {
          Assert.AreEqual(attrRef.RotationX, attrMesh.RotationX, 1e-3f);
          Assert.AreEqual(attrRef.RotationY, attrMesh.RotationY, 1e-3f);
          Assert.AreEqual(attrRef.RotationZ, attrMesh.RotationZ, 1e-3f);
          Assert.AreEqual(attrRef.RotationW, attrMesh.RotationW, 1e-3f);
        });

        Assert.Multiple(() =>
        {
          Assert.AreEqual(attrRef.ScaleX, attrMesh.ScaleX, 1e-3f);
          Assert.AreEqual(attrRef.ScaleY, attrMesh.ScaleY, 1e-3f);
          Assert.AreEqual(attrRef.ScaleZ, attrMesh.ScaleZ, 1e-3f);
        });

        // Shape will only have a placeholder resource. Lookup in resources.
        MeshResource mesh = (MeshResource)resources[shape.PartAt(i).UniqueKey()];
        ValidateMesh(mesh, reference.PartAt(i));
      }
    }

    [TestCase]
    public void TestMeshSet()
    {
      List<Vector3> vertices = new List<Vector3>();
      List<int> indices = new List<int>();
      List<int> wireIndices = new List<int>();
      List<Vector3> normals = new List<Vector3>();
      List<MeshResource> meshes = new List<MeshResource>();
      uint[] colours;
      Common.MakeHiResSphere(vertices, indices, normals);

      // Build per vertex colours with colour cycling.
      colours = new uint[vertices.Count];
      for (int i = 0; i < colours.Length; ++i)
      {
        colours[i] = Colour.Cycle(i).Value;
      }

      // Build a wire frame sphere (with many redundant lines).
      wireIndices.Capacity = indices.Count * 2;
      for (int i = 0; i + 2 < indices.Count; i += 3)
      {
        wireIndices.Add(indices[i + 0]);
        wireIndices.Add(indices[i + 1]);
        wireIndices.Add(indices[i + 1]);
        wireIndices.Add(indices[i + 2]);
        wireIndices.Add(indices[i + 2]);
        wireIndices.Add(indices[i + 0]);
      }

      // Create a mesh part.
      uint nextId = 1;
      SimpleMesh part;

      // Vertices and indices only.
      part = new SimpleMesh(nextId++, Net.MeshDrawType.Triangles);
      part.AddVertices(vertices);
      part.AddIndices(indices);
      meshes.Add(part);

      // Vertices, indices and colours.
      part = new SimpleMesh(nextId++, Net.MeshDrawType.Triangles);
      part.AddVertices(vertices);
      part.AddIndices(indices);
      part.AddColours(colours);
      meshes.Add(part);

      // Points and colours only (essentially a point cloud)
      part = new SimpleMesh(nextId++, Net.MeshDrawType.Points);
      part.AddVertices(vertices);
      part.AddColours(colours);
      meshes.Add(part);

      // Lines
      part = new SimpleMesh(nextId++, Net.MeshDrawType.Triangles);
      part.AddVertices(vertices);
      part.AddIndices(wireIndices);
      meshes.Add(part);

      // One with the lot.
      part = new SimpleMesh(nextId++, Net.MeshDrawType.Triangles);
      part.AddVertices(vertices);
      part.AddNormals(normals);
      part.AddColours(colours);
      part.AddIndices(indices);
      meshes.Add(part);

      ShapeTestFramework.CreateShapeFunction create = () => { return new MeshSet(); };

      // Simple test first. One part.
      ShapeTestFramework.TestShape(new MeshSet(42).AddPart(meshes[0]), create, ValidateMeshSetShape);

      // Now a multi-part MeshSet.
      MeshSet set = new MeshSet(42, 1);
      Matrix4 transform;

      for (int i = 0; i < meshes.Count; ++i)
      {
        transform = Rotation.ToMatrix4(new Quaternion(new Vector3(i, i + 1, i - 3).Normalised, (float)Math.PI * (i + 1) * 6.0f / 180.0f));
        transform.Translation = new Vector3(i, i - 3.2f, 1.5f * i);
        transform.ApplyScaling(new Vector3(0.75f, 0.75f, 0.75f));
        set.AddPart(meshes[i], transform);
      }
      ShapeTestFramework.TestShape(set, create, ValidateMeshSetShape);
    }

    public static void ValidatePointCloudShape(Shape shapeArg, Shape referenceArg, Dictionary<ulong, Resource> resources)
    {
      ShapeTestFramework.ValidateShape(shapeArg, referenceArg, resources);
      PointCloudShape shape = (PointCloudShape)shapeArg;
      PointCloudShape reference = (PointCloudShape)referenceArg;

      Assert.AreEqual(reference.PointSize, shape.PointSize);
      Assert.NotNull(reference.PointCloud);
      Assert.NotNull(shape.PointCloud);

      Assert.AreEqual(reference.PointCloud.ID, shape.PointCloud.ID);

      // Resolve the mesh resource.
      Resource resource;
      Assert.IsTrue(resources.TryGetValue(shape.PointCloud.UniqueKey(), out resource));
      // Remember, resource will be a SimpleMesh, not a PointCloud.
      MeshResource cloud = (MeshResource)resource;
      ValidateMesh(cloud, reference.PointCloud);
    }

    [TestCase]
    public void PointCloudTest()
    {
      List<Vector3> vertices = new List<Vector3>();
      List<Vector3> normals = new List<Vector3>();
      List<int> indices = new List<int>();
      uint[] colours;
      Common.MakeHiResSphere(vertices, indices, normals);

      // Build per vertex colours with colour cycling.
      colours = new uint[vertices.Count];
      for (int i = 0; i < colours.Length; ++i)
      {
        colours[i] = Colour.Cycle(i).Value;
      }

      PointCloud cloud = new PointCloud(1, vertices.Count);
      cloud.AddPoints(vertices);
      cloud.AddNormals(normals);
      cloud.AddColours(colours);

      ShapeTestFramework.CreateShapeFunction create = () => { return new PointCloudShape(); };
      ShapeTestFramework.TestShape(new PointCloudShape(cloud, 41, 1, 8), create, ValidatePointCloudShape);

      // Run a cloud with an indexed sub-set.
      uint[] indexedSubSet = new uint[vertices.Count / 2];
      for (uint i = 0; i < indexedSubSet.Length; ++i)
      {
        indexedSubSet[i] = i;
      }
      
      ShapeTestFramework.TestShape(new PointCloudShape(cloud, 41, 1, 8).SetIndices(indexedSubSet), create, ValidatePointCloudShape);
    }
  }
}
