#include "3estcpbase.h"

#include <cstdio>
#include <cstring>

#ifdef WIN32
#include <winsock2.h>
#endif // WIN32

#include <errno.h>      /* Error number definitions */
#include <fcntl.h>      /* File control definitions */

#ifdef __linux__
#include <linux/serial.h>    /* serial_struct */
#include <linux/tty.h>       /* tty_struct -> alt_speed */
#endif // __linux__

#ifndef WIN32
#include <sys/socket.h>
#include <sys/ioctl.h>
#include <sys/time.h>
#include <sys/types.h>  /* bzero, etc */
#include <termios.h>    /* POSIX terminal control definitions */
#include <unistd.h>
#include <fcntl.h>

#include <sys/stat.h>
#include <sys/ioctl.h>            /* ioctl */
#include <netinet/in.h>           /* sockaddr_in */
#include <netinet/tcp.h>
#include <netdb.h>                /* hostent, servent */
#include <arpa/inet.h>            /* inet_ntoa */
#endif // !WIN32

using namespace std;

namespace tes
{
namespace tcpbase
{
#ifdef WIN32
  typedef DWORD intVal_t;
  typedef int socklen_t;
#else  // WIN32
  typedef int intVal_t;
#endif // WIN32

  int create()
  {
    int sock = -1;
#ifdef WIN32
    //Initialise the WinSock2 stack
    WSADATA wsa;
    if (WSAStartup(MAKEWORD(2, 1), &wsa) != 0)
    {
      return false;
    }

    DWORD optVal;
#else
    int optVal;
#endif

    // Create a socket for stream communications
    sock = (int)::socket(AF_INET, SOCK_STREAM, 0);

    if (sock < 0)
    {
      return -1;
    }

#ifdef __APPLE__
    // Don't throw a SIGPIPE signal
    optVal = 1;
    if (::setsockopt(sock, SOL_SOCKET, SO_NOSIGPIPE, &optVal, sizeof(optVal)) < 0)
    {
      tcpbase::close(sock);
      return -1;
    }
#endif // __APPLE__

    optVal = 1;
    if (::setsockopt(sock, SOL_SOCKET, SO_KEEPALIVE, (char *)&optVal, sizeof(optVal)) < 0)
    {
      tcpbase::close(sock);
      return -1;
    }

    //// Disable lingering on socket close
    //struct linger ling;
    //ling.l_onoff = 0;
    //ling.l_linger = 0;
    //if (::setsockopt(sock, SOL_SOCKET, SO_LINGER, (char *)&ling, sizeof(ling)) < 0)
    //{
    //  tcpbase::close(sock);
    //  return -1;
    //}

    // Enable socket re-use (un-bind)
    int reUse = 1;
    if (::setsockopt(sock, SOL_SOCKET, SO_REUSEADDR, (char *)&reUse, sizeof(reUse)) < 0)
    {
      tcpbase::close(sock);
      return -1;
    }

    //dumpSocketOptions(sock);
    return sock;
  }


  void close(int socket)
  {
    if (socket != -1)
    {
  #ifdef WIN32
      ::shutdown(socket, SD_SEND);
      ::closesocket(socket);
  #else
      ::shutdown(socket, SHUT_RDWR);
      ::close(socket);
  #endif
    }
  }


  bool setReceiveTimeout(int socket, unsigned timeoutMs)
  {
#ifdef WIN32
    DWORD tv = timeoutMs; // milliseconds
    if (::setsockopt(socket, SOL_SOCKET, SO_RCVTIMEO, (const char*)&tv, sizeof(tv)) < 0)
    {
      return false;
    }
#else  // Linux

    struct timeval tv;
    timevalFromMs(tv, timeoutMs);
    if (::setsockopt(socket, SOL_SOCKET, SO_RCVTIMEO, (char *)&tv, sizeof(tv)) < 0)
    {
      return false;
    }
#endif

    return true;
  }


  unsigned getReceiveTimeout(int socket)
  {
#ifdef WIN32
    DWORD tv = 0; // milliseconds
    socklen_t len = sizeof(tv);
    if (::getsockopt( socket, SOL_SOCKET, SO_RCVTIMEO, (char*)&tv, &len) < 0)
    {
      return false;
    }

    return unsigned(tv);
#else  // Linux

    struct timeval tv;
    socklen_t len;
    if (::getsockopt(socket, SOL_SOCKET, SO_RCVTIMEO, (char *)&tv, &len) < 0)
    {
      return false;
    }

    return unsigned(1000u * tv.tv_sec + tv.tv_usec / 1000u);
#endif
  }


  bool setSendTimeout(int socket, unsigned timeoutMs)
  {
#ifdef WIN32
    DWORD tv = timeoutMs; // milliseconds
    if (::setsockopt(socket, SOL_SOCKET, SO_SNDTIMEO, (const char*)&tv, sizeof(tv)) < 0)
    {
      return false;
    }
#else  // Linux

    struct timeval tv;
    timevalFromMs(tv, timeoutMs);
    if (::setsockopt(socket, SOL_SOCKET, SO_SNDTIMEO, (char *)&tv, sizeof(tv)) < 0)
    {
      return false;
    }
#endif

    return true;
  }


  unsigned getSendTimeout(int socket)
  {
#ifdef WIN32
    DWORD tv = 0; // milliseconds
    socklen_t len;
    if (::getsockopt(socket, SOL_SOCKET, SO_SNDTIMEO, (char*)&tv, &len) < 0)
    {
      return false;
    }

    return unsigned(tv);
#else  // Linux

    struct timeval tv;
    socklen_t len;
    if (::getsockopt(socket, SOL_SOCKET, SO_SNDTIMEO, (char *)&tv, &len) < 0)
    {
      return false;
    }

    return unsigned(1000u * tv.tv_sec + tv.tv_usec / 1000u);
#endif
  }


  void enableBlocking(int socket)
  {
#ifndef WIN32
    // Disable blocking on read.
    int socketFlags = fcntl(socket, F_GETFL) & ~O_NONBLOCK;
    fcntl(socket, F_SETFL, socketFlags);
#endif // WIN32
  }

  void disableBlocking(int socket)
  {
#ifndef WIN32
    // Disable blocking on read.
    int socketFlags = fcntl(socket, F_GETFL) | O_NONBLOCK;
    fcntl(socket, F_SETFL, socketFlags);
#endif // WIN32
  }


  void timevalFromMs(timeval &tv, unsigned milliseconds)
  {
    // Split into seconds an micro seconds.
    tv.tv_sec = milliseconds*1000;
    tv.tv_usec = tv.tv_usec % 1000000;    // Convert to microseconds
    tv.tv_sec /= 1000000;
  }


  void dumpSocOpt(int socket, const char *name, int opt)
  {
    int optVal = 0;
    socklen_t len = sizeof(optVal);

    ::getsockopt(socket, SOL_SOCKET, opt, (char *)&optVal, &len);
    printf("%s %d\n", name, optVal);
  }


  void dumpSocketOptions(int socket)
  {
    int optVal = 0;
    socklen_t len = sizeof(optVal);

    static struct
    {
      const char *name;
      int opt;
    }
    dopt[] =
    {
    { "SO_DEBUG", SO_DEBUG },
    { "SO_ACCEPTCONN", SO_ACCEPTCONN },
    { "SO_REUSEADDR", SO_REUSEADDR },
    { "SO_KEEPALIVE", SO_KEEPALIVE },
    { "SO_DONTROUTE", SO_DONTROUTE },
    { "SO_BROADCAST", SO_BROADCAST },
    { "SO_USELOOPBACK", SO_USELOOPBACK },
    { "SO_OOBINLINE", SO_OOBINLINE },
#ifndef WIN32
    { "SO_REUSEPORT", SO_REUSEPORT },
    { "SO_TIMESTAMP", SO_TIMESTAMP },
    { "SO_TIMESTAMP_MONOTONIC", SO_TIMESTAMP_MONOTONIC },
    { "SO_DONTTRUNC", SO_DONTTRUNC },
    { "SO_WANTMORE", SO_WANTMORE },
    { "SO_WANTOOBFLAG", SO_WANTOOBFLAG },
#endif // WIN32
    { "SO_SNDBUF", SO_SNDBUF },
    { "SO_RCVBUF", SO_RCVBUF },
    { "SO_SNDLOWAT", SO_SNDLOWAT },
    { "SO_RCVLOWAT", SO_RCVLOWAT },
    { "SO_SNDTIMEO", SO_SNDTIMEO },
    { "SO_RCVTIMEO", SO_RCVTIMEO },
    { "SO_ERROR", SO_ERROR },
    { "SO_TYPE", SO_TYPE },
#ifdef __APPLE__
    { "SO_LABEL", SO_LABEL },
    { "SO_PEERLABEL", SO_PEERLABEL },
    { "SO_NREAD", SO_NREAD },
    { "SO_NKE", SO_NKE },
    { "SO_NOSIGPIPE", SO_NOSIGPIPE },
    { "SO_NOADDRERR", SO_NOADDRERR },
    { "SO_NWRITE", SO_NWRITE },
    { "SO_REUSESHAREUID", SO_REUSESHAREUID },
    { "SO_NOTIFYCONFLICT", SO_NOTIFYCONFLICT },
    { "SO_UPCALLCLOSEWAIT", SO_UPCALLCLOSEWAIT },
    { "SO_LINGER_SEC", SO_LINGER_SEC },
    { "SO_RANDOMPORT", SO_RANDOMPORT },
    { "SO_NP_EXTENSIONS", SO_NP_EXTENSIONS },
#endif // __APPLE__
    };

    for (int i = 0; i < sizeof(dopt) / sizeof(dopt[0]); ++i)
    {
      dumpSocOpt(socket, dopt[i].name, dopt[i].opt);
    }

//    getsockopt(socket, SOL_SOCKET, SO_DEBUG, (char *)&optVal, &len);
//    printf("SO_DEBUG %d\n", optVal);
//    getsockopt(socket, SOL_SOCKET, SO_REUSEADDR, (char *)&optVal, &len);
//    printf("SO_REUSEADDR %d\n", optVal);
//    getsockopt(socket, SOL_SOCKET, SO_REUSEPORT, (char *)&optVal, &len);
//    printf("SO_REUSEPORT %d\n", optVal);
//    getsockopt(socket, SOL_SOCKET, SO_KEEPALIVE, (char *)&optVal, &len);
//    printf("SO_KEEPALIVE %d\n", optVal);
//    getsockopt(socket, SOL_SOCKET, SO_DONTROUTE, (char *)&optVal, &len);
//    printf("SO_DONTROUTE %d\n", optVal);
//    getsockopt(socket, SOL_SOCKET, SO_BROADCAST, (char *)&optVal, &len);
//    printf("SO_BROADCAST %d\n", optVal);
//    getsockopt(socket, SOL_SOCKET, SO_OOBINLINE, (char *)&optVal, &len);
//    printf("SO_OOBINLINE %d\n", optVal);
//    getsockopt(socket, SOL_SOCKET, SO_SNDBUF, (char *)&optVal, &len);
//    printf("SO_SNDBUF %d\n", optVal);
//    getsockopt(socket, SOL_SOCKET, SO_RCVBUF, (char *)&optVal, &len);
//    printf("SO_RCVBUF %d\n", optVal);
//    getsockopt(socket, SOL_SOCKET, SO_SNDLOWAT, (char *)&optVal, &len);
//    printf("SO_SNDLOWAT %d\n", optVal);
//    getsockopt(socket, SOL_SOCKET, SO_RCVLOWAT, (char *)&optVal, &len);
//    printf("SO_RCVLOWAT %d\n", optVal);
//    getsockopt(socket, SOL_SOCKET, SO_SNDTIMEO, (char *)&optVal, &len);
//    printf("SO_SNDTIMEO %d\n", optVal);
//    getsockopt(socket, SOL_SOCKET, SO_RCVTIMEO, (char *)&optVal, &len);
//    printf("SO_RCVTIMEO %d\n", optVal);
//    getsockopt(socket, SOL_SOCKET, SO_TYPE, (char *)&optVal, &len);
//    printf("SO_TYPE %d\n", optVal);
//    getsockopt(socket, SOL_SOCKET, SO_ERROR, (char *)&optVal, &len);
//    printf("SO_ERROR %d\n", optVal);
//    getsockopt(socket, SOL_SOCKET, SO_NOSIGPIPE, (char *)&optVal, &len);
//    printf("SO_NOSIGPIPE %d\n", optVal);

    struct linger ling;
    ling.l_onoff = 0;
    ling.l_linger = 0;
    len = sizeof(ling);
    getsockopt(socket, SOL_SOCKET, SO_LINGER, (char *)&ling, &len);
    printf("SO_LINGER %d:%d\n", ling.l_onoff, ling.l_linger);
  }


  unsigned short getSocketPort(int socket)
  {
    struct sockaddr_in sin;
    socklen_t len = sizeof(sin);
    if (getsockname(socket, (struct sockaddr *)&sin, &len) == -1)
    {
      return 0;
    }

    return ntohs(sin.sin_port);
  }


  const char *sockErrStr(int err)
  {
#ifdef WIN32
    switch (err)
    {
    case 0:
      return "";
    case WSANOTINITIALISED:
      return "socket system not initialised";
    case WSAENETDOWN:
      return "net subsystem down";
    case WSAENOTCONN:
      return "not connected";
    case WSAEACCES:
      return "broadcast access";
    case WSAEINTR:
      return "cancelled";
    case WSAEINPROGRESS:
      return "in progress";
    case WSAEFAULT:
      return "invalid buffer";
    case WSAENETRESET:
      return "keep alive failed";
    case WSAENOBUFS:
      return "no buffer space";
    case WSAENOTSOCK:
      return "invalid descriptor";
    case WSAEOPNOTSUPP:
      return "not supported";
    case WSAESHUTDOWN:
      return "shutdown";
    case WSAEWOULDBLOCK:
      return "would block";
    case WSAEMSGSIZE:
      return "truncated";
    case WSAEINVAL:
      return "unbound";
    case WSAECONNABORTED:
      return "aborted";
    case WSAECONNRESET:
      return "connection reset";
    case WSAETIMEDOUT:
      return "timedout";
    default:
      break;
    }

#else  // WIN32
    switch (err)
    {
    case 0:
      return "";

    //case EAGAIN:
    case EWOULDBLOCK:
      return "would block";

    case EBADF:
      return "invalid descriptor";
    case ECONNRESET:
      return "connection reset";
    case EDESTADDRREQ:
      return "unbound";
    case EFAULT:
      return "invalid user argument";
    case EINTR:
      return "interrupted";
    case EINVAL:
      return "invalid argument";
    case EISCONN:
      return "existing connection";
    case EMSGSIZE:
      return "message size";
    case ENOBUFS:
      return "no buffer space";
    case ENOMEM:
      return "out of memory";
    case ECONNREFUSED:
      return "connection refused";
    case ENOTCONN:
      return "not connected";
    case ENOTSOCK:
      return "invalid descriptor";
    case EOPNOTSUPP:
      return "invalid flags";
    case EPIPE:
      return "pipe";

    default:
      break;
    }
#endif // WIN32

    return "unknown";
  }


  bool isConnected(int socket)
  {
    if (socket == -1)
    {
      return false;
    }

    char ch = 0;
    int flags = MSG_PEEK;
#ifndef WIN32
    flags |= MSG_DONTWAIT;
#endif // WIN32
    int read = ::recv(socket, &ch, 1, flags);

    if (read == 0)
    {
      // Socket has been closed.
      return false;
    }
    else if (read < 0)
    {
#ifdef WIN32
      int err = WSAGetLastError();
      switch (err)
      {
      case WSANOTINITIALISED:
      case WSAENETDOWN:
      case WSAENOTCONN:
      case WSAENETRESET:
      case WSAENOTSOCK:
      case WSAESHUTDOWN:
      case WSAEINVAL:
      case WSAECONNRESET:
      case WSAECONNABORTED:
        return false;
      default:
        break;
      }
#else  // WIN32
      int err = errno;
      switch (err)
      {
        // Disconnection states.
      case ECONNRESET:
      case ENOTCONN:
      case ENOTSOCK:
        return false;
      default:
        break;
      }
#endif // WIN32
    }

    return true;
  }


  void setNoDelay(int socket, bool noDelay)
  {
    intVal_t optVal = (noDelay) ? 1 : 0;
    ::setsockopt(socket, IPPROTO_TCP, TCP_NODELAY, (char *)&optVal, sizeof(optVal));
  }

  bool noDelay(int socket)
  {
    intVal_t optVal = 0;
    socklen_t len = sizeof(optVal);
    ::getsockopt(socket, IPPROTO_TCP, TCP_NODELAY, (char*)&optVal, &len);
    return optVal != 0;
  }


  bool checkSend(int socket, int ret, bool reportDisconnect)
  {
    if (ret < 0)
    {
#ifdef WIN32
      int err = WSAGetLastError();
      //WSAECONNABORTED
      if (err != WSAEWOULDBLOCK && err != WSAECONNRESET)
      {
        if (err != WSAECONNRESET && err != WSAECONNABORTED)
        {
          fprintf(stderr, "send error: %s\n", sockErrStr(err));
        }
        return false;
      }
#else  // WIN32
      int err = errno;
      if (err != EAGAIN && err != EWOULDBLOCK && err != ECONNRESET)
      {
        fprintf(stderr, "send error: %s\n", sockErrStr(err));
        return false;
      }
#endif // WIN32
    }

    return true;
  }


  bool checkRecv(int socket, int ret, bool reportDisconnect)
  {
    if (ret < 0)
    {
#ifdef WIN32
      int err = WSAGetLastError();
      if (err != WSAEWOULDBLOCK && err != WSAECONNRESET)
      {
        fprintf(stderr, "recv error: %s\n", sockErrStr(err));
        return false;
      }
#else  // WIN32
      int err = errno;
      if (err != EAGAIN && err != EWOULDBLOCK && err != ECONNRESET)
      {
        fprintf(stderr, "recv error: %s\n", sockErrStr(err));
        return false;
      }
#endif // WIN32
    }

    return true;
  }

  int getSendBufferSize(int socket)
  {
    intVal_t bufferSize = 0;
    socklen_t len = sizeof(bufferSize);
    if (::getsockopt(socket, SOL_SOCKET, SO_SNDBUF, (char *)&bufferSize, &len) < 0)
    {
      return -1;
    }
    return bufferSize;
  }

  bool setSendBufferSize(int socket, int bufferSize)
  {
    intVal_t bufferSizeVal = bufferSize;
    socklen_t len = sizeof(bufferSize);
    if (::setsockopt(socket, SOL_SOCKET, SO_SNDBUF, (char *)&bufferSize, len) < 0)
    {
      return false;
    }
    return true;
  }


  int getReceiveBufferSize(int socket)
  {
    intVal_t bufferSize = 0;
    socklen_t len = sizeof(bufferSize);
    if (::getsockopt(socket, SOL_SOCKET, SO_RCVBUF, (char *)&bufferSize, &len) < 0)
    {
      return -1;
    }
    return bufferSize;
  }

  bool setReceiveBufferSize(int socket, int bufferSize)
  {
    intVal_t bufferSizeVal = bufferSize;
    socklen_t len = sizeof(bufferSize);
    if (::setsockopt(socket, SOL_SOCKET, SO_RCVBUF, (char *)&bufferSize, len) < 0)
    {
      return false;
    }
    return true;
  }
}
}
