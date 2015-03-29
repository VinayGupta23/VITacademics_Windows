using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;


namespace VITacademics.Managers
{
    /// <summary>
    /// Provides helper methods to read and write text or objects (via serialization) to files.
    /// </summary>
    internal static class StorageHelper
    {
        public static async Task<bool> TryWriteAsync(StorageFile file, string content)
        {
            try
            {
                await FileIO.WriteTextAsync(file, content);
                return true;
            }
            catch
            {
                return false;
            }
        }

        public static async Task<bool> TryWriteAsync(StorageFile file, object contentGraph, params Type[] knownTypes)
        {
            Stream writeStream = null;
            bool result = false;
            try
            {
                DataContractSerializer serializer = new DataContractSerializer(contentGraph.GetType(), knownTypes);
                writeStream = await file.OpenStreamForWriteAsync();
                serializer.WriteObject(writeStream, contentGraph);
                result = true;
            }
            catch { }
            finally
            {
                if (writeStream != null)
                    writeStream.Dispose();
            }
            return result;
        }

        public static async Task<string> TryReadAsync(StorageFile file)
        {
            try
            {
                string content = await FileIO.ReadTextAsync(file);
                return content;
            }
            catch
            {
                return null;
            }
        }

        public static async Task<T> TryReadAsync<T>(StorageFile file, params Type[] knownTypes) where T : class
        {
            Stream readStream = null;
            T content = null;
            try
            {
                DataContractSerializer serializer = new DataContractSerializer(typeof(T), knownTypes);
                readStream = await file.OpenStreamForReadAsync();
                content = serializer.ReadObject(readStream) as T;
            }
            catch
            {
                content = null;
            }
            finally
            {
                if (readStream != null)
                    readStream.Dispose();
            }
            return content;
        }
    }
}
