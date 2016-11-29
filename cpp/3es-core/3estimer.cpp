
#include "3estimer.h"

#include <chrono>
#include <cmath>
#include <sstream>

#ifdef _MSC_VER
#pragma warning(disable : 4996)
#ifndef _CRT_SECURE_NO_WARNINGS
#define _CRT_SECURE_NO_WARNINGS
#endif // !_CRT_SECURE_NO_WARNINGS
#endif // _MSC_VER

typedef std::chrono::high_resolution_clock Clock;
using namespace tes;

struct TimerData
{
  Clock::time_point startTime;
  Clock::time_point endTime;

  inline void init()
  {
    startTime = endTime = Clock::now();
  }

  inline void start()
  {
    startTime = Clock::now();
  }

  inline void mark()
  {
    endTime = Clock::now();
  }

  inline double elapsedS() const
  {
    long long elapsed = std::chrono::duration_cast<std::chrono::microseconds>(endTime - startTime).count();
    return elapsed * 1e-6;
  }

  inline long long elapsedMS() const
  {
    long long elapsed = std::chrono::duration_cast<std::chrono::milliseconds>(endTime - startTime).count();
    return elapsed;
  }

  inline long long elapsedUS() const
  {
    long long elapsed = std::chrono::duration_cast<std::chrono::microseconds>(endTime - startTime).count();
    return elapsed;
  }

  inline long long elapsedNS() const
  {
    long long elapsed = std::chrono::duration_cast<std::chrono::nanoseconds>(endTime - startTime).count();
    return elapsed;
  }
};


namespace
{
  TimerData* getTimerData(char* data)
  {
    TimerData* td = reinterpret_cast<TimerData*>(data);
    return td;
  }

  const TimerData* getTimerData(const char* data)
  {
    const TimerData* td = reinterpret_cast<const TimerData*>(data);
    return td;
  }
}


Timer::Timer()
{
  static_assert(sizeof(TimerData) <= sizeof(data), "Timer data is not large enough.");
  TimerData* td = getTimerData(data);
  td->init();
}


void Timer::start()
{
  TimerData* td = getTimerData(data);
  td->start();
}


long long Timer::restart()
{
  TimerData* td = getTimerData(data);
  td->mark();
  long long elapsedMs = td->elapsedMS();
  td->start();
  return elapsedMs;
}


void Timer::mark()
{
  TimerData* td = getTimerData(data);
  td->mark();
}


bool Timer::hasElapsedMs(long long milliseconds)
{
  TimerData* td = getTimerData(data);
  td->mark();
  return td->elapsedMS() >= milliseconds;
}


long long Timer::elapsedNowMS()
{
  TimerData* td = getTimerData(data);
  td->mark();
  return td->elapsedMS();
}


long long Timer::elapsedNowUS()
{
  TimerData* td = getTimerData(data);
  td->mark();
  return td->elapsedUS();
}


void Timer::elapsed(unsigned int& seconds, unsigned int& milliseconds, unsigned int& microseconds) const
{
  split(elapsedUS(), seconds, milliseconds, microseconds);
}


void Timer::elapsed(Timing &timing) const
{
  const TimerData* td = getTimerData(data);
  long long timeNs = td->elapsedNS();
  split(timeNs, timing);
}


void Timer::elapsedNow(Timing &timing)
{
  TimerData* td = getTimerData(data);
  td->mark();
  long long timeNs = td->elapsedNS();
  split(timeNs, timing);
}


void Timer::split(long long timeNs, Timing &timing)
{
  long long us = timeNs / 1000ull;
  long long ms = us / 1000ull;
  timing.s = timeNs / 1000000000ull;
  timing.ms = (unsigned short)(ms % 1000ull);
  timing.us = (unsigned short)(us % 1000ull);
  timing.ns = (unsigned short)(timeNs % 1000ull);
}


void Timer::split(long long us, unsigned int& seconds, unsigned int& milliseconds, unsigned int& microseconds)
{
  long long ms = us / 1000ull;
  seconds = (unsigned int)(ms / 1000ull);
  milliseconds = (unsigned int)(ms % 1000ull);
  microseconds = (unsigned int)(us % 1000ull);
}


double Timer::elapsedS() const
{
  const TimerData* td = getTimerData(data);
  return td->elapsedS();
}


long long Timer::elapsedMS() const
{
  const TimerData* td = getTimerData(data);
  return td->elapsedMS();
}


long long Timer::elapsedUS() const
{
  const TimerData* td = getTimerData(data);
  return td->elapsedUS();
}


namespace
{
  bool addTimeStringUnit(std::stringstream& str, unsigned int& seconds,
                         const unsigned int secondsInUnit, const char* unitName,
                         bool havePreviousUnit)
  {
    if (seconds >= secondsInUnit)
    {
      unsigned int units = seconds / secondsInUnit;
      if (havePreviousUnit)
        str << ' ';
      str << units << " " << unitName << ((units > 1) ? "s" : "");
      seconds = seconds % secondsInUnit;
      return true;
    }
    return false;
  }
}


char *tes::timeValueString(char *buffer, size_t bufferLen, Timer &t)
{
  unsigned int s, ms, us;
  t.elapsed(s, ms, us);
  return tes::timeValueString(buffer, bufferLen, s, ms, us);
}


std::string tes::timeValueString(Timer& t)
{
  unsigned int s, ms, us;
  t.elapsed(s, ms, us);
  return tes::timeValueString(s, ms, us);
}


char *tes::timeValueString(char *buffer, size_t bufferLen, unsigned int s, unsigned int ms, unsigned int us)
{
  if (bufferLen > 0)
  {
    std::string str = tes::timeValueString(s, ms, us);
    strncpy(buffer, str.c_str(), bufferLen);
    buffer[bufferLen-1] = '\0';
  }
  return buffer;
}


std::string tes::timeValueString(unsigned int s, unsigned int ms, unsigned int us)
{
  std::stringstream str;

  const unsigned int secondsInMinute = 60u;
  const unsigned int secondsInHour = secondsInMinute*60u;
  const unsigned int secondsInDay = secondsInHour*24u;

  bool haveLargeUnits = false;
  haveLargeUnits = addTimeStringUnit(str, s, secondsInDay, "day", haveLargeUnits) || haveLargeUnits;
  haveLargeUnits = addTimeStringUnit(str, s, secondsInHour, "hour", haveLargeUnits) || haveLargeUnits;
  haveLargeUnits = addTimeStringUnit(str, s, secondsInMinute, "minute", haveLargeUnits) || haveLargeUnits;

  if (s)
  {
    if (haveLargeUnits)
      str << ", ";
    str << (double(s) + ms/1000.0) << "s";
  }
  else if (ms)
  {
    if (haveLargeUnits)
      str << ", ";
    str << (double(ms) + us/1000.0) << "ms";
  }
  else if (!haveLargeUnits || us)
  {
    if (haveLargeUnits)
      str << ", ";
    str << us << "us";
  }

  return str.str();
}


const char *tes::timeValueString(char *buffer, size_t bufferLen, double seconds)
{
  if (bufferLen > 0)
  {
    std::string str = tes::timeValueString(seconds);
    strncpy(buffer, str.c_str(), bufferLen);
    buffer[bufferLen-1] = '\0';
  }
  return buffer;
}


std::string tes::timeValueString(double seconds)
{
  unsigned int s, ms, us;
  s = (unsigned int)floor(seconds);
  seconds -= floor(seconds);
  seconds *= 1000.0;  // Now in milliseconds.
  ms = (unsigned int)floor(seconds);
  seconds -= floor(seconds);
  seconds *= 1000.0;  // Now in microseconds.
  us = (unsigned int)floor(seconds);
  return tes::timeValueString(s, ms, us);
}


const char *tes::timeValueString(char *buffer, size_t bufferLen, long double seconds)
{
  if (bufferLen > 0)
  {
    std::string str = tes::timeValueString(seconds);
    strncpy(buffer, str.c_str(), bufferLen);
    buffer[bufferLen-1] = '\0';
  }
  return buffer;
}


std::string tes::timeValueString(long double seconds)
{
  return tes::timeValueString(double(seconds));
}
