using VL.Core.Import;
using VL.Core.PublicAPI;

namespace VL.SharpRegion;

[ProcessNode]
[Region(SupportedBorderControlPoints = ControlPointType.Border)]
public class InlayRegion : IRegion<InlayRegion.IInlay>
{
    private readonly Dictionary<InputDescription, object?> _inputs = new();
    private readonly Dictionary<OutputDescription, object?> _outputs = new();

    // Context values provided by the region
    private string _contextString = "Hello from Context";
    private float _contextFloat = 42.0f;

    IInlay? inlay;

    public InlayRegion() { }

    public void Update()
    {
        if (inlay is null)
            return;

        // Update context values each frame
        _contextFloat += 0.1f;
        _contextString = $"Frame at {System.DateTime.UtcNow:yyyy-MM-dd HH:mm:ss}";

        // Execute user's patched logic
        inlay.Execute(_contextString, _contextFloat);
    }

    void IRegion<IInlay>.SetPatchInlayFactory(Func<IInlay> patchInlayFactory)
    {
        if (inlay is null)
            inlay = patchInlayFactory();
    }

    void IRegion<IInlay>.AcknowledgeInput(in InputDescription cp, object? outerValue)
    {
        // Called for each input from outside the region
        _inputs[cp] = outerValue;
    }

    void IRegion<IInlay>.AcknowledgeOutput(
        in OutputDescription cp,
        IInlay patchInstance,
        object? innerValue
    )
    {
        // Called from inside the patch to pass output values to the region
        _outputs[cp] = innerValue;
    }

    void IRegion<IInlay>.RetrieveInput(
        in InputDescription cp,
        IInlay patchInstance,
        out object? innerValue
    )
    {
        // Called from inside the patch to get input values
        // This is where we can inject context values!

        // Check if this is a request for a context value
        // The key question: How does the system identify which InputDescription
        // corresponds to a context value vs. a regular input?

        // Attempt 1: Check by Name
        if (cp.Name == "Context String")
        {
            innerValue = _contextString;
            return;
        }
        else if (cp.Name == "Context Float")
        {
            innerValue = _contextFloat;
            return;
        }

        // Attempt 2: Check by Id
        if (cp.Id == "ContextString")
        {
            innerValue = _contextString;
            return;
        }
        else if (cp.Id == "ContextFloat")
        {
            innerValue = _contextFloat;
            return;
        }

        // Attempt 3: Check if it's a link that we didn't receive from outside
        // Context values would be links that don't exist in _inputs
        if (cp.IsLink && !_inputs.ContainsKey(cp))
        {
            // This might be a context value request
            // But we need a way to identify which one...
        }

        // Default: try to get from regular inputs
        _inputs.TryGetValue(cp, out innerValue);
    }

    void IRegion<IInlay>.RetrieveOutput(in OutputDescription cp, out object? outerValue)
    {
        // Called from outside to get output values from the region
        _outputs.TryGetValue(cp, out outerValue);
    }

    public interface IInlay
    {
        void Execute(string contextString, float contextFloat);
    }
}
