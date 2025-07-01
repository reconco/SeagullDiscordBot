using System;

namespace BuildVersionUpdater // Note: actual namespace depends on the project name.
{
    internal class Program
    {
        static string ASSEMBLY_INFO_FILE = "SeagullDiscordBot\\SeagullDiscordBot.csproj";
        static string BUILD_VERSION_FILE = "BuildVersion.txt";

        static int[] version = new int[4] { 1, 0, 2206, 123 };

        static void Main(string[] args)
        {
            if (args.Count() != 2)
            {
                Console.WriteLine("Wrong parameters");
                return;
            }

            ASSEMBLY_INFO_FILE = args[0];
            BUILD_VERSION_FILE = args[1];

            if (File.Exists(BUILD_VERSION_FILE))
                ReadBuildVersion();
            else
                ReadAssemblyInfoVersion();

            UpdateVersion();

            SaveAssemblyInfoVersion();
            SaveBuildVersion();

			Console.WriteLine("Version update sucessed...");
		}

        static private void UpdateVersion()
        {
            string yearMonth = DateTime.Now.ToString("yyMM");
            int currentYearMonth = int.Parse(yearMonth);

            if (version[2] != currentYearMonth)
            {
                version[2] = currentYearMonth;
                version[3] = 0;
            }
            else
                version[3]++;
        }

        static private void SaveAssemblyInfoVersion()
        {
            StreamReader sr = new StreamReader(ASSEMBLY_INFO_FILE);
            if (sr == null)
                return;

            List<string> lines = new List<string>();
            while(true)
            {
                if (sr.EndOfStream == true)
                    break;

                string line = sr.ReadLine();
                if (line == null)
                    break;

                if(line.Contains("<Version>") && line.Contains("</Version>"))
                {
                    line = $"<Version>{version[0]}.{version[1]}.{version[2]}.{version[3]}</Version>";
                }

                lines.Add(line);
            }

            sr.Close();


            StreamWriter sw = new StreamWriter(ASSEMBLY_INFO_FILE);
            if (sw == null)
                return;

            for (int i = 0; i < lines.Count; i++)
            {
                sw.WriteLine(lines[i]);
            }
            sw.Close();
        }


        static private void ReadAssemblyInfoVersion()
        {
            StreamReader sr = new StreamReader(ASSEMBLY_INFO_FILE);
            if (sr == null)
                return;

            string assemblyInfoStr = sr.ReadToEnd();
            sr.Close();

            int startIdx = assemblyInfoStr.IndexOf("\n<Version>");
            assemblyInfoStr = assemblyInfoStr.Remove(0, startIdx + "\n<Version>".Length);

            int lastIdx = assemblyInfoStr.IndexOf("</Version>");
            assemblyInfoStr = assemblyInfoStr.Remove(lastIdx);

            string[] newVer = assemblyInfoStr.Split('.');
            for (int i = 0; i < version.Length; i++)
            {
                version[i] = int.Parse(newVer[i]);
            }
        }

        static private void ReadBuildVersion()
        {
            StreamReader sr = new StreamReader(BUILD_VERSION_FILE);
            if (sr == null)
                return;

            string line = sr.ReadLine();
            sr.Close();

            if (line == null)
                return;

            string[] newVer = line.Split('.');
            for (int i = 0; i < version.Length; i++)
            {
                version[i] = int.Parse(newVer[i]);
            }
        }

        static private void SaveBuildVersion()
        {
            StreamWriter sw = new StreamWriter(BUILD_VERSION_FILE);
            
            for(int i = 0; i < version.Length; i++)
            {
                sw.Write(version[i]);

                if(i != version.Length - 1)
                    sw.Write(".");
            }
            sw.Close();
        }
    }
}