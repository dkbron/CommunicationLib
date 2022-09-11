using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommunicationLib.Common
{
    public class OperationResult
    {
        /// <summary>
        /// 指示本次操作是否成功。
        /// </summary>
        public bool IsSuccess { get; set; } = false;

        /// <summary>
        /// 错误信息
        /// </summary>
        public string ErrorMsg { get; set; } = "unknow error!";

        public Exception Ex { get; set; }
         
        public OperationResult()
        {
            IsSuccess = true;
        } 

        public OperationResult(string msg)
        {
            IsSuccess = false; 
            ErrorMsg = msg;
        }

        public OperationResult(Exception ex)
        {
            IsSuccess = false;
            Ex = ex;
            ErrorMsg = ex.Message;
        }
    }

    public class OperationResult<T>:OperationResult
    {
        /// <summary>
        /// 操作结果
        /// </summary>
        public T Result { get; set; }

        public OperationResult(T result)
        {
            IsSuccess = true;
            Result = result;
        }
    }



}
