using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Dynamics.AX.Metadata.Core.MetaModel;
using Microsoft.Dynamics.AX.Metadata.MetaModel;

namespace Functions_for_Dynamics_Operations.ToolbaxCode
{
    internal class SysOpBaseCode : GenToolbox
    {
        public SysOpBaseCode(string prefix) 
        {
            Prefix = prefix;
        }

        internal override string ClassName()
        {
            // Override this to return the class name
            return "SysOpBase";
        }

        internal override void CreateClassMethods()
        {
            axClass.AddMethod(CreateInitMethod());
            axClass.AddMethod(CreateRunServiceMethod());
            axClass.AddMethod(CreateProcessMethod());
            axClass.AddMethod(CreateScheduleRunThreadMethod());
            axClass.AddMethod(CreateSetContractMethod());
            axClass.AddMethod(CreateSetContractInternalMethod());
            axClass.AddMethod(CreateFinishMethod());
        }

        internal AxMethod CreateInitMethod()
        {
            return new AxMethod
            {
                Name = "init",
                Visibility = CompilerVisibility.Internal,
                Source = $@"    public void init()
    {{
        if(this.isExecutingInBatch())
        {{
            batchHeader = BatchHeader::construct(this.getCurrentBatchHeader().parmBatchHeaderId());
        }}
        // Unique id per run 
        processRunId = newGuid();
    }}{Environment.NewLine}"
            };
        }

        internal AxMethod CreateRunServiceMethod()
        {
            return new AxMethod
            {
                Name = "runService",
                Visibility = CompilerVisibility.Internal,
                Source = $@"    public void runService({Prefix}SysOpContract _contract)
    {{
        contract = _contract;

        this.init();

        this.process();

        this.finish();
    }}{Environment.NewLine}"
            };
        }

        internal AxMethod CreateProcessMethod()
        {
            return new AxMethod
            {
                Name = "process",
                Visibility = CompilerVisibility.Protected,
                Source = $@"    protected void process()
    {{
    }}{Environment.NewLine}"
            };
        }

        internal AxMethod CreateScheduleRunThreadMethod()
        {
            return new AxMethod
            {
                Name = "scheduleRunThread",
                Visibility = CompilerVisibility.Public,
                Source = $@"    public SysOperationServiceController scheduleRunThread({Prefix}SysOpContract _threadContract, str _caption)
    {{
        SysOperationServiceController thread = new SysOperationServiceController(_threadContract.className, _threadContract.methodName, SysOperationExecutionMode::Synchronous);
        this.setContract(thread.getDataContractObject('_contract'), _threadContract);
        if (this.isExecutingInBatch() && batchHeader)
        {{
            thread.batchInfo().parmCaption(strFmt('%1 - %2', this.getCurrentBatchTask().Caption, _caption));
            thread.batchInfo().parmGroupId(this.getCurrentBatchTask().GroupId);
            batchHeader.addRuntimeTask(thread, this.getCurrentBatchTask().BatchJobId);
        }}
        else
        {{
            // If you need a prompt then use startOperation
            thread.runOperation();
        }}
        return thread;
    }}{Environment.NewLine}"
            };
        }

        internal AxMethod CreateSetContractMethod()
        {
            return new AxMethod
            {
                Name = "setContract",
                Visibility = CompilerVisibility.Public,
                Source = $@"    public Object setContract(Object _contractTo, Object _contractFrom)
    {{
        DictClass contractDictClass = new DictClass(classIdGet(_contractTo));
        this.setContractInternal(contractDictClass, _contractTo, _contractFrom);
        return _contractTo;
    }}{Environment.NewLine}"
            };
        }

        internal AxMethod CreateSetContractInternalMethod()
        {
            return new AxMethod
            {
                Name = "setContractInternal",
                Visibility = CompilerVisibility.Private,
                Source = $@"    private void setContractInternal(DictClass _class, Object _contractTo, Object _contractFrom)
    {{
        for(Counter a = 1;_class.objectMethodCnt() >= a; a++)
        {{
            if(subStr(_class.objectMethod(a), 0, 4) == 'parm')
            {{
                _class.callObject(_class.objectMethod(a), _contractTo, _class.callObject(_class.objectMethod(a), _contractFrom));
            }}
        }}
        if(_class.extend() != 0)
        {{
            DictClass parentContract = new DictClass(_class.extend());
            if(parentContract)
            {{
                this.setContractInternal(parentContract,_contractTo,_contractFrom);
            }}
        }}
    }}{Environment.NewLine}"
            };
        }

        internal AxMethod CreateFinishMethod()
        {
            return new AxMethod
            {
                Name = "finish",
                Visibility = CompilerVisibility.Public,
                Source = $@"    public void finish()
    {{
        if(this.isExecutingInBatch() && batchHeader && threadCnt != 0)
        {{
            batchHeader.save();
        }}
    }}{Environment.NewLine}"
            };
        }

        internal override string GetClassCode()
        {
            return $@"internal class {Prefix}{ClassName()} extends SysOperationServiceBase
{{
    {Prefix}SysOpContract contract;
    BatchHeader batchHeader;
    guid processRunId;
    int threadCnt;

}}";

        }
    }
}
