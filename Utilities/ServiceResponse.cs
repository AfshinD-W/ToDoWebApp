using System.Text.Json.Serialization;

namespace SSToDo.Utilities
{
    public class ServiceResponse<T>
    {
        [JsonIgnore]
        public string Message { get; set; }
        [JsonIgnore]
        public T Data { get; set; }

        public ServiceResponse(string message)
        {
            Message = message;
        }

        public ServiceResponse(T data)
        {
            Data = data;
        }

        public object Result => (object)Data ?? Message;
    }
}
