using JetBrains.Annotations;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Shouldly;
using ZLibBindings.Constants;
using ZLibBindings.State;
using ZLibWrapper.Logic;

namespace ZLibWrapper.Tests.Logic;

[TestClass]
[TestSubject(typeof(ZLibPumpLogic))]
public unsafe class ZLibPumpLogicTest
{
    private void TestDecision(
        z_stream_s state,
        ZReturnCode returnCode,
        ZFlushValue flushValue,
        ZLibPumpAction expectedAction)
    {
        ZLibPumpLogic.GetNextAction(&state, returnCode, flushValue).ShouldBe(expectedAction);
    }

    [TestMethod]
    [DataRow(ZReturnCode.Z_ERRNO)]
    [DataRow(ZReturnCode.Z_STREAM_ERROR)]
    [DataRow(ZReturnCode.Z_DATA_ERROR)]
    [DataRow(ZReturnCode.Z_MEM_ERROR)]
    [DataRow(ZReturnCode.Z_VERSION_ERROR)]
    public void FatalErrorCodes_ShouldFail(ZReturnCode returnCode)
    {
        TestDecision(new z_stream_s(), returnCode, ZFlushValue.Z_NO_FLUSH, ZLibPumpAction.FailToDecide);
    }

    [TestMethod]
    public void BufferErrorWhenNeitherInputNorOutputAreConsumed_ShouldFail()
    {
        TestDecision(new z_stream_s{avail_in = 1, avail_out = 1}, ZReturnCode.Z_BUF_ERROR, ZFlushValue.Z_NO_FLUSH, ZLibPumpAction.FailToDecide);
    }

    [TestMethod]
    public void ConsumedInputBuffer_ShouldCompleteInput()
    {
        TestDecision(new z_stream_s(), ZReturnCode.Z_BUF_ERROR, ZFlushValue.Z_NO_FLUSH, ZLibPumpAction.RequestMoreInputSpace);
    }

    [TestMethod]
    public void ConsumedOutputBuffer_ShouldRequestMoreSpace()
    {
        TestDecision(new z_stream_s { avail_in = 1 }, ZReturnCode.Z_BUF_ERROR, ZFlushValue.Z_NO_FLUSH, ZLibPumpAction.RequestMoreOutputSpace);
    }

    [TestMethod]
    public void OkErrorCode_ShouldContinue()
    {
        TestDecision(new z_stream_s(), ZReturnCode.Z_OK, ZFlushValue.Z_NO_FLUSH, ZLibPumpAction.Continue);
    }

    [TestMethod]
    [DataRow(ZFlushValue.Z_PARTIAL_FLUSH)]
    [DataRow(ZFlushValue.Z_SYNC_FLUSH)]
    [DataRow(ZFlushValue.Z_FULL_FLUSH)]
    [DataRow(ZFlushValue.Z_FINISH)]
    [DataRow(ZFlushValue.Z_BLOCK)]
    [DataRow(ZFlushValue.Z_TREES)]
    public void OkCodeOnFlushAndFullOutput_ShouldRequestMoreOutputSpace(ZFlushValue flushValue)
    {
        TestDecision(new z_stream_s(), ZReturnCode.Z_OK, flushValue, ZLibPumpAction.RequestMoreOutputSpace);
    }

    [TestMethod]
    [DataRow(ZFlushValue.Z_PARTIAL_FLUSH)]
    [DataRow(ZFlushValue.Z_SYNC_FLUSH)]
    [DataRow(ZFlushValue.Z_FULL_FLUSH)]
    [DataRow(ZFlushValue.Z_FINISH)]
    [DataRow(ZFlushValue.Z_BLOCK)]
    [DataRow(ZFlushValue.Z_TREES)]
    public void OkCodeOnFlush_ShouldCompleteInput(ZFlushValue flushValue)
    {
        TestDecision(new z_stream_s{avail_out = 1}, ZReturnCode.Z_OK, flushValue, ZLibPumpAction.RequestMoreInputSpace);
    }

    [TestMethod]
    public void EndOfStream_ShouldCompleteInput()
    {
        TestDecision(new z_stream_s(), ZReturnCode.Z_STREAM_END, ZFlushValue.Z_NO_FLUSH, ZLibPumpAction.RequestMoreInputSpace);
    }
}
