using System;
using Microsoft.Dynamics.AX.Metadata.Core.MetaModel;
using Microsoft.Dynamics.AX.Metadata.MetaModel;

namespace Functions_for_Dynamics_Operations.ToolboxCode
{
    internal class UtilsCode : GenToolbox
    {
        public UtilsCode(string prefix)
        {
            Prefix = prefix;
        }

        internal override string ClassName()
        {
            // Override this to return the class name
            return "Utils";
        }

        internal override void CreateClassMethods()
        {
            axClass.AddMethod(CreateContentTypeMethod());
            axClass.AddMethod(CreateUniqueFilenameMethod());
            axClass.AddMethod(CreateStreamFromStrMethod());
            axClass.AddMethod(CreateFileNameFromSegmentMethod());
            axClass.AddMethod(CreateGetFileNameFromCommonMethod());
            axClass.AddMethod(CreateGetFileNameFromIndexMethod());
            axClass.AddMethod(CreateGetClrErrorMethod());
            axClass.AddMethod(CreateGetInfoLogErrorMethod());
            axClass.AddMethod(CreateUserHasSecurityPrivilegeMethod());
            axClass.AddMethod(CreateCompressMethod());
            axClass.AddMethod(CreateDecompressMethod());
        }

        internal AxMethod CreateContentTypeMethod()
        {
            return new AxMethod
            {
                Name = "getContentType",
                Visibility = CompilerVisibility.Public,
                IsStatic = true,
                Source = $@"    public static str getContentType({Prefix}MediaType _mediaType)
    {{
        switch (_mediaType)
        {{
            case {Prefix}MediaType::AppXml:
                return 'application/xml';
            case {Prefix}MediaType::AppJson:
                return 'application/json';
            case {Prefix}MediaType::AppFormUrlEncoded:
                return 'application/x-www-form-urlencoded';
        }}

        return '';
    }}{Environment.NewLine}"
            };
        }

        internal AxMethod CreateUniqueFilenameMethod()
        {
            return new AxMethod
            {
                Name = "getUniqueFilename",
                Visibility = CompilerVisibility.Public,
                IsStatic = true,
                Source = $@"    public static Filename getUniqueFilename(Filename _filename)
    {{
        // Build a unique filename to allow the file to be moved so we can delete the original
        str filename = System.IO.Path::GetFileNameWithoutExtension(_filename);
        str dt = System.DateTime::UtcNow.ToString('MM-dd-yyyy hmmss.ffff tt');
        str extension = System.IO.Path::GetExtension(_filename);
        // New filename including date, time and milliseconds
        return strFmt('%1_%2%3', filename, dt, extension);
    }}{Environment.NewLine}"
            };
        }

        internal AxMethod CreateStreamFromStrMethod()
        {
            return new AxMethod
            {
                Name = "getStreamFromStr",
                Visibility = CompilerVisibility.Public,
                IsStatic = true,
                Source = $@"    public static System.IO.Stream getStreamFromStr(str _string)
    {{
        /*
        System.Text.Encoding::ASCII
        System.Text.Encoding::BigEndianUnicode
        System.Text.Encoding::Default
        System.Text.Encoding::Unicode
        System.Text.Encoding::UTF32
        System.Text.Encoding::UTF7
        System.Text.Encoding::UTF8
        System.Text.Encoding::Unicode
        */
        var stream = new System.IO.MemoryStream();
        var writer = new System.IO.StreamWriter(stream);
        writer.Write(_string);
        writer.Flush();
        stream.Position = 0;

        return stream;
    }}{Environment.NewLine}"
            };
        }

        internal AxMethod CreateFileNameFromSegmentMethod()
        {
            return new AxMethod
            {
                Name = "getFileNameFromSegment",
                Visibility = CompilerVisibility.Public,
                IsStatic = true,
                Source = $@"    public static str getFileNameFromSegment(System.String[] _segements)
    {{
        System.Collections.IEnumerator strings;
        System.String value;
        str ret;

        try
        {{
            strings = _segements.GetEnumerator();
            while (strings.MoveNext())
            {{
                value = strings.Current;
            }}
            // We need the last value - and nothing else seems to work
            ret = value.ToString();
        }}
        catch(Exception::CLRError)
        {{
            throw error({Prefix}{ClassName()}::getClrError());
        }}

        return ret;
    }}{Environment.NewLine}"
            };
        }

        internal AxMethod CreateGetFileNameFromCommonMethod()
        {
            return new AxMethod
            {
                Name = "getFileNameFromCommon",
                Visibility = CompilerVisibility.Public,
                IsStatic = true,
                Source = $@"    public static Filename getFileNameFromCommon(Common _common)
    {{
        SysDictTable sdt = new SysDictTable(_common.TableId);

        if (sdt.clusterIndex())
        {{
            return {Prefix}{ClassName()}::getFileNameFromIndex(_common, sdt.clusterIndex());
        }}
        else if (sdt.replacementKey())
        {{
            return {Prefix}{ClassName()}::getFileNameFromIndex(_common, sdt.replacementKey());
        }}
        else if (sdt.primaryIndex())
        {{
            return {Prefix}{ClassName()}::getFileNameFromIndex(_common, sdt.primaryIndex());
        }}

        return '';
    }}{Environment.NewLine}"
            };
        }

        internal AxMethod CreateGetFileNameFromIndexMethod()
        {
            return new AxMethod
            {
                Name = "getFileNameFromIndex",
                Visibility = CompilerVisibility.Public,
                IsStatic = true,
                Source = $@"    public static Filename getFileNameFromIndex(Common _common, int _indexId)
    {{
        SysDictIndex sdi = new SysDictIndex(_common.TableId, _indexId);
        Filename filename;

        if (sdi)
        {{
            for (int i = 1; i <= sdi.numberOfFields(); i++)
            {{
                filename += filename == '' ? any2Str(_common.(sdi.field(i))) : strFmt('_%1', any2Str(_common.(sdi.field(i))));
            }}
        }}

        return filename;
    }}{Environment.NewLine}"
            };
        }

        internal AxMethod CreateGetClrErrorMethod()
        {
            return new AxMethod
            {
                Name = "getClrError",
                Visibility = CompilerVisibility.Public,
                IsStatic = true,
                Source = $@"    public static str getClrError()
    {{
        str ret;

        new InteropPermission(InteropKind::ClrInterop).assert();

        System.Exception ex = CLRInterop::getLastException();
        if(ex)
        {{
            ret = ex.ToString();
        }}
        else
        {{
            ret = 'Unhandled CLR exception';
        }}

        CodeAccessPermission::revertAssert();

        return ret;
    }}{Environment.NewLine}"
            };
        }

        internal AxMethod CreateUserHasSecurityPrivilegeMethod()
        {
            return new AxMethod
            {
                Name = "userHasSecurityPrivilege",
                Visibility = CompilerVisibility.Public,
                IsStatic = true,
                Source = $@"    public static boolean userHasSecurityPrivilege(SecurityPrivilegeName _securityPrivilege, UserId _userId = curUserId())
    {{
        SecurityRolePrivilegeExplodedGraph securityRolePrivilegeExplodedGraph;
        SecurityPrivilege securityPrivilege;
        SecurityUserRole securityUserRole;

        select firstonly securityPrivilege
            where securityPrivilege.Identifier == _securityPrivilege
        exists join securityRolePrivilegeExplodedGraph
            where securityRolePrivilegeExplodedGraph.SecurityPrivilege == securityPrivilege.RecId
        exists join securityUserRole
            where securityUserRole.SecurityRole == securityRolePrivilegeExplodedGraph.SecurityRole
               && securityUserRole.User == _userId
               && securityUserRole.AssignmentStatus == RoleAssignmentStatus::Enabled
               && (securityUserRole.ValidFrom < DateTimeUtil::utcNow() || securityUserRole.ValidFrom == utcDateTimeNull())
               && (securityUserRole.ValidTo > DateTimeUtil::utcNow() || securityUserRole.ValidTo == utcDateTimeNull());
 
        return securityPrivilege.RecId != 0;
    }}{Environment.NewLine}"
            };
        }

        internal AxMethod CreateGetInfoLogErrorMethod()
        {
            return new AxMethod
            {
                Name = "getInfoLogError",
                Visibility = CompilerVisibility.Public,
                IsStatic = true,
                Source = $@"    public static str getInfoLogError({Prefix}ExceptionType _exceptionType = {Prefix}ExceptionType::ErrorsAndWarnings)
    {{
        const str NewLine = '\n';
        str logMessage;
        int i;

        SysInfologEnumerator sysInfologEnumerator = SysInfologEnumerator::newData(infolog.infologData());

        void setInfologData()
        {{
            SysInfologMessageStruct infoMessageStruct = SysInfologMessageStruct::construct(sysInfologEnumerator.currentMessage());
        
            str logString;

            while (i <= infoMessageStruct.prefixDepth())
            {{
                logString = logString + infoMessageStruct.preFixTextElement(i) + '. ';
                i++;
            }}

            logString = logString + infoMessageStruct.message();
            logMessage += logString;
        }}
    
        while (sysInfologEnumerator.moveNext())
        {{
            i = 1;

            if (logMessage)
            {{
                logMessage += Newline;
            }}
            
            switch (_exceptionType)
            {{
                case {Prefix}ExceptionType::ErrorsAndWarnings:
                    if (sysInfologEnumerator.currentException() != Exception::Info)
                    {{
                        setInfologData();
                    }}
                    break;
                case {Prefix}ExceptionType::Errors:
                    if (sysInfologEnumerator.currentException() != Exception::Info && sysInfologEnumerator.currentException() != Exception::Warning)
                    {{
                        setInfologData();
                    }}
                    break;
                case {Prefix}ExceptionType::All:
                    setInfologData();
                    break;
            }}
        }}

        return logMessage;
    }}{Environment.NewLine}"
            };
        }

        internal AxMethod CreateCompressMethod()
        {
            return new AxMethod
            {
                Name = "compress",
                Visibility = CompilerVisibility.Public,
                IsStatic = true,
                Source = $@"    public static str compress(str _stringToCompress)
    {{
        System.Byte[] buffer = System.Text.Encoding::UTF8.GetBytes(_stringToCompress);

        using (System.IO.MemoryStream ms = new System.IO.MemoryStream())
        {{
            using (System.IO.Compression.GZipStream zip = new System.IO.Compression.GZipStream(ms, System.IO.Compression.CompressionMode::Compress, true))
            {{
                zip.Write(buffer, 0, buffer.Length);
            }}

            ms.Position = 0;

            System.Byte[] compressed = new System.Byte[ms.Length]();
            ms.Read(compressed, 0, compressed.Length);

            System.Byte[] gzBuffer = new System.Byte[compressed.Length + 4]();
            System.Buffer::BlockCopy(compressed, 0, gzBuffer, 4, compressed.Length);
            System.Buffer::BlockCopy(System.BitConverter::GetBytes(buffer.Length), 0, gzBuffer, 0, 4);

            return System.Convert::ToBase64String(gzBuffer);
        }}
    }}{Environment.NewLine}"
            };
        }

        internal AxMethod CreateDecompressMethod()
        {
            return new AxMethod
            {
                Name = "decompress",
                Visibility = CompilerVisibility.Public,
                IsStatic = true,
                Source = $@"    public static str decompress(str _stringToDecompress)
    {{
        System.Byte[] gzBuffer = System.Convert::FromBase64String(_stringToDecompress);

        using (System.IO.MemoryStream ms = new System.IO.MemoryStream())
        {{
            int msgLength = System.BitConverter::ToInt32(gzBuffer, 0);
            ms.Write(gzBuffer, 4, gzBuffer.Length - 4);

            System.Byte[] buffer = new System.Byte[msgLength]();

            ms.Position = 0;
            using (System.IO.Compression.GZipStream zip = new System.IO.Compression.GZipStream(ms, System.IO.Compression.CompressionMode::Decompress))
            {{
                zip.Read(buffer, 0, buffer.Length);
            }}

            return System.Text.Encoding::UTF8.GetString(buffer);
        }}
    }}{Environment.NewLine}"
            };
        }

        internal override string GetClassCode()
        {
            return $@"internal class {Prefix}{ClassName()}
{{

}}";
        }
    }
}
