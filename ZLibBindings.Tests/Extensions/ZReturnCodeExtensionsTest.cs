using System;
using System.Runtime.InteropServices;
using JetBrains.Annotations;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Shouldly;
using ZLibBindings.Constants;
using ZLibBindings.State;
using ZLibWrapper;
using ZLibWrapper.Extensions;

namespace ZLibBindings.Tests.Extensions;

[TestClass]
[TestSubject(typeof(ZReturnCodeExtensions))]
public unsafe class ZReturnCodeExtensionsTest
{
    private readonly string  _errorMessage;
    private readonly byte* _errorMessagePointer;

    public ZReturnCodeExtensionsTest()
    {
        _errorMessage = "This is a fatal error";
        _errorMessagePointer = (byte*)Marshal.StringToHGlobalAnsi(_errorMessage);
    }

    [TestMethod]
    [DataRow(ZReturnCode.Z_ERRNO)]
    [DataRow(ZReturnCode.Z_STREAM_ERROR)]
    [DataRow(ZReturnCode.Z_DATA_ERROR)]
    [DataRow(ZReturnCode.Z_MEM_ERROR)]
    [DataRow(ZReturnCode.Z_VERSION_ERROR)]
    public void FatalErrorCodes_ShouldThrowWithErrorMessage(ZReturnCode returnCode)
    {
        var action = () =>
        {
            var state = new z_stream_s{msg = _errorMessagePointer};
            returnCode.GuardAgainstFatalErrors(&state);
        };
        var exception = action.ShouldThrow<ZLibException>();
        exception.Message.ShouldBe(_errorMessage);
        exception.ReturnCode.ShouldBe(returnCode);
    }

    [TestMethod]
    [DataRow(ZReturnCode.Z_BUF_ERROR)]
    public void NonFatalErrors_ShouldPassThrough(ZReturnCode returnCode)
    {
        var state = new z_stream_s();
        returnCode.GuardAgainstFatalErrors(&state).ShouldBe(returnCode);
    }

    [TestMethod]
    [DataRow(ZReturnCode.Z_OK)]
    [DataRow(ZReturnCode.Z_STREAM_END)]
    [DataRow(ZReturnCode.Z_NEED_DICT)]
    public void ReturnCodes_ShouldPassThrough(ZReturnCode returnCode)
    {
        var state = new z_stream_s();
        returnCode.GuardAgainstFatalErrors(&state).ShouldBe(returnCode);
    }

    [TestCleanup]
    public void CleanUp()
    {
        Marshal.FreeHGlobal((IntPtr)_errorMessagePointer);
    }
}
