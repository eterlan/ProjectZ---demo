using System.IO;
using UnityEngine;

namespace ProjectZ.LoadData
{
    public static class JsonReader
    {
        /// <summary>
        /// LoadJson with fileName
        /// </summary>
        /// <typeparam name="T">DataType as fileName</typeparam>
        /// <returns>DataType[] array</returns>
        public static T[] LoadData<T>()
        {
            var fileName = typeof(T).Name + ".json";
            return LoadData<T>(fileName);
        }
        
        /// <summary>
        /// Serialize Json with Custom FileName 
        /// </summary>
        /// <param name="fileName">Custom fileName</param>
        /// <typeparam name="T">DataType</typeparam>
        /// <returns>DataType[] array</returns>
        public static T[] LoadData<T>(string fileName)
        {
            var filePath = Path.Combine(Application.streamingAssetsPath, fileName);
            if (File.Exists(filePath))
            {
                var dataAsJson = File.ReadAllText(filePath);
                return JsonHelper.FromJson<T>(dataAsJson);
            }

            Debug.LogError("File not found. Check the fileName please.");
            return null;
        }
    }

    public enum DataType
    {
        FactorInfo,
        BehaviourInfo,
    }
}