//
// author Kazys Stepanas
//
using Xunit;
using Xunit.Abstractions;
using System;
using System.Collections.Generic;
using Tes.Maths;
using Tes.Shapes;
using Tes.TestSupport;

#pragma warning disable xUnit1013

namespace Tes.CoreTests
{
  public class Shapes
  {
    private readonly ITestOutputHelper _output;

    public Shapes(ITestOutputHelper output)
    {
      _output = output;
    }

    [Fact]
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

    [Fact]
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

    [Fact]
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

    [Fact]
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

    [Fact]
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

    [Fact]
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

    [Fact]
    public void SphereTest()
    {
      ShapeTestFramework.CreateShapeFunction create = () => { return new Sphere(); };
      ShapeTestFramework.TestShape(new Sphere(), create);
      ShapeTestFramework.TestShape(new Sphere(42), create);
      ShapeTestFramework.TestShape(new Sphere(42, 1), create);
      ShapeTestFramework.TestShape(new Sphere(42, new Vector3(1.2f, 2.3f, 3.4f), 0.75f), create);
      ShapeTestFramework.TestShape(new Sphere(42, 1, new Vector3(1.2f, 2.3f, 3.4f), 0.75f), create);
    }

    [Fact]
    public void StarTest()
    {
      ShapeTestFramework.CreateShapeFunction create = () => { return new Star(); };
      ShapeTestFramework.TestShape(new Star(), create);
      ShapeTestFramework.TestShape(new Star(42), create);
      ShapeTestFramework.TestShape(new Star(42, 1), create);
      ShapeTestFramework.TestShape(new Star(42, new Vector3(1.2f, 2.3f, 3.4f), 0.75f), create);
      ShapeTestFramework.TestShape(new Star(42, 1, new Vector3(1.2f, 2.3f, 3.4f), 0.75f), create);
    }

    public void ValidateText2D(Shape shapeArg, Shape referenceArg, Dictionary<ulong, Resource> resources)
    {
      ShapeTestFramework.ValidateShape(shapeArg, referenceArg, resources);
      Text2D shape = (Text2D)shapeArg;
      Text2D reference = (Text2D)referenceArg;
      Assert.Equal(reference.Text, shape.Text);
    }

    [Fact]
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
    public void ValidateText3D(Shape shapeArg, Shape referenceArg, Dictionary<ulong, Resource> resources)
    {
      ShapeTestFramework.ValidateShape(shapeArg, referenceArg, resources);
      Text3D shape = (Text3D)shapeArg;
      Text3D reference = (Text3D)referenceArg;
      Assert.Equal(reference.Text, shape.Text);
    }

    [Fact]
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

    public void ValidateMeshShape(Shape shapeArg, Shape referenceArg, Dictionary<ulong, Resource> resources)
    {
      ShapeTestFramework.ValidateShape(shapeArg, referenceArg, resources);
      MeshShape shape = (MeshShape)shapeArg;
      MeshShape reference = (MeshShape)referenceArg;

      Assert.Equal(shape.DrawType, reference.DrawType);
      if (reference.Vertices != null)
      {
        Assert.NotNull(shape.Vertices);
        Assert.Equal(reference.Vertices.Length, shape.Vertices.Length);
        bool verticesMatch = true;
        for (int i = 0; i < shape.Vertices.Length; ++i)
        {
          if (reference.Vertices[i] != shape.Vertices[i])
          {
            verticesMatch = false;
            _output.WriteLine("vertex mismatch [{0}] : ({1},{2},{3}) != ({4},{5},{6})",
                                  i, reference.Vertices[i].X, reference.Normals[i].Y, reference.Normals[i].Z,
                                  shape.Normals[i].X, shape.Normals[i].Y, shape.Normals[i].Z);
          }
        }

        Assert.True(verticesMatch);
      }

      if (reference.Normals != null)
      {
        Assert.NotNull(shape.Normals);
        Assert.Equal(reference.Normals.Length, shape.Normals.Length);
        bool normalsMatch = true;
        for (int i = 0; i < shape.Normals.Length; ++i)
        {
          if (reference.Normals[i].X != shape.Normals[i].X ||
              reference.Normals[i].Y != shape.Normals[i].Y ||
              reference.Normals[i].Z != shape.Normals[i].Z)
          {
            normalsMatch = false;
            _output.WriteLine("normal mismatch [{0}] : ({1},{2},{3}) != ({4},{5},{6})",
                                  i, reference.Normals[i].X, reference.Normals[i].Y, reference.Normals[i].Z,
                                  shape.Normals[i].X, shape.Normals[i].Y, shape.Normals[i].Z);
          }
        }

        Assert.True(normalsMatch);
      }

      if (reference.Colours != null)
      {
        Assert.NotNull(shape.Colours);
        Assert.Equal(reference.Colours.Length, shape.Colours.Length);
        bool coloursMatch = true;
        for (int i = 0; i < shape.Colours.Length; ++i)
        {
          if (reference.Colours[i] != shape.Colours[i])
          {
            _output.WriteLine("colour mismatch [{0}] : 0x{1} != 0x{2}",
                                  i, reference.Colours[i].ToString("x"), shape.Colours[i].ToString("x"));
            coloursMatch = false;
          }
        }

        Assert.True(coloursMatch);
      }

      if (reference.Indices != null)
      {
        Assert.NotNull(shape.Indices);
        Assert.Equal(reference.Indices.Length, shape.Indices.Length);
        bool indicesMatch = true;
        for (int i = 0; i < shape.Indices.Length; ++i)
        {
          if (reference.Indices[i] != shape.Indices[i])
          {
            _output.WriteLine("index mismatch [{0}] : {1} != {2}",
                                  i, reference.Indices[i], shape.Indices[i]);
            indicesMatch = false;
          }
        }

        Assert.True(indicesMatch);
      }
    }

    [Fact]
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

    public void ValidateMesh(MeshResource mesh, MeshResource reference)
    {
      Assert.Equal(reference.ID, mesh.ID);
      Assert.Equal(reference.TypeID, mesh.TypeID);
      Assert.Equal(reference.Transform, mesh.Transform);
      Assert.Equal(reference.Tint, mesh.Tint);
      Assert.Equal(reference.DrawType, mesh.DrawType);
      Assert.Equal(reference.IndexSize, mesh.IndexSize);
      Assert.Equal(reference.VertexCount(), mesh.VertexCount());
      Assert.Equal(reference.IndexCount(), mesh.IndexCount());

      if (mesh.VertexCount() > 0)
      {
        Assert.NotNull(reference.Vertices());
        Assert.NotNull(mesh.Vertices());

        Vector3 refv, meshv;
        for (int i = 0; i < mesh.VertexCount(); ++i)
        {
          refv = reference.Vertices()[i];
          meshv = mesh.Vertices()[i];
          Assert.Equal(refv.X, meshv.X);
          Assert.Equal(refv.Y, meshv.Y);
          Assert.Equal(refv.Z, meshv.Z);
        }
      }

      if (mesh.IndexCount() > 0)
      {
        if (reference.IndexSize == 2)
        {
          Assert.NotNull(mesh.Indices2());

          for (int i = 0; i < mesh.IndexCount(); ++i)
          {
            Assert.Equal(reference.Indices2()[i], mesh.Indices2()[i]);
          }
        }
        else
        {
          Assert.NotNull(reference.Indices4());
          Assert.NotNull(mesh.Indices4());

          for (int i = 0; i < mesh.IndexCount(); ++i)
          {
            Assert.Equal(reference.Indices4()[i], mesh.Indices4()[i]);
          }
        }
      }

      if (mesh.Normals() != null)
      {
        Assert.NotNull(mesh.Normals());

        Vector3 refn, meshn;
        for (int i = 0; i < mesh.VertexCount(); ++i)
        {
          refn = reference.Normals()[i];
          meshn = mesh.Normals()[i];
          Assert.Equal(refn.X, meshn.X);
          Assert.Equal(refn.Y, meshn.Y);
          Assert.Equal(refn.Z, meshn.Z);
        }
      }

      if (mesh.Colours() != null)
      {
        Assert.NotNull(mesh.Colours());

        for (int i = 0; i < mesh.VertexCount(); ++i)
        {
          Assert.Equal(reference.Colours()[i], mesh.Colours()[i]);
        }
      }

      if (mesh.UVs() != null)
      {
        Assert.NotNull(mesh.UVs());

        Vector2 refuv, meshuv;
        for (int i = 0; i < mesh.VertexCount(); ++i)
        {
          refuv = reference.UVs()[i];
          meshuv = mesh.UVs()[i];
          Assert.Equal(refuv.X, meshuv.X);
          Assert.Equal(refuv.Y, meshuv.Y);
        }
      }
    }

    void ValidateMeshSetShape(Shape shapeArg, Shape referenceArg, Dictionary<ulong, Resource> resources)
    {
      ShapeTestFramework.ValidateShape(shapeArg, referenceArg, resources);
      MeshSet shape = (MeshSet)shapeArg;
      MeshSet reference = (MeshSet)referenceArg;

      Assert.Equal(reference.PartCount, shape.PartCount);

      Net.ObjectAttributes attrRef = new Net.ObjectAttributes();
      Net.ObjectAttributes attrMesh = new Net.ObjectAttributes();
      for (int i = 0; i < shape.PartCount; ++i)
      {
        // Transforms may not be exactly equal. We need to convert to PRS components and compare approximate equality
        // there as this is what the transfer does.
        attrRef.SetFromTransform(reference.PartTransformAt(i));
        attrMesh.SetFromTransform(shape.PartTransformAt(i));

        attrRef.Colour = reference.PartColourAt(i).Value;
        attrMesh.Colour = shape.PartColourAt(i).Value;

        AssertExt.Near(attrRef.X, attrMesh.X, 1e-3f);
        AssertExt.Near(attrRef.Y, attrMesh.Y, 1e-3f);
        AssertExt.Near(attrRef.Z, attrMesh.Z, 1e-3f);

        AssertExt.Near(attrRef.RotationX, attrMesh.RotationX, 1e-3f);
        AssertExt.Near(attrRef.RotationY, attrMesh.RotationY, 1e-3f);
        AssertExt.Near(attrRef.RotationZ, attrMesh.RotationZ, 1e-3f);
        AssertExt.Near(attrRef.RotationW, attrMesh.RotationW, 1e-3f);

        AssertExt.Near(attrRef.ScaleX, attrMesh.ScaleX, 1e-3f);
        AssertExt.Near(attrRef.ScaleY, attrMesh.ScaleY, 1e-3f);
        AssertExt.Near(attrRef.ScaleZ, attrMesh.ScaleZ, 1e-3f);

        Assert.Equal(attrRef.Colour, attrMesh.Colour);

        // Shape will only have a placeholder resource. Lookup in resources.
        MeshResource mesh = (MeshResource)resources[shape.PartResource(i).UniqueKey()];
        ValidateMesh(mesh, reference.PartResource(i));
      }
    }

    [Fact]
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

    public void ValidatePointCloudShape(Shape shapeArg, Shape referenceArg, Dictionary<ulong, Resource> resources)
    {
      ShapeTestFramework.ValidateShape(shapeArg, referenceArg, resources);
      PointCloudShape shape = (PointCloudShape)shapeArg;
      PointCloudShape reference = (PointCloudShape)referenceArg;

      Assert.Equal(reference.PointScale, shape.PointScale);
      Assert.NotNull(reference.PointCloud);
      Assert.NotNull(shape.PointCloud);

      Assert.Equal(reference.PointCloud.ID, shape.PointCloud.ID);

      // Resolve the mesh resource.
      Resource resource;
      Assert.True(resources.TryGetValue(shape.PointCloud.UniqueKey(), out resource));
      // Remember, resource will be a SimpleMesh, not a PointCloud.
      MeshResource cloud = (MeshResource)resource;
      ValidateMesh(cloud, reference.PointCloud);
    }

    [Fact]
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

#pragma warning restore xUnit1013
