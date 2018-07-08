using System.IO;
using System.Xml;
using System.Text;
using System.Xml.Serialization;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;

namespace XFramework.Core
{
    /// <summary>
    /// 序列化助手类
    /// </summary>
    public class SerializeHelper
    {
        /// <summary>
        /// 对象序列化成Json字符串
        /// </summary>
        /// <typeparam name="T">T</typeparam>
        /// <param name="obj">要序列化的对象（对象需使用DataContract特性标记，属性需使用DataMember特性标记）</param>
        /// <returns></returns>
        public static string SerializeToJson<T>(T obj) where T : class
        {
            var jsonSerializer = new DataContractJsonSerializer(typeof(T));
            using (var ms = new MemoryStream())
            {
                jsonSerializer.WriteObject(ms, obj);
                string strJson = Encoding.UTF8.GetString(ms.ToArray());
                return strJson;
            }
        }

        /// <summary>
        /// Json字符串反序列化成对象
        /// </summary>
        /// <typeparam name="T">T</typeparam>
        /// <param name="strJson">Json字符串</param>
        /// <returns></returns>
        public static T DeserializeFromJson<T>(string strJson) where T : class
        {
            var jsonSerializer = new DataContractJsonSerializer(typeof(T));
            using (var ms = new MemoryStream(Encoding.UTF8.GetBytes(strJson)))
            {
                T obj = jsonSerializer.ReadObject(ms) as T;
                return obj;
            }
        }

        /// <summary>
        /// 对象序列化成 XML 字符串
        /// </summary>
        /// <typeparam name="T">T</typeparam>
        /// <param name="obj">要序列化的对象</param>
        /// <returns></returns>
        public static string SerializeToXml<T>(T obj) where T : class
        {
            XmlSerializer serializer = new XmlSerializer(typeof(T));
            return SerializeToXml(serializer, obj);
        }

        /// <summary>
        /// 对象序列化成 XML 字符串
        /// </summary>
        /// <typeparam name="T">T</typeparam>
        /// <param name="obj">要序列化的对象</param>
        /// <param name="root">指定根对象的名称</param>
        /// <returns></returns>
        public static string SerializeToXml<T>(T obj, XmlRootAttribute root) where T : class
        {
            XmlSerializer serializer = new XmlSerializer(typeof(T), root);
            return SerializeToXml(serializer, obj);
        }

        /// <summary>
        /// 对象序列化成 XML 字符串
        /// </summary>
        /// <typeparam name="T">T</typeparam>
        /// <param name="obj">要序列化的对象</param>
        /// <param name="root">指定根对象的名称</param>
        /// <param name="defaultNamespace">xml命名空间</param>
        /// <returns></returns>
        public static string SerializeToXml<T>(T obj, XmlRootAttribute root,string defaultNamespace) where T : class
        {
            XmlSerializer serializer = new XmlSerializer(typeof(T), null, new System.Type[] { }, root, defaultNamespace);
            return SerializeToXml(serializer, obj);
        }

        /// <summary>
        /// 对象序列化成 XML 字符串
        /// </summary>
        /// <typeparam name="T">T</typeparam>
        /// <param name="serializer">序列化器</param>
        /// <param name="obj">要序列化的对象</param>
        /// <returns></returns>
        public static string SerializeToXml<T>(XmlSerializer serializer, T obj) where T : class
        {
            using (MemoryStream ms = new MemoryStream())
            {
                serializer.Serialize(ms, obj);
                string xml = Encoding.UTF8.GetString(ms.ToArray());
                return xml;
            }
        }

        /// <summary>
        /// XML 字符串 反序列化成对象
        /// </summary>
        /// <typeparam name="T">T</typeparam>
        /// <param name="xml">xml内容</param>
        /// <param name="root">指定根对象的名称</param>
        /// <returns></returns>
        public static T DeserialFromXml<T>(string xml) where T : class
        {
            XmlSerializer serializer = new XmlSerializer(typeof(T));
            return DeserialFromXml<T>(serializer, xml);
        }

        /// <summary>
        /// XML 字符串 反序列化成对象
        /// </summary>
        /// <typeparam name="T">T</typeparam>
        /// <param name="xml">xml内容</param>
        /// <param name="root">指定根对象的名称</param>
        /// <returns></returns>
        public static T DeserialFromXml<T>(string xml, XmlRootAttribute root) where T : class
        {
            XmlSerializer serializer = new XmlSerializer(typeof(T), root);
            return DeserialFromXml<T>(serializer, xml);
        }

        /// <summary>
        /// XML 字符串 反序列化成对象
        /// </summary>
        /// <typeparam name="T">T</typeparam>
        /// <param name="xml">xml内容</param>
        /// <param name="root">指定根对象的名称</param>
        /// <param name="defaultNamespace">xml命名空间</param>
        /// <returns></returns>
        public static T DeserialFromXml<T>(string xml, XmlRootAttribute root, string defaultNamespace) where T : class
        {
            XmlSerializer serializer = new XmlSerializer(typeof(T), null, new System.Type[] { }, root, defaultNamespace);
            return DeserialFromXml<T>(serializer, xml);
        }

        /// <summary>
        /// XML 字符串 反序列化成对象
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="serializer">序列化器</param>
        /// <param name="xml">xml内容</param>
        /// <returns></returns>
        public static T DeserialFromXml<T>(XmlSerializer serializer, string xml) where T : class
        {
            using (Stream xmlStream = new MemoryStream(Encoding.UTF8.GetBytes(xml)))
            {
                using (XmlReader xmlReader = XmlReader.Create(xmlStream))
                {
                    T obj = serializer.Deserialize(xmlReader) as T;
                    return obj;
                }
            }
        }

        /// <summary>
        /// DataContract　序列化
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="encodeName"></param>
        /// <returns></returns>
        public static string DataContractSerialize(object obj, string encodeName = "utf-8")
        {
            Encoding encoding = Encoding.GetEncoding(encodeName) ?? Encoding.UTF8;
            string xml = string.Empty;
            DataContractSerializer serializer = new DataContractSerializer(obj.GetType());
            using (System.IO.MemoryStream ms = new System.IO.MemoryStream())
            {
                serializer.WriteObject(ms, obj);
                xml = encoding.GetString(ms.ToArray());
                return xml;
            }
        }

        /// <summary>
        /// DataContract　反序列化
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="objData"></param>
        /// <returns></returns>
        public static T DataContractDeSerialize<T>(string objData, string encodeName = "utf-8")
        {
            Encoding encoding = Encoding.GetEncoding(encodeName) ?? Encoding.UTF8;
            DataContractSerializer serializer = new DataContractSerializer(typeof(T));
            byte[] buffer = encoding.GetBytes(objData);
            using (MemoryStream ms = new MemoryStream(buffer))
            {
                T value = (T)serializer.ReadObject(ms);
                return value;
            }
        }
    }
}
