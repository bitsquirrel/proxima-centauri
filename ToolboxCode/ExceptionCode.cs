using System;
using Microsoft.Dynamics.AX.Metadata.Core.MetaModel;
using Microsoft.Dynamics.AX.Metadata.MetaModel;

namespace Functions_for_Dynamics_Operations.ToolboxCode
{
    internal class ExceptionCode : GenToolbox
    {
        public ExceptionCode(string prefix)
        {
            Prefix = prefix;
        }
        internal override string ClassName()
        {
            // Override this to return the class name
            return "Exception";
        }
        internal override void CreateClassMethods()
        {
            axClass.AddMethod(CreateConstructMethod());
            axClass.AddMethod(CreateRetryTransiantMethod());
            axClass.AddMethod(CreateDelayRetryMethod());
            axClass.AddMethod(CreateRetryDuplicateKeyMethod());
            axClass.AddMethod(CreateRetryUpdateConflictMethod());
            axClass.AddMethod(CreateRetryDeadlockMethod());
            axClass.AddMethod(CreateExceptionExampleMethod());
        }

        internal AxMethod CreateConstructMethod()
        {
            return new AxMethod
            {
                Name = "construct",
                Visibility = CompilerVisibility.Internal,
                Source = $@"    public static {Prefix}Exception construct()
    {{
        {Prefix}Exception excep = new {Prefix}Exception();
        // Raondom number with which to calculate the back off wait
        excep.backOffWait = RandomGenerate::construct().randomInt(50, 1000);
        return excep;
    }}{Environment.NewLine}"
            };
        }

        internal AxMethod CreateRetryTransiantMethod()
        {
            return new AxMethod
            {
                Name = "retryTransientSqlConnectionError",
                Visibility = CompilerVisibility.Internal,
                Source = $@"    public boolean retryTransientSqlConnectionError(boolean _throwOnFail = true, int _maxRetries = defaultMaxRetries)
    {{
        if (retriesSqlTrans < _maxRetries)
        {{
            {Prefix}Exception::delayRetry(retriesSqlTrans);
            retriesSqlTrans++;
            return true;
        }}
        else
        {{
            if (_throwOnFail)
            {{
                throw Exception::TransientSqlConnectionError;
            }}
            else
            {{
                return false;
            }}
        }}
    }}{Environment.NewLine}"
            };
        }

        internal AxMethod CreateDelayRetryMethod()
        {
            return new AxMethod
            {
                Name = "delayRetry",
                Visibility = CompilerVisibility.Internal,
                Source = $@"    public static void delayRetry(int _retryCount)
    {{
        var delay = 5000 * power(2, min(_retryCount, 5));
        sleep(min(60 * 1000, delay));
    }}{Environment.NewLine}"
            };
        }

        internal AxMethod CreateRetryDuplicateKeyMethod()
        {
            return new AxMethod
            {
                Name = "retryDuplicateKeyException",
                Visibility = CompilerVisibility.Internal,
                Source = $@"    public boolean retryDuplicateKeyException(boolean _throwOnFail = true, int _maxRetries = defaultMaxRetries)
    {{
        if (retriesDupKey <= _maxRetries)
        {{
            {Prefix}Exception::delayRetry(retriesDupKey);
            retriesDupKey++;
            return true;
        }}
        else
        {{
            if (_throwOnFail)
            {{
                throw Exception::DuplicateKeyExceptionNotRecovered;
            }}
            else
            {{
                return false;
            }}
        }}
    }}{Environment.NewLine}"
            };
        }

        internal AxMethod CreateRetryUpdateConflictMethod()
        {
            return new AxMethod
            {
                Name = "retryUpdateConflictException",
                Visibility = CompilerVisibility.Internal,
                Source = $@"    public boolean retryUpdateConflictException(boolean _throwOnFail = true, int _maxRetries = defaultMaxRetries)
    {{
        if (appl.ttsLevel() == 0)
        {{
            if (retriesUpdCon <= _maxRetries)
            {{
                sleep(retriesUpdCon * backOffWait);
                retriesUpdCon++;
                return true;
            }}
            else
            {{
                if (_throwOnFail)
                {{
                    throw Exception::UpdateConflictNotRecovered;
                }}
                else
                {{
                    return false;
                }}
            }}
        }}
        else
        {{
            if (_throwOnFail)
            {{
                throw Exception::UpdateConflictNotRecovered;
            }}
            else
            {{
                return false;
            }}
        }}
    }}{Environment.NewLine}"
            };
        }

        internal AxMethod CreateRetryDeadlockMethod()
        {
            return new AxMethod
            {
                Name = "retryDeadlockException",
                Visibility = CompilerVisibility.Internal,
                Source = $@"    public boolean retryDeadlockException(boolean _throwOnFail = true, int _maxRetries = defaultMaxRetries)
    {{
        if (retriesDead <= _maxRetries)
        {{
            sleep(retriesDead * backOffWait);
            retriesDead++;
            return true;
        }}
        else
        {{
            if (_throwOnFail)
            {{
                throw Exception::Deadlock;
            }}
            else
            {{
                return false;
            }}
        }}
    }}{Environment.NewLine}"
            };
        }

        internal AxMethod CreateExceptionExampleMethod()
        {
            return new AxMethod()
            {
                Name = "exampleException",
                Visibility = CompilerVisibility.Internal,
                IsStatic = false,
                Source = $@"internal void exampleException()
    {{
        Microsoft.Dynamics.Ax.Xpp.TransientSqlConnectionError transientSqlConnectionError;
        Microsoft.Dynamics.Ax.Xpp.UpdateConflictException updateConflictException;
        Microsoft.Dynamics.Ax.Xpp.DuplicateKeyException duplicateKeyException;
        Microsoft.Dynamics.Ax.Xpp.DeadlockException deadlockException;
        System.Exception ex;

        {Prefix}Exception excep = {Prefix}Exception::construct();

        try
        {{
            ttsbegin;

            // Your code logic here, and be very sure that you are not in a TTS and retry
            // the code in the catch block. This is a very common mistake and can lead to
            // infinite loops, deadlocks, duplicate key exceptions, etc.

            ttscommit;
        }}
        catch (transientSqlConnectionError)
        {{
            if (excep.retryTransientSqlConnectionError())
            {{
                retry;
            }}
        }}
        catch (updateConflictException)
        {{
            if (excep.retryUpdateConflictException())
            {{
                retry;
            }}
        }}
        catch (duplicateKeyException)
        {{
            if (excep.retryDuplicateKeyException())
            {{
                retry;
            }}
        }}
        catch (deadlockException)
        {{
            if (excep.retryDeadlockException())
            {{
                retry;
            }}
        }}
        catch (ex)
        {{
            // TODO : Choose what to do here, throw or simply show the error
            throw error(ex.ToString());
        }}
    }}"
            };
        }

        internal override string GetClassCode()
        {
            return $@"internal class {Prefix}Exception
{{
    #OCCRetryCount

    private const int defaultMaxRetries = #RetryNum;
    
    private int retriesSqlTrans;
    private int retriesDupKey;
    private int retriesUpdCon;
    private int retriesDead;
    private int backOffWait;

}}";
        }
    }
}
