namespace WgUserControl.Services;

internal sealed class SafeServiceHandle : IDisposable
{
    public nint Handle { get; }

    public SafeServiceHandle(nint handle)
    {
        if (handle == 0)
        {
            throw Win32ExceptionFactory.FromLastWin32Error();
        }

        Handle = handle;
    }

    public void Dispose()
    {
        if (Handle != 0)
        {
            NativeMethods.CloseServiceHandle(Handle);
        }
    }
}
