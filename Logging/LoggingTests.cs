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
            Config config = Config.Get();
            Assert.IsTrue(config.Group.Required);
            Assert.IsTrue(config.Logger.FileEnabled);
            Assert.IsFalse(config.Logger.SQLEnabled);
            // Cannot currently test SQLEnabled case, as unimplemented.
            const String FIRST_AND_LAST_LINE_TEST_STRING = "This file & contents should be overwritten during LogTasks.Start().Confirm at end of test.";
            File.WriteAllText(LogTasks.LOGGER_FILE, FIRST_AND_LAST_LINE_TEST_STRING);
            String firstLine = File.ReadLines(LogTasks.LOGGER_FILE).First(); // gets the first line from file.
            String lastLine = File.ReadLines(LogTasks.LOGGER_FILE).Last(); // gets the last line from file.
            Assert.AreEqual(firstLine, FIRST_AND_LAST_LINE_TEST_STRING);
            Assert.AreEqual(lastLine, FIRST_AND_LAST_LINE_TEST_STRING);
            config.UUT.SerialNumber = "StartTestTextEnabledGroupRequired";
            RichTextBox rtf= new RichTextBox();
            LogTasks.Start(config, ref rtf);
            Log.Information($"Hello {Environment.UserName}!");
            Log.CloseAndFlush();
            Assert.IsTrue(File.Exists(LogTasks.LOGGER_FILE));
            config.UUT.EventCode = EventCodes.PASS;
            // If not set, is EventCodes.UNSET, which isn't allowed in class LogTasks.

            String fileName = $"{config.UUT.Number}_{config.UUT.SerialNumber}_{config.Group.ID}";
            Console.WriteLine($"FileName                : '{fileName}'");
            String[] files = Directory.GetFiles(config.Logger.FilePath, $"{fileName}_*.txt", SearchOption.TopDirectoryOnly);
            // files is the set of all files in config.Logger.FilePath like config.UUT.Number_Config.UUT.SerialNumber_Config.Group.ID_*.txt.
            Int32 maxNumber = 0; String s;
            Console.WriteLine($"Files.Count()           : '{files.Count()}'");
            foreach (String f in files) {
                s = f;
                Console.WriteLine($"Initial             : '{s}'");
                s = s.Replace($"{config.Logger.FilePath}{fileName}", String.Empty);
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
            fileName += $"_{++maxNumber}_{config.UUT.EventCode}.txt";

            Assert.IsFalse(File.Exists($"{config.Logger.FilePath}{fileName}"));
            File.Move(LogTasks.LOGGER_FILE, $"{config.Logger.FilePath}{fileName}");
            Assert.IsFalse(File.Exists(LogTasks.LOGGER_FILE));
            Assert.IsTrue(File.Exists($"{config.Logger.FilePath}{fileName}"));
            lastLine = File.ReadLines($"{config.Logger.FilePath}{fileName}").Last(); // gets the last line from a file.
            Assert.AreNotEqual(lastLine, FIRST_AND_LAST_LINE_TEST_STRING);
            Assert.IsTrue(lastLine.Contains($"Hello {Environment.UserName}!"));
        }

        [TestMethod()]
        public void StopTestTextEnabledGroupRequired() {
            DialogResult dr = MessageBox.Show($"Select Group ID1", $"Select Group ID1", MessageBoxButtons.OKCancel);
            if (dr == DialogResult.Cancel) Assert.Inconclusive();
            Config config = Config.Get();
            config.UUT.SerialNumber = "StopTestTextEnabledGroupRequired";
            Assert.IsTrue(config.Group.Required);
            Assert.IsTrue(config.Logger.FileEnabled);
            Assert.IsFalse(config.Logger.SQLEnabled);
            // Cannot currently test SQLEnabled case, as unimplemented.

            if (File.Exists(LogTasks.LOGGER_FILE)) File.Delete(LogTasks.LOGGER_FILE);
            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Information()
                .WriteTo.File(LogTasks.LOGGER_FILE, outputTemplate: LogTasks.LOGGER_TEMPLATE, fileSizeLimitBytes: null, retainedFileCountLimit: null)
                .CreateLogger();
            Log.Information($"START                  : {DateTime.Now}");
            Log.Information($"UUT Customer           : {config.UUT.Customer}");
            Log.Information($"UUT Test Specification : {config.UUT.TestSpecification}");
            Log.Information($"UUT Description        : {config.UUT.Description}");
            Log.Information($"UUT Type               : {config.UUT.Type}");
            Log.Information($"UUT Number             : {config.UUT.Number}");
            Log.Information($"UUT Revision           : {config.UUT.Revision}");
            Log.Information($"UUT Serial Number      : {config.UUT.SerialNumber}");
            Log.Information($"UUT Group ID           : {config.Group.ID}");
            Log.Information($"UUT Group Summary      : {config.Group.Summary}");
            Log.Information($"UUT Group Detail       \n{config.Group.Detail}");
            Log.Information($"Environment.UserName   : {Environment.UserName}\n");
            Log.Information($"Hello {Environment.UserName}!");
            Assert.IsTrue(File.Exists(LogTasks.LOGGER_FILE));
            config.UUT.EventCode = EventCodes.PASS;
            // If not set, is EventCodes.UNSET, which isn't allowed in class LogTasks.

            LogTasks.Stop(config);
            Assert.IsFalse(File.Exists(LogTasks.LOGGER_FILE));
            var directory = new DirectoryInfo(config.Logger.FilePath);
            var newestFile = directory.GetFiles().OrderByDescending(f => f.LastWriteTime).First();
            Assert.IsTrue(File.Exists($"{config.Logger.FilePath}{newestFile}"));
            String lastLine = File.ReadLines($"{config.Logger.FilePath}{newestFile}").Last(); // gets the last line from a file.
            Assert.IsTrue(lastLine.Contains("STOP"));
        }
    }
}