﻿using System;
using Mosa.Runtime;
using Mosa.Runtime.x86;

using System.Runtime.InteropServices;

namespace lonos.kernel.core
{
    /// <summary>
    /// Page Table
    /// </summary>
    public unsafe static class PageTableX86
    {

        public static uint AddrPageDirectory;
        public static uint AddrPageTable;

        public const uint PagesPerDictionaryEntry = 1024;
        public const uint EntriesPerPageEntryEntry = 1024;

        public const ulong InitialAddressableVirtMemory = 0x100000000; // 4GB
        public const uint InitialPageTableEntries = (uint)(InitialAddressableVirtMemory / 4096); // pages for 4GB
        public const uint InitialDirectoryEntries = InitialPageTableEntries / PagesPerDictionaryEntry; // pages for 4GB

        private const uint InitalPageDirectorySize = PageDirectoryEntry.EntrySize * InitialDirectoryEntries;
        private const uint InitalPageTableSize = PageTableEntry.EntrySize * InitialPageTableEntries;

        public const uint InitalMemoryAllocationSize = InitalPageDirectorySize + InitalPageTableSize;

        /// <summary>
        /// Sets up the PageTable
        /// </summary>
        public static void Setup(Addr entriesAddr)
        {
            KernelMessage.WriteLine("Setup PageTable");

            AddrPageDirectory = entriesAddr;
            AddrPageTable = entriesAddr + InitalPageDirectorySize;

            PrintAddress();

            // Setup Page Directory
            PageDirectoryEntry* pde = (PageDirectoryEntry*)AddrPageDirectory;
            PageTableEntry* pte = (PageTableEntry*)AddrPageTable;

            KernelMessage.WriteLine("Total Pages: {0}", InitialPageTableEntries);
            KernelMessage.WriteLine("Total Page Dictionary Entries: {0}", InitialDirectoryEntries);

            var pidx = 0;
            for (int didx = 0; didx < InitialDirectoryEntries; didx++)
            {
                for (int index = 0; index < EntriesPerPageEntryEntry; index++)
                {
                    pte[pidx] = new PageTableEntry();
                    pte[pidx].Present = true;
                    pte[pidx].Writable = true;
                    pte[pidx].User = true;
                    pte[pidx].PhysicalAddress = (uint)(pidx * 4096);

                    pidx++;
                }

                pde[didx] = new PageDirectoryEntry();
                pde[didx].Present = true;
                pde[didx].Writable = true;
                pde[didx].User = true;
                pde[didx].PageTableEntry = &pte[didx * PagesPerDictionaryEntry];
            }

            // TODO/BUG: Why this line will not be printed??
            KernelMessage.WriteLine("Total Page Table Entries: {0}", pidx);

            // Unmap the first page for null pointer exceptions
            MapVirtualAddressToPhysical(0x0, 0x0, false);

            // Set CR3 register on processor - sets page directory
            KernelMessage.WriteLine("Set CR3 to {0:X8}", AddrPageDirectory);
            Flush();

            KernelMessage.Write("Enable Paging... ");

            PageTable.EnableKernelWriteProtection();

            // Set CR0 register on processor - turns on virtual memory
            Native.SetCR0(Native.GetCR0() | 0x80000000);

            KernelMessage.WriteLine("Done");
        }

        private static void PrintAddress() {
            KernelMessage.WriteLine("PageDirectory: {0:X8}", AddrPageDirectory);
            KernelMessage.WriteLine("PageTable: {0:X8}", AddrPageTable);
        }

        private static bool WritableAddress(uint physAddr)
        {
            return false;
        }

        public static void KernelSetup(Addr entriesAddr)
        {
            AddrPageDirectory = entriesAddr;
            AddrPageTable = entriesAddr + InitalPageDirectorySize;
            //PrintAddress();
        }

        public static PageTableEntry* GetTableEntry(uint forVirtualAddress)
        {
            return (PageTableEntry*)(AddrPageTable + ((forVirtualAddress & 0xFFFFF000u) >> 10));
        }

        /// <summary>
        /// Maps the virtual address to physical.
        /// </summary>
        /// <param name="virtualAddress">The virtual address.</param>
        /// <param name="physicalAddress">The physical address.</param>
        public static void MapVirtualAddressToPhysical(Addr virtualAddress, Addr physicalAddress, bool present = true)
        {
            //FUTURE: traverse page directory from CR3 --- do not assume page table is linearly allocated

            //Intrinsic.Store32(new IntPtr(Address.PageTable), ((virtualAddress & 0xFFFFF000u) >> 10), physicalAddress & 0xFFFFF000u | 0x04u | 0x02u | (present ? 0x1u : 0x0u));

            // Hint: TableEntries are  not initialized. So set every field
            // Hint: All Table Entries have a known location (yet).
            // FUTURE: Change this! Allocate dynamicly

            var entry = GetTableEntry(virtualAddress);
            entry->PhysicalAddress = physicalAddress;
            entry->Present = present;
            entry->Writable = true;
            entry->User = true;

            //Native.Invlpg(virtualAddress); Not implemented in MOSA yet

            // workarround:
            Flush();
        }

        /// <summary>
        /// Gets the physical memory.
        /// </summary>
        /// <param name="virtualAddress">The virtual address.</param>
        /// <returns></returns>
        public static uint GetPhysicalAddressFromVirtual(IntPtr virtualAddress)
        {
            //FUTURE: traverse page directory from CR3 --- do not assume page table is linearly allocated
            return Intrinsic.Load32(new IntPtr(AddrPageTable), (((uint)virtualAddress.ToInt32() & 0xFFFFF000u) >> 10)) + ((uint)virtualAddress.ToInt32() & 0xFFFu);
        }

        public static void SetKernelWriteProtectionForAllInitialPages()
        {
            PageTableEntry* pte = (PageTableEntry*)AddrPageTable;
            for (int index = 0; index < InitialPageTableEntries; index++)
            {
                var e = &pte[index];
                e->Writable = false;
            }
        }

        public static void Flush()
        {
            Native.SetCR3(AddrPageDirectory);
        }

        public static void Flush(Addr virtAddr)
        {
            Flush(); //TODO: Use Native.InvPg!
        }

        public unsafe static void SetKernelWriteProtectionForRegion(uint virtAddr, uint size)
        {
            //KernelMessage.WriteLine("Unprotect Memory: Start={0:X}, End={1:X}", virtAddr, virtAddr + size);
            var pages = KMath.DivCeil(size, 4096);
            for (var i = 0; i < pages; i++)
            {
                var entry = GetTableEntry(virtAddr);
                entry->Writable = true;

                virtAddr += 4096;
            }

            PageTable.Flush();
        }

        [StructLayout(LayoutKind.Explicit, Pack = 1, Size = 4)]
        unsafe public struct PageDirectoryEntry
        {
            [FieldOffset(0)]
            private uint data;

            public const byte EntrySize = 4;

            private class Offset
            {
                public const byte Present = 0;
                public const byte Readonly = 1;
                public const byte User = 2;
                public const byte WriteThrough = 3;
                public const byte DisableCache = 4;
                public const byte Accessed = 5;
                private const byte UNKNOWN6 = 6;
                public const byte PageSize4Mib = 7;
                private const byte IGNORED8 = 8;
                public const byte Custom = 9;
                public const byte Address = 12;
            }

            private const byte AddressBitSize = 20;
            private const uint AddressMask = 0xFFFFF000;

            private uint PageTableAddress
            {
                get { return data & AddressMask; }
                set
                {
                    Assert.True(value << AddressBitSize == 0, "PageDirectoryEntry.Address needs to be 4k aligned");
                    data = data.SetBits(Offset.Address, AddressBitSize, value, Offset.Address);
                }
            }

            internal PageTableEntry* PageTableEntry
            {
                get { return (PageTableEntry*)PageTableAddress; }
                set { PageTableAddress = (uint)value; }
            }

            public bool Present
            {
                get { return data.IsBitSet(Offset.Present); }
                set { data = data.SetBit(Offset.Present, value); }
            }

            public bool Writable
            {
                get { return data.IsBitSet(Offset.Readonly); }
                set { data = data.SetBit(Offset.Readonly, value); }
            }

            public bool User
            {
                get { return data.IsBitSet(Offset.User); }
                set { data = data.SetBit(Offset.User, value); }
            }

            public bool WriteThrough
            {
                get { return data.IsBitSet(Offset.WriteThrough); }
                set { data = data.SetBit(Offset.WriteThrough, value); }
            }

            public bool DisableCache
            {
                get { return data.IsBitSet(Offset.DisableCache); }
                set { data = data.SetBit(Offset.DisableCache, value); }
            }

            public bool Accessed
            {
                get { return data.IsBitSet(Offset.Accessed); }
                set { data = data.SetBit(Offset.Accessed, value); }
            }

            public bool PageSize4Mib
            {
                get { return data.IsBitSet(Offset.PageSize4Mib); }
                set { data = data.SetBit(Offset.PageSize4Mib, value); }
            }

            public byte Custom
            {
                get { return (byte)data.GetBits(Offset.Custom, 2); }
                set { data = data.SetBits(Offset.Custom, 2, value); }
            }
        }



        [StructLayout(LayoutKind.Explicit, Pack = 1, Size = 4)]
        public struct PageTableEntry
        {
            [FieldOffset(0)]
            private uint Value;

            public const byte EntrySize = 4;

            private class Offset
            {
                public const byte Present = 0;
                public const byte Readonly = 1;
                public const byte User = 2;
                public const byte WriteThrough = 3;
                public const byte DisableCache = 4;
                public const byte Accessed = 5;
                public const byte Dirty = 6;
                private const byte SIZE0 = 7;
                public const byte Global = 8;
                public const byte Custom = 9;
                public const byte Address = 12;
            }

            private const byte AddressBitSize = 20;
            private const uint AddressMask = 0xFFFFF000;

            /// <summary>
            /// 4k aligned physical address
            /// </summary>
            public uint PhysicalAddress
            {
                get { return Value & AddressMask; }
                set
                {
                    Assert.True(value << AddressBitSize == 0, "PageTableEntry.PhysicalAddress needs to be 4k aligned");
                    Value = Value.SetBits(Offset.Address, AddressBitSize, value, Offset.Address);
                }
            }

            public bool Present
            {
                get { return Value.IsBitSet(Offset.Present); }
                set { Value = Value.SetBit(Offset.Present, value); }
            }

            public bool Writable
            {
                get { return Value.IsBitSet(Offset.Readonly); }
                set { Value = Value.SetBit(Offset.Readonly, value); }
            }

            public bool User
            {
                get { return Value.IsBitSet(Offset.User); }
                set { Value = Value.SetBit(Offset.User, value); }
            }

            public bool WriteThrough
            {
                get { return Value.IsBitSet(Offset.WriteThrough); }
                set { Value = Value.SetBit(Offset.WriteThrough, value); }
            }

            public bool DisableCache
            {
                get { return Value.IsBitSet(Offset.DisableCache); }
                set { Value = Value.SetBit(Offset.DisableCache, value); }
            }

            public bool Accessed
            {
                get { return Value.IsBitSet(Offset.Accessed); }
                set { Value = Value.SetBit(Offset.Accessed, value); }
            }

            public bool Global
            {
                get { return Value.IsBitSet(Offset.Global); }
                set { Value = Value.SetBit(Offset.Global, value); }
            }

            public bool Dirty
            {
                get { return Value.IsBitSet(Offset.Dirty); }
                set { Value = Value.SetBit(Offset.Dirty, value); }
            }

            public byte Custom
            {
                get { return (byte)Value.GetBits(Offset.Custom, 2); }
                set { Value = Value.SetBits(Offset.Custom, 2, value); }
            }

        }
    }


}