using System.IO;
using System.Xml;
using System.Text;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;

namespace SQLite.Net
{
    public class XmlSerializer : IBlobSerializer
    {
        public List<Type> KnownTypes { get; set; }

        public byte[] Serialize<T>(T obj)
        {
            using (var memoryStream = new MemoryStream())
            using (var reader = new StreamReader(memoryStream))
            {
                var serializer = new DataContractSerializer(obj.GetType(), this.KnownTypes);
                serializer.WriteObject(memoryStream, obj);
                var bytes = new byte[memoryStream.Length];
                memoryStream.Position = 0;
                memoryStream.Read(bytes, 0, bytes.Length);
                return bytes;
            }
        }

        public object Deserialize(byte[] data, Type type)
        {
            using (var stream = new MemoryStream(data))
            {
                var serializer = new DataContractSerializer(type, this.KnownTypes);
                return serializer.ReadObject(stream);
            }
        }

        public bool CanDeserialize(Type type)
        {
            // hack for now
            DataContractSerializer serializer = null;
            try
            {
                serializer = new DataContractSerializer(type, this.KnownTypes);
            }
            catch (Exception ex)
            {
                while (ex != null)
                {
                    System.Diagnostics.Debug.WriteLine(ex.Message);
                    ex = ex.InnerException;
                }
            }

            return serializer != null;
        }
    }

    public class JsonSerializer : IBlobSerializer
    {
        public List<Type> KnownTypes { get; set; }

        public byte[] Serialize<T>(T obj)
        {
            using (var memoryStream = new MemoryStream())
            using (var reader = new StreamReader(memoryStream))
            {
                var serializer = new DataContractJsonSerializer(obj.GetType(), this.KnownTypes);
                serializer.WriteObject(memoryStream, obj);
                var bytes = new byte[memoryStream.Length];
                memoryStream.Position = 0;
                memoryStream.Read(bytes, 0, bytes.Length);
                return bytes;
            }
        }

        public object Deserialize(byte[] data, Type type)
        {
            using (var stream = new MemoryStream(data))
            {
                var serializer = new DataContractJsonSerializer(type, this.KnownTypes);
                return serializer.ReadObject(stream);
            }
        }

        public bool CanDeserialize(Type type)
        {
            // hack for now
            DataContractJsonSerializer serializer = null;
            try
            {
                serializer = new DataContractJsonSerializer(type, this.KnownTypes);
            }
            catch (Exception ex)
            {
                while (ex != null)
                {
                    System.Diagnostics.Debug.WriteLine(ex.Message);
                    ex = ex.InnerException;
                }
            }

            return serializer != null;
        }
    }
}
