namespace LifeCycleService.ApiClient
{
    public class JsonResult
    {
        public bool Success { get; set; }
        public string Message { get; set; }
    }

    public class JsonResult<T>
        : JsonResult
    {
        public T Data { get; set; }
    }
}