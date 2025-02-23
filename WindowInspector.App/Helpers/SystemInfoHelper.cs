using System;
using System.Runtime.InteropServices;
using System.Windows;

namespace WindowInspector.App.Helpers;

public static class SystemInfoHelper
{
    [DllImport("kernel32.dll")]
    private static extern bool GetSystemTimes(
        out long idleTime,
        out long kernelTime,
        out long userTime);

    private static long _lastIdleTime;
    private static long _lastKernelTime;
    private static long _lastUserTime;
    private static DateTime _lastCheckTime = DateTime.MinValue;

    public static TimeSpan GetOptimalPollingInterval()
    {
        if (SystemParameters.PowerLineStatus == PowerLineStatus.Offline)
            return TimeSpan.FromMilliseconds(250); // Battery saving mode

        var cpuLoad = GetCpuUsage();
        const int baseInterval = 100;

        return cpuLoad switch
        {
            > 80 => TimeSpan.FromMilliseconds(baseInterval * 2),    // High load: slower updates
            > 50 => TimeSpan.FromMilliseconds(baseInterval * 1.5),  // Medium load
            _ => TimeSpan.FromMilliseconds(baseInterval)            // Normal load
        };
    }

    private static double GetCpuUsage()
    {
        try
        {
            if (!GetSystemTimes(out var idleTime, out var kernelTime, out var userTime))
                return 0;

            if (_lastCheckTime == DateTime.MinValue)
            {
                _lastIdleTime = idleTime;
                _lastKernelTime = kernelTime;
                _lastUserTime = userTime;
                _lastCheckTime = DateTime.Now;
                return 0;
            }

            var idleDelta = idleTime - _lastIdleTime;
            var kernelDelta = kernelTime - _lastKernelTime;
            var userDelta = userTime - _lastUserTime;

            var totalDelta = kernelDelta + userDelta;
            var utilization = (totalDelta - idleDelta) * 100.0 / totalDelta;

            _lastIdleTime = idleTime;
            _lastKernelTime = kernelTime;
            _lastUserTime = userTime;
            _lastCheckTime = DateTime.Now;

            return utilization;
        }
        catch
        {
            return 0; // Default to assuming low CPU usage on error
        }
    }
} 