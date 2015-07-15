using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.Threading;
using System.Diagnostics;

namespace heckdarn
{
    public class ProcessHelper
    {
        public Process Process { get; protected set; }
        public Window MainWindow { get; protected set; }
        protected ProcessHelper(Process process)
        {
            Process = process;
            MainWindow = new Window(Process.MainWindowHandle);
        }
        public static IEnumerable<ProcessHelper> FindAllByName(string name)
        {
            Process[] processes = Process.GetProcessesByName(name);
            if (processes == null || processes.Length == 0)
            {
                throw new Exception("Could not find process.");
            }
            foreach(Process process in processes)
                yield return new ProcessHelper(process);
        }
    }
}
