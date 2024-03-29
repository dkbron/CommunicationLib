﻿using CommunicationLib.Common;
using Microsoft.VisualBasic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace CommunicationLib.Core.Network
{
    public class NetworkTools
    {
        #region 检查设置的IP地址是否正确，返回正确的IP地址 
        /// <summary>
        /// 是否为ip
        /// </summary>
        /// <param name="ip"></param>
        /// <returns></returns>
        public static bool IsIP(string ip)
        {
            bool isIp = System.Text.RegularExpressions.Regex.IsMatch(ip,
                @"^((2[0-4]\d|25[0-5]|[01]?\d\d?)\.){3}(2[0-4]\d|25[0-5]|[01]?\d\d?)$");
              
            return isIp;
        }
        #endregion

        #region 检查设置的端口号是否正确，返回正确的端口号
        /// <summary>
        /// 检查设置的端口号是否正确，并返回正确的端口号,无效端口号返回-1。
        /// </summary>
        /// <param name="port">设置的端口号</param>        
        public static bool IsPort(int port)
        {
            //最小有效端口号
            const int MINPORT = 0;
            //最大有效端口号
            const int MAXPORT = 65535;

            //检测端口范围
            if ((port < MINPORT) || (port > MAXPORT))
            { 
                return false;
            }
            return true;
        }
        #endregion

        //获得本地主机IP
        public static string GetLocalIp()
        {
            ///获取本地的IP地址
            string AddressIP = string.Empty;
            foreach (IPAddress _IPAddress in Dns.GetHostEntry(Dns.GetHostName()).AddressList)
            {
                if (_IPAddress.AddressFamily.ToString() == "InterNetwork")
                {
                    AddressIP = _IPAddress.ToString();
                }
            }
            return AddressIP;
        } 
        //将ASCII码转换为16进制码【HEX】
        /// <summary>
        /// 将ASCII码转换为16进制码【HEX】
        /// </summary>
        /// <param name="TargetString">目标字符串</param>
        /// <returns></returns>
        public static string StringConvertToHEX(string TargetString)
        { 
            string TempResult = "";

            try
            {
                for (Int16 a = 0; a < TargetString.Length; a++)
                {
                    TempResult += Conversion.Hex(Strings.Asc(Strings.Mid(TargetString, 1 + a, 1)));
                }
            }
            catch (Exception ex)
            { 
                return "";
            }

            return TempResult;
        }
        public static string EndPointToUIString(EndPoint endPoint)
        {
            string ip = endPoint.ToString().Split(':')[0];
            string port = endPoint.ToString().Split(':')[1];

            return $"{ip} [{port}]";
        }


        public static string IPEndPointToUIString(IPEndPoint endPoint)
        {
            string ip = endPoint.ToString().Split(':')[0];
            string port = endPoint.ToString().Split(':')[1];

            return $"{ip} [{port}]";
        }

        public static IPEndPoint EndPointToIPEndPoint(EndPoint endPoint)
        { 
            IPEndPoint ipEndPoint = (IPEndPoint)endPoint;
            return ipEndPoint;
        }

    }
}
