using System.ComponentModel;
using System.Runtime.InteropServices;

namespace WgUserControl.Services;

internal static class Win32ExceptionFactory
{
    public static Win32Exception FromLastWin32Error() => new(Marshal.GetLastWin32Error());
}
