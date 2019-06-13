using UnityEngine;

namespace ProjectZ.LoadData
{
    public static class JsonHelper
    {
        public static T[] FromJson<T>(string json)
        {
            Wrapper<T> wrapper = JsonUtility.FromJson<Wrapper<T>>(json);
            return wrapper.Items;
        }

        public static string ToJson<T>(T[] array)
        {
            Wrapper<T> wrapper = new Wrapper<T>();
            wrapper.Items = array;
            return JsonUtility.ToJson(wrapper);
        }

        public static string ToJson<T>(T[] array, bool prettyPrint)
        {
            Wrapper<T> wrapper = new Wrapper<T>();
            wrapper.Items = array;
            return JsonUtility.ToJson(wrapper, prettyPrint);
        }
        /// <summary>
        /// Convert Str to Enum
        /// </summary>
        /// <param name="str">String to be parsed</param>
        /// <typeparam name="T">Enum Type</typeparam>
        /// <returns></returns>
        public static T ParseEnum<T>(string str)
        {
            return (T)System.Enum.Parse(typeof(T), str);
        }

        [System.Serializable]
        private class Wrapper<T>
        {
            public T[] Items;
        }
    }
}