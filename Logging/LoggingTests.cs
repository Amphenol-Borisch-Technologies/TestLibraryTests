using System.IO;
using System;
using System.Linq;
using Serilog;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ABTTestLibrary.AppConfig;
using ABTTestLibrary.TestSupport;
using ABTTestLibrary.Logging;
using System.Reflection;
using System.Windows.Forms;

namespace ABTTestLibraryTests.Logging {
    [TestClass()]
    public class LoggingTests {
        [TestMethod()]
        public void StartTestTextEnabledGroupRequired() {
            DialogResult dr = MessageBox.Show($"Select Group ID1", $"Select Group ID1", MessageBoxButtons.OKCancel);
            if (dr == DialogResult.Cancel) Assert.Inconclusive();
            Config Config = Config.Get();
            Assert.IsTrue(Config.Group.Required);
            Assert.IsTrue(Config.Logger.FileEnabled);
            Assert.IsFalse(Config.Logger.SQLEnabled);
            // Cannot currently test SQLEnabled case, as unimplemented.
            const String FIRST_AND_LAST_LINE_TEST_STRING = "This file & contents should be overwritten during LogTasks.Start().Confirm at end of test.";
            File.WriteAllText(LogTasks.LOGGER_FILE, FIRST_AND_LAST_LINE_TEST_STRING);
            String firstLine = File.ReadLines(LogTasks.LOGGER_FILE).First(); // gets the first line from file.
            String lastLine = File.ReadLines(LogTasks.LOGGER_FILE).Last(); // gets the last line from file.
            Assert.AreEqual(firstLine, FIRST_AND_LAST_LINE_TEST_STRING);
            Assert.AreEqual(lastLine, FIRST_AND_LAST_LINE_TEST_STRING);
            Config.UUT.SerialNumber = "StartTestTextEnabledGroupRequired";
            RichTextBox rtf= new RichTextBox();
            LogTasks.Start(Config, ref rtf);
            Log.Information($"Hello {Environment.UserName}!");
            Log.CloseAndFlush();
            Assert.IsTrue(File.Exists(LogTasks.LOGGER_FILE));
            Config.UUT.EventCode = EventCodes.PASS;
            // If not set, is EventCodes.UNSET, which isn't allowed in class LogTasks.

            String fileName = $"{Config.UUT.Number}_{Config.UUT.SerialNumber}_{Config.Group.ID}";
            Console.WriteLine($"FileName                : '{fileName}'");
            String[] files = Directory.GetFiles(Config.Logger.FilePath, $"{fileName}_*.txt", SearchOption.TopDirectoryOnly);
            // files is the set of all files in Config.Logger.FilePath like Config.UUT.Number_Config.UUT.SerialNumber_Config.Group.ID_*.txt.
            Int32 maxNumber = 0; String s;
            Console.WriteLine($"Files.Count()           : '{files.Count()}'");
            foreach (String f in files) {
                s = f;
                Console.WriteLine($"Initial             : '{s}'");
                s = s.Replace($"{Config.Logger.FilePath}{fileName}", String.Empty);
                Console.WriteLine($"FilePath + fileName : '{s}'");
                s = s.Replace(".txt", String.Empty);
                Console.WriteLine($".txt                : '{s}'");
                s = s.Replace("_", String.Empty);
                Console.WriteLine($"_                   : '{s}'");
                foreach (FieldInfo fi in typeof(EventCodes).GetFields()) s = s.Replace((String)fi.GetValue(null), String.Empty);
                Console.WriteLine($"foreach...          : '{s}'");
                if (Int32.Parse(s) > maxNumber) maxNumber = Int32.Parse(s);
                Console.WriteLine($"maxNumber           : '{maxNumber}'\n");
                // Sample Console.WriteLine() output for final (3rd) iteration of foreach:
                //   FileName            : 'UUT_Number_ID1_Testing'
                //   Files.Count()       : '3'
                //   Initial             : 'P:\Test\TDR\D4522137\Functional\UUT_Number_ID1_Testing_3_P.txt'
                //   FilePath + fileName : '_3_P.txt'
                //   .txt                : '_3_P'
                //   _                   : '3P'
                //   .TrimEnd(AFP)       : '3'
                //   maxNumber           : '3'
            }
            fileName += $"_{++maxNumber}_{Config.UUT.EventCode}.txt";

            Assert.IsFalse(File.Exists($"{Config.Logger.FilePath}{fileName}"));
            File.Move(LogTasks.LOGGER_FILE, $"{Config.Logger.FilePath}{fileName}");
            Assert.IsFalse(File.Exists(LogTasks.LOGGER_FILE));
            Assert.IsTrue(File.Exists($"{Config.Logger.FilePath}{fileName}"));
            lastLine = File.ReadLines($"{Config.Logger.FilePath}{fileName}").Last(); // gets the last line from a file.
            Assert.AreNotEqual(lastLine, FIRST_AND_LAST_LINE_TEST_STRING);
            Assert.IsTrue(lastLine.Contains($"Hello {Environment.UserName}!"));
        }

        [TestMethod()]
        public void StopTestTextEnabledGroupRequired() {
            DialogResult dr = MessageBox.Show($"Select Group ID1", $"Select Group ID1", MessageBoxButtons.OKCancel);
            if (dr == DialogResult.Cancel) Assert.Inconclusive();
            Config Config = Config.Get();
            Config.UUT.SerialNumber = "StopTestTextEnabledGroupRequired";
            Assert.IsTrue(Config.Group.Required);
            Assert.IsTrue(Config.Logger.FileEnabled);
            Assert.IsFalse(Config.Logger.SQLEnabled);
            // Cannot currently test SQLEnabled case, as unimplemented.

            if (File.Exists(LogTasks.LOGGER_FILE)) File.Delete(LogTasks.LOGGER_FILE);
            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Information()
                .WriteTo.File(LogTasks.LOGGER_FILE, outputTemplate: LogTasks.LOGGER_TEMPLATE, fileSizeLimitBytes: null, retainedFileCountLimit: null)
                .CreateLogger();
            Log.Information($"START                  : {DateTime.Now}");
            Log.Information($"UUT Customer           : {Config.UUT.Customer}");
            Log.Information($"UUT Test Specification : {Config.UUT.TestSpecification}");
            Log.Information($"UUT Description        : {Config.UUT.Description}");
            Log.Information($"UUT Type               : {Config.UUT.Type}");
            Log.Information($"UUT Number             : {Config.UUT.Number}");
            Log.Information($"UUT Revision           : {Config.UUT.Revision}");
            Log.Information($"UUT Serial Number      : {Config.UUT.SerialNumber}");
            Log.Information($"UUT Group ID           : {Config.Group.ID}");
            Log.Information($"UUT Group Summary      : {Config.Group.Summary}");
            Log.Information($"UUT Group Detail       \n{Config.Group.Detail}");
            Log.Information($"Environment.UserName   : {Environment.UserName}\n");
            Log.Information($"Hello {Environment.UserName}!");
            Assert.IsTrue(File.Exists(LogTasks.LOGGER_FILE));
            Config.UUT.EventCode = EventCodes.PASS;
            // If not set, is EventCodes.UNSET, which isn't allowed in class LogTasks.

            LogTasks.Stop(Config);
            Assert.IsFalse(File.Exists(LogTasks.LOGGER_FILE));
            var directory = new DirectoryInfo(Config.Logger.FilePath);
            var newestFile = directory.GetFiles().OrderByDescending(f => f.LastWriteTime).First();
            Assert.IsTrue(File.Exists($"{Config.Logger.FilePath}{newestFile}"));
            String lastLine = File.ReadLines($"{Config.Logger.FilePath}{newestFile}").Last(); // gets the last line from a file.
            Assert.IsTrue(lastLine.Contains("STOP"));
        }
    }
}