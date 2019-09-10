﻿// This file is part of Lonos Project, an Operating System written in C#. Web: https://www.lonos.io
// Licensed under the GNU 2.0 license. See LICENSE.txt file in the project root for full license information.

namespace lonos.Kernel.Core.Scheduling
{
    public enum ThreadStatus
    {
        Empty = 0,
        Creating,
        ScheduleForStart,
        Running,
        Waiting,
        Terminated,
    }
}
