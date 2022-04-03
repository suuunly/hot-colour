namespace HotColour.Data.Response
{
    public record DataResponse<T>(string Error, T Data)
    {
        public static DataResponse<T?> Fail(string errorMessage)
        {
            return new DataResponse<T?>(errorMessage, default);
        }
    
        public static DataResponse<T> Success(T data)
        {
            return new DataResponse<T>("", data);
        }

        public bool Successful => Error == string.Empty;
    }
}