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

            VStudioUtils.LogToGenOutput(ToString());
        }

        public void Log(string log)
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            VStudioUtils.LogToGenOutput($"{log} - Exception : {ToString()}");
        }
    }
}
