using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System;
using System.IO;

namespace Smart_Pacifier___Tool.Resources.OutputResources
{


    public static class EnvLoader
    {
        public static void LoadEnvFile(string path)
        {
            if (!File.Exists(path))
            {
                Console.WriteLine($"Environment file not found at: {path}");
                return;
            }

            foreach (var line in File.ReadAllLines(path))
            {
                if (string.IsNullOrWhiteSpace(line) || line.StartsWith("#"))
                    continue;

                var parts = line.Split('=', 2);
                if (parts.Length == 2)
                {
                    Environment.SetEnvironmentVariable(parts[0].Trim(), parts[1].Trim());
                }
            }
        }
    }

}
