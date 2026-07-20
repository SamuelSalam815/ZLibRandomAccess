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
        ZLibPumpAction expectedAction)
    {
        ZLibPumpLogic.GetNextAction(&state, returnCode).ShouldBe(expectedAction);
    }

    [TestMethod]
    [DataRow(ZReturnCode.Z_ERRNO)]
    [DataRow(ZReturnCode.Z_STREAM_ERROR)]
    [DataRow(ZReturnCode.Z_DATA_ERROR)]
    [DataRow(ZReturnCode.Z_MEM_ERROR)]
    [DataRow(ZReturnCode.Z_VERSION_ERROR)]
    public void FatalErrorCodes_ShouldFail(ZReturnCode returnCode)
    {
        TestDecision(new z_stream_s(), returnCode, ZLibPumpAction.FailToDecide);
    }

    [TestMethod]
    public void BufferErrorWhenNeitherInputNorOutputAreConsumed_ShouldFail()
    {
        TestDecision(
            new z_stream_s { avail_in = 1, avail_out = 1 },
            ZReturnCode.Z_BUF_ERROR,
            ZLibPumpAction.FailToDecide);
    }

    [TestMethod]
    public void ConsumedInputBuffer_ShouldRequestMoreSpace()
    {
        TestDecision(new z_stream_s { avail_out = 1 }, ZReturnCode.Z_BUF_ERROR, ZLibPumpAction.RequestMoreInputSpace);
    }

    [TestMethod]
    public void ConsumedOutputBuffer_ShouldRequestMoreSpace()
    {
        TestDecision(new z_stream_s { avail_in = 1 }, ZReturnCode.Z_BUF_ERROR, ZLibPumpAction.RequestMoreOutputSpace);
    }

    [TestMethod]
    public void ConsumedInputAndConsumedOutput_ShouldRequestMoreOutputSpace()
    {
        TestDecision(new z_stream_s(), ZReturnCode.Z_BUF_ERROR, ZLibPumpAction.RequestMoreOutputSpace);
    }

    [TestMethod]
    public void OkErrorCode_ShouldContinue()
    {
        TestDecision(new z_stream_s { avail_in = 1, avail_out = 1 }, ZReturnCode.Z_OK, ZLibPumpAction.Continue);
    }

    [TestMethod]
    public void EndOfStream_ShouldCompleteStream()
    {
        TestDecision(new z_stream_s(), ZReturnCode.Z_STREAM_END, ZLibPumpAction.CompleteStream);
    }
}
