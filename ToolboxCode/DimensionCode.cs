using Microsoft.Dynamics.AX.Metadata.Core.MetaModel;
using Microsoft.Dynamics.AX.Metadata.MetaModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Functions_for_Dynamics_Operations.ToolboxCode
{
    internal class DimensionCode : GenToolbox
    {
        public DimensionCode(string prefix)
        {
            Prefix = prefix;
        }

        internal override string ClassName()
        {
            // Override this to return the class name
            return "DimensionUtil";
        }

        internal override void CreateClassMethods()
        {
            axClass.AddMethod(CreateConstructMethod());
            axClass.AddMethod(CreateMergeDimensionsMap());
            axClass.AddMethod(CreateValidateDimension());
            axClass.AddMethod(CreateMergeDimensions());
            axClass.AddMethod(CreateGetValueByDefaultDimension());
            axClass.AddMethod(CreateGetValueByLedgerDimension());
            axClass.AddMethod(CreateSetValueToDefaultDimension());
            axClass.AddMethod(CreateSetValueToDefaultDimensionCon());
            axClass.AddMethod(CreateSetDefaultDimensionRecId());
            axClass.AddMethod(CreateCreateDimensionValue());
        }

        internal override string GetClassCode()
        {
            return $@"internal class {Prefix}{ClassName()}
{{
    static public str DefaultDimension = 'DefaultDimension';
    static public Name BusinessUnit = 'BusinessUnit';
    static public Name Department = 'Department';
    static public Name CostCenter = 'CostCenter';
    static public Name TradeName = 'TradeName';
    static public Name CostUnit = 'CostUnit';
    static public Name Customer = 'Customer';
    static public Name Project = 'Project';

}}";
        }

        internal AxMethod CreateConstructMethod()
        {
            return new AxMethod
            {
                Name = "construct",
                Visibility = CompilerVisibility.Public,
                Source = $@"    public static {Prefix}DimensionUtil construct()
    {{
        {Prefix}DimensionUtil dim = new {Prefix}DimensionUtil();
        // Initialize dimension utilities properties here if needed
        return dim;
    }}{Environment.NewLine}"
            };
        }

        internal AxMethod CreateMergeDimensionsMap()
        {
            return new AxMethod
            {
                Name = "mergeDimensionsMap",
                Visibility = CompilerVisibility.Public,
                Source = $@"    public static DimensionDefault mergeDimensionsMap(Map _dimensionAndValue)
    {{
        MapEnumerator dimsAndValues = _dimensionAndValue.getEnumerator();
        DimensionDefault defDim;

        while (dimsAndValues.moveNext())
        {{
            if ({Prefix}DimensionUtil::validateDimension(dimsAndValues.currentKey(), dimsAndValues.currentValue()))
            {{
                defDim = {Prefix}DimensionUtil::mergeDimensions(dimsAndValues.currentKey(), dimsAndValues.currentValue(), defDim);
            }}
        }}

        return defDim;
    }}{Environment.NewLine}"
            };
        }
        internal AxMethod CreateValidateDimension()
        {
            return new AxMethod
            {
                Name = "validateDimension",
                Visibility = CompilerVisibility.Public,
                Source = $@"    public static boolean validateDimension(Name _dimensionAttributeName, DimensionValue _dimensionValue)
    {{
        DimensionAttribute dimAttribute = DimensionAttribute::findByName(_dimensionAttributeName);

        if (!dimAttribute)
        {{
            throw error(strFmt('Dimension %1 does not exist', _dimensionAttributeName));
        }}

        if (!DimensionAttributeValue::findByDimensionAttributeAndValue(dimAttribute, _dimensionValue))
        {{
            throw error(strFmt('Dimension value %1, does not exist for Dimension %2', _dimensionValue, _dimensionAttributeName));
        }}

        return true;
    }}{Environment.NewLine}"
            };
        }
        internal AxMethod CreateMergeDimensions()
        {
            return new AxMethod
            {
                Name = "mergeDimensions",
                Visibility = CompilerVisibility.Public,
                Source = $@"    public static DimensionDefault mergeDimensions(Name _dimensionAttributeName, DimensionValue _dimensionValue, DimensionDefault _defaultDimension)
    {{
        DimensionAttributeValue dimensionAttributeValue = DimensionAttributeValue::findByDimensionAttributeAndValue(DimensionAttribute::findByName(_dimensionAttributeName), _dimensionValue, false, true);

        DimensionAttributeValueSetStorage dimensionStorage = DimensionAttributeValueSetStorage::find(_defaultDimension);
        dimensionStorage.addItem(dimensionAttributeValue);
        // Generate the new defaultDimension
        return dimensionStorage.save();
    }}{Environment.NewLine}"
            };
        }
        internal AxMethod CreateGetValueByDefaultDimension()
        {
            return new AxMethod
            {
                Name = "getValueByDefaultDimension",
                Visibility = CompilerVisibility.Public,
                Source = $@"    public static DimensionValue getValueByDefaultDimension(DimensionDefault  _dimensionDefault, Name _dimensionName)
    {{
        FieldId fieldIdDim = DimensionAttributeValueSet::getDimensionValueFieldId(_dimensionName);
        if (fieldIdDim)
        {{
            DimensionAttributeValueSet dimValueTable = DimensionAttributeValueSet::find(_dimensionDefault);
            if (dimValueTable.RecId)
            {{
                return dimValueTable.(fieldIdDim);
            }}
        }}

        return '';
    }}{Environment.NewLine}"
            };
        }
        internal AxMethod CreateGetValueByLedgerDimension()
        {
            return new AxMethod
            {
                Name = "getValueByLedgerDimension",
                Visibility = CompilerVisibility.Public,
                Source = $@"    public static DimensionValue getValueByLedgerDimension(LedgerDimensionAccount _dimensionDefault, Name _dimensionName)
    {{
        // Get the field id for the dimension value based on the dimension name
        FieldId fieldIdDim = DimensionAttributeValueCombination::getDimensionValueFieldId(_dimensionName);
        if (fieldIdDim)
        {{
            DimensionAttributeValueCombination dimValueTable = DimensionAttributeValueCombination::find(_dimensionDefault);
            if (dimValueTable.RecId)
            {{
                return dimValueTable.(fieldIdDim);
            }}
        }}
        return '';
    }}{Environment.NewLine}"
            };
        }
        internal AxMethod CreateSetValueToDefaultDimension()
        {
            return new AxMethod
            {
                Name = "setValueToDefaultDimension",
                Visibility = CompilerVisibility.Public,
                Source = $@"    public static DimensionDefault setValueToDefaultDimension(DimensionDefault _dimensionDefault,Name _dimensionName, DimensionValue  _newDimensionValue)
    {{
        return {Prefix}DimensionUtil::setDefaultDimensionRecId(_dimensionDefault, DimensionAttribute::findByName(_dimensionName).RecId, _newDimensionValue);
    }}{Environment.NewLine}"
            };
        }
        internal AxMethod CreateSetValueToDefaultDimensionCon()
        {
            return new AxMethod
            {
                Name = "setValueToDefaultDimensionCon",
                Visibility = CompilerVisibility.Public,
                Source = $@"    public static DimensionDefault setValueToDefaultDimensionCon(DimensionDefault _dimensionDefault,container _dimensionNameValueCon)
    {{
        DimensionDefault res = _dimensionDefault;

        for (int i = 1; i <= conLen(_dimensionNameValueCon); i += 2)
        {{
            res = {Prefix}DimensionUtil::setValueToDefaultDimension(res, conPeek(_dimensionNameValueCon, i), conPeek(_dimensionNameValueCon, i + 1));
        }}

        return res;
    }}{Environment.NewLine}"
            };
        }
        internal AxMethod CreateSetDefaultDimensionRecId()
        {
            return new AxMethod
            {
                Name = "setDefaultDimensionRecId",
                Visibility = CompilerVisibility.Public,
                Source = $@"    public static DimensionDefault setDefaultDimensionRecId(DimensionDefault _dimensionDefault,RefRecId _dimensionAttributeRecId, DimensionValue  _newDimensionValue)
    {{
        DimensionDefault newDimensionDefault = _dimensionDefault;

        if (_dimensionAttributeRecId)
        {{
            DimensionAttributeValueSetStorage dimStorage = DimensionAttributeValueSetStorage::find(_dimensionDefault);

            if (_newDimensionValue)
            {{
                DimensionAttributeValue dimensionAttributeValue = DimensionAttributeValue::findByDimensionAttributeAndValue(DimensionAttribute::find(_dimensionAttributeRecId), _newDimensionValue, false, true);
                dimStorage.addItem(dimensionAttributeValue);
            }}
            else
            {{
                dimStorage.removeDimensionAttribute(_dimensionAttributeRecId);
            }}

            newDimensionDefault = dimStorage.save();
        }}

        return newDimensionDefault;
    }}{Environment.NewLine}"
            };
        }
        internal AxMethod CreateCreateDimensionValue()
        {
            return new AxMethod
            {
                Name = "createDimensionValue",
                Visibility = CompilerVisibility.Public,
                Source = $@"    public static boolean createDimensionValue(DimensionValueContract _dimensionValueContract)
    {{
        #DimensionServiceFaults

        if (!_dimensionValueContract)
        {{
            throw AifFault::faultList(""@SYS323942"", #DimensionValueContractIsNotInitialized);
        }}

        if (!_dimensionValueContract.parmDimensionAttribute())
        {{
            throw AifFault::faultList(""@SYS323943"", #DimensionAttributeIsNotInitialized);
        }}

        if (!_dimensionValueContract.parmValue())
        {{
            throw AifFault::faultList(""@SYS323944"", #DimensionAttributeValueIsNotSpecified);
        }}

        if (_dimensionValueContract.parmPersonnelNumber())
        {{
            if (!HcmWorker::findByPersonnelNumber(_dimensionValueContract.parmPersonnelNumber()))
            {{
                throw AifFault::faultList(""@SYS183537"", #InvalidPersonnelNumber);
            }}
        }}

        DimensionAttribute dimensionAttribute = DimensionAttribute::findByName(_dimensionValueContract.parmDimensionAttribute());

        if (!dimensionAttribute)
        {{
            throw AifFault::faultList(""@SYS323856"", #DimensionAttributeDoesNotExist);
        }}

        if (dimensionAttribute.Type != DimensionAttributeType::CustomList)
        {{
            throw AifFault::faultList(""@SYS323857"", #DimensionAttributeIsNotUserDefined);
        }}

        DimensionValueMask dimensionValueMask = FinancialTagCategory::find(dimensionAttribute.financialTagCategory()).Mask;

        if (dimensionValueMask && !FinancialTagCategory::doesValueMatchMask(_dimensionValueContract.parmValue(), dimensionValueMask))
        {{
            throw AifFault::faultList(strFmt(""@SYS135845"", _dimensionValueContract.parmValue(), dimensionValueMask), #DimensionAttributeValueMaskDoesNotMatch);
        }}

        if (_dimensionValueContract.parmActiveTo() != dateNull() && _dimensionValueContract.parmActiveTo() < _dimensionValueContract.parmActiveFrom())
        {{
            throw AifFault::faultList(""@SYS135850"", #DimensionAttributeValueActiveDateError);
        }}

        DimensionAttributeValue dimensionAttributeValue;
        DimensionFinancialTag dimensionFinancialTag;

        select firstonly DimensionAttribute, EntityInstance from dimensionAttributeValue
        join RecId, Value from dimensionFinancialTag
            where dimensionAttributeValue.DimensionAttribute == dimensionAttribute.RecId
            && dimensionFinancialTag.RecId == dimensionAttributeValue.EntityInstance
            && dimensionFinancialTag.Value == _dimensionValueContract.parmValue();

        if (!dimensionFinancialTag)
        {{
            dimensionAttributeValue.clear();

            ttsbegin;

            dimensionFinancialTag.Value = _dimensionValueContract.parmValue();
            dimensionFinancialTag.Description = _dimensionValueContract.parmDescription();
            dimensionFinancialTag.FinancialTagCategory = dimensionAttribute.financialTagCategory();
            dimensionFinancialTag.insert();

            dimensionAttributeValue.EntityInstance = dimensionFinancialTag.RecId;
            dimensionAttributeValue.DimensionAttribute = dimensionAttribute.RecId;
            dimensionAttributeValue.ActiveFrom = _dimensionValueContract.parmActiveFrom();
            dimensionAttributeValue.ActiveTo = _dimensionValueContract.parmActiveTo();
            dimensionAttributeValue.IsBlockedForManualEntry = _dimensionValueContract.parmIsBlockedForManualEntry();
            dimensionAttributeValue.IsSuspended = _dimensionValueContract.parmIsSuspended();
            dimensionAttributeValue.Owner = HcmWorker::findByPersonnelNumber(_dimensionValueContract.parmPersonnelNumber()).RecId;
            dimensionAttributeValue.GroupDimension = _dimensionValueContract.parmGroupDimension();
            dimensionAttributeValue.insert();

            ttscommit;

            return true;
        }}

        return false;
    }}{Environment.NewLine}"
            };
        }
    }
}
