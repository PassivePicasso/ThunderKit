namespace ThunderKit.Core.Pipelines
{
    public abstract class PipelineJob : ComposableElement
    {
        public const string RunStepsMenu = "Run Steps/";

        public abstract void Execute(Pipeline pipeline);
    }
}