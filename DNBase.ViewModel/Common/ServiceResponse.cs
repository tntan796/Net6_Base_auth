using System;

namespace DNBase.ViewModel
{
    public class ServiceResponse
    {
        public object Data { get; private set; }
        public int StatusCode { get; private set; }
        public bool Succeeded { get; private set; }
        public string Code { get; private set; }
        public string Message { get; private set; }

        private ServiceResponse()
        {
        }

        public static ServiceResponse Succeed(int statusCode, object data = default, string code = "", string message = "") => new ServiceResponse
        {
            StatusCode = statusCode,
            Data = data,
            Code = code,
            Message = message,
            Succeeded = true
        };

        public static ServiceResponse Fail(int statusCode, string code, string message) => new ServiceResponse
        {
            StatusCode = statusCode,
            Code = code,
            Message = message,
            Succeeded = false
        };

        public T GetData<T>()
        {
            try
            {
                var result = Convert.ChangeType(Data, typeof(T));

                return result == null ? default : (T)result;
            }
            catch (InvalidCastException)
            {
                throw new InvalidCastException("Data type is not correct.");
            }
        }
    }
}