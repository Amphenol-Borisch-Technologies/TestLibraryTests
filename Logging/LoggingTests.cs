using System.IO;
using System;
using System.Linq;
using Serilog;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ABTTestLibrary.Config;
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
            ConfigLib configLib = ConfigLib.Get();
            ConfigTest configTest = ConfigTest.Get();
            Assert.IsTrue(configTest.Group.Required);
            Assert.IsTrue(configLib.Logger.FileEnabled);
            Assert.IsFalse(configLib.Logger.SQLEnabled);
            // Cannot currently test SQLEnabled case, as unimplemented.
            const String FIRST_AND_LAST_LINE_TEST_STRING = "This file & contents should be overwritten during LogTasks.Start().Confirm at end of test.";
            File.WriteAllText(LogTasks.LOGGER_FILE, FIRST_AND_LAST_LINE_TEST_STRING);
            String firstLine = File.ReadLines(LogTasks.LOGGER_FILE).First(); // gets the first line from file.
            String lastLine = File.ReadLines(LogTasks.LOGGER_FILE).Last(); // gets the last line from file.
            Assert.AreEqual(firstLine, FIRST_AND_LAST_LINE_TEST_STRING);
            Assert.AreEqual(lastLine, FIRST_AND_LAST_LINE_TEST_STRING);
            configLib.UUT.SerialNumber = "StartTestTextEnabledGroupRequired";
            RichTextBox rtf= new RichTextBox();
            LogTasks.Start(configLib, "ClientAssemblyName", "ClientAssemblyVersion", configTest.Group, ref rtf);
            Log.Information($"Hello {Environment.UserName}!");
            Log.CloseAndFlush();
            Assert.IsTrue(File.Exists(LogTasks.LOGGER_FILE));
            configLib.UUT.EventCode = EventCodes.PASS;
            // If not set, is EventCodes.UNSET, which isn't allowed in class LogTasks.

            String fileName = $"{configLib.UUT.Number}_{configLib.UUT.SerialNumber}_{configTest.Group.ID}";
            Console.WriteLine($"FileName                : '{fileName}'");
            String[] files = Directory.GetFiles(configLib.Logger.FilePath, $"{fileName}_*.txt", SearchOption.TopDirectoryOnly);
            // files is the set of all files in configLib.Logger.FilePath like configLib.UUT.Number_configLib.UUT.SerialNumber_configTest.Group.ID_*.txt.
            Int32 maxNumber = 0; String s;
            Console.WriteLine($"Files.Count()           : '{files.Count()}'");
            foreach (String f in files) {
                s = f;
                Console.WriteLine($"Initial             : '{s}'");
                s = s.Replace($"{configLib.Logger.FilePath}{fileName}", String.Empty);
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
            fileName += $"_{++maxNumber}_{configLib.UUT.EventCode}.txt";

            Assert.IsFalse(File.Exists($"{configLib.Logger.FilePath}{fileName}"));
            File.Move(LogTasks.LOGGER_FILE, $"{configLib.Logger.FilePath}{fileName}");
            Assert.IsFalse(File.Exists(LogTasks.LOGGER_FILE));
            Assert.IsTrue(File.Exists($"{configLib.Logger.FilePath}{fileName}"));
            lastLine = File.ReadLines($"{configLib.Logger.FilePath}{fileName}").Last(); // gets the last line from a file.
            Assert.AreNotEqual(lastLine, FIRST_AND_LAST_LINE_TEST_STRING);
            Assert.IsTrue(lastLine.Contains($"Hello {Environment.UserName}!"));
        }

        [TestMethod()]
        public void StopTestTextEnabledGroupRequired() {
            DialogResult dr = MessageBox.Show($"Select Group ID1", $"Select Group ID1", MessageBoxButtons.OKCancel);
            if (dr == DialogResult.Cancel) Assert.Inconclusive();
            ConfigLib configLib = ConfigLib.Get();
            ConfigTest configTest = ConfigTest.Get();
            configLib.UUT.SerialNumber = "StopTestTextEnabledGroupRequired";
            Assert.IsTrue(configTest.Group.Required);
            Assert.IsTrue(configLib.Logger.FileEnabled);
            Assert.IsFalse(configLib.Logger.SQLEnabled);
            // Cannot currently test SQLEnabled case, as unimplemented.

            if (File.Exists(LogTasks.LOGGER_FILE)) File.Delete(LogTasks.LOGGER_FILE);
            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Information()
                .WriteTo.File(LogTasks.LOGGER_FILE, outputTemplate: LogTasks.LOGGER_TEMPLATE, fileSizeLimitBytes: null, retainedFileCountLimit: null)
                .CreateLogger();
            Log.Information($"START                  : {DateTime.Now}");
            Log.Information($"UUT Customer           : {configLib.UUT.Customer}");
            Log.Information($"UUT Test Specification : {configLib.UUT.TestSpecification}");
            Log.Information($"UUT Description        : {configLib.UUT.Description}");
            Log.Information($"UUT Type               : {configLib.UUT.Type}");
            Log.Information($"UUT Number             : {configLib.UUT.Number}");
            Log.Information($"UUT Revision           : {configLib.UUT.Revision}");
            Log.Information($"UUT Serial Number      : {configLib.UUT.SerialNumber}");
            Log.Information($"UUT Group ID           : {configTest.Group.ID}");
            Log.Information($"UUT Group Summary      : {configTest.Group.Summary}");
            Log.Information($"UUT Group Detail       \n{configTest.Group.Detail}");
            Log.Information($"Environment.UserName   : {Environment.UserName}\n");
            Log.Information($"Hello {Environment.UserName}!");
            Assert.IsTrue(File.Exists(LogTasks.LOGGER_FILE));
            configLib.UUT.EventCode = EventCodes.PASS;
            // If not set, is EventCodes.UNSET, which isn't allowed in class LogTasks.

            LogTasks.Stop(configLib, configTest.Group);
            Assert.IsFalse(File.Exists(LogTasks.LOGGER_FILE));
            var directory = new DirectoryInfo(configLib.Logger.FilePath);
            var newestFile = directory.GetFiles().OrderByDescending(f => f.LastWriteTime).First();
            Assert.IsTrue(File.Exists($"{configLib.Logger.FilePath}{newestFile}"));
            String lastLine = File.ReadLines($"{configLib.Logger.FilePath}{newestFile}").Last(); // gets the last line from a file.
            Assert.IsTrue(lastLine.Contains("STOP"));
        }
    }
}