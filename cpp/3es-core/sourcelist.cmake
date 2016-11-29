
list(APPEND PUBLIC_HEADERS
  3esbounds.h
  3escollatedpacket.h
  3escolour.h
  3esconnection.h
  3esconnectionmonitor.h
  3escoordinateframe.h
  3escoreutil.h
  3escrc.h
  3esendian.h
  3esfeature.h
  3esmaths.h
  3esmatrix3.h
  3esmatrix3.inl
  3esmatrix4.h
  3esmatrix4.inl
  3esmeshmessages.h
  3esmessages.h
  3esmeta.h
  3espacketbuffer.h
  3espacketheader.h
  3espacketreader.h
  3espacketstream.h
  3espacketwriter.h
  3esplanegeom.h
  3esquaternion.h
  3esquaternion.inl
  3esquaternionarg.h
  3esresource.h
  3esresourcepacker.h
  3esrotation.h
  3esrotation.inl
  3esserver.h
  3esservermacros.h
  3esserverutil.h
  3esspheretessellator.h
  3esspinlock.h
  3estcplistensocket.h
  3estcpsocket.h
  3estimer.h
  3estransferprogress.h
  3estrigeom.h
  3estrigeom.inl
  3esv3arg.h
  3esvector3.h

  shapes/3esarrow.h
  shapes/3esbox.h
  shapes/3escapsule.h
  shapes/3escone.h
  shapes/3escylinder.h
  shapes/3esmeshset.h
  shapes/3esmeshshape.h
  shapes/3esplane.h
  shapes/3espointcloud.h
  shapes/3espointcloudshape.h
  shapes/3esshape.h
  shapes/3esshapes.h
  shapes/3essimplemesh.h
  shapes/3essphere.h
  shapes/3esstar.h
  shapes/3estext2d.h
  shapes/3estext3d.h
)


list(APPEND SOURCES
  3esbounds.cpp
  3escollatedpacket.cpp
  3escolour.cpp
  3escoreutil.cpp
  3escrc.cpp
  3esendian.cpp
  3esfeature.cpp
  3esmatrix3.cpp
  3esmatrix4.cpp
  3esmessages.cpp
  3espacketbuffer.cpp
  3espacketheader.cpp
  3espacketreader.cpp
  3espacketstream.cpp
  3espacketwriter.cpp
  3esquaternion.cpp
  3esresource.cpp
  3esresourcepacker.cpp
  3esrotation.cpp
  3esspheretessellator.cpp
  3esspinlock.cpp
  3estimer.cpp
  3esvector3.cpp
  3esvector4.cpp

  shapes/3esarrow.cpp
  shapes/3escapsule.cpp
  shapes/3escone.cpp
  shapes/3escylinder.cpp
  shapes/3esmeshset.cpp
  shapes/3esmeshshape.cpp
  shapes/3esplane.cpp
  shapes/3espointcloud.cpp
  shapes/3espointcloudshape.cpp
  shapes/3esshape.cpp
  shapes/3esshapes.cpp
  shapes/3essimplemesh.cpp
  shapes/3estext2d.cpp
  shapes/3estext3d.cpp
)

list(APPEND PRIVATE_SOURCES
  private/3esitemtransfer.h
  private/3estcpconnection.cpp
  private/3estcpconnection.h
  private/3estcpconnectionmonitor.cpp
  private/3estcpconnectionmonitor.h
  private/3estcpserver.cpp
  private/3estcpserver.h
)

if(TES_SOCKETS STREQUAL "custom")
  list(APPEND PRIVATE_HEADERS
    tcp/3estcpbase.h
    tcp/3estcpdetail.h
  )

  list(APPEND PRIVATE_SOURCES
    tcp/3estcpbase.cpp
    tcp/3estcplistensocket.cpp
    tcp/3estcpsocket.cpp
  )
elseif(TES_SOCKETS STREQUAL "POCO")
  list(APPEND PRIVATE_HEADERS
    poco/3estcpdetail.h
  )

  list(APPEND PRIVATE_SOURCES
    poco/3estcplistensocket.cpp
    poco/3estcpsocket.cpp
  )
elseif(TES_SOCKETS STREQUAL "Qt")
  list(APPEND PRIVATE_HEADERS
    qt/3estcpdetail.h
  )

  list(APPEND PRIVATE_SOURCES
    qt/3estcplistensocket.cpp
    qt/3estcpsocket.cpp
  )
endif()
