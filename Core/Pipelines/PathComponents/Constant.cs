namespace ThunderKit.Core.Pipelines.PathComponents
{
    public class Constant : PathComponent
    {
        public string Value;
        public override string GetPath(PathReference output, Manifests.Manifest manifest, Pipeline pipeline)
        {
            return Value;
        }
    }
}
