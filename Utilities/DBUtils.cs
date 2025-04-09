using Microsoft.Dynamics.AX.Data.Management;
using Microsoft.Dynamics.Framework.Tools.MetaModel.Core;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using static System.Net.Mime.MediaTypeNames;

namespace Functions_for_Dynamics_Operations.Utilities
{
    internal class DBUtils
    {
        public static IVsOutputWindowPane CustomPane;

        public void SyncDBByModel()
        {
            IVsOutputWindow outWindow = Package.GetGlobalService(typeof(SVsOutputWindow)) as IVsOutputWindow;
            // Use e.g. Tools -> Create GUID to make a stable, but unique GUID for your pane.
            // Also, in a real project, this should probably be a static constant, and not a local variable
            Guid customGuid = new Guid("0F44E2D1-F5FA-4d2d-AB30-22BE8ECD9789");
            string customTitle = "9A developer tools";

            outWindow.CreatePane(ref customGuid, customTitle, 1, 1);
            outWindow.GetPane(ref customGuid, out CustomPane);
            CustomPane.Activate();

            IDesignMetaModelService modelService = VStudioUtils.GetDesignMetaModelService();
            string tables = "";

            SyncOptions syncOptions = new SyncOptions() { SyncMode = Microsoft.Dynamics.AX.Data.Management.Enum.SyncMode.PartialList, SkipInitialSchema = true, ParallelExecution = true, MonitorSync = true };
            IList<string> syncList = syncOptions.PartialSyncList;  

            foreach (string table in VStudioUtils.GetDesignMetaModelService().CurrentMetadataProvider.Tables.ListObjectsForModel(VStudioUtils.GetActiveAXProjectModelName()))
            {
                tables += tables == "" ? table : $",{table}";
                syncList.Add(table);
            }



            // Microsoft.Dynamics.AX.Framework.Database.Tools.SyncEngine.Run()
            /*
            if (Microsoft.Dynamics.AX.Framework.Database.Tools.SyncEngine.Run(@"C:\AOSService\PackagesLocalDirectory", modelService.CurrentConfiguration.BusinessDatabaseConnectionString, syncOptions))
            {
                CustomPane.OutputString($"Great success{Environment.NewLine}");
            }
            else
            {
                CustomPane.OutputString($"That escelated really fast{Environment.NewLine}");
            }
            */
            /*
            IVsOutputWindow outWindow = Package.GetGlobalService(typeof(SVsOutputWindow)) as IVsOutputWindow;
            // Use e.g. Tools -> Create GUID to make a stable, but unique GUID for your pane.
            // Also, in a real project, this should probably be a static constant, and not a local variable
            Guid customGuid = new Guid("0F44E2D1-F5FA-4d2d-AB30-22BE8ECD9789");
            string customTitle = "9A developer tools";

            outWindow.CreatePane(ref customGuid, customTitle, 1, 1);
            outWindow.GetPane(ref customGuid, out CustomPane);
            CustomPane.Activate();

            string arguments = $"-synclist=\"{tables}\" -syncmode=\"partiallist\" -skipInitialSchema -metadatabinaries=\"C:\\AOSService\\PackagesLocalDirectory\" -connect=\"{modelService.CurrentConfiguration.BusinessDatabaseConnectionString}\" -verbosity=\"Detailed\"";

            using (Process process = Process.GetCurrentProcess())
            {
                process.StartInfo.FileName = @"C:\AOSService\PackagesLocalDirectory\Bin\syncengine.exe";
                process.StartInfo.Arguments = arguments;
                process.StartInfo.RedirectStandardOutput = true;
                process.StartInfo.UseShellExecute = false;
                process.StartInfo.RedirectStandardOutput = true;
                process.StartInfo.RedirectStandardError = true;
                process.StartInfo.CreateNoWindow = true;
                process.OutputDataReceived += Process_OutputDataReceived;
                process.ErrorDataReceived += Process_ErrorDataReceived;
                process.Start();

                process.BeginOutputReadLine();
                process.BeginErrorReadLine();
            }
            */
        }

        public bool ValidateParameters(SyncOptions syncOptions)
        {
            /*
            string str = string.Empty;
            if (syncOptions.MetadataDir == null)
                str = "Metadata binary path is not specified\n";
            else if (((System.Enum)(object)syncOptions.SyncMode).HasFlag((System.Enum)(object)(SyncMode)131072) && !((System.Enum)(object)syncOptions.SyncMode).HasFlag((System.Enum)(object)(SyncMode)655360) && string.IsNullOrEmpty(this.AnalysisDirectory))
                str = "AnalysisDirectory is required for AnalysisOnly syncmode. Please provide an analysis directory with the parameter -analysisDirectory \n";
            else if (((System.Enum)(object)syncOptions.SyncMode).HasFlag((System.Enum)(object)(SyncMode)1) && syncOptions.PartialSyncList.Count == 0 && syncOptions.DropTableExtensionList.Count == 0 && this.TableExtensionList.Count<string>() == 0)
                str = "Tables or views for partial sync is not specified\n";
            else if (syncOptions.SyncMode != (SyncMode)5118 && syncOptions.SyncMode != (SyncMode)1053694 && ((System.Enum)(object)syncOptions.SyncMode).HasFlag((System.Enum)(object)(SyncMode)256) && this.DropTableOrViewList.Count<string>() == 0)
                str = "Tables or views for drop is not specified\n";
            else if (((System.Enum)(object)syncOptions.SyncMode).HasFlag((System.Enum)(object)(SyncMode)1024) && ((System.Enum)(object)syncOptions.SyncMode).HasFlag((System.Enum)(object)(SyncMode)512))
                str = "SyncMode can either be PartialSecurity or FullSecurity, not both\n";
            else if (((System.Enum)(object)syncOptions.SyncMode).HasFlag((System.Enum)(object)(SyncMode)262144) && ((System.Enum)(object)syncOptions.SyncMode).HasFlag((System.Enum)(object)(SyncMode)256))
                str = "Cannot drop tables in online mode\n";
            else if (((System.Enum)(object)syncOptions.SyncMode).HasFlag((System.Enum)(object)(SyncMode)1024) && !syncOptions.SecurityRoleList.Any() && !syncOptions.SecurityRoleExtensionList.Any<string>() && !this.SecurityDutyList.Any<string>() && !this.SecurityDutyExtensionList.Any<string>() && !this.SecurityPrivilegeList.Any<string>() && !this.DropDutyList.Any<string>() && !this.DropDutyExtensionList.Any<string>() && !this.DropRoleList.Any<string>() && !this.DropRoleExtensionList.Any<string>() && !this.DropPrivilegeList.Any<string>() && !this.DropPolicyList.Any<string>() && !this.SecurityPolicyList.Any<string>() && !this.MIDisplayList.Any<string>() && !this.MIActionList.Any<string>() && !this.MIOutputList.Any<string>() && !this.FormList.Any<string>() && !this.ReportList.Any<string>())
                str = "Security object list for partial security sync is not specified\n";
            else if (syncOptions.SyncMode != (SyncMode)5118 && syncOptions.SyncMode != (SyncMode)1053694 && syncOptions.SyncMode != (SyncMode)1315582 && syncOptions.SyncMode != (SyncMode)6553616 && syncOptions.SyncMode != (SyncMode)13959174 && ((System.Enum)(object)this.SyncMode).HasFlag((System.Enum)(object)(SyncMode)4096) && !this.ADEsList.Any<string>() && !this.DropADEsList.Any<string>())
                str = "Aggregate data entity for sync is not specified\n";
            if (string.IsNullOrEmpty(str))
                return string.IsNullOrEmpty(str);
            Console.WriteLine("Errors: \n" + str);
            */
            // Parameters.PrintHelp();
            return false;
        }


        private static void Process_ErrorDataReceived(object sender, DataReceivedEventArgs e)
        {
            if (sender is Process process)
            {
                /*
                using (var reader = process.StandardError)
                {
                    CustomPane.OutputString($"{reader.ReadLine()}{Environment.NewLine}");
                }
                */
            }
        }

        private static void Process_OutputDataReceived(object sender, DataReceivedEventArgs e)
        {
            if (e != null && e.Data != "")
            {
                CustomPane.OutputString($"{e.Data}{Environment.NewLine}");
            }
        }

    }
}
