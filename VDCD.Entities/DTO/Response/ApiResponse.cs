namespace VDCD.Entities.DTO.Response
{
    public class ApiResponse<T>
    {
        public int Status { get; set; }

        public string Message { get; set; } = string.Empty;

        public T? Data { get; set; }

        public bool IsSuccess { get; set; }

        public ApiResponse()
        {
        }

        public ApiResponse(
            int status,
            string message,
            T? data,
            bool isSuccess)
        {
            Status = status;
            Message = message;
            Data = data;
            IsSuccess = isSuccess;
        }

        public static ApiResponse<T> Success(
            T? data,
            string message = "Thành công",
            int status = 200)
        {
            return new ApiResponse<T>
            {
                Status = status,
                Message = message,
                Data = data,
                IsSuccess = true
            };
        }

        public static ApiResponse<T> Fail(
            string message,
            int status = 400,
            T? data = default)
        {
            return new ApiResponse<T>
            {
                Status = status,
                Message = message,
                Data = data,
                IsSuccess = false
            };
        }
    }
}