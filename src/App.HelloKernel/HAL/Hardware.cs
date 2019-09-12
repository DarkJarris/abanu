﻿// This file is part of Lonos Project, an Operating System written in C#. Web: https://www.lonos.io
// Licensed under the GNU 2.0 license. See LICENSE.txt file in the project root for full license information.

using System;
using Lonos;
using Lonos.Runtime;
using Mosa.DeviceSystem;
using Mosa.Runtime.x86;

namespace Lonos.Kernel
{
    /// <summary>
    /// Hardware
    /// </summary>
    public sealed class Hardware : BaseHardwareAbstraction
    {
        /// <summary>
        /// Gets the size of the page.
        /// </summary>
        public override uint PageSize => 4096;

        /// <summary>
        /// Gets a block of memory from the kernel
        /// </summary>
        /// <param name="address">The address.</param>
        /// <param name="size">The size.</param>
        public override ConstrainedPointer GetPhysicalMemory(IntPtr address, uint size)
        {
            var virtAddr = (Addr)SysCalls.GetPhysicalMemory(address, size);

            return new ConstrainedPointer(virtAddr, size);
        }

        /// <summary>
        /// Disables all interrupts.
        /// </summary>
        public override void DisableAllInterrupts()
        {
            Native.Cli();
        }

        /// <summary>
        /// Enables all interrupts.
        /// </summary>
        public override void EnableAllInterrupts()
        {
            Native.Sti();
        }

        /// <summary>
        /// Processes the interrupt.
        /// </summary>
        /// <param name="irq">The irq.</param>
        public override void ProcessInterrupt(byte irq)
        {
            HAL.ProcessInterrupt(irq);
        }

        /// <summary>
        /// Sleeps the specified milliseconds.
        /// </summary>
        /// <param name="milliseconds">The milliseconds.</param>
        public override void Sleep(uint milliseconds)
        {
        }

        /// <summary>
        /// Allocates the virtual memory.
        /// </summary>
        public override ConstrainedPointer AllocateVirtualMemory(uint size, uint alignment)
        {
            var address = (IntPtr)SysCalls.RequestMemory(size);

            return new ConstrainedPointer(address, size);
        }

        /// <summary>
        /// Gets the physical address.
        /// </summary>
        public override IntPtr TranslateVirtualToPhysicalAddress(IntPtr virtualAddress)
        {
            return (IntPtr)SysCalls.TranslateVirtualToPhysicalAddress(virtualAddress);
        }

        /// <summary>
        /// Requests an IO read/write port interface from the kernel
        /// </summary>
        /// <param name="port">The port number.</param>
        public override BaseIOPortReadWrite GetReadWriteIOPort(ushort port)
        {
            return new X86IOPortReadWrite(port);
        }

        /// <summary>
        /// Requests an IO read/write port interface from the kernel
        /// </summary>
        /// <param name="port">The port number.</param>
        public override BaseIOPortRead GetReadIOPort(ushort port)
        {
            return new X86IOPortReadWrite(port);
        }

        /// <summary>
        /// Requests an IO write port interface from the kernel
        /// </summary>
        /// <param name="port">The port number.</param>
        public override BaseIOPortWrite GetWriteIOPort(ushort port)
        {
            return new X86IOPortWrite(port);
        }

        /// <summary>
        /// Debugs the write.
        /// </summary>
        /// <param name="message">The message.</param>
        public override void DebugWrite(string message)
        {
            //Boot.Console.Write(message);
        }

        /// <summary>
        /// Debugs the write line.
        /// </summary>
        /// <param name="message">The message.</param>
        public override void DebugWriteLine(string message)
        {
            //Boot.Console.WriteLine(message);
        }

        /// <summary>
        /// Aborts with the specified message.
        /// </summary>
        /// <param name="message">The message.</param>
        public override void Abort(string message)
        {
            //Panic.Error(message);
        }
    }
}