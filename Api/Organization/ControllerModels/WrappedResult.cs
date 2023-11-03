namespace Cuplan.Organization.ControllerModels;

public class WrappedResult<TWrapped>
{
    public WrappedResult(TWrapped result)
    {
        Result = result;
    }

    public TWrapped Result { get; set; }
}