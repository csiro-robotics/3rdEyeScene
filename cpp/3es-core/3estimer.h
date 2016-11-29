
#ifndef __INSEXTTIMER_
#define __INSEXTTIMER_

#include "3es-core.h"

#include <string>

namespace tes
{
  /// A timing information structure.
  struct _3es_coreAPI Timing
  {
    /// Number of seconds elapsed.
    long long s;
    /// Number of milliseconds [0, 1000).
    unsigned short ms;
    /// Number of microseconds [0, 1000).
    unsigned short us;
    /// Number of nanoseconds [0, 1000).
    unsigned short ns;

    /// Initialise to zero.
    inline Timing()
    {
      s = 0;
      ms = us = ns = 0u;
    }
  };

  /// A high precision timer implementation. Actual precision is platform
  /// dependent.
  ///
  /// On Windows, uses @c QueryPerformanceCounter(). Unix based platforms use
  /// @c gettimeofday().
  ///
  /// General usage is to call @c start() at the start of timing and @c mark()
  /// at the end. Various elapsed methods may be used to determine the elapsed
  /// time.
  ///
  /// A timer may be restarted by calling @c start() and @c mark() again. A timer
  /// cannot be paused.
  class _3es_coreAPI Timer
  {
  public:
    /// Constructor. Verifies data size.
    Timer();

    /// Starts the timer by recording the current time.
    void start();

    /// Restarts the timer and returns the time elapsed until this call.
    /// @return The time elapsed (ms) from the last @c start() or @c restart() call
    ///   to this call.
    long long restart();

    /// Records the current time as the end time for elapsed calls.
    void mark();

    /// Checks to see if the given time interval has elapsed.
    /// This destroys information recorded on the last @c mark() call.
    /// @param milliseconds The elapsed timer interval to check (milliseconds).
    /// @return True if @p milliseconds has elapsed since @c start() was called.
    bool hasElapsedMs(long long milliseconds);

    /// Return the time elapsed now. Similar to calling @c mark() then @c elapsedMS().
    /// This destroys information recorded on the last @c mark() call.
    /// @return The time elapsed to now in milliseconds.
    long long elapsedNowMS();

    /// Return the time elapsed now. Similar to calling @c mark() then @c elapsedUS().
    /// This destroys information recorded on the last @c mark() call.
    /// @return The time elapsed to now in microseconds.
    long long elapsedNowUS();

    /// Calculates the elapsed time between @c start() and @c mark().
    /// The result is broken up into seconds, ms and us.
    /// @param[out] seconds The number of whole seconds elapsed.
    /// @param[out] milliseconds The number of whole milliseconds elapsed [0, 999].
    /// @param[out] microseconds The number of whole microseconds elapsed [0, 999].
    void elapsed(unsigned int& seconds, unsigned int& milliseconds, unsigned int& microseconds) const;

    /// Calculates the elapsed time between @c start() and @c mark().
    /// The result is broken up into seconds, ms and us.
    /// @param[out] timing Elapsed timing written here.
    void elapsed(Timing &timing) const;

    /// Marks the current end time (@c mark()) and calculates the elapsed time since @c start().
    /// The result is broken up into seconds, ms and us.
    /// @param[out] timing Elapsed timing written here.
    void elapsedNow(Timing &timing);

    /// Splits a nanoseconds value.
    /// @param timeNs The nanoseconds only value to split.
    /// @param[out] timing Elapsed timing written here.
    static void split(long long timeNs, Timing &timing);

    /// Splits a microsecond value into seconds and milliseconds.
    /// @param timeUs The microseconds only value to split.
    /// @param seconds The number of whole seconds in @p timeUs
    /// @param milliseconds The number of whole milliseconds in @p timeUs.
    /// @param microseconds The remaining microseconds left in @p timeUs. Always < 1000.
    static void split(long long timeUs, unsigned int& seconds, unsigned int& milliseconds, unsigned int& microseconds);

    /// Determines the elapsed time between recorded start and mark times.
    /// Elapsed time is returned in seconds with a fractional component.
    ///
    /// Undefined before calling @c start() and @c mark().
    /// @return The elapsed time in seconds.
    double elapsedS() const;

    /// Determines the elapsed time between recorded start and mark times.
    /// Elapsed time is returned in whole milliseconds.
    ///
    /// Undefined before calling @c start() and @c mark().
    /// @return The elapsed time in milliseconds.
    long long elapsedMS() const;

    /// Determines the elapsed time between recorded start and mark times.
    /// Elapsed time is returned in whole microseconds. Precision may be less than
    /// a microsecond depending on the platform.
    ///
    /// Undefined before calling @c start() and @c mark().
    /// @return The elapsed time in microseconds.
    long long elapsedUS() const;

  private:
    /// Internal data allocation.
    char data[16];
  };

  /// Converts a @c Timer to a time string indicating the elapsed time.
  ///
  /// The string is built differently depending on the amount of time elapsed.
  /// For values greater than one second, the display string is formatted:
  /// @verbatim
  ///   [# day[s],] [# hour[s],] [# minute[s],] [#.#s]
  /// @endverbatim
  /// Where '#' is replaced by the appropriate digits. Each element is display
  /// only if it is non-zero. Plurals are expressed for values greater than 1.
  ///
  /// Times less than one second and greater than one millisecond are displayed:
  /// @verbatim
  ///   #.#ms
  /// @endverbatim
  ///
  /// Otherwise, the string is formatted in microseconds:
  /// @verbatim
  ///   #.#us
  /// @endverbatim
  ///
  /// @param[in,out] buffer The buffer into which to write the time value string.
  ///   The time value string is written here as detailed above.
  ///   Buffer overflows are guarded against.
  /// @param bufferLen The number of bytes available in @p buffer.
  /// @param t Timer to convert to a string.
  /// @return A pointer to @c buffer.
  char _3es_coreAPI *timeValueString(char *buffer, size_t bufferLen, Timer &t);

  /// @overload
  /// @par Note
  /// This may not be safe to call because of the use of @c std::string.
  /// This will be unsafe when using a different core library version.
  /// Most notably this occurs under Visual Studio when using a different
  /// Visual Studio between libraries including mixing debug and release
  /// runtime libraries.
  std::string _3es_coreAPI timeValueString(Timer& t);


  /// Converts a time value int a time string indicating the time elapsed.
  ///
  /// The string is built differently depending on the amount of time elapsed.
  /// For values greater than one second, the display string is formatted:
  /// @verbatim
  ///   [# day[s],] [# hour[s],] [# minute[s],] [#.#s]
  /// @endverbatim
  /// Where '#' is replaced by the appropriate digits. Each element is display
  /// only if it is non-zero. Plurals are expressed for values greater than 1.
  ///
  /// Times less than one second and greater than one millisecond are displayed:
  /// @verbatim
  ///   #.#ms
  /// @endverbatim
  ///
  /// Otherwise, the string is formatted in microseconds:
  /// @verbatim
  ///   #.#us
  /// @endverbatim
  ///
  /// @param[in,out] buffer The buffer into which to write the time value string.
  ///   The time value string is written here as detailed above.
  ///   Buffer overflows are guarded against.
  /// @param bufferLen The number of bytes available in @p buffer.
  /// @param s The number of seconds elapsed.
  /// @param ms The number of milliseconds elapsed (must be < 1000).
  /// @param us The number of micro seconds elapsed (must be < 1000).
  /// @return A pointer to @c buffer.
  char _3es_coreAPI *timeValueString(char *buffer, size_t bufferLen,
                                       unsigned int s,
                                       unsigned int ms = 0u,
                                       unsigned int us = 0u);

  /// @overload
  /// @par Note
  /// This may not be safe to call because of the use of @c std::string.
  /// This will be unsafe when using a different core library version.
  /// Most notably this occurs under Visual Studio when using a different
  /// Visual Studio between libraries including mixing debug and release
  /// runtime libraries.
  std::string _3es_coreAPI timeValueString(unsigned int s,
                                             unsigned int ms = 0,
                                             unsigned int us = 0u);

  /// @overload
  const char _3es_coreAPI *timeValueString(char *buffer, size_t bufferLen, double seconds);

  /// @overload
  std::string _3es_coreAPI timeValueString(double seconds);

  /// @overload
  const char _3es_coreAPI *timeValueString(char *buffer, size_t bufferLen, long double seconds);

  /// @overload
  std::string _3es_coreAPI timeValueString(long double seconds);
}

#endif // __TIMER_

