using EnvDTE;
using Functions_for_Dynamics_Operations.Functions;
using Microsoft.Dynamics.AX.Framework.BestPractices;
using Microsoft.Dynamics.AX.Framework.BestPractices.FixerIntegration;
using Microsoft.Dynamics.AX.Metadata.MetaModel;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using System;
using System.Data;
using System.IO;
using System.Xml;

namespace Functions_for_Dynamics_Operations
{
    internal class BestPracticeFunc
    {
        readonly string BpFileLocation, BpSuppressionPath;
        readonly ModelInfo ModelInfo;
        readonly string ModelPath;

        public BestPracticeFunc(ModelInfo modelInfo)
        {
            ModelInfo = modelInfo;
            // Between UDE and OneBox there are differences in the locations of the model stores
            string modelStore = "", notUsed = "";

            if (RuntimeHost.IsCloudHosted())
            {
                (modelStore, notUsed) = RuntimeHost.GetModelStoreAndFrameworkDirectories();
            }
            else
                modelStore = Microsoft.Dynamics.Framework.Tools.Configuration.ConfigurationHelper.CurrentConfiguration.ModelStoreFolder;

            BpSuppressionPath = Path.Combine(modelStore, ModelInfo.Module);
            // The model key consists of two directories
            ModelPath = Path.Combine(modelStore, ModelInfo.Key.ToString());
            // Using the model name prefix the BP suppression file name
            BpFileLocation = Path.Combine(new[] { ModelPath, "AxIgnoreDiagnosticList", $"{ModelInfo.Name}_BPSuppressions.xml" });

            InitFile();
        }

        protected void InitFile()
        {
            if (ModelInfo != null)
            {
                if (!File.Exists(BpFileLocation))
                {
                    File.WriteAllText(BpFileLocation, GetNewFileContent());

                    VStudioUtils.LogToOutput($"Best Practice file created{Environment.NewLine}");
                }
            }
        }

        public void WriteToBPSuppressionFile(BestPracticeFound bestPracticeFound)
        {
            XmlDocument xmlDocument = new XmlDocument();
            xmlDocument.Load(BpFileLocation);

            XmlElement element = xmlDocument.CreateElement("Diagnostic");

            XmlNode xmlNode = xmlDocument.CreateNode(XmlNodeType.Element, "DiagnosticType", "");
            xmlNode.InnerXml = bestPracticeFound.DiagnosticType;

            element.AppendChild(xmlNode);

            XmlNode xmlNode1 = xmlDocument.CreateNode(XmlNodeType.Element, "Severity", "");
            xmlNode1.InnerXml = bestPracticeFound.Severity;

            element.AppendChild(xmlNode1);

            XmlNode xmlNode2 = xmlDocument.CreateNode(XmlNodeType.Element, "Path", "");
            xmlNode2.InnerXml = bestPracticeFound.Path;

            element.AppendChild(xmlNode2);

            XmlNode xmlNode3 = xmlDocument.CreateNode(XmlNodeType.Element, "Moniker", "");
            xmlNode3.InnerXml = bestPracticeFound.Moniker;

            element.AppendChild(xmlNode3);

            XmlNode xmlNode4 = xmlDocument.CreateNode(XmlNodeType.Element, "Justification", "");
            xmlNode4.InnerXml = bestPracticeFound.Justification;

            element.AppendChild(xmlNode4);

            XmlNodeList xmlNodeList = xmlDocument.GetElementsByTagName("Diagnostic");
            if (xmlNodeList != null)
            {
                foreach (XmlElement xmlNode5 in xmlNodeList)
                {
                    if (xmlNode5.SelectSingleNode("Path").InnerXml.ToLower() == bestPracticeFound.Path.ToLower()
                        && xmlNode5.SelectSingleNode("Moniker").InnerXml.ToLower() == bestPracticeFound.Moniker.ToLower()
                        && xmlNode5.SelectSingleNode("Justification").InnerXml.ToLower() == bestPracticeFound.Justification.ToLower()
                        && xmlNode5.SelectSingleNode("DiagnosticType").InnerXml.ToLower() == bestPracticeFound.DiagnosticType.ToLower())
                    {
                        // This best practice already exists
                        return;
                    }
                }
            }

            // If not found then just add it to the 
            XmlNode xmlNodeItems = xmlDocument.SelectSingleNode("/IgnoreDiagnostics/Items");

            xmlNodeItems.AppendChild(element);

            // Save the file
            xmlDocument.Save(BpFileLocation);
        }

        public DataTable LoadBPFile()
        {
            return GetBestPractices();
        }

        public DataTable GetBestPractices()
        {
            DataTable dataTable = GridUtils.GetAXDataTableBP();
            // Return empty if the file does not exist
            if (!File.Exists(Path.Combine(BpSuppressionPath, "BPCheck.xml")))
                return dataTable;

            XmlDocument xmlDocument = new XmlDocument();
            xmlDocument.Load(Path.Combine(BpSuppressionPath, "BPCheck.xml"));

            XmlNodeList diagnostics = xmlDocument.GetElementsByTagName("Diagnostic");
            foreach (XmlNode xmlNode in diagnostics)
            {
                string diagnosticType = xmlNode.SelectSingleNode("DiagnosticType") != null ? xmlNode.SelectSingleNode("DiagnosticType").InnerText : "";
                string severity = xmlNode.SelectSingleNode("Severity") != null ? xmlNode.SelectSingleNode("Severity").InnerXml : "";
                string path = xmlNode.SelectSingleNode("Path") != null ? xmlNode.SelectSingleNode("Path").InnerXml : "";
                string moniker = xmlNode.SelectSingleNode("Moniker") != null ? xmlNode.SelectSingleNode("Moniker").InnerXml : "";
                string elementType = xmlNode.SelectSingleNode("ElementType") != null ? xmlNode.SelectSingleNode("ElementType").InnerXml : "";
                string message = xmlNode.SelectSingleNode("Message") != null ? xmlNode.SelectSingleNode("Message").InnerXml : "";
                // Bypass informational, is not relevant
                if (severity == "Informational")
                    continue;

                GridUtils.SetRowValueBP(dataTable, severity, path, elementType, message, moniker, diagnosticType);
            }

            return dataTable;
        }

#warning TESTING
        public static void JumpToBestPractice(string path)
        {
            // Microsoft.Dynamics.Framework.Tools.MetaModel
            foreach (Microsoft.Dynamics.AX.Framework.BestPractices.Extensions.BestPracticeCheckerTargets item in Microsoft.Dynamics.AX.Framework.BestPractices.Extensions.BPExtensionsUtil.GetOnlyElementTargets())
            {

            }

            BestPracticeFixerVSRunner bestPractice = new BestPracticeFixerVSRunner();

            bestPractice.RunBPFixersOnProject(VStudioUtils.GetSelectedProjectNode());



            foreach (var item in Microsoft.Dynamics.AX.Framework.BestPractices.BPDiagnosticsHandler.CurrentDiagnosticsHandler.Diagnostics())
            {
                DTE dte = Package.GetGlobalService(typeof(SDTE)) as DTE;

                var obj = Microsoft.Dynamics.Framework.Tools.MetaModel.MetaModelUtility.GetDocDataFromRDT(item.Path);

            }
            /*
            Microsoft.Dynamics.AX.Framework.BestPractices.IBestPracticeChecker bestPracticeChecker = (IBestPracticeChecker)new Microsoft.Dynamics.AX.Framework.BestPractices.CheckerBase();
            
            Microsoft.Dynamics.AX.Framework.BestPractices.CheckerBase checkerBase = new Microsoft.Dynamics.AX.Framework.BestPractices.CheckerBase();
            */
        }

        protected string GetNewFileContent()
        {
            return $@"<?xml version=""1.0"" encoding=""utf-8""?>
<IgnoreDiagnostics xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" xmlns:xsd=""http://www.w3.org/2001/XMLSchema"">
  <xs:schema xmlns:xs=""http://www.w3.org/2001/XMLSchema"" id=""defaultSchema"" attributeFormDefault=""unqualified"" elementFormDefault=""qualified"">
    <xsd:element name=""Name"" type=""xsd:string"" />
    <xsd:element name=""Items"">
      <xsd:complexType>
        <xsd:sequence>
          <xsd:element minOccurs=""0"" maxOccurs=""unbounded"" name=""Diagnostic"">
            <xsd:complexType>
              <xsd:sequence>
                <xsd:element name=""DiagnosticType"" type=""xsd:string"" />
                <xsd:element name=""Severity"" type=""xsd:string"" />
                <xsd:element name=""Path"" type=""xsd:string"" />
                <xsd:element name=""Moniker"" type=""xsd:string"" />
                <xsd:element name=""Justification"" type=""xsd:string"" />
              </xsd:sequence>
            </xsd:complexType>
          </xsd:element>
        </xsd:sequence>
      </xsd:complexType>
    </xsd:element>
  </xs:schema>
  <Name>{ModelInfo.Name}_BPSuppressions</Name>
  <Items>
  </Items>
</IgnoreDiagnostics>";
        }
    }

    internal class BestPractice
    {
        public string DiagnosticType { get; set; }
        public string Severity { get; set; }
        public string Path { get; set; }
        public string Moniker { get; set; }
        public string Justification { get; set; }
    }

    internal class BestPracticeFound : BestPractice
    {
        public string ElementType { get; set; }
        public string Message { get; set; }
    }
}
