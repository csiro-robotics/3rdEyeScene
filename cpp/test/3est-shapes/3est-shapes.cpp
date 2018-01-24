//
// author: Kazys Stepanas
//

#include <3escoordinateframe.h>
#include <3esconnectionmonitor.h>
#include <3esmessages.h>
#include <3espacketbuffer.h>
#include <3espacketreader.h>
#include <3espacketwriter.h>
#include <3estcpsocket.h>
#include <3esserver.h>
#include <shapes/3esshapes.h>

#include <gtest/gtest.h>

#include <iostream>
#include <vector>

namespace tes
{
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
    EXPECT_EQ(shape.data().attributes.scale[2], reference.data().attributes.position[2]);
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
  void validateClient(TcpSocket &socket, const T &shape, const ServerInfoMessage &serverInfo)
  {
    ServerInfoMessage readServerInfo;
    std::vector<uint8_t> readBuffer(0xffffu);
    PacketBuffer packetBuffer;
    T readShape;
    bool serverInfoRead = false;
    bool shapeMsgRead = false;

    int readCount = 0;
    while ((readCount = socket.readAvailable(readBuffer.data(), readBuffer.size())) > 0)
    {
      packetBuffer.addBytes(readBuffer.data(), readCount);
      // FIXME: endian fix.
      PacketHeader *packetHeader = packetBuffer.extractPacket();

      if (packetHeader)
      {
        PacketReader reader(*packetHeader);

        EXPECT_EQ(reader.marker(), PacketMarker);
        EXPECT_EQ(reader.versionMajor(), PacketVersionMajor);
        EXPECT_EQ(reader.versionMinor(), PacketVersionMinor);

        if (reader.routingId() == MtServerInfo)
        {
          serverInfoRead = true;
          readServerInfo.read(reader);

          // Validate server info.
          EXPECT_EQ(readServerInfo.timeUnit, serverInfo.timeUnit);
          EXPECT_EQ(readServerInfo.defaultFrameTime, serverInfo.defaultFrameTime);
          EXPECT_EQ(readServerInfo.coordinateFrame, serverInfo.coordinateFrame);
          EXPECT_EQ(readServerInfo.reserved, serverInfo.reserved);
        }
        else if (reader.routingId() == shape.routingId())
        {
          // Shape message the shape.
          uint16_t messageType = 0;
          uint32_t shapeId = 0;
          EXPECT_EQ(reader.readElement(messageType), sizeof(messageType));
          shapeMsgRead = true;

          // Peek the shape ID.
          reader.peek((uint8_t *)&shapeId, sizeof(shapeId));

          EXPECT_EQ(shapeId, shape.id());

          switch (messageType)
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

        packetBuffer.releasePacket(packetHeader);
      }
      // else fail?
    }

    EXPECT_GT(readCount, -1);
    EXPECT_TRUE(serverInfoRead);
    EXPECT_TRUE(shapeMsgRead);

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
    Server *server = Server::create(serverSettings, &info);

    // Create client.
    TcpSocket client;

    client.open("127.0.0.1", serverSettings.listenPort);

    // Wait for connection.
    if (server->connectionMonitor()->waitForConnection(20000U) > 0)
    {
      server->connectionMonitor()->commitConnections();
    }

    EXPECT_GT(server->connectionCount(), 1);
    EXPECT_TRUE(client.isConnected());

    // Send server messages.
    server->create(shape);
    server->updateTransfers(0);
    server->updateFrame(0.0f, true);

    // Process client messages.
    validateClient(client, shape, info);

    server->close();

    server->connectionMonitor()->stop();
    server->connectionMonitor()->join();

    server->dispose();
    server = nullptr;

    client.close();
  }

  TEST(Shapes, Arrow)
  {
    testShape(Arrow(42, Vector3f(1.2f, 2.3f, 3.4f), Vector3f(1, 1, 1).normalised(), 2.0f, 0.05f));
  }
}