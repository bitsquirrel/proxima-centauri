using System;
using Microsoft.Dynamics.AX.Metadata.Core.MetaModel;
using Microsoft.Dynamics.AX.Metadata.MetaModel;
using Microsoft.Dynamics.Framework.Tools.ProjectSystem;

namespace Functions_for_Dynamics_Operations.ToolboxCode
{
    internal class SysOpContractCode : GenToolbox
    {
        public SysOpContractCode(VSProjectNode vSProjectNode, string prefix) 
        {
            VSProjectNode = vSProjectNode;
            Prefix = prefix;
        }

        internal override string ClassName()
        {
            // Override this to return the class name
            return "SysOpContract";
        }

        internal override void CreateClassMethods()
        {
            axClass.AddMethod(CreateConstructMethod());

            axClass.AddMethod(CreatePackMethod());

            axClass.AddMethod(CreateUnpackMethod());

            axClass.AddMethod(CreateParmPackedMethod());
        }

        internal AxMethod CreateConstructMethod()
        {
            return new AxMethod
            {
                Name = "construct",
                Visibility = CompilerVisibility.Internal,
                IsStatic = true,
                Source = $@"    public static {Prefix}{ClassName()} construct()
    {{
        return new {Prefix}{ClassName()}();
    }}{Environment.NewLine}"
            };
        }

        internal AxMethod CreateParmPackedMethod()
        {
            return new AxMethod
            {
                Visibility = CompilerVisibility.Internal,
                Name = "parmPackedObject",
                Source = $@"    [DataMemberAttribute('PackedObject'), SysOperationControlVisibility(false)]
    public container parmPackedObject(container _packedObject = packedObject)
    {{
        packedObject = _packedObject;
        return packedObject;
    }}{Environment.NewLine}"  
            };
        }

        internal AxMethod CreatePackMethod()
        {
            return new AxMethod
            {
                Visibility = CompilerVisibility.Internal,
                Name = "pack",

                Source = $@"    public container pack()
    {{
        return [#CurrentVersion, #CurrentList];
    }}{Environment.NewLine}"
            };
        }

        internal AxMethod CreateUnpackMethod()
        {
            return new AxMethod
            {
                Visibility = CompilerVisibility.Internal,
                Name = "unpack",
                Source = $@"    public boolean unpack(container packedClass)
    {{
        Version version = RunBase::getVersion(packedClass);

        switch (version)
        {{
            case #CurrentVersion:
                [version, #CurrentList] = packedClass;
                break;
            default:
                return false;
        }}

        return true;
    }}{Environment.NewLine}"
            };
        }

        internal override string GetClassCode()
        {
            return $@"[DataContractAttribute('Contract')]
internal class {Prefix}{ClassName()} implements SysPackable
{{
    public MethodName methodName;
    public ClassName className;
    
    container packedObject;

    #define.CurrentVersion(1)
    #localmacro.CurrentList
        packedObject
    #endmacro

}}";
        }

        internal string GetConstructorCode()
        {
            return $@"public static SysOpContract construct()";
        }
    }
}
