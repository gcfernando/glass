// -----------------------------------------------------------------------------
//  Glass.Message — reliable Windows version detection.
//
//  Environment.OSVersion is governed by the host application's compatibility
//  manifest: a process without a <supportedOS> entry for Windows 10/11 is told it
//  is running on Windows 8 (6.2.x), which would silently disable Mica, Acrylic,
//  and DWM rounded corners on a perfectly capable machine. RtlGetVersion reports
//  the true OS version regardless of the manifest, so the library queries it once
//  and only falls back to Environment.OSVersion if the native call is unavailable
//  (e.g. on a non-Windows unit-test host).
//
//  File        : OsVersion.cs
//  Developer   ::> Gehan Fernando
// -----------------------------------------------------------------------------

using System;
using System.Runtime.InteropServices;

namespace Glass;

/// <summary>
/// Resolves the real running Windows version (cached for the process) and answers
/// the few capability questions Glass.Message asks before requesting modern DWM
/// chrome.
/// </summary>
internal static class OsVersion
{
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
    private struct OsVersionInfoEx
    {
        public int OSVersionInfoSize;
        public int MajorVersion;
        public int MinorVersion;
        public int BuildNumber;
        public int PlatformId;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 128)]
        public string CSDVersion;
        public ushort ServicePackMajor;
        public ushort ServicePackMinor;
        public ushort SuiteMask;
        public byte ProductType;
        public byte Reserved;
    }

    [DllImport("ntdll.dll")]
    private static extern int RtlGetVersion(ref OsVersionInfoEx versionInfo);

    // Resolved once at first use; the OS version never changes during a session.
    private static readonly (int Major, int Build) _version = Resolve();

    /// <summary>The true OS major version (10 on both Windows 10 and Windows 11).</summary>
    internal static int Major => _version.Major;

    /// <summary>The true OS build number (22000+ on Windows 11).</summary>
    internal static int Build => _version.Build;

    /// <summary>Windows 11 (build 22000+), where Mica and DWM rounded corners are available.</summary>
    internal static bool IsWindows11OrGreater => _version.Major == 10 && _version.Build >= 22000;

    /// <summary>Windows 10 version 1803 (build 17134+), where the Acrylic blur-behind is available.</summary>
    internal static bool IsWindows10_1803OrGreater => _version.Major == 10 && _version.Build >= 17134;

    private static (int, int) Resolve()
    {
        try
        {
            var info = new OsVersionInfoEx { OSVersionInfoSize = Marshal.SizeOf<OsVersionInfoEx>() };
            if (RtlGetVersion(ref info) == 0)   // STATUS_SUCCESS
            {
                return (info.MajorVersion, info.BuildNumber);
            }
        }
        catch { /* ntdll unavailable (e.g. non-Windows test host) — fall back below */ }

        var v = Environment.OSVersion.Version;
        return (v.Major, v.Build);
    }
}
