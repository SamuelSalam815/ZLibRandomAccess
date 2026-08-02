namespace LogSimulator;

#pragma warning disable CS9113 // Parameter is unread.
// Suppressed 'unread parameter' warning because passing the object allows compiler to infer 'T'
public class UnexpectedCaseException<T>(T? switchObject) : Exception(
#pragma warning restore CS9113 // Parameter is unread.
    $"Switch was thought to be exhaustive on {typeof(T)}, but was not actually exhaustive!");

public static class UnexpectedCaseException
{
    public static UnexpectedCaseException<T> CreateFrom<T>(T? switchObject)
    {
        return new UnexpectedCaseException<T>(switchObject);
    }
}
