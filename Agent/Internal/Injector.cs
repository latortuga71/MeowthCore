﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Agent.Internal
{
    public abstract class Injector
    {
        public abstract bool Inject(byte[] shellcode, int pid = 0, string exeToRun = @"C:\windows\system32\notepad.exe");
    }
}
