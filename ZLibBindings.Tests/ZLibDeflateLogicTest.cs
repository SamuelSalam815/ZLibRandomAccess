using JetBrains.Annotations;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Shouldly;
using ZLibBindings.Constants;
using ZLibBindings.State;
using ZLibWrapper;

namespace ZLibBindings.Tests;

[TestClass]
[TestSubject(typeof(ZLibDeflateLogic))]
public unsafe class ZLibDeflateLogicTest
{
    private void TestDecision(z_stream_s state, ZReturnCode returnCode, ZLibWriteAction expectedAction)
    {
        ZLibDeflateLogic.GetNextAction(&state, returnCode).ShouldBe(expectedAction);
    }

    [TestMethod]
    [DataRow(ZReturnCode.Z_ERRNO)]
    [DataRow(ZReturnCode.Z_STREAM_ERROR)]
    [DataRow(ZReturnCode.Z_DATA_ERROR)]
    [DataRow(ZReturnCode.Z_MEM_ERROR)]
    [DataRow(ZReturnCode.Z_VERSION_ERROR)]
    public void FatalErrorCodes_ShouldFail(ZReturnCode returnCode)
    {
        TestDecision(new z_stream_s(), returnCode, ZLibWriteAction.FailToDecide);
    }

    [TestMethod]
    public void BufferErrorWhenNeitherInputNorOutputAreConsumed_ShouldFail()
    {
        TestDecision(new z_stream_s{avail_in = 1, avail_out = 1}, ZReturnCode.Z_BUF_ERROR, ZLibWriteAction.FailToDecide);
    }

    [TestMethod]
    public void ConsumedInputBuffer_ShouldCompleteInput()
    {
        TestDecision(new z_stream_s(), ZReturnCode.Z_BUF_ERROR, ZLibWriteAction.CompleteInput);
    }

    [TestMethod]
    public void ConsumedOutputBuffer_ShouldRequestMoreSpace()
    {
        TestDecision(new z_stream_s { avail_in = 1 }, ZReturnCode.Z_BUF_ERROR, ZLibWriteAction.RequestMoreOutputSpace);
    }

    [TestMethod]
    public void OkErrorCode_ShouldContinue()
    {
        TestDecision(new z_stream_s(), ZReturnCode.Z_OK, ZLibWriteAction.Continue);
    }

    [TestMethod]
    public void EndOfStream_ShouldCompleteInput()
    {
        TestDecision(new z_stream_s(), ZReturnCode.Z_STREAM_END, ZLibWriteAction.CompleteInput);
    }
}
