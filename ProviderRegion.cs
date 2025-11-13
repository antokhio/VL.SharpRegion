using VL.Core;
using VL.Core.Import;
using VL.Core.PublicAPI;
using VL.Lib.Collections;
using VL.Lib.Control;
using VL.Lib.Primitive;

namespace VL.SharpRegion
{
    public class ProviderStore : ScopedValueStore { }

    /************************************************************************
     * WARNING: CONTAINS ERROR DO NOT USE
     *
     * Stored just for reference to pin point errro
     ***********************************************************************/
    [Obsolete]
    [ProcessNode(FragmentSelection = FragmentSelection.Explicit, HasStateOutput = true)]
    public class ProviderRegion : IDisposable
    {
        private readonly NodeContext _nodeContext;
        private Spread<object> _inputs;
        private ICustomRegionPatch _patch;
        private readonly ProviderStore _store = new();
        private IDisposable _currentScope;

        [Fragment]
        public int Test { get; set; }

        [Fragment]
        public ProviderRegion(NodeContext nodeContext)
        {
            _nodeContext = nodeContext;
        }

        private bool _invalidate = true;

        [Fragment]
        public void Update(ICustomRegion input)
        {
            if (_invalidate)
            {
                if (_patch != null)
                    IDisposableUtils.TryDispose(_patch, out var patchDisposed);

                if (_currentScope != null)
                    _currentScope.Dispose();

                // Initilize patch
                _patch = input.CreateRegionPatch(_nodeContext, _inputs, out var initialOutputs);

                // Create signatures for Recive (Local)
                var storeInputs = new[]
                {
                    new BorderControlPointDescription("Provider", typeof(ProviderRegion), 0, false),
                };
                // Create refernces for Recive (Local)
                var stroeValues = new[] { this };

                // Bind store inputs
                _store.Configurate(storeInputs);

                // THIS IS INCORRENT see the ContextProviderRegion for correct implementation
                // Bind store
                _currentScope = _store.ActivateScope(_nodeContext, stroeValues);

                _invalidate = false;
            }
            else
            {
                // Update store
                _patch.Update(_inputs, out var outputs, Spread<object>.Empty);
            }
        }

        public void Dispose()
        {
            if (_patch != null)
                IDisposableUtils.TryDispose(_patch, out var patchDisposed);

            // THIS WOULD DESTROY SCOPE GLOBALLY see the ContextProviderRegion for correct implementation
            _currentScope?.Dispose();
        }

        // Get provider inside region
        public static ProviderRegion Provider(
            [Pin(Visibility = Model.PinVisibility.Hidden)] NodeContext nodeContext
        ) => ScopedValueStore.LookupByName<ProviderRegion>(nodeContext, "Provider", true);
    }
}
