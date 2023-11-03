namespace Cuplan.Organization.ControllerModels;

/// <summary>
///     Wraps a specific type so the resulting JSON is contained within an object always.
///     Required for Javascript code.
/// </summary>
/// <typeparam name="TWrapped"></typeparam>
public class WrappedResult<TWrapped>
{
    public WrappedResult(TWrapped result)
    {
        Result = result;
    }

    public TWrapped Result { get; set; }
}