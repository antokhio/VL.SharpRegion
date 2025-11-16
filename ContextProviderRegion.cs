using VL.Core;
using VL.Core.Import;
using VL.Core.PublicAPI;
using VL.Lib.Control;

namespace VL.SharpRegion;

public class ContextProviderStore : ScopedValueStore { }

[ProcessNode(HasStateOutput = true)]
[Region(SupportedBorderControlPoints = ControlPointType.None)]
public class ContextProviderRegion : IRegion<ContextProviderRegion.IInlay>
{
    public const string CONTEXT_PROVIDER_REGION = "ContextProviderRegion";

    private readonly Dictionary<InputDescription, object?> _inputs = new();
    private readonly Dictionary<OutputDescription, object?> _outputs = new();
    private readonly ContextProviderStore _store = new();

    private IInlay? inlay;
    private NodeContext? _nodeContext;

    public ContextProviderRegion(NodeContext nodeContext)
    {
        _nodeContext = nodeContext;

        // Create signatures for lookup inside region
        var storeInputs = new[]
        {
            new BorderControlPointDescription(
                CONTEXT_PROVIDER_REGION,
                typeof(ContextProviderRegion),
                0,
                false
            ),
        };

        // Bind store inputs (only clears if inputs changed)
        _store.Configurate(storeInputs);
    }

    public int Test { get; set; }

    public void Update()
    {
        if (inlay is null)
            return;

        // Create references for lookup inside region
        var storeValues = new object[] { this };

        // Activate scope for THIS execution
        // This pushes a new DataLayer onto the stack
        using (var scope = _store.ActivateScope(_nodeContext, storeValues))
        {
            // Execute user's patched logic while scope is active
            inlay.Execute(this);

            // Scope automatically disposed here, popping the layer
        }
    }

    void IRegion<IInlay>.SetPatchInlayFactory(Func<IInlay> patchInlayFactory)
    {
        if (inlay is null)
            inlay = patchInlayFactory();
    }

    void IRegion<IInlay>.AcknowledgeInput(in InputDescription cp, object? outerValue)
    {
        _inputs[cp] = outerValue;
    }

    void IRegion<IInlay>.AcknowledgeOutput(
        in OutputDescription cp,
        IInlay patchInstance,
        object? innerValue
    )
    {
        _outputs[cp] = innerValue;
    }

    void IRegion<IInlay>.RetrieveInput(
        in InputDescription cp,
        IInlay patchInstance,
        out object? innerValue
    )
    {
        _inputs.TryGetValue(cp, out innerValue);
    }

    void IRegion<IInlay>.RetrieveOutput(in OutputDescription cp, out object? outerValue)
    {
        _outputs.TryGetValue(cp, out outerValue);
    }

    // Static helper to retrieve the ContextProviderRegion from inside the region
    public static ContextProviderRegion GetProvider(
        [Pin(Visibility = Model.PinVisibility.Hidden)] NodeContext nodeContext
    ) =>
        ScopedValueStore.LookupByName<ContextProviderRegion>(
            nodeContext,
            CONTEXT_PROVIDER_REGION,
            true
        );

    public interface IInlay
    {
        // The region instance is passed as a parameter
        // Users inside can connect to this and access the region's services
        void Execute(ContextProviderRegion provider);
    }
}
