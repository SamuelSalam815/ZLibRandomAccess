namespace LogSimulator.Checks;

public abstract partial record AbilityCheckResolution
{
    public sealed record ByDeclaration(AbilityCheckRequest Request, bool DidSucceed)
        : AbilityCheckResolution(Request)
    {
        public override bool IsSuccess() => DidSucceed;

        public override GameEventDescription GetDescription()
        {
            var result = new GameEventDescription();
            result.AddLine("Testing '{0}'...", Request.Question);
            result.AddLine(DidSucceed ? "Automatically passed test!" : "Automatically failed test!");
            return result;
        }
    }
}
