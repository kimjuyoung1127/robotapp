// Folder: App - Application controllers and services; single UnityEngine entry point.
namespace KineTutor3D.App.Fairino
{
    /// <summary>
    /// FAIRINO 명령 실행 결과입니다.
    /// </summary>
    public readonly struct FairinoResult
    {
        /// <summary>
        /// 에러 코드입니다. 0이면 성공입니다.
        /// </summary>
        public int ErrorCode { get; }

        /// <summary>
        /// 사용자 표시용 메시지입니다.
        /// </summary>
        public string Message { get; }

        /// <summary>
        /// 성공 여부입니다.
        /// </summary>
        public bool IsSuccess => ErrorCode == 0;

        /// <summary>
        /// 결과를 생성합니다.
        /// </summary>
        public FairinoResult(int errorCode, string message)
        {
            ErrorCode = errorCode;
            Message = message ?? string.Empty;
        }

        /// <summary>
        /// 성공 결과를 생성합니다.
        /// </summary>
        public static FairinoResult Ok(string message = "OK")
        {
            return new FairinoResult(0, message);
        }

        /// <summary>
        /// 실패 결과를 생성합니다.
        /// </summary>
        public static FairinoResult Fail(int errorCode, string message)
        {
            return new FairinoResult(errorCode, message);
        }
    }

    /// <summary>
    /// 값을 포함하는 FAIRINO 명령 실행 결과입니다.
    /// </summary>
    public readonly struct FairinoResult<T>
    {
        /// <summary>
        /// 에러 코드입니다. 0이면 성공입니다.
        /// </summary>
        public int ErrorCode { get; }

        /// <summary>
        /// 사용자 표시용 메시지입니다.
        /// </summary>
        public string Message { get; }

        /// <summary>
        /// 성공 여부입니다.
        /// </summary>
        public bool IsSuccess => ErrorCode == 0;

        /// <summary>
        /// 결과 값입니다.
        /// </summary>
        public T Value { get; }

        /// <summary>
        /// 결과를 생성합니다.
        /// </summary>
        public FairinoResult(int errorCode, string message, T value)
        {
            ErrorCode = errorCode;
            Message = message ?? string.Empty;
            Value = value;
        }

        /// <summary>
        /// 성공 결과를 생성합니다.
        /// </summary>
        public static FairinoResult<T> Ok(T value, string message = "OK")
        {
            return new FairinoResult<T>(0, message, value);
        }

        /// <summary>
        /// 실패 결과를 생성합니다.
        /// </summary>
        public static FairinoResult<T> Fail(int errorCode, string message)
        {
            return new FairinoResult<T>(errorCode, message, default);
        }
    }
}
