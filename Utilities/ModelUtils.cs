using System;
using Excel = Microsoft.Office.Interop.Excel;
using Functions_for_Dynamics_Operations.Constants;
using Microsoft.Office.Interop.Excel;
using System.IO;
using System.Runtime.InteropServices;

namespace Functions_for_Dynamics_Operations.Utilities
{
    internal class ModelUtils
    {
        public void ExportModelDetails()
        {
            Models models = new Models(VStudioUtils.GetDesignMetaModelService());
            // Create an Excel application and workbook
            Excel.Application excelApp = new Excel.Application();
            Workbook workbook = excelApp.Workbooks.Add(Type.Missing);
            Worksheet worksheet = (Worksheet)workbook.ActiveSheet;

            worksheet.Name = "D365 F&O Models";
            int row = 1;

            SetRowHeaders(worksheet, row);

            foreach (ModelObjectCount counts in models.ModelCounts)
            {
                row++;

                SetModelRowValues(worksheet, row, counts);
            }

            string dekstopFilename = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), $"Model Objects Export {DateTime.UtcNow.ToString("dd-MM-yyyy hh:mm").Replace(":", "")}");

            workbook.SaveAs(dekstopFilename, XlFileFormat.xlWorkbookDefault, Type.Missing, Type.Missing, false, false, XlSaveAsAccessMode.xlNoChange, XlSaveConflictResolution.xlLocalSessionChanges, Type.Missing, Type.Missing);

            excelApp.Quit();
            Marshal.ReleaseComObject(worksheet);
            Marshal.ReleaseComObject(workbook);
            Marshal.ReleaseComObject(excelApp);

            VStudioUtils.LogToOutput($"{dekstopFilename} created");
        }

        public void SetRowHeaders(Worksheet worksheet, int row)
        {
            worksheet.Cells[row, 1] = "Model name";
            worksheet.Cells[row, 2] = "Publisher";
            worksheet.Cells[row, 3] = "Layer";
            worksheet.Cells[row, 4] = "Version";

            worksheet.Cells[row, 5] = "Enums";
            worksheet.Cells[row, 6] = "Enum Extensions";
            worksheet.Cells[row, 7] = "Edts";
            worksheet.Cells[row, 8] = "Edt Extensions";
            worksheet.Cells[row, 9] = "Tables";
            worksheet.Cells[row, 10] = "Table Extensions";
            worksheet.Cells[row, 11] = "Views";
            worksheet.Cells[row, 12] = "View Extensions";
            worksheet.Cells[row, 13] = "Queries";
            worksheet.Cells[row, 14] = "Query Extensions";
            worksheet.Cells[row, 15] = "Data Entities";
            worksheet.Cells[row, 16] = "Data Entity Extensions";
            worksheet.Cells[row, 17] = "Composite Data Entities";
            worksheet.Cells[row, 18] = "Aggregate Data Entities";
            worksheet.Cells[row, 19] = "Maps";
            worksheet.Cells[row, 20] = "Table Collections";
            worksheet.Cells[row, 21] = "Classes";
            worksheet.Cells[row, 22] = "Forms";
            worksheet.Cells[row, 23] = "Form Extensions";
            worksheet.Cells[row, 24] = "Tiles";
            worksheet.Cells[row, 25] = "Menus";
            worksheet.Cells[row, 26] = "Menu Extensions";
            worksheet.Cells[row, 27] = "Action Menu Items";
            worksheet.Cells[row, 28] = "Output Menu Items";
            worksheet.Cells[row, 29] = "Display Menu Items";
            worksheet.Cells[row, 30] = "Action Menu Item Extensions";
            worksheet.Cells[row, 31] = "Output Menu Item Extensions";
            worksheet.Cells[row, 32] = "Display Menu Item Extensions";
            worksheet.Cells[row, 33] = "Perspectives";
            worksheet.Cells[row, 34] = "Kpis";
            worksheet.Cells[row, 35] = "Reports";
            worksheet.Cells[row, 36] = "Report Style Templates";
            worksheet.Cells[row, 37] = "Report Data Sources";
            worksheet.Cells[row, 38] = "Report Images";
            worksheet.Cells[row, 39] = "Workflow Categories";
            worksheet.Cells[row, 40] = "Workflow Approvals";
            worksheet.Cells[row, 41] = "Workflow Approval Extenions";
            worksheet.Cells[row, 42] = "Workflow Tasks";
            worksheet.Cells[row, 43] = "Workflow Task Extensions";
            worksheet.Cells[row, 44] = "Workflow Automated Tasks";
            worksheet.Cells[row, 45] = "Workflow Types";
            worksheet.Cells[row, 46] = "Workflow Type Extensions";
            worksheet.Cells[row, 47] = "Providers";
            worksheet.Cells[row, 48] = "Label Files";
            worksheet.Cells[row, 49] = "Resources";
            worksheet.Cells[row, 50] = "Configurations";
            worksheet.Cells[row, 51] = "Security Roles";
            worksheet.Cells[row, 52] = "Security Role Extenions";
            worksheet.Cells[row, 53] = "Security Duties";
            worksheet.Cells[row, 54] = "Security Duty Extenions";
            worksheet.Cells[row, 55] = "Security Privileges";
            worksheet.Cells[row, 56] = "Security Policies";
            worksheet.Cells[row, 57] = "References";
            worksheet.Cells[row, 58] = "Services";
            worksheet.Cells[row, 59] = "Service Groups";
        }

        internal void SetModelRowValues(Worksheet worksheet, int row, ModelObjectCount modelCounts)
        {
            worksheet.Cells[row, 1] = modelCounts.ModelInfo.Name;
            worksheet.Cells[row, 2] = modelCounts.ModelInfo.Publisher;
            worksheet.Cells[row, 3] = modelCounts.ModelInfo.Layer;
            worksheet.Cells[row, 4] = modelCounts.ModelInfo.VersionBuild;

            worksheet.Cells[row, 5] = modelCounts.Enums;
            worksheet.Cells[row, 6] = modelCounts.EnumExtensions;
            worksheet.Cells[row, 7] = modelCounts.Edts;
            worksheet.Cells[row, 8] = modelCounts.EdtExtensions;
            worksheet.Cells[row, 9] = modelCounts.Tables;
            worksheet.Cells[row, 10] = modelCounts.TableExtensions;
            worksheet.Cells[row, 11] = modelCounts.Views;
            worksheet.Cells[row, 12] = modelCounts.ViewExtensions;
            worksheet.Cells[row, 13] = modelCounts.Queries;
            worksheet.Cells[row, 14] = modelCounts.QueryExtensions;
            worksheet.Cells[row, 15] = modelCounts.DataEntities;
            worksheet.Cells[row, 16] = modelCounts.DataEntityExtensions;
            worksheet.Cells[row, 17] = modelCounts.CompositeDataEntities;
            worksheet.Cells[row, 18] = modelCounts.AggregateDataEntities;
            worksheet.Cells[row, 19] = modelCounts.Maps;
            worksheet.Cells[row, 20] = modelCounts.TableCollections;
            worksheet.Cells[row, 21] = modelCounts.Classes;
            worksheet.Cells[row, 22] = modelCounts.Forms;
            worksheet.Cells[row, 23] = modelCounts.FormExtensions;
            worksheet.Cells[row, 24] = modelCounts.Tiles;
            worksheet.Cells[row, 25] = modelCounts.Menus;
            worksheet.Cells[row, 26] = modelCounts.MenuExtensions;
            worksheet.Cells[row, 27] = modelCounts.ActionMenuItems;
            worksheet.Cells[row, 28] = modelCounts.OutputMenuItems;
            worksheet.Cells[row, 29] = modelCounts.DisplayMenuItems;
            worksheet.Cells[row, 30] = modelCounts.ActionMenuItemExtensions;
            worksheet.Cells[row, 31] = modelCounts.OutputMenuItemExtensions;
            worksheet.Cells[row, 32] = modelCounts.DisplayMenuItemExtensions;
            worksheet.Cells[row, 33] = modelCounts.Perspectives;
            worksheet.Cells[row, 34] = modelCounts.Kpis;
            worksheet.Cells[row, 35] = modelCounts.Reports;
            worksheet.Cells[row, 36] = modelCounts.ReportStyleTemplates;
            worksheet.Cells[row, 37] = modelCounts.ReportDataSources;
            worksheet.Cells[row, 38] = modelCounts.ReportImages;
            worksheet.Cells[row, 39] = modelCounts.WorkflowCategories;
            worksheet.Cells[row, 40] = modelCounts.WorkflowApprovals;
            worksheet.Cells[row, 41] = modelCounts.WorkflowApprovalExtenions;
            worksheet.Cells[row, 42] = modelCounts.WorkflowTasks;
            worksheet.Cells[row, 43] = modelCounts.WorkflowTaskExtensions;
            worksheet.Cells[row, 44] = modelCounts.WorkflowAutomatedTasks;
            worksheet.Cells[row, 45] = modelCounts.WorkflowTypes;
            worksheet.Cells[row, 46] = modelCounts.WorkflowTypeExtensions;
            worksheet.Cells[row, 47] = modelCounts.Providers;
            worksheet.Cells[row, 48] = modelCounts.LabelFiles;
            worksheet.Cells[row, 49] = modelCounts.Resources;
            worksheet.Cells[row, 50] = modelCounts.Configurations;
            worksheet.Cells[row, 51] = modelCounts.SecurityRoles;
            worksheet.Cells[row, 52] = modelCounts.SecurityRoleExtenions;
            worksheet.Cells[row, 53] = modelCounts.SecurityDuties;
            worksheet.Cells[row, 54] = modelCounts.SecurityDutyExtenions;
            worksheet.Cells[row, 55] = modelCounts.SecurityPrivileges;
            worksheet.Cells[row, 56] = modelCounts.SecurityPolicies;
            worksheet.Cells[row, 57] = modelCounts.References;
            worksheet.Cells[row, 58] = modelCounts.Services;
            worksheet.Cells[row, 59] = modelCounts.ServiceGroups;
        }
    }
}
