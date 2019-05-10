using Bubble.model;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bubble.util
{
    class UIUtil
    {
        public static bool writeSettings()
        {
            FileStream fs = new FileStream("setting.json", FileMode.Create);
            string json = JsonConvert.SerializeObject(new Setting());
            byte[] data = System.Text.Encoding.Default.GetBytes(json);
            fs.Write(data, 0, data.Length);
            fs.Flush();
            fs.Close();

            return true;
        }
        public static void readSettings()
        {
            FileStream fs = new FileStream("setting.json", FileMode.Open);
            StreamReader sr = new StreamReader(fs);
            StringBuilder sb = new StringBuilder();
            String line = null;
            while ((line = sr.ReadLine()) != null)
            {
                sb.Append(line);
            }
            Console.WriteLine(sb.ToString());

            Setting setting = JsonConvert.DeserializeObject<Setting>(sb.ToString());
            sr.Close();
            fs.Close();
        }

    }
}
