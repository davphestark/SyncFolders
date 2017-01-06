using System;
using System.Configuration;

namespace SyncFolders
{
    class Program
    {
        static void Main(string[] args)
        {
            if (args.Length == 0)
            {
                //use app.config when debugging
                string Folder1 = ConfigurationManager.AppSettings.Get("startingDir");
                string Folder2 = ConfigurationManager.AppSettings.Get("copyToDir");
                if (!String.IsNullOrEmpty(Folder1) && !String.IsNullOrEmpty(Folder2))
                {
                    bool bothWays = Convert.ToBoolean(ConfigurationManager.AppSettings.Get("biDirectional"));
                    bool onlyLog = Convert.ToBoolean(ConfigurationManager.AppSettings.Get("justLog"));
                    startProcess(Folder1, Folder2, bothWays, onlyLog);
                }
                else
                {
                    noParamsMsg();
                }
            }
            else if (args.Length < 4)
            {
                noParamsMsg();
            }
            else
            {
                string Folder1 = String.IsNullOrEmpty(args[0]) ? "" : args[0];
                string Folder2 = String.IsNullOrEmpty(args[1]) ? "" : args[1];
                bool bothWays = Convert.ToBoolean(args[2]);
                bool onlyLog = Convert.ToBoolean(args[3]);
                startProcess(Folder1, Folder2, bothWays, onlyLog);
            }
            Console.WriteLine("done");
            Console.ReadKey();
        }
        static void startProcess(string f1, string f2, bool biDirectional,bool onlyLog)
        {
            Console.WriteLine(" {0} to {1} both directions {2} only log? {3}", f1, f2, biDirectional, onlyLog);
            Sync oSync = new Sync(f1, f2, onlyLog, biDirectional);
            oSync.startSync();
        }
        static void noParamsMsg()
        {
            Console.WriteLine("Please enter Folder1, Folder2 SyncBothWays, OnlyLog");
        }
    }
}
