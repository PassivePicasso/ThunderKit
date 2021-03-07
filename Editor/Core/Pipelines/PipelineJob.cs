namespace ThunderKit.Core.Pipelines
{
    public abstract class PipelineJob : ComposableElement
    {
        public abstract void Execute(Pipeline pipeline);
    }
}