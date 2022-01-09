namespace AopWebApi.Services
{
    public enum ResultTypeEnum
    {
        Default = 0,
        AsyncException = 1,
        TaskCancelled = 2,
        SyncException = 3,
        SyncSuccess = 4,
        AsyncSuccess = 5,
    }
}
