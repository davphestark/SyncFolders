using System;
using System.Globalization;
using System.IO;

namespace SyncFolders
{
    public class Log
    {
        public string LogFile { get; set; }
        public DateTime logStart { get; set; }
        public DateTime logEnd { get; set; }
        public long fileCount { get; set; }
        public long copyCount { get; set; }
        public long updateCount { get; set; }
        public long dirCount { get; set; }
        public long deleteDirCount { get; set; }
        public long equalCount { get; set; }
        public long diffLengthCount { get; set; }
        public long sameDateModCount { get; set; }
        public long diffBtyeCount { get; set; }
        public Log(string dir)
        {
            fileCount = 0;
            copyCount = 0;
            updateCount = 0;
            dirCount = 0;
            deleteDirCount = 0;
            equalCount = 0;
            diffLengthCount = 0;
            sameDateModCount = 0;
            diffBtyeCount = 0;
            logStart = DateTime.Now;
            LogFile = Path.Combine(dir, "syncLog" + logStart.ToString("yyyyMMddHHmmss") + ".txt");
            startLog();
        } 
        public void logMsg(string msg)
        {
            using (StreamWriter w = File.AppendText(LogFile))
            {
                w.WriteLine(msg);
            }
        }        
        public void startLog()
        {
            logMsg("-----Staring Sync: " + DateTime.Now.ToString("F", CultureInfo.CreateSpecificCulture("en-US")) + "---");
        }
        public void endSection(string msg)
        {
            var sectionEnd = DateTime.Now;
            logMsg(msg + "in: " + FormatTimeSpan(sectionEnd - logStart));
        }
        public void endLog()
        {
            logEnd = DateTime.Now;
            logMsg(string.Format($"----- { fileCount.ToString()} files in {dirCount.ToString()} directories reviewed ---"));
            logMsg(string.Format($"----- { equalCount.ToString()} files checked for changes, {diffLengthCount.ToString()} different lengths,  {sameDateModCount.ToString()} same DateMod and {(diffLengthCount + diffBtyeCount).ToString()} different files  ---"));
            logMsg(string.Format($"----- {copyCount.ToString()} files copied, {updateCount.ToString()} files updated, {deleteDirCount.ToString()} directories deleted in {FormatTimeSpan(logEnd - logStart)} ---"));
            logMsg("-----End Sync: " + logEnd.ToString("F", CultureInfo.CreateSpecificCulture("en-US")) + "---");
        }
        //could be an extention but didn't want a static for this since it is only called one right now
        private string FormatTimeSpan(TimeSpan time)
        {
            string formatted = string.Format("{0}{1}{2}{3}",
            time.Duration().Days > 0 ? string.Format("{0:0} day{1}, ", time.Days, time.Days == 1 ? String.Empty : "s") : string.Empty,
            time.Duration().Hours > 0 ? string.Format("{0:0} hour{1}, ", time.Hours, time.Hours == 1 ? String.Empty : "s") : string.Empty,
            time.Duration().Minutes > 0 ? string.Format("{0:0} minute{1}, ", time.Minutes, time.Minutes == 1 ? String.Empty : "s") : string.Empty,
            time.Duration().Seconds > 0 ? string.Format("{0:0}.{1} second{2}", time.Seconds,time.Milliseconds, time.Seconds == 1 ? String.Empty : "s") : string.Empty);

            return formatted;

        }
    }
}
