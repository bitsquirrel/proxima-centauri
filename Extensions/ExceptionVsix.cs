using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio;
using System;
using Microsoft.VisualStudio.Shell;

namespace Functions_for_Dynamics_Operations
{
    internal class ExceptionVsix : Exception
    {
        public void Log()
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            VStudioUtils.LogToOutput(ToString());
        }

        public void Log(string log)
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            VStudioUtils.LogToOutput($"{log} - Exception : {ToString()}");
        }
    }
}
