using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Heraldo.Global.Structs;
using UnityEngine;

namespace Heraldo.Global
{
    public class JsonParser
    {
        private static string fileName = "save.json";
        private static string password = "\x4270 \xD052 \x6DCD \x0FE9 \x9C33" +
                                         "\x1869 \xED1A \xFA47 \xE99A" +
                                         "\xD5EE \xF595 \x0994 \x1AC4" +
                                         "\x3A9F \xA54A \x63C4";
        static string GetFullPath()
        {  
           return Path.Combine(Application.persistentDataPath, fileName);
        }

        public static bool SaveFileExists()
        {
            return File.Exists(GetFullPath());
        }
        
        public static GameSaveData? ReadSaveData()
        {
            try
            {
                string fileData;
                //Read data
                using (FileStream stream = new(GetFullPath(), FileMode.Open))
                {
                    using (StreamReader reader = new(stream))
                    {
                        fileData = reader.ReadToEnd();
                    }
                }
                //Decrypt data
                fileData = fileData
                    .Select((val, ind) => (char)(val ^ password[ind % password.Length]))
                    .ToArray().ToString();
                //Parse to object
                return JsonUtility.FromJson<GameSaveData>(fileData);
            }
            catch (Exception e)
            {
                Debug.LogException(e);
                return null;
            }
        }

        public static async Task WriteSaveData(GameSaveData data)
        {
            //Jsonify data
            char[] fileData = JsonUtility.ToJson(data).ToCharArray();
            //Encrypt data
            fileData = fileData
                .Select((val, ind) => (char)(val ^ password[ind % password.Length]))
                .ToArray();
            using (FileStream stream = new(GetFullPath(), FileMode.Create))
            {
                using (StreamWriter writer = new(stream))
                {
                    await writer.WriteAsync(fileData);
                }
            }
        }

        public static void DeleteSaveData()
        {
            File.Delete(GetFullPath());
        }
    }
}