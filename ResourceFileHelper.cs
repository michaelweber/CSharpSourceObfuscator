using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Resources;
using System.Text;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;

namespace RoslynObfuscator
{
    public class EmbeddedResourceData
    {
        public string Name;
        public byte[] Data;
    }

    public static class ResourceFileHelper
    {
        public static string CreateResXFromEmbeddedResourceData(List<EmbeddedResourceData> embeddedData)
        {
            string path = Path.GetTempFileName().Replace(".tmp", ".resx");
            ResXResourceWriter rw = new ResXResourceWriter(path);
            foreach (var data in embeddedData)
            {
                string tempFilePath = Path.GetTempFileName();
                File.WriteAllBytes(tempFilePath, data.Data);
                ResXDataNode resXDataNode = new ResXDataNode(data.Name, new ResXFileRef(tempFilePath, "System.IO.MemoryStream"));
                rw.AddResource(resXDataNode);
            }

            rw.Generate();
            rw.Close();

            return path;
        }

        public static MemoryStream ReadResXFileAsMemoryStream(string inFile)
        {
            var readers = new List<ReaderInfo>();
            var resources = readResources(inFile);
            using (var outStream = writeResources(resources))
            {
                //outstream is closed, so we create a new memory stream based on its buffer.
                var openStream = new MemoryStream(outStream.GetBuffer());
                return openStream;
            }
        }

        private static MemoryStream writeResources(ReaderInfo resources)
        {
            var memoryStream = new MemoryStream();
            using (var resourceWriter = new ResourceWriter(memoryStream))
            {
                writeResources(resources, resourceWriter);
            }
            return memoryStream;
        }

        private static void writeResources(ReaderInfo readerInfo, ResourceWriter resourceWriter)
        {
            foreach (ResourceEntry entry in readerInfo.resources)
            {
                string key = entry.Name;
                object value = entry.Value;
                resourceWriter.AddResource(key, value);
            }
        }

        private static ReaderInfo readResources(string fileName)
        {
            ReaderInfo readerInfo = new ReaderInfo();
            var path = Path.GetDirectoryName(fileName);
            var resXReader = new ResXResourceReader(fileName);
            resXReader.BasePath = path;

            using (resXReader)
            {
                IDictionaryEnumerator resEnum = resXReader.GetEnumerator();
                while (resEnum.MoveNext())
                {
                    string name = (string)resEnum.Key;
                    object value = resEnum.Value;
                    addResource(readerInfo, name, value, fileName);
                }
            }

            return readerInfo;
        }

        private static void addResource(ReaderInfo readerInfo, string name, object value, string fileName)
        {
            ResourceEntry entry = new ResourceEntry(name, value);

            if (readerInfo.resourcesHashTable.ContainsKey(name))
            {
                //Duplicate resource name. We'll ignore and continue.
                return;
            }

            readerInfo.resources.Add(entry);
            readerInfo.resourcesHashTable.Add(name, value);
        }

        internal sealed class ReaderInfo
        {
            // We use a list to preserve the resource ordering (primarily for easier testing),
            // but also use a hash table to check for duplicate names.
            public ArrayList resources { get; }
            public Hashtable resourcesHashTable { get; }

            public ReaderInfo()
            {
                resources = new ArrayList();
                resourcesHashTable = new Hashtable(StringComparer.OrdinalIgnoreCase);
            }
        }

        /// <summary>
        /// Name value resource pair to go in resources list
        /// </summary>
        private class ResourceEntry
        {
            public ResourceEntry(string name, object value)
            {
                this.Name = name;
                this.Value = value;
            }

            public string Name { get; }
            public object Value { get; }
        }
    }
}
