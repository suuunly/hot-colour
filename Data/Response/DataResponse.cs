namespace HotColour.Data.Response
{
    public record DataResponse<T>(TypeOfFailure Error, T Data, string ErrorMessage = "")
    {
        public static DataResponse<T> Fail(TypeOfFailure failure, string errorMessage = "")
        {
            return new DataResponse<T>(failure, default, errorMessage);
        }
    
        public static DataResponse<T> Success(T data)
        {
            return new DataResponse<T>(TypeOfFailure.None, data);
        }

        public bool Successful => Error == TypeOfFailure.None;
    }
    
    public record DataResponse(TypeOfFailure Error, string ErrorMessage = "")
    {
        public static DataResponse Fail(TypeOfFailure error, string errorMessage = "")
        {
            return new DataResponse(error, errorMessage);
        }
    
        public static DataResponse Success()
        {
            return new DataResponse(TypeOfFailure.None);
        }

        public bool Successful => Error == TypeOfFailure.None;
    }
}