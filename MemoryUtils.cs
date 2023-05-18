using System.Runtime.InteropServices;

namespace ZTAntiDump;

public static unsafe class MemoryUtils
{
    [DllImport("kernel32.dll", SetLastError = false)]
    internal static extern bool VirtualProtect(byte* address, int size, uint newProtection, out uint oldProtection);

    [DllImport("msvcrt.dll", EntryPoint = "memcpy", CallingConvention = CallingConvention.Cdecl, SetLastError = false)]
    internal static extern void* MemCpy(void* dest, byte[] src, int count);
}