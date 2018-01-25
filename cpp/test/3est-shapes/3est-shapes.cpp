//
// author: Kazys Stepanas
//

#include <3escoordinateframe.h>
#include <3esconnectionmonitor.h>
#include <3esmaths.h>
#include <3esmessages.h>
#include <3espacketbuffer.h>
#include <3espacketreader.h>
#include <3espacketwriter.h>
#include <3estcpsocket.h>
#include <3esserver.h>
#include <3esserverutil.h>
#include <3esspheretessellator.h>
#include <shapes/3esshapes.h>

#include <3estcplistensocket.h>

#include <gtest/gtest.h>

#include <chrono>
#include <iostream>
#include <thread>
#include <vector>

namespace tes
{
  void makeHiResSphere(std::vector<Vector3f> &vertices, std::vector<unsigned> &indices, std::vector<Vector3f> *normals)
  {
    // Start with a unit sphere so we have normals precalculated.
    // Use a fine subdivision to ensure we need multiple data packets to transfer vertices.
    sphereSubdivision(vertices, indices, 1.0f, Vector3f::zero, 5);

    // Normals as vertices. Scale and offset.
    if (normals)
    {
      normals->resize(vertices.size());
      for (size_t i = 0; i < normals->size(); ++i)
      {
        (*normals)[i] = vertices[i];
      }
    }

    const float radius = 5.5f;
    const Vector3f sphereCentre(0.5f, 0, -0.25f);
    for (size_t i = 0; i < vertices.size(); ++i)
    {
      vertices[i] = sphereCentre + vertices[i] * radius;
    }
  }


  void validateShape(const Shape &shape, const Shape &reference)
  {
    EXPECT_EQ(shape.routingId(), reference.routingId());
    EXPECT_EQ(shape.isComplex(), reference.isComplex());

    EXPECT_EQ(shape.data().id, reference.data().id);
    EXPECT_EQ(shape.data().category, reference.data().category);
    EXPECT_EQ(shape.data().flags, reference.data().flags);
    EXPECT_EQ(shape.data().reserved, reference.data().reserved);

    EXPECT_EQ(shape.data().attributes.colour, reference.data().attributes.colour);

    EXPECT_EQ(shape.data().attributes.position[0], reference.data().attributes.position[0]);
    EXPECT_EQ(shape.data().attributes.position[1], reference.data().attributes.position[1]);
    EXPECT_EQ(shape.data().attributes.position[2], reference.data().attributes.position[2]);

    EXPECT_EQ(shape.data().attributes.rotation[0], reference.data().attributes.rotation[0]);
    EXPECT_EQ(shape.data().attributes.rotation[1], reference.data().attributes.rotation[1]);
    EXPECT_EQ(shape.data().attributes.rotation[2], reference.data().attributes.rotation[2]);
    EXPECT_EQ(shape.data().attributes.rotation[3], reference.data().attributes.rotation[3]);

    EXPECT_EQ(shape.data().attributes.scale[0], reference.data().attributes.scale[0]);
    EXPECT_EQ(shape.data().attributes.scale[1], reference.data().attributes.scale[1]);
    EXPECT_EQ(shape.data().attributes.scale[2], reference.data().attributes.scale[2]);
  }


  template <typename T>
  void validateText(const T &shape, const T &reference)
  {
    validateShape(static_cast<const Shape>(shape), static_cast<const Shape>(reference));
    EXPECT_EQ(shape.textLength(), reference.textLength());
    EXPECT_STREQ(shape.text(), reference.text());
  }


  void validateShape(const Text2D &shape, const Text2D &reference)
  {
    validateText(shape, reference);
  }


  void validateShape(const Text3D &shape, const Text3D &reference)
  {
    validateText(shape, reference);
  }


  void validateShape(const MeshShape &shape, const MeshShape &reference)
  {
    validateShape(static_cast<const Shape>(shape), static_cast<const Shape>(reference));

    EXPECT_EQ(shape.drawType(), reference.drawType());
    EXPECT_EQ(shape.vertexCount(), reference.vertexCount());
    EXPECT_EQ(shape.vertexStride(), reference.vertexStride());
    EXPECT_EQ(shape.normalsCount(), reference.normalsCount());
    EXPECT_EQ(shape.normalsStride(), reference.normalsStride());
    EXPECT_EQ(shape.indexCount(), reference.indexCount());

    // Validate vertices.
    Vector3f v, r;
    if (shape.vertexCount() == reference.vertexCount())
    {
      for (unsigned i = 0; i < shape.vertexCount(); ++i)
      {
        v = Vector3f(shape.vertices() + i * shape.vertexStride());
        r = Vector3f(reference.vertices() + i * reference.vertexStride());

        if (v != r)
        {
          std::cerr << "Vertex mismatch at " << i << '\n';
          EXPECT_EQ(v, r);
        }
      }
    }

    if (shape.indexCount() == reference.indexCount())
    {
      unsigned is, ir;
      for (unsigned i = 0; i < shape.indexCount(); ++i)
      {
        is = shape.indices()[i];
        ir = reference.indices()[i];

        if (is != ir)
        {
          std::cerr << "Index mismatch at " << i << '\n';
          EXPECT_EQ(is, ir);
        }
      }
    }

    if (shape.normalsCount() == reference.normalsCount())
    {
      for (unsigned i = 0; i < shape.normalsCount(); ++i)
      {
        v = Vector3f(shape.normals() + i * shape.normalsStride());
        r = Vector3f(reference.normals() + i * reference.normalsStride());

        if (v != r)
        {
          std::cerr << "Normal mismatch at " << i << '\n';
          EXPECT_EQ(v, r);
        }
      }
    }
  }


  template <class T>
  void validateClient(TcpSocket &socket, const T &shape, const ServerInfoMessage &serverInfo,
                      unsigned timeoutSec = 10)
  {
    typedef std::chrono::steady_clock Clock;
    ServerInfoMessage readServerInfo;
    std::vector<uint8_t> readBuffer(0xffffu);
    PacketBuffer packetBuffer;
    T readShape;
    auto startTime = Clock::now();
    int readCount = 0;
    bool endMsgReceived = false;
    bool serverInfoRead = false;
    bool shapeMsgRead = false;

    memset(&readServerInfo, 0, sizeof(readServerInfo));

    // Keep looping until we get a CIdEnd ControlMessage or timeoutSec elapses.
    while (!endMsgReceived && std::chrono::duration_cast<std::chrono::seconds>(Clock::now() - startTime).count() < timeoutSec)
    {
      readCount = socket.readAvailable(readBuffer.data(), readBuffer.size());
      // Assert no read errors.
      ASSERT_TRUE(readCount >= 0);
      if (readCount < 0)
      {
        break;
      }

      if  (readCount == 0)
      {
        // Nothing read. Wait.
        std::this_thread::yield();
        continue;
      }

      packetBuffer.addBytes(readBuffer.data(), readCount);

      while (PacketHeader *packetHeader = packetBuffer.extractPacket())
      {
        if (packetHeader)
        {
          PacketReader reader(*packetHeader);

          EXPECT_EQ(reader.marker(), PacketMarker);
          EXPECT_EQ(reader.versionMajor(), PacketVersionMajor);
          EXPECT_EQ(reader.versionMinor(), PacketVersionMinor);

          switch (reader.routingId())
          {
          case MtServerInfo:
            serverInfoRead = true;
            readServerInfo.read(reader);

            // Validate server info.
            EXPECT_EQ(readServerInfo.timeUnit, serverInfo.timeUnit);
            EXPECT_EQ(readServerInfo.defaultFrameTime, serverInfo.defaultFrameTime);
            EXPECT_EQ(readServerInfo.coordinateFrame, serverInfo.coordinateFrame);

            for (int i = 0; i < sizeof(readServerInfo.reserved) / sizeof(readServerInfo.reserved[0]); ++i)
            {
              EXPECT_EQ(readServerInfo.reserved[i], serverInfo.reserved[i]);
            }
            break;

          case MtControl:
          {
            // Only interested in the CIdEnd message to mark the end of the stream.
            ControlMessage msg;
            ASSERT_TRUE(msg.read(reader));

            if (reader.messageId() == CIdEnd)
            {
              endMsgReceived = true;
            }
            break;
          }

          default:
            if (reader.routingId() == shape.routingId())
            {
              // Shape message the shape.
              uint32_t shapeId = 0;
              shapeMsgRead = true;

              // Peek the shape ID.
              reader.peek((uint8_t *)&shapeId, sizeof(shapeId));

              EXPECT_EQ(shapeId, shape.id());

              switch (reader.messageId())
              {
              case OIdCreate:
                EXPECT_TRUE(readShape.readCreate(reader));
                break;

              case OIdUpdate:
                EXPECT_TRUE(readShape.readUpdate(reader));
                break;

              case OIdData:
                EXPECT_TRUE(readShape.readData(reader));
                break;
              }
            }
            break;
          }

          packetBuffer.releasePacket(packetHeader);
        }
      }
      // else fail?
    }

    EXPECT_GT(readCount, -1);
    EXPECT_TRUE(serverInfoRead);
    EXPECT_TRUE(shapeMsgRead);
    EXPECT_TRUE(endMsgReceived);

    // Validate the shape state.
    if (shapeMsgRead)
    {
      validateShape(readShape, shape);
    }
  }


  template <class T>
  void testShape(const T &shape)
  {
    // Initialise server.
    ServerInfoMessage info;
    initDefaultServerInfo(&info);
    info.coordinateFrame = XYZ;
    // Collation/compression not supported yet.
    unsigned serverFlags = 0;//SF_Collate | SF_Compress;
    // if (haveOption("compress", argc, argv))
    // {
    //   serverFlags |= SF_Compress;
    // }
    ServerSettings serverSettings(serverFlags);
    serverSettings.portRange = 1000;
    Server *server = Server::create(serverSettings, &info);

    // std::cout << "Start on port " << serverSettings.listenPort << std::endl;
    ASSERT_TRUE(server->connectionMonitor()->start(tes::ConnectionMonitor::Asynchronous));
    // std::cout << "Server listening on port " << server->connectionMonitor()->port() << std::endl;;

    // Create client and connect.
    TcpSocket client;
    client.open("127.0.0.1", server->connectionMonitor()->port());

    // Wait for connection.
    if (server->connectionMonitor()->waitForConnection(5000U) > 0)
    {
      server->connectionMonitor()->commitConnections();
    }

    EXPECT_GT(server->connectionCount(), 0);
    EXPECT_TRUE(client.isConnected());

    // Send server messages from another thread. Otherwise large packets may block.
    std::thread sendThread([server, &shape] ()
    {
      server->create(shape);
      server->updateTransfers(0);
      server->updateFrame(0.0f, true);

      // Send end message.
      ControlMessage ctrlMsg;
      memset(&ctrlMsg, 0, sizeof(ctrlMsg));
      sendMessage(*server, MtControl, CIdEnd, ctrlMsg);
    });

    // Process client messages.
    validateClient(client, shape, info);

    client.close();

    sendThread.join();
    server->close();

    server->connectionMonitor()->stop();
    server->connectionMonitor()->join();

    server->dispose();
    server = nullptr;
  }

  TEST(Shapes, Arrow)
  {
    testShape(Arrow(42, Vector3f(1.2f, 2.3f, 3.4f), Vector3f(1, 1, 1).normalised(), 2.0f, 0.05f));
    testShape(Arrow(42, 1, Vector3f(1.2f, 2.3f, 3.4f), Vector3f(1, 1, 1).normalised(), 2.0f, 0.05f));
  }

  TEST(Shapes, Box)
  {
    testShape(Box(42, Vector3f(1.2f, 2.3f, 3.4f), Vector3f(1, 3, 2), Quaternionf().setAxisAngle(Vector3f(1, 1, 1).normalised(), degToRad(18.0f))));
    testShape(Box(42, 1, Vector3f(1.2f, 2.3f, 3.4f), Vector3f(1, 3, 2), Quaternionf().setAxisAngle(Vector3f(1, 1, 1).normalised(), degToRad(18.0f))));
  }

  TEST(Shapes, Capsule)
  {
    testShape(Capsule(42, Vector3f(1.2f, 2.3f, 3.4f), Vector3f(1, 1, 1).normalised(), 0.3f, 2.05f));
    testShape(Capsule(42, 1, Vector3f(1.2f, 2.3f, 3.4f), Vector3f(1, 1, 1).normalised(), 0.3f, 2.05f));
  }

  TEST(Shapes, Cone)
  {
    testShape(Cone(42, Vector3f(1.2f, 2.3f, 3.4f), Vector3f(1, 1, 1).normalised(), degToRad(35.0f), 3.0f));
    testShape(Cone(42, 1, Vector3f(1.2f, 2.3f, 3.4f), Vector3f(1, 1, 1).normalised(), degToRad(35.0f), 3.0f));
  }

  TEST(Shapes, Cylinder)
  {
    testShape(Cylinder(42, Vector3f(1.2f, 2.3f, 3.4f), Vector3f(1, 1, 1).normalised(), 0.25f, 1.05f));
    testShape(Cylinder(42, 1, Vector3f(1.2f, 2.3f, 3.4f), Vector3f(1, 1, 1).normalised(), 0.25f, 1.05f));
  }

  // TEST(Shapes, MeshSet)
  // {
  //   testShape(MeshSet(42, Vector3f(1.2f, 2.3f, 3.4f), Vector3f(1, 1, 1).normalised(), 2.0f, 0.05f));
  // }

  TEST(Shapes, Mesh)
  {
    std::vector<Vector3f> vertices;
    std::vector<unsigned> indices;
    std::vector<Vector3f> normals;
    makeHiResSphere(vertices, indices, &normals);

    // I> Test each constructor.
    // 1. drawType, verts, vcount, vstrideBytes, pos, rot, scale
    testShape(MeshShape(DtPoints, vertices.data()->v, unsigned(vertices.size()), sizeof(*vertices.data()),
              Vector3f(1.2f, 2.3f, 3.4f), Quaternionf().setAxisAngle(Vector3f(1, 1, 1), degToRad(18)),
              Vector3f(1.0f, 1.2f, 0.8f)));
    // 2. drawType, verts, vcount, vstrideBytes, indices, icount, pos, rot, scale
    testShape(MeshShape(DtTriangles, vertices.data()->v, unsigned(vertices.size()), sizeof(*vertices.data()),
              indices.data(), unsigned(indices.size()),
              Vector3f(1.2f, 2.3f, 3.4f), Quaternionf().setAxisAngle(Vector3f(1, 1, 1), degToRad(18)),
              Vector3f(1.0f, 1.2f, 0.8f)));
    // 3. drawType, verts, vcount, vstrideBytes, id, pos, rot, scale
    testShape(MeshShape(DtPoints, vertices.data()->v, unsigned(vertices.size()), sizeof(*vertices.data()),
              42,
              Vector3f(1.2f, 2.3f, 3.4f), Quaternionf().setAxisAngle(Vector3f(1, 1, 1), degToRad(18)),
              Vector3f(1.0f, 1.2f, 0.8f)));
    // 4. drawType, verts, vcount, vstrideBytes, indices, icount, id, pos, rot, scale
    testShape(MeshShape(DtTriangles, vertices.data()->v, unsigned(vertices.size()), sizeof(*vertices.data()),
              indices.data(), unsigned(indices.size()),
              42,
              Vector3f(1.2f, 2.3f, 3.4f), Quaternionf().setAxisAngle(Vector3f(1, 1, 1), degToRad(18)),
              Vector3f(1.0f, 1.2f, 0.8f)));
    // 5. drawType, verts, vcount, vstrideBytes, indices, icount, id, cat, pos, rot, scale
    testShape(MeshShape(DtTriangles, vertices.data()->v, unsigned(vertices.size()), sizeof(*vertices.data()),
              indices.data(), unsigned(indices.size()),
              42, 1,
              Vector3f(1.2f, 2.3f, 3.4f), Quaternionf().setAxisAngle(Vector3f(1, 1, 1), degToRad(18)),
              Vector3f(1.0f, 1.2f, 0.8f)));

    // II> Test with uniform normal.
    testShape(MeshShape(DtVoxels, vertices.data()->v, unsigned(vertices.size()), sizeof(*vertices.data()),
              42,
              Vector3f(1.2f, 2.3f, 3.4f), Quaternionf().setAxisAngle(Vector3f(1, 1, 1), degToRad(18)),
              Vector3f(1.0f, 1.2f, 0.8f)).setUniformNormal(Vector3f(0.1f, 0.1f, 0.1f)));

    // III> Test will many normals.
    testShape(MeshShape(DtTriangles, vertices.data()->v, unsigned(vertices.size()), sizeof(*vertices.data()),
              indices.data(), unsigned(indices.size()),
              42, 1,
              Vector3f(1.2f, 2.3f, 3.4f), Quaternionf().setAxisAngle(Vector3f(1, 1, 1), degToRad(18)),
              Vector3f(1.0f, 1.2f, 0.8f)).setNormals(normals.data()->v, sizeof(*normals.data())));
  }

  TEST(Shapes, Plane)
  {
    testShape(Plane(42, Vector3f(1.2f, 2.3f, 3.4f), Vector3f(1, 1, 1).normalised(), 5.0f, 0.75f));
    testShape(Plane(42, 1, Vector3f(1.2f, 2.3f, 3.4f), Vector3f(1, 1, 1).normalised(), 5.0f, 0.75f));
  }

  // TEST(Shapes, PointCloud)
  // {
  //   testShape(PointCloudShape(42, Vector3f(1.2f, 2.3f, 3.4f), Vector3f(1, 1, 1).normalised(), 2.0f, 0.05f));
  // }

  TEST(Shapes, Sphere)
  {
    testShape(Sphere(42, Vector3f(1.2f, 2.3f, 3.4f), 1.26f));
    testShape(Sphere(42, 1, Vector3f(1.2f, 2.3f, 3.4f), 1.26f));
  }

  TEST(Shapes, Star)
  {
    testShape(Star(42, Vector3f(1.2f, 2.3f, 3.4f), 1.26f));
    testShape(Star(42, 1, Vector3f(1.2f, 2.3f, 3.4f), 1.26f));
  }

  TEST(Shapes, Text2D)
  {
    testShape(Text2D("Transient Text2D", Vector3f(1.2f, 2.3f, 3.4f)));
    testShape(Text2D("Persistent Text2D", 42, Vector3f(1.2f, 2.3f, 3.4f)));
    testShape(Text2D("Persistent, categorised Text2D", 42, 1, Vector3f(1.2f, 2.3f, 3.4f)));
  }

  TEST(Shapes, Text3D)
  {
    // Validate all the constructors.
    testShape(Text3D("Transient Text3D", Vector3f(1.2f, 2.3f, 3.4f), 14));
    testShape(Text3D("Transient oriented Text3D", Vector3f(1.2f, 2.3f, 3.4f), Vector3f(1, 2, 3).normalised(), 8));
    testShape(Text3D("Persistent Text3D", 42, Vector3f(1.2f, 2.3f, 3.4f), 23));
    testShape(Text3D("Persistent oriented Text3D", 42, Vector3f(1.2f, 2.3f, 3.4f), Vector3f(1, 2, 3).normalised(), 12));
    testShape(Text3D("Persistent, categorised, oriented Text3D", 42, 1, Vector3f(1.2f, 2.3f, 3.4f), Vector3f(1, 2, 3).normalised(), 15));
  }
}
