using System;
using System.Diagnostics;
using static ZTAntiDump.MemoryUtils;

namespace ZTAntiDump;

internal static class Program
{
    public static void Main()
    {
        unsafe /*Anti Dumping*/
        {
            // Get the base address of the process
            var processModule = Process.GetCurrentProcess().MainModule;
            var baseAddress = (byte*)processModule!.BaseAddress;

            // Get the address of the PE header (offset 0x3c from the base)
            var peHeaderPtr = baseAddress + 0x3c;
            peHeaderPtr = baseAddress + *(uint*)peHeaderPtr;
            peHeaderPtr += 0x6;

            // Get the number of sections from the PE header
            var sectionCount = *(ushort*)peHeaderPtr;
            peHeaderPtr += 14;

            // Get the size of the optional header
            var optionalHeaderSize = *(ushort*)peHeaderPtr;

            // Move the pointer to the end of the optional header
            peHeaderPtr = peHeaderPtr + 0x4 + optionalHeaderSize;

            // VirtualProtect: Change the protection of memory region to read-write (0x40)
            _ = VirtualProtect(peHeaderPtr - 16, 8, 0x40, out _);

            // Zero out the address of the metadata directory
            *(uint*)(peHeaderPtr - 12) = 0;

            // Get the address of the metadata directory
            var metadataDirectory = baseAddress + *(uint*)(peHeaderPtr - 16);

            // Zero out the metadata directory address
            *(uint*)(peHeaderPtr - 16) = 0;

            // VirtualProtect: Change the protection of metadata directory to read-write
            _ = VirtualProtect(metadataDirectory, 0x48, 0x40, out _);

            // Get the address of the metadata header
            var metadataHeader = baseAddress + *(uint*)(metadataDirectory + 8);

            // Zero out the metadata directory
            for (var i = 0; i < 6; ++i)
                *((uint*)metadataDirectory + i) = 0;

            // VirtualProtect: Change the protection of metadata header to read-write
            _ = VirtualProtect(metadataHeader, 4, 0x40, out _);

            // Zero out the metadata header
            *(uint*)metadataHeader = 0;

            // Iterate over each section and zero out its contents
            for (var i = 0; i < sectionCount; ++i)
            {
                // VirtualProtect: Change the protection of section to read-write
                _ = VirtualProtect(peHeaderPtr, 8, 0x40, out _);

                // Copy an empty byte array to the section
                _ = MemCpy(peHeaderPtr, new byte[8], 8);
            }
        }

        Console.WriteLine("Tables are zeroed out!");
        Console.ReadKey();
    }
}