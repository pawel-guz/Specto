namespace Specto
{
    public interface IVisualizationDataReceiver
    {
        void Send(VisualizationData data, bool force = false);
    }
}
