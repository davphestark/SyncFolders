using System;
using System.IO;
using System.Linq;

namespace SyncFolders
{
    public class Sync
    {
        public string startPath { get; set; }
        public string secondPath { get; set; }
        public bool logOnly { get; set; }
        public bool bothWays { get; set; }
        public Log log { get; set; }
        
        public Sync()
        {
            logOnly = true;
            bothWays = false;
        }
        public Sync(string path1, string path2, bool isLog, bool bothWays) : this()
        {
            startPath = path1;
            secondPath = path2;
            logOnly = isLog;
            this.bothWays = bothWays;
        }

        public bool startSync()
        {
            try
            {
                log = new Log(startPath);
                startCopy();
                log.endLog();
                return true;
            }
            catch (Exception e)
            {
                Console.WriteLine("Error: " + e.Message);
                return false;
            }
        }
        private bool startCopy()
        {
            try
            {
                DeleteEmptyDirs(startPath);
                log.endSection("--Empty directories deleted completed ");
                CopyDir(startPath, secondPath);
                if (bothWays)
                {
                    bothWays = false;
                    var tmpPath = secondPath;
                    secondPath = startPath;
                    startPath = tmpPath;
                    startCopy();
                }
                return true;

            } catch (Exception e)
            {
                {
                    Console.WriteLine("Error: " + e.Message);
                    return false;
                }
            }
        }
        public bool NotIgnoreList(string fileName)
        {
            if (fileName.EndsWith(".db", StringComparison.CurrentCultureIgnoreCase) ||
                (fileName.Contains("syncLog") && fileName.EndsWith(".txt", StringComparison.CurrentCultureIgnoreCase)))
            {
                return false;
            }
            return true;
        }
        public void CopyDir(string searchDir, string copyDir)
        {
            if (!Directory.Exists(searchDir)) {
                log.logMsg("Starting Folder (" + searchDir +" ) does not exist");
                return;
            }

            try {
                ProcessFiles(searchDir, copyDir);
           
            } catch (Exception e) {
                log.logMsg("full copy error: " + e.Message);
            }
        }
        //switched from a get all files and directories to avoid it craping out when it ran into unathorized directories and files
        public void ProcessFiles(string dir, string copyDir)
        {
            try {
                foreach (string startFile in Directory.GetFiles(dir)) {
                   if (NotIgnoreList(startFile)) {
                        if (File.Exists(copyDir + startFile.Substring(dir.Length))) {
                            //needed a better check for modified files, pulled this function from http://stackoverflow.com/questions/1358510/how-to-compare-2-files-fast-using-net
                            //now only if they are different take the newer one.
                            if (!FilesAreEqual(new FileInfo(startFile), new FileInfo(copyDir + startFile.Substring(dir.Length)))) {
                                if (File.GetLastWriteTime(startFile) > File.GetLastWriteTime(copyDir + startFile.Substring(dir.Length))) {
                                    if (!logOnly) {
                                        File.Copy(startFile, copyDir + startFile.Substring(dir.Length), true);
                                        Console.Write(".");
                                    }
                                    log.updateCount++;
                                    log.logMsg(String.Format("{0} newer, copied to folder: {1}", startFile, copyDir + startFile.Substring(dir.Length)));
                                }
                            }
                        }
                        else {
                            if (!logOnly) {
                                File.Copy(startFile, copyDir + startFile.Substring(dir.Length));
                                Console.Write(".");
                            }
                            log.copyCount++;
                            log.logMsg(String.Format("{0}, copied to folder: {1}", startFile, copyDir + startFile.Substring(dir.Length)));
                        }
                        log.fileCount++;
                    }
                }
                foreach (string startDir in Directory.GetDirectories(dir)) {
                    if (!logOnly)   {
                        try {
                            Directory.CreateDirectory(copyDir + startDir.Substring(dir.Length));
                        }
                        catch (Exception e) {
                            log.logMsg(String.Format("unabled to create folder: {0} {1}", copyDir + startDir.Substring(dir.Length), e.Message));
                        }
                    }
                    log.dirCount++;
                    ProcessFiles(startDir, copyDir + startDir.Substring(dir.Length));
                }
            }
            catch (UnauthorizedAccessException e) {
                log.logMsg("access error: " + e.Message);
            }
            catch (Exception e) {
                log.logMsg("Proccessing Dir error: " + e.Message);
            }
        }
        public void DeleteEmptyDirs(string dir)
        {
            if (String.IsNullOrEmpty(dir))
            {
                log.logMsg(String.Format("{0} is  a null reference or an empty string", dir));
            }
             
            try
            {
                foreach (var d in Directory.EnumerateDirectories(dir))
                {
                    DeleteEmptyDirs(d);
                }

                var entries = Directory.EnumerateFileSystemEntries(dir);

                if (!entries.Any())
                {
                    try
                    {
                        if (!logOnly)
                        {
                            Directory.Delete(dir);
                        }
                        log.deleteDirCount++;
                        log.logMsg(String.Format("Deleted blank folder: {0}", dir));
                    }
                    catch (UnauthorizedAccessException) { }
                    catch (DirectoryNotFoundException) { }
                }
            }
            catch (UnauthorizedAccessException) { }
            catch (Exception e)
            {
                log.logMsg("delete empty dir error: " + e.Message);
            }
        }
        
        private const int BYTES_TO_READ = sizeof(Int64);
        private bool FilesAreEqual(FileInfo first, FileInfo second)
        {
            log.equalCount++;
            if (first.Length != second.Length)
            {
                log.diffLengthCount++;
                return false;
            }
            if (first.LastWriteTime == second.LastWriteTime)
            {
                log.sameDateModCount++;
                return true;
            }

            int iterations = (int)Math.Ceiling((double)first.Length / BYTES_TO_READ);
            using (FileStream fs1 = first.OpenRead())
            using (FileStream fs2 = second.OpenRead())
            {
                byte[] one = new byte[BYTES_TO_READ];
                byte[] two = new byte[BYTES_TO_READ];

                for (int i = 0; i < iterations; i++)
                {
                    fs1.Read(one, 0, BYTES_TO_READ);
                    fs2.Read(two, 0, BYTES_TO_READ);

                    if (BitConverter.ToInt64(one, 0) != BitConverter.ToInt64(two, 0))
                    {
                        log.diffBtyeCount++;
                        return false;
                    }
                }
            }

            return true;
        }
    }
}
