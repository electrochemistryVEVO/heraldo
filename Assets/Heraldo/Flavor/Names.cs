using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Heraldo.Global;
using UnityEngine;
using Random = System.Random;

namespace Heraldo.Flavor
{
    public class Names
    {
        private static string[] nombresPeruanos = new string[]{};
        private static string[] nombresChilenos = new string[]{};

        private static void ShufflePeruanos()
        {
            var random = new Random(0);
            for (int i = 0; i < nombresPeruanos.Length - 1; i++)
            {
                int gen = random.Next(i, nombresPeruanos.Length);
                (nombresPeruanos[gen], nombresPeruanos[i]) = (nombresPeruanos[i], nombresPeruanos[gen]);
            }
        }

        private static void ReadFiles()
        {
            var chilenosPath = Path.Combine(Application.streamingAssetsPath, "nombres_chilenos.json");
            var peruanosPath = Path.Combine(Application.streamingAssetsPath, "nombres_peruanos.json");
            try
            {
                string fileData;
                //Read data
                using (FileStream stream = new(chilenosPath, FileMode.Open))
                {
                    using (StreamReader reader = new(stream))
                    {
                        fileData = reader.ReadToEnd();
                    }
                } 
                //Parse to object
                nombresChilenos = JsonUtility.FromJson<NamesRecord>(fileData).namesData;
                //Read data
                using (FileStream stream = new(peruanosPath, FileMode.Open))
                {
                    using (StreamReader reader = new(stream))
                    { 
                        fileData = reader.ReadToEnd();
                    }
                } 
                //Parse to object
                nombresPeruanos = JsonUtility.FromJson<NamesRecord>(fileData).namesData;
                //Shuffle
                ShufflePeruanos();
            }
            catch (Exception e)
            {
                Debug.LogException(e); 
            }
        }

        public static string[] GetRandomBulk(int num)
        {
            if(nombresChilenos.Length == 0 || nombresPeruanos.Length == 0)ReadFiles();
            //Como la lista ya esta aleatorizada, solo queda tomar subarray
            return nombresPeruanos.Take(num).ToArray();
        }
    }
}