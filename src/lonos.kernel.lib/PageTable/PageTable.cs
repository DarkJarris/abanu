﻿using System;
using Mosa.Runtime;
using Mosa.Runtime.x86;

using System.Runtime.InteropServices;

namespace lonos.kernel.core
{
    /// <summary>
    /// Page Table
    /// </summary>
    public unsafe static class PageTable
    {

        public enum PageTableType
        {
            x86,
            x64
        }

        public static PageTableType Type;

        public static void ConfigureType(PageTableType type) => Type = type;

        public static USize InitalPageDirectorySize
        {
            get
            {
                if (Type == PageTableType.x86)
                    return PageTableX86.InitalPageDirectorySize;
                else
                    throw new NotImplementedException();
            }
        }

        public static USize InitalPageTableSize
        {
            get
            {
                if (Type == PageTableType.x86)
                    return PageTableX86.InitalPageTableSize;
                else
                    throw new NotImplementedException();
            }
        }

        public static void Setup(Addr addrPageDirectory, Addr addrPageTable, PageTableType type)
        {
            Type = type;
            if (type == PageTableType.x86)
                PageTableX86.Setup(addrPageDirectory, addrPageTable);
        }

        public static void KernelSetup(Addr addrPageDirectory, Addr addrPageTable, PageTableType type)
        {
            Type = type;
            if (type == PageTableType.x86)
                PageTableX86.KernelSetup(addrPageDirectory, addrPageTable);
        }

        public static void MapVirtualAddressToPhysical(Addr virtualAddress, Addr physicalAddress, bool present = true)
        {
            if (Type == PageTableType.x86)
                PageTableX86.MapVirtualAddressToPhysical(virtualAddress, physicalAddress, present);
        }

        public static void EnableKernelWriteProtection()
        {
            // Set CR0.WP
            Native.SetCR0(Native.GetCR0() | 0x10000);
        }

        public static void DisableKernelWriteProtection()
        {
            // Set CR0.WP
            Native.SetCR0((uint)(Native.GetCR0() & ~0x10000));
        }

        public static void SetKernelWriteProtectionForAllInitialPages()
        {
            if (Type == PageTableType.x86)
                PageTableX86.SetKernelWriteProtectionForAllInitialPages();
        }

        public static void Flush()
        {
            if (Type == PageTableType.x86)
                PageTableX86.Flush();
        }

        public static void Flush(Addr virtAddr)
        {
            if (Type == PageTableType.x86)
                PageTableX86.Flush(virtAddr);
        }

        public static void SetKernelWriteProtectionForRegion(uint virtAddr, uint size)
        {
            if (Type == PageTableType.x86)
                PageTableX86.SetKernelWriteProtectionForRegion(virtAddr, size);
        }

    }

}
