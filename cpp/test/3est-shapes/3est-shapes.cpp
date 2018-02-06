//
// author: Kazys Stepanas
//

#include <3escoordinateframe.h>
#include <3esconnectionmonitor.h>
#include <3esmaths.h>
#include <3esmathsstream.h>
#include <3esmessages.h>
#include <3espacketbuffer.h>
#include <3espacketreader.h>
#include <3espacketwriter.h>
#include <3estcpsocket.h>
#include <3esserver.h>
#include <3esserverutil.h>
#include <3esspheretessellator.h>
#include <3estcplistensocket.h>
#include <shapes/3espointcloud.h>
#include <shapes/3esshapes.h>
#include <shapes/3essimplemesh.h>

#include <gtest/gtest.h>

#include <chrono>
#include <iostream>
#include <thread>
#include <vector>
#include <unordered_map>

namespace tes
{
  typedef std::unordered_map<uint64_t, Resource *> ResourceMap;

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


  // Validate a mesh resource.
  void validateMesh(const MeshResource &mesh, const MeshResource &reference)
  {
    // Check members.
    EXPECT_EQ(mesh.id(), reference.id());
    EXPECT_EQ(mesh.typeId(), reference.typeId());
    EXPECT_EQ(mesh.uniqueKey(), reference.uniqueKey());

    EXPECT_TRUE(mesh.transform().equals(reference.transform()));
    EXPECT_EQ(mesh.tint(), reference.tint());
    EXPECT_EQ(mesh.vertexCount(), reference.vertexCount());
    EXPECT_EQ(mesh.indexCount(), reference.indexCount());

    // Check vertices and vertex related components.
    unsigned meshStride, refStride = 0;
    if (reference.vertexCount() && mesh.vertexCount() == reference.vertexCount())
    {
      const float *meshVerts = mesh.vertices(meshStride);
      const float *refVerts = reference.vertices(refStride);

      ASSERT_NE(meshVerts, nullptr);
      ASSERT_NE(refVerts, nullptr);

      for (unsigned i = 0; i < mesh.vertexCount(); ++i)
      {
        if (meshVerts[0] != refVerts[0] || meshVerts[1] != refVerts[1] || meshVerts[2] != refVerts[2])
        {
          FAIL() << "vertex[" << i << "]: (" << meshVerts[0] << ',' << meshVerts[1] << ',' << meshVerts[2]
                 << ") != (" << refVerts[0] << ',' << refVerts[1] << ',' << refVerts[2] << ")";
        }

        meshVerts += meshStride / sizeof(float);
        refVerts += refStride / sizeof(float);
      }

      // Check normals.
      if (reference.normals(refStride))
      {
        ASSERT_TRUE(mesh.normals(meshStride)) << "Mesh missing normals.";

        const float *meshNorms = mesh.normals(meshStride);
        const float *refNorms = reference.normals(refStride);

        for (unsigned i = 0; i < mesh.vertexCount(); ++i)
        {
          if (meshNorms[0] != refNorms[0] || meshNorms[1] != refNorms[1] || meshNorms[2] != refNorms[2])
          {
            FAIL() << "normal[" << i << "]: (" << meshNorms[0] << ',' << meshNorms[1] << ',' << meshNorms[2]
                  << ") != (" << refNorms[0] << ',' << refNorms[1] << ',' << refNorms[2] << ")";
          }

          meshNorms += meshStride / sizeof(float);
          refNorms += refStride / sizeof(float);
        }
      }

      // Check colours.
      if (reference.colours(refStride))
      {
        ASSERT_TRUE(mesh.colours(meshStride)) << "Mesh missing colours.";

        const uint32_t *meshColours = mesh.colours(meshStride);
        const uint32_t *refColours = reference.colours(refStride);

        for (unsigned i = 0; i < mesh.vertexCount(); ++i)
        {
          if (*meshColours != *refColours)
          {
            FAIL() << "colour[" << i << "]: 0x"
                   << std::hex << std::setw(8) << std::setfill('0')
                   << *meshColours << " != 0x" << *refColours
                   << std::dec << std::setw(0) << std::setfill(' ');
          }

          meshColours += meshStride / sizeof(uint32_t);
          refColours += refStride / sizeof(uint32_t);
        }
      }

      // Check UVs.
      if (reference.uvs(refStride))
      {
        ASSERT_TRUE(mesh.uvs(meshStride)) << "Mesh missing UVs.";

        const float *meshUVs = mesh.uvs(meshStride);
        const float *refUVs = reference.uvs(refStride);

        for (unsigned i = 0; i < mesh.vertexCount(); ++i)
        {
          if (meshUVs[0] != refUVs[0] || meshUVs[1] != refUVs[1])
          {
            FAIL() << "uv[" << i << "]: (" << meshUVs[0] << ',' << meshUVs[1]
                   << ") != (" << refUVs[0] << ',' << refUVs[1] << ")";
          }

          meshUVs += meshStride / sizeof(float);
          refUVs += refStride / sizeof(float);
        }
      }
    }

    // Check indices.
    if (reference.indexCount() && mesh.indexCount() == reference.indexCount())
    {
      unsigned meshWidth = 0, refWidth = 0;
      const uint8_t *meshInds = mesh.indices(meshStride, meshWidth);
      const uint8_t *refInds = reference.indices(refStride, refWidth);

      ASSERT_NE(meshInds, nullptr);
      ASSERT_NE(refInds, nullptr);

      // Handle index widths.
      std::function<unsigned (const uint8_t*)> meshGetIndex, refGetIndex;

      auto getIndex1 = [] (const uint8_t *mem) { return unsigned(*mem); };
      auto getIndex2 = [] (const uint8_t *mem) { return unsigned(*reinterpret_cast<const uint16_t *>(mem)); };
      auto getIndex4 = [] (const uint8_t *mem) { return unsigned(*reinterpret_cast<const uint32_t *>(mem)); };

      switch (meshWidth)
      {
        case 1: meshGetIndex = getIndex1; break;
        case 2: meshGetIndex = getIndex2; break;
        case 4: meshGetIndex = getIndex4; break;
        default: ASSERT_TRUE(false) << "Unexpected index width.";
      }

      switch (refWidth)
      {
        case 1: refGetIndex = getIndex1; break;
        case 2: refGetIndex = getIndex2; break;
        case 4: refGetIndex = getIndex4; break;
        default: ASSERT_TRUE(false) << "Unexpected index width.";
      }

      for (unsigned i = 0; i < mesh.indexCount(); ++i)
      {
        EXPECT_EQ(meshGetIndex(meshInds), refGetIndex(refInds));

        meshInds += meshStride;
        refInds += refStride;
      }
    }
  }


  void validateShape(const Shape &shape, const Shape &reference, const ResourceMap &resources)
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
  void validateText(const T &shape, const T &reference, const ResourceMap &resources)
  {
    validateShape(static_cast<const Shape>(shape), static_cast<const Shape>(reference), resources);
    EXPECT_EQ(shape.textLength(), reference.textLength());
    EXPECT_STREQ(shape.text(), reference.text());
  }


  void validateShape(const Text2D &shape, const Text2D &reference, const ResourceMap &resources)
  {
    validateText(shape, reference, resources);
  }


  void validateShape(const Text3D &shape, const Text3D &reference, const ResourceMap &resources)
  {
    validateText(shape, reference, resources);
  }


  void validateShape(const MeshShape &shape, const MeshShape &reference, const ResourceMap &resources)
  {
    validateShape(static_cast<const Shape>(shape), static_cast<const Shape>(reference), resources);

    EXPECT_EQ(shape.drawType(), reference.drawType());
    EXPECT_EQ(shape.vertexCount(), reference.vertexCount());
    EXPECT_EQ(shape.vertexStride(), reference.vertexStride());
    EXPECT_EQ(shape.normalsCount(), reference.normalsCount());
    EXPECT_EQ(shape.normalsStride(), reference.normalsStride());
    EXPECT_EQ(shape.indexCount(), reference.indexCount());

    // Validate vertices.
    Vector3f v, r;
    if (shape.vertexCount() == reference.vertexCount() && shape.vertexCount())
    {
      ASSERT_NE(shape.vertices(), nullptr);
      ASSERT_NE(reference.vertices(), nullptr);
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

    if (shape.indexCount() == reference.indexCount() && shape.indexCount())
    {
      ASSERT_NE(shape.indices(), nullptr);
      ASSERT_NE(reference.indices(), nullptr);
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

    if (shape.normalsCount() == reference.normalsCount() && shape.normalsCount())
    {
      ASSERT_NE(shape.normals(), nullptr);
      ASSERT_NE(reference.normals(), nullptr);
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

    if (reference.colours())
    {
      ASSERT_NE(shape.colours(), nullptr);
    }

    if (shape.vertexCount() == reference.vertexCount() && reference.colours())
    {
      Colour cs, cr;
      for (unsigned i = 0; i < shape.vertexCount(); ++i)
      {
        cs = shape.colours()[i];
        cr = reference.colours()[i];

        if (cs != cr)
        {
          std::cerr << "Colour mismatch at " << i << '\n';
          EXPECT_EQ(cs, cr);
        }
      }
    }
  }


  void validateShape(const PointCloudShape &shape, const PointCloudShape &reference, const ResourceMap &resources)
  {
    validateShape(static_cast<const Shape>(shape), static_cast<const Shape>(reference), resources);

    EXPECT_EQ(shape.pointSize(), reference.pointSize());
    EXPECT_EQ(shape.indexCount(), reference.indexCount());

    // Note: We can't compare the contents of shape.mesh() as it is a placeholder reference.
    // The real mesh is received and validated separately.
    ASSERT_NE(shape.mesh(), nullptr);
    ASSERT_NE(reference.mesh(), nullptr);
    EXPECT_EQ(shape.mesh()->id(), reference.mesh()->id());
    EXPECT_EQ(shape.mesh()->typeId(), reference.mesh()->typeId());
    EXPECT_EQ(shape.mesh()->uniqueKey(), reference.mesh()->uniqueKey());

    if (shape.indexCount() == reference.indexCount())
    {
      for (unsigned i = 0; i < shape.indexCount(); ++i)
      {
        EXPECT_EQ(shape.indices()[i], reference.indices()[i]);
      }
    }

    // Validate resources. Fetch the transferred resource and compare against the reference resource.
    auto resIter = resources.find(shape.mesh()->uniqueKey());
    ASSERT_NE(resIter, resources.end());
    ASSERT_EQ(resIter->second->typeId(), reference.mesh()->typeId());

    const MeshResource *mesh = static_cast<const MeshResource *>(resIter->second);
    validateMesh(*mesh, *reference.mesh());
  }


  void validateShape(const MeshSet &shape, const MeshSet &reference, const ResourceMap &resources)
  {
    validateShape(static_cast<const Shape>(shape), static_cast<const Shape>(reference), resources);

    EXPECT_EQ(shape.partCount(), reference.partCount());

    for (int i = 0; i < std::min(shape.partCount(), reference.partCount()); ++i)
    {
      // Remember, the mesh in shape is only a placeholder for the ID. The real mesh is in resources.
      // Validate resources. Fetch the transferred resource and compare against the reference resource.
      auto resIter = resources.find(shape.partAt(i)->uniqueKey());
      ASSERT_NE(resIter, resources.end());
      ASSERT_EQ(resIter->second->typeId(), reference.partAt(i)->typeId());

      const MeshResource *part = static_cast<const MeshResource *>(resIter->second);
      const MeshResource *refPart = reference.partAt(i);

      EXPECT_TRUE(shape.partTransform(i).equals(reference.partTransform(i)));
      validateMesh(*part, *refPart);
    }
  }


  template <class T>
  void handleShapeMessage(PacketReader &reader, T &shape, const T &referenceShape)
  {
    // Shape message the shape.
    uint32_t shapeId = 0;

    // Peek the shape ID.
    reader.peek((uint8_t *)&shapeId, sizeof(shapeId));

    EXPECT_EQ(shapeId, referenceShape.id());

    switch (reader.messageId())
    {
    case OIdCreate:
      EXPECT_TRUE(shape.readCreate(reader));
      break;

    case OIdUpdate:
      EXPECT_TRUE(shape.readUpdate(reader));
      break;

    case OIdData:
      EXPECT_TRUE(shape.readData(reader));
      break;
    }
  }


  void handleMeshMessage(PacketReader &reader, ResourceMap &resources)
  {
    uint32_t meshId = 0;
    reader.peek((uint8_t *)&meshId, sizeof(meshId));
    auto resIter = resources.find(MeshPlaceholder(meshId).uniqueKey());
    SimpleMesh *mesh = nullptr;

    // If it exists, make sure it's a mesh.
    if (resIter != resources.end())
    {
      ASSERT_TRUE(resIter->second->typeId() == MtMesh);
      mesh = static_cast<SimpleMesh *>(resIter->second);
    }

    switch (reader.messageId())
    {
      case MmtInvalid:
        EXPECT_TRUE(false) << "Invalid mesh message sent";
        break;

      case MmtDestroy:
        delete mesh;
        if (resIter != resources.end())
        {
          resources.erase(resIter);
        }
        break;

      case MmtCreate:
        // Create message. Should not already exists.
        EXPECT_EQ(mesh, nullptr) << "Recreating exiting mesh.";
        delete mesh;
        mesh = new SimpleMesh(meshId);
        EXPECT_TRUE(mesh->readCreate(reader));
        resources.insert(std::make_pair(mesh->uniqueKey(), mesh));
        break;

      // Not handling these messages.
      case MmtRedefine:
      case MmtFinalise:
        break;

    default:
      EXPECT_NE(mesh, nullptr);
      if (mesh)
      {
        EXPECT_TRUE(mesh->readTransfer(reader.messageId(), reader));
      }
      break;
    }
  }


  template <class T>
  void validateClient(TcpSocket &socket, const T &referenceShape, const ServerInfoMessage &serverInfo,
                      unsigned timeoutSec = 10)
  {
    typedef std::chrono::steady_clock Clock;
    ServerInfoMessage readServerInfo;
    std::vector<uint8_t> readBuffer(0xffffu);
    ResourceMap resources;
    PacketBuffer packetBuffer;
    T shape;
    auto startTime = Clock::now();
    int readCount = 0;
    bool endMsgReceived = false;
    bool serverInfoRead = false;
    bool shapeMsgRead = false;

    memset(&readServerInfo, 0, sizeof(readServerInfo));

    // Keep looping until we get a CIdEnd ControlMessage or timeoutSec elapses.
    while (!endMsgReceived && std::chrono::duration_cast<std::chrono::seconds>(Clock::now() - startTime).count() < timeoutSec)
    {
      readCount = socket.readAvailable(readBuffer.data(), int(readBuffer.size()));
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

          case MtMesh:
            handleMeshMessage(reader, resources);
            break;

          default:
            if (reader.routingId() == referenceShape.routingId())
            {
              shapeMsgRead = true;
              handleShapeMessage(reader, shape, referenceShape);
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
      validateShape(shape, referenceShape, resources);
    }

    for (auto &&resource : resources)
    {
      delete resource.second;
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

    EXPECT_GT(server->connectionCount(), 0u);
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

  TEST(Shapes, MeshSet)
  {
    std::vector<Vector3f> vertices;
    std::vector<unsigned> indices;
    std::vector<unsigned> wireIndices;
    std::vector<Vector3f> normals;
    std::vector<uint32_t> colours;
    std::vector<SimpleMesh *> meshes;
    makeHiResSphere(vertices, indices, &normals);

    // Build per vertex colours by colour cycling.
    colours.resize(vertices.size());
    for (size_t i = 0; i < vertices.size(); ++i)
    {
      colours[i] = Colour::cycle(unsigned(i)).c;
    }

    // Build a line based indexing scheme for a wireframe sphere.
    for (size_t i = 0; i < indices.size(); i += 3)
    {
      wireIndices.push_back(indices[i + 0]);
      wireIndices.push_back(indices[i + 1]);
      wireIndices.push_back(indices[i + 1]);
      wireIndices.push_back(indices[i + 2]);
      wireIndices.push_back(indices[i + 2]);
      wireIndices.push_back(indices[i + 0]);
    }

    // Build a number of meshes to include in the mesh set.
    SimpleMesh *mesh = nullptr;
    unsigned nextMeshId = 1;

    // Vertices and indices only.
    mesh = new SimpleMesh(nextMeshId++, unsigned(vertices.size()), unsigned(indices.size()), DtTriangles);
    mesh->setVertices(0, vertices.data(), unsigned(vertices.size()));
    mesh->setIndices(0, indices.data(), unsigned(indices.size()));
    meshes.push_back(mesh);

    // Vertices, indices and colours
    mesh = new SimpleMesh(nextMeshId++, unsigned(vertices.size()), unsigned(indices.size()), DtTriangles,
                          SimpleMesh::Vertex | SimpleMesh::Index | SimpleMesh::Colour);
    mesh->setVertices(0, vertices.data(), unsigned(vertices.size()));
    mesh->setNormals(0, normals.data(), unsigned(normals.size()));
    mesh->setColours(0, colours.data(), unsigned(colours.size()));
    mesh->setIndices(0, indices.data(), unsigned(indices.size()));
    meshes.push_back(mesh);

    // Points and colours only (essentially a point cloud)
    mesh = new SimpleMesh(nextMeshId++, unsigned(vertices.size()), unsigned(indices.size()), DtPoints,
                          SimpleMesh::Vertex | SimpleMesh::Colour);
    mesh->setVertices(0, vertices.data(), unsigned(vertices.size()));
    mesh->setColours(0, colours.data(), unsigned(colours.size()));
    meshes.push_back(mesh);

    // Lines.
    mesh = new SimpleMesh(nextMeshId++, unsigned(vertices.size()), unsigned(wireIndices.size()), DtLines);
    mesh->setVertices(0, vertices.data(), unsigned(vertices.size()));
    mesh->setIndices(0, wireIndices.data(), unsigned(wireIndices.size()));
    meshes.push_back(mesh);

    // One with the lot.
    mesh = new SimpleMesh(nextMeshId++, unsigned(vertices.size()), unsigned(indices.size()), DtTriangles,
                          SimpleMesh::Vertex | SimpleMesh::Index | SimpleMesh::Normal | SimpleMesh::Colour);
    mesh->setVertices(0, vertices.data(), unsigned(vertices.size()));
    mesh->setNormals(0, normals.data(), unsigned(normals.size()));
    mesh->setColours(0, colours.data(), unsigned(colours.size()));
    mesh->setIndices(0, indices.data(), unsigned(indices.size()));
    meshes.push_back(mesh);

    // First do a single part MeshSet.
    testShape(MeshSet(meshes[0], 42));

    // Now a multi-part MeshSet.
    {
      MeshSet set(42, 1, int(meshes.size()));

      Matrix4f transform = Matrix4f::identity;
      for (int i = 0; i < int(meshes.size()); ++i)
      {
        transform = prsTransform(Vector3f(i, i - 3.2f, 1.5f * i),
                                 Quaternionf().setAxisAngle(Vector3f(i, i + 1, i - 3).normalised(), degToRad((i + 1) * 6.0f)),
                                 Vector3f(0.75f, 0.75f, 0.75f));
        set.setPart(i, meshes[i], transform);
      }
      testShape(set);
    }

    for (auto &&mesh : meshes)
    {
      delete mesh;
    }
  }

  TEST(Shapes, Mesh)
  {
    std::vector<Vector3f> vertices;
    std::vector<unsigned> indices;
    std::vector<Vector3f> normals;
    makeHiResSphere(vertices, indices, &normals);

    // Build a colour cycle for per-vertex colours.
    std::vector<uint32_t> colours(vertices.size());
    for (unsigned i = 0; i < unsigned(colours.size()); ++i)
    {
      colours[i] = tes::Colour::cycle(i).c;
    }

    // I> Test each constructor.
    // 1. drawType, verts, vcount, vstrideBytes, pos, rot, scale
    testShape(MeshShape(DtPoints, vertices.data()->v, unsigned(vertices.size()), sizeof(*vertices.data()),
              Vector3f(1.2f, 2.3f, 3.4f), Quaternionf().setAxisAngle(Vector3f(1, 1, 1), degToRad(18.0f)),
              Vector3f(1.0f, 1.2f, 0.8f)));
    // 2. drawType, verts, vcount, vstrideBytes, indices, icount, pos, rot, scale
    testShape(MeshShape(DtTriangles, vertices.data()->v, unsigned(vertices.size()), sizeof(*vertices.data()),
              indices.data(), unsigned(indices.size()),
              Vector3f(1.2f, 2.3f, 3.4f), Quaternionf().setAxisAngle(Vector3f(1, 1, 1), degToRad(18.0f)),
              Vector3f(1.0f, 1.2f, 0.8f)));
    // 3. drawType, verts, vcount, vstrideBytes, id, pos, rot, scale
    testShape(MeshShape(DtPoints, vertices.data()->v, unsigned(vertices.size()), sizeof(*vertices.data()),
              42,
              Vector3f(1.2f, 2.3f, 3.4f), Quaternionf().setAxisAngle(Vector3f(1, 1, 1), degToRad(18.0f)),
              Vector3f(1.0f, 1.2f, 0.8f)));
    // 4. drawType, verts, vcount, vstrideBytes, indices, icount, id, pos, rot, scale
    testShape(MeshShape(DtTriangles, vertices.data()->v, unsigned(vertices.size()), sizeof(*vertices.data()),
              indices.data(), unsigned(indices.size()),
              42,
              Vector3f(1.2f, 2.3f, 3.4f), Quaternionf().setAxisAngle(Vector3f(1, 1, 1), degToRad(18.0f)),
              Vector3f(1.0f, 1.2f, 0.8f)));
    // 5. drawType, verts, vcount, vstrideBytes, indices, icount, id, cat, pos, rot, scale
    testShape(MeshShape(DtTriangles, vertices.data()->v, unsigned(vertices.size()), sizeof(*vertices.data()),
              indices.data(), unsigned(indices.size()),
              42, 1,
              Vector3f(1.2f, 2.3f, 3.4f), Quaternionf().setAxisAngle(Vector3f(1, 1, 1), degToRad(18.0f)),
              Vector3f(1.0f, 1.2f, 0.8f)));

    // II> Test with uniform normal.
    testShape(MeshShape(DtVoxels, vertices.data()->v, unsigned(vertices.size()), sizeof(*vertices.data()),
              42,
              Vector3f(1.2f, 2.3f, 3.4f), Quaternionf().setAxisAngle(Vector3f(1, 1, 1), degToRad(18.0f)),
              Vector3f(1.0f, 1.2f, 0.8f)).setUniformNormal(Vector3f(0.1f, 0.1f, 0.1f)));

    // III> Test will many normals.
    testShape(MeshShape(DtTriangles, vertices.data()->v, unsigned(vertices.size()), sizeof(*vertices.data()),
              indices.data(), unsigned(indices.size()),
              42, 1,
              Vector3f(1.2f, 2.3f, 3.4f), Quaternionf().setAxisAngle(Vector3f(1, 1, 1), degToRad(18.0f)),
              Vector3f(1.0f, 1.2f, 0.8f)).setNormals(normals.data()->v, sizeof(*normals.data())));

    // IV> Test with colours.
    testShape(MeshShape(DtTriangles, vertices.data()->v, unsigned(vertices.size()), sizeof(*vertices.data()),
              indices.data(), unsigned(indices.size()),
              Vector3f(1.2f, 2.3f, 3.4f), Quaternionf().setAxisAngle(Vector3f(1, 1, 1), degToRad(18.0f)),
              Vector3f(1.0f, 1.2f, 0.8f)).setColours(colours.data()));
  }

  TEST(Shapes, Plane)
  {
    testShape(Plane(42, Vector3f(1.2f, 2.3f, 3.4f), Vector3f(1, 1, 1).normalised(), 5.0f, 0.75f));
    testShape(Plane(42, 1, Vector3f(1.2f, 2.3f, 3.4f), Vector3f(1, 1, 1).normalised(), 5.0f, 0.75f));
  }

  TEST(Shapes, PointCloud)
  {
    std::vector<Vector3f> vertices;
    std::vector<unsigned> indices;
    std::vector<Vector3f> normals;
    makeHiResSphere(vertices, indices, &normals);

    PointCloud cloud(42);
    cloud.addPoints(vertices.data(), unsigned(vertices.size()));

    // Full res cloud.
    testShape(PointCloudShape(&cloud, 42, 0, 8));

    // Indexed (sub-sampled) cloud. Just use half the points.
    indices.resize(0);
    for (unsigned i = 0; i < unsigned(vertices.size()/2); ++i)
    {
      indices.push_back(i);
    }
    testShape(PointCloudShape(&cloud, 42, 0, 8).setIndices(indices.data(), unsigned(indices.size())));
  }

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
