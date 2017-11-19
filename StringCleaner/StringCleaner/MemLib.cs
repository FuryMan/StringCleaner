using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

namespace Memory
{
    public class Mem
    {
        private const int PROCESS_CREATE_THREAD = 2;

        private const int PROCESS_QUERY_INFORMATION = 1024;

        private const int PROCESS_VM_OPERATION = 8;

        private const int PROCESS_VM_WRITE = 32;

        private const int PROCESS_VM_READ = 16;

        private const uint MEM_COMMIT = 4096u;

        private const uint MEM_RESERVE = 8192u;

        private const uint PAGE_READWRITE = 4u;

        public static IntPtr pHandle;

        public Process procs = null;

        public Dictionary<string, IntPtr> modules = new Dictionary<string, IntPtr>();

        private ProcessModule mainModule;

        [DllImport("kernel32.dll")]
        public static extern IntPtr OpenProcess(uint dwDesiredAccess, int bInheritHandle, int dwProcessId);

        [DllImport("kernel32.dll")]
        private static extern bool WriteProcessMemory(IntPtr hProcess, IntPtr lpBaseAddress, string lpBuffer, UIntPtr nSize, out IntPtr lpNumberOfBytesWritten);

        [DllImport("kernel32.dll", CharSet = CharSet.Unicode)]
        private static extern uint GetPrivateProfileString(string lpAppName, string lpKeyName, string lpDefault, StringBuilder lpReturnedString, uint nSize, string lpFileName);

        [DllImport("kernel32.dll")]
        private static extern bool ReadProcessMemory(IntPtr hProcess, UIntPtr lpBaseAddress, [Out] byte[] lpBuffer, UIntPtr nSize, IntPtr lpNumberOfBytesRead);

        [DllImport("kernel32.dll", EntryPoint = "CloseHandle")]
        private static extern bool _CloseHandle(IntPtr hObject);

        [DllImport("kernel32.dll")]
        public static extern int CloseHandle(IntPtr hObject);

        [DllImport("kernel32.dll", CharSet = CharSet.Auto)]
        public static extern IntPtr GetModuleHandle(string lpModuleName);

        [DllImport("kernel32.dll")]
        private static extern bool WriteProcessMemory(IntPtr hProcess, UIntPtr lpBaseAddress, byte[] lpBuffer, UIntPtr nSize, IntPtr lpNumberOfBytesWritten);

        public bool OpenGameProcess(int procID)
        {
            bool flag = procID != 0;
            bool result;
            if (flag)
            {
                this.procs = Process.GetProcessById(procID);
                bool flag2 = !this.procs.Responding;
                if (flag2)
                {
                    result = false;
                }
                else
                {
                    Mem.pHandle = Mem.OpenProcess(2035711u, 1, procID);
                    this.mainModule = this.procs.MainModule;
                    this.getModules();
                    result = true;
                }
            }
            else
            {
                result = false;
            }
            return result;
        }

        public void getModules()
        {
            bool flag = this.procs == null;
            if (!flag)
            {
                this.modules.Clear();
                foreach (ProcessModule processModule in this.procs.Modules)
                {
                    bool flag2 = processModule.ModuleName != "" && processModule.ModuleName != null && !this.modules.ContainsKey(processModule.ModuleName);
                    if (flag2)
                    {
                        this.modules.Add(processModule.ModuleName, processModule.BaseAddress);
                    }
                }
            }
        }

        public int getProcIDFromName(string name)
        {
            Process[] processes = Process.GetProcesses();
            Process[] array = processes;
            int result;
            for (int i = 0; i < array.Length; i++)
            {
                Process process = array[i];
                bool flag = process.ProcessName == name;
                if (flag)
                {
                    result = process.Id;
                    return result;
                }
            }
            result = 0;
            return result;
        }

        public string LoadCode(string name, string file)
        {
            StringBuilder stringBuilder = new StringBuilder(1024);
            bool flag = file != "";
            if (flag)
            {
                uint privateProfileString = Mem.GetPrivateProfileString("codes", name, "", stringBuilder, (uint)file.Length, file);
            }
            else
            {
                stringBuilder.Append(name);
            }
            return stringBuilder.ToString();
        }

        private UIntPtr LoadUIntPtrCode(string name, string path = "")
        {
            string text = this.LoadCode(name, path);
            string value = text.Substring(text.IndexOf('+') + 1);
            bool flag = string.IsNullOrEmpty(value);
            UIntPtr result;
            if (flag)
            {
                result = (UIntPtr)0uL;
            }
            else
            {
                long num = 0;
                bool flag2 = Convert.ToInt64(value, 16) > 0;
                if (flag2)
                {
                    num = Convert.ToInt64(value, 16);
                }
                bool flag3 = text.Contains("base") || text.Contains("main");
                UIntPtr uIntPtr;
                if (flag3)
                {
                    uIntPtr = (UIntPtr)((ulong)((long)((int)this.mainModule.BaseAddress + num)));
                }
                else
                {
                    bool flag4 = !text.Contains("base") && !text.Contains("main") && text.Contains("+");
                    if (flag4)
                    {
                        string[] array = text.Split(new char[]
                        {
                            '+'
                        });
                        bool flag5 = this.modules.Count == 0 || !this.modules.ContainsKey(array[0]);
                        if (flag5)
                        {
                            this.getModules();
                        }
                        Debug.WriteLine("module=" + array[0]);
                        IntPtr value2 = this.modules[array[0]];
                        uIntPtr = (UIntPtr)((ulong)((long)((int)value2 + num)));
                    }
                    else
                    {
                        uIntPtr = (UIntPtr)((ulong)((long)num));
                    }
                }
                result = uIntPtr;
            }
            return result;
        }

        public string readString(string code, string file = "")
        {
            byte[] array = new byte[5];
            UIntPtr lpBaseAddress = this.getCode(code, file, 4);
            bool flag = !this.LoadCode(code, file).Contains(",");
            if (flag)
            {
                lpBaseAddress = this.LoadUIntPtrCode(code, file);
            }
            else
            {
                lpBaseAddress = this.getCode(code, file, 4);
            }
            bool flag2 = Mem.ReadProcessMemory(Mem.pHandle, lpBaseAddress, array, (UIntPtr)20uL, IntPtr.Zero);
            string result;
            if (flag2)
            {
                result = Encoding.UTF8.GetString(array);
            }
            else
            {
                result = "";
            }
            return result;
        }

        public bool WriteProcessMemory(string code, string type, string write, string file = "")
        {
            byte[] lpBuffer = new byte[4];
            int num = 4;
            bool flag = !this.LoadCode(code, file).Contains(",");
            UIntPtr lpBaseAddress;
            if (flag)
            {
                lpBaseAddress = this.LoadUIntPtrCode(code, file);
            }
            else
            {
                lpBaseAddress = this.getCode(code, file, 4);
            }
            bool flag2 = type == "float";
            if (flag2)
            {
                lpBuffer = BitConverter.GetBytes(Convert.ToSingle(write));
                num = 4;
            }
            else
            {
                bool flag3 = type == "int";
                if (flag3)
                {
                    lpBuffer = BitConverter.GetBytes(Convert.ToInt64(write));
                    num = 4;
                }
                else
                {
                    bool flag4 = type == "byte";
                    if (flag4)
                    {
                        lpBuffer = new byte[1];
                        lpBuffer = BitConverter.GetBytes(Convert.ToInt64(write));
                        num = 1;
                    }
                    else
                    {
                        bool flag5 = type == "string";
                        if (flag5)
                        {
                            lpBuffer = new byte[write.Length];
                            lpBuffer = Encoding.UTF8.GetBytes(write);
                            num = write.Length;
                        }
                    }
                }
            }
            return Mem.WriteProcessMemory(Mem.pHandle, lpBaseAddress, lpBuffer, (UIntPtr)((ulong)((long)num)), IntPtr.Zero);
        }

        private UIntPtr getCode(string name, string path, int size = 4)
        {
            string text = this.LoadCode(name, path);
            bool flag = text == "";
            UIntPtr result;
            if (flag)
            {
                result = UIntPtr.Zero;
            }
            else
            {
                string text2 = text;
                bool flag2 = text.Contains("+");
                if (flag2)
                {
                    text2 = text.Substring(text.IndexOf('+') + 1);
                }
                byte[] array = new byte[size];
                bool flag3 = text2.Contains(',');
                if (flag3)
                {
                    List<int> list = new List<int>();
                    string[] array2 = text2.Split(new char[]
                    {
                        ','
                    });
                    string[] array3 = array2;
                    for (int i = 0; i < array3.Length; i++)
                    {
                        string value = array3[i];
                        list.Add(Convert.ToInt32(value, 16));
                    }
                    int[] array4 = list.ToArray();
                    bool flag4 = text.Contains("base") || text.Contains("main");
                    if (flag4)
                    {
                        Mem.ReadProcessMemory(Mem.pHandle, (UIntPtr)((ulong)((long)((int)this.mainModule.BaseAddress + array4[0]))), array, (UIntPtr)((ulong)((long)size)), IntPtr.Zero);
                    }
                    else
                    {
                        bool flag5 = !text.Contains("base") && !text.Contains("main") && text.Contains("+");
                        if (flag5)
                        {
                            string[] array5 = text.Split(new char[]
                            {
                                '+'
                            });
                            IntPtr value2 = this.modules[array5[0]];
                            Mem.ReadProcessMemory(Mem.pHandle, (UIntPtr)((ulong)((long)((int)value2 + array4[0]))), array, (UIntPtr)((ulong)((long)size)), IntPtr.Zero);
                        }
                        else
                        {
                            Mem.ReadProcessMemory(Mem.pHandle, (UIntPtr)((ulong)((long)array4[0])), array, (UIntPtr)((ulong)((long)size)), IntPtr.Zero);
                        }
                    }
                    uint num = BitConverter.ToUInt32(array, 0);
                    UIntPtr uIntPtr = (UIntPtr)0uL;
                    for (int j = 1; j < array4.Length; j++)
                    {
                        uIntPtr = new UIntPtr(num + Convert.ToUInt32(array4[j]));
                        Mem.ReadProcessMemory(Mem.pHandle, uIntPtr, array, (UIntPtr)((ulong)((long)size)), IntPtr.Zero);
                        num = BitConverter.ToUInt32(array, 0);
                    }
                    result = uIntPtr;
                }
                else
                {
                    long num2 = Convert.ToInt64(text2, 16);
                    bool flag6 = text.Contains("base") || text.Contains("main");
                    if (flag6)
                    {
                        Mem.ReadProcessMemory(Mem.pHandle, (UIntPtr)((ulong)((long)((int)this.mainModule.BaseAddress + num2))), array, (UIntPtr)((ulong)((long)size)), IntPtr.Zero);
                    }
                    else
                    {
                        bool flag7 = !text.Contains("base") && !text.Contains("main") && text.Contains("+");
                        if (flag7)
                        {
                            string[] array6 = text.Split(new char[]
                            {
                                '+'
                            });
                            IntPtr value3 = this.modules[array6[0]];
                            Mem.ReadProcessMemory(Mem.pHandle, (UIntPtr)((ulong)((long)((int)value3 + num2))), array, (UIntPtr)((ulong)((long)size)), IntPtr.Zero);
                        }
                        else
                        {
                            Mem.ReadProcessMemory(Mem.pHandle, (UIntPtr)((ulong)((long)num2)), array, (UIntPtr)((ulong)((long)size)), IntPtr.Zero);
                        }
                    }
                    uint value4 = BitConverter.ToUInt32(array, 0);
                    UIntPtr uIntPtr2 = new UIntPtr(value4);
                    value4 = BitConverter.ToUInt32(array, 0);
                    result = uIntPtr2;
                }
            }
            return result;
        }

        public void closeProcess()
        {
            Mem.CloseHandle(Mem.pHandle);
        }
    }
}