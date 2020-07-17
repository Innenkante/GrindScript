using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SoG.ModLauncher
{
    class Injection
    {
        private IntPtr _handle;

        public Injection(IntPtr handle)
        {
            _handle = handle;
        }

        private bool WriteString(IntPtr address, string text)
        {
            var buffer = new ASCIIEncoding().GetBytes(text);
            
            return WinApi.WriteProcessMemory(_handle, address, buffer, buffer.Length, out IntPtr bytesWritten);
        }

        private IntPtr AllocateMemory()
        {
           return WinApi.VirtualAllocEx(_handle, IntPtr.Zero, 1024, WinApi.AllocationType.Commit,
                WinApi.MemoryProtection.ExecuteReadWrite);

        }

        private IntPtr GetLoadLibrary()
        {
            var module = WinApi.GetModuleHandle("kernel32.dll");
            var loadLibrary = WinApi.GetProcAddress(module, "LoadLibraryA");

            return loadLibrary;
        }

        public bool Inject(string module)
        {
            var loadLibrary = GetLoadLibrary();

            var modulePointer = AllocateMemory();

            WriteString(modulePointer, module);

            var result = WinApi.CreateRemoteThread(_handle, IntPtr.Zero, 0, loadLibrary, modulePointer, 0, out IntPtr threadId);

            if (result != IntPtr.Zero)
                return true;
            else
            {
                return false; //else not needed but for the sake of readability left in there
            }

        }
    }
}
