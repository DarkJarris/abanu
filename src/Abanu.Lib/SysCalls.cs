﻿// This file is part of Abanu, an Operating System written in C#. Web: https://www.abanu.org
// Licensed under the GNU 2.0 license. See LICENSE.txt file in the project root for full license information.

using System;
using Abanu.Kernel;
using Abanu.Kernel.Core;

namespace Abanu.Runtime
{

    /// <summary>
    /// The API to the Kernel
    /// </summary>
    // Pure calls. This is no Framework. No helpers!
    public static unsafe class SysCalls
    {
        public static uint RequestMemory(uint size)
        {
            return MessageManager.Send(SysCallTarget.RequestMemory, size);
        }

        public static uint GetProcessIDForCommand(SysCallTarget target)
        {
            return MessageManager.Send(SysCallTarget.GetProcessIDForCommand, (uint)target);
        }

        public static uint GetPhysicalMemory(Addr physAddr, USize size)
        {
            return MessageManager.Send(SysCallTarget.GetPhysicalMemory, physAddr, size);
        }

        public static uint TranslateVirtualToPhysicalAddress(Addr virtAddr)
        {
            return MessageManager.Send(SysCallTarget.TranslateVirtualToPhysicalAddress, virtAddr);
        }

        public static MemoryRegion RequestMessageBuffer(uint size, uint targetProcessID)
        {
            return new MemoryRegion(MessageManager.Send(SysCallTarget.RequestMessageBuffer, size, targetProcessID), size);
        }

        public static void SetThreadPriority(int priority)
        {
            MessageManager.Send(SysCallTarget.SetThreadPriority, (uint)priority);
        }

        public static void ThreadSleep(uint time)
        {
            MessageManager.Send(SysCallTarget.ThreadSleep, time);
        }

        // TODO: Datetime
        public static long GetSystemTime()
        {
            throw new NotImplementedException();
        }

        public static Addr GetElfSectionsAddress()
        {
            return MessageManager.Send(SysCallTarget.GetElfSectionsAddress);
        }

        public static void GetFramebufferInfo(MemoryRegion buf)
        {
            MessageManager.Send(SysCallTarget.GetFramebufferInfo, buf.Start);
        }

        public static void SetServiceStatus(ServiceStatus status)
        {
            MessageManager.Send(SysCallTarget.SetServiceStatus, (uint)status);
        }

        public static void RegisterService(SysCallTarget target)
        {
            MessageManager.Send(SysCallTarget.RegisterService, (uint)target);
        }

        public static void RegisterInterrupt(byte irq)
        {
            MessageManager.Send(SysCallTarget.RegisterInterrupt, irq);
        }

        public static void WriteDebugMessage(MemoryRegion buf, string message)
        {
            NullTerminatedString.Set((byte*)buf.Start, message);
            MessageManager.Send(SysCallTarget.WriteDebugMessage, buf.Start);
        }

        public static int GetProcessByName(MemoryRegion buf, string processName)
        {
            NullTerminatedString.Set((byte*)buf.Start, processName);
            return (int)MessageManager.Send(SysCallTarget.GetProcessByName, buf.Start);
        }

        public static void KillProcess(int processID)
        {
            MessageManager.Send(SysCallTarget.KillProcess, (uint)processID);
        }

        public static FileHandle OpenFile(MemoryRegion buf, string path)
        {
            NullTerminatedString.Set((byte*)buf.Start, path);
            return (int)MessageManager.Send(SysCallTarget.OpenFile, buf.Start);
        }

        public static SSize ReadFile(FileHandle handle, MemoryRegion buf)
        {
            return MessageManager.Send(SysCallTarget.ReadFile, handle, buf.Start, buf.Size);
        }

        public static SSize WriteFile(FileHandle handle, MemoryRegion buf)
        {
            return MessageManager.Send(SysCallTarget.WriteFile, handle, buf.Start, buf.Size);
        }

        public static SSize CreateFifo(MemoryRegion buf, string path)
        {
            NullTerminatedString.Set((byte*)buf.Start, path);
            return (int)MessageManager.Send(SysCallTarget.CreateFifo, buf.Start);
        }

        public static void CreateMemoryProcess(MemoryRegion buf, uint size)
        {
            MessageManager.Send(SysCallTarget.CreateMemoryProcess, buf.Start, size);
        }

        public static void WriteDebugChar(char c)
        {
            MessageManager.Send(SysCallTarget.WriteDebugChar, (byte)c);
        }

    }

}
