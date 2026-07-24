namespace LogSimulator;

public class UnexpectedCaseException<T>(T? switchObject) : Exception(
    $"Switch was thought to be exhaustive on {typeof(T)}, but was not actually exhaustive!");

public static class UnexpectedCaseException
{
    public static UnexpectedCaseException<T> CreateFrom<T>(T? switchObject)
    {
        return new UnexpectedCaseException<T>(switchObject);
    }
}
