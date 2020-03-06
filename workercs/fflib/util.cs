using System;
using System.Net.Sockets;
using System.IO;
using System.Text;
using System.Collections.Generic;
using pb = global::Google.Protobuf;

using System.Linq;

namespace ff
{
    class Util
    {
        public static byte[] bytesPBBuffer = new byte[4096];
        public static double tan30  = Math.Tan(Math.PI / 6);
        public static double tan60  = Math.Tan(Math.PI / 3);
        public static string strBrokerListen = "tcp://127.0.0.1:43210";
        public static string strGateListen = "tcp://*:44000";
        public static byte[] MergeArray(byte[] array1, byte[] array2)
        {
            byte[] ret = new byte[array1.Length + array2.Length];
            Array.Copy(array1, 0, ret, 0, array1.Length);
            Array.Copy(array2, 0, ret, array1.Length, array2.Length);
            return ret;
        }
        public static byte[] MergeArray(byte[][] listByteArray)
        {
            int totalLen = 0;
            foreach (var each in listByteArray)
            {
                totalLen += each.Length;
            }
            byte[] ret = new byte[totalLen];
            totalLen = 0;
            foreach (var each in listByteArray)
            {
                each.CopyTo(ret, totalLen);
                totalLen += each.Length;
            }
            return ret;
        }
        public static byte[] String2Byte(string data)
        {
            return System.Text.UTF8Encoding.Default.GetBytes(data);
        }
        public static string Byte2String(byte[] data)
        {
            return System.Text.UTF8Encoding.Default.GetString(data);
        }
        public static string Byte2String(byte[] data, int index, long count)
        {
            return System.Text.UTF8Encoding.Default.GetString(data, index, (int)count);
        }
        public static byte[] Pb2Byte(pb::IMessage retMsg)
        {
            pb::CodedOutputStream cos = new pb::CodedOutputStream(bytesPBBuffer);
            retMsg.WriteTo(cos);
            byte[] ret = new byte[cos.Position];
            Array.Copy(bytesPBBuffer, 0, ret, 0, cos.Position);

            return ret;
        }
        public static T Byte2Pb<T>(byte[] data) where T : pb::IMessage, new()
        {
            T retMsg = new T();
            retMsg.MergeFrom(new pb::CodedInputStream(data));
            return retMsg;
        }
        public static int Distance(int x1, int y1, int x2, int y2)
        {
            return Math.Max(Math.Abs(x1 - x2), Math.Abs(y1 - y2));
        }
        public static long GetNowTimeMs()
        {
            System.DateTime currentTime = DateTime.Now;
            long ret = ((Int64)currentTime.Ticks) / 10000;
            return ret;
        }
        public static string InitClassByNames(string[] names)
        {
            string nspace = "ff";

            var q = from t in System.Reflection.Assembly.GetExecutingAssembly().GetTypes()
                where t.IsClass && t.Namespace == nspace
                select t;
            foreach(var t in q.ToList())
            {
                if (Array.IndexOf(names, t.Name) >= 0)
                {
                    object[] paraNone = new object[]{};
                    System.Reflection.MethodInfo method = t.GetMethod("Instance");
                    if (method != null){
                        //Console.WriteLine(t.Name + ":" + method);
                        var ret = method.Invoke(null, paraNone);
                        System.Reflection.MethodInfo initMethod = t.GetMethod("Init");
                        if (initMethod != null)
                        {
                            object retB = initMethod.Invoke(ret, paraNone);
                            if (retB != null && retB is bool)
                            {
                                bool b = (bool)retB;
                                FFLog.Trace(string.Format("{0} init {1}", t.Name, b?"ok":"failed"));
                            }
                        }
                    }
                }
            }
            return "";
        }
		public static int CalDirection(int srcx, int srcy, int destx, int desty)
        {
            int xoffet = destx - srcx;
            int yoffet = desty - srcy;
            //console.log('calDirection %d,%d,%d,%d, [%d,%d]', srcx, srcy, destx, desty, xoffet, yoffet);
            if (xoffet > 0)
            {
                int tanValue = Math.Abs(yoffet / xoffet);
                if (tanValue < Util.tan30)
                {
                    return (int)Direction.RIGHT;
                }
                else if (tanValue < Util.tan60)
                {
                    if (yoffet > 0)
                    {
                        return (int)Direction.RIGHT_DOWN;
                    }
                    return (int)Direction.UP_RIGHT;
                }
                else
                {
                    if (yoffet > 0)
                    {
                        return (int)Direction.DOWN;
                    }
                    return (int)Direction.UP;
                }
            }
            else if (xoffet < 0)
            {
                int tanValue = Math.Abs(yoffet / xoffet);
                if (tanValue < Util.tan30)
                {
                    return (int)Direction.LEFT;
                }
                else if (tanValue < Util.tan60)
                {
                    if (yoffet > 0)
                    {
                        return (int)Direction.DOWN_LEFT;
                    }
                    return (int)Direction.LEFT_UP;
                }
                else
                {
                    if (yoffet > 0)
                    {
                        return (int)Direction.DOWN;
                    }
                    return (int)Direction.UP;
                }
            }
            else
            {//xoffet == 0
                if (yoffet > 0)
                {
                    return (int)Direction.DOWN;
                }
                return (int)Direction.UP;
            }
        }
        public static GamePoint CalPointByDirLen(int dir, int len)
        {
            int [,] cfgDirOffset = new int[8, 2] {{0, -1}, {1, -1}, {1, 0}, {1, 1}, {0, 1}, {-1, 1}, {-1, 0}, {-1, -1}};
            return new GamePoint(cfgDirOffset[dir, 0] * len, cfgDirOffset[dir, 1] * len);
        }
    }
}
