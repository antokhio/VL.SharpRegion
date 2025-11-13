using VL.Core;
using VL.Core.Import;
using VL.Core.PublicAPI;
using VL.Lib.Collections;

namespace VL.SharpRegion
{
    [ProcessNode(
        Name = "SharpRegion",
        HasStateOutput = true,
        FragmentSelection = FragmentSelection.Explicit
    )]
    public class SharpRegion : ICustomRegion
    {
        // Test something i want to provide
        public record SharpRegionRecord(int Test);

        private ICustomRegionPatch? _currentPatch;
        private SharpRegionRecord _sharedRecord;

        // ICustomRegion implementation
        public Spread<BorderControlPointDescription> Inputs { get; private set; }
        public Spread<BorderControlPointDescription> Outputs { get; private set; }
        public Spread<IncomingLinkDescription> IncomingLinks { get; private set; }
        public Spread<object> InputValues { get; private set; }
        public IReadOnlyList<object> OutputValues { private get; set; }
        public Spread<object> IncomingLinkValues { get; private set; }
        public bool PatchHasChanged { get; private set; }

        [Fragment]
        public SharpRegion()
        {
            _sharedRecord = new SharpRegionRecord(42);

            // Define no border control points initially
            Inputs = Spread<BorderControlPointDescription>.Empty;
            Outputs = Spread<BorderControlPointDescription>.Empty;
            IncomingLinks = Spread<IncomingLinkDescription>.Empty;
            InputValues = Spread<object>.Empty;
            IncomingLinkValues = Spread<object>.Empty;
        }

        [Fragment]
        public void Update(ICustomRegion input)
        {
            if (input.PatchHasChanged)
            {
                _currentPatch?.Update(Spread<object>.Empty, out _, Spread<object>.Empty);
            }
        }

        public ICustomRegionPatch CreateRegionPatch(
            NodeContext context,
            IReadOnlyList<object> initialInputs,
            out Spread<object> initialOutputs
        )
        {
            // Create the user's patch and inject the record
            initialOutputs = Spread<object>.Empty;

            _currentPatch = new SharpRegionPatch(_sharedRecord);

            // What to do here ???
            return _currentPatch;
        }

        // Question: How to get actual patch in here?
        private class SharpRegionPatch : ICustomRegionPatch
        {
            private readonly SharpRegionRecord _record;

            public SharpRegionPatch(SharpRegionRecord record)
            {
                _record = record;
            }

            public ICustomRegionPatch Update(
                IReadOnlyList<object> inputs,
                out Spread<object> outputs,
                IReadOnlyList<object> incomingLinks
            )
            {
                // How to expose _record to nodes inside?
                outputs = Spread<object>.Empty;
                return this;
            }
        }
    }
}
