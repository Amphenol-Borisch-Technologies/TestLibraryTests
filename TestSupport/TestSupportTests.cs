using System;
using System.Collections.Generic;
using System.Windows.Forms;
using ABTTestLibrary.AppConfig;
using ABTTestLibrary.TestSupport;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Reflection;
using static System.Net.Mime.MediaTypeNames;

namespace ABTTestLibraryTests.TestSupport {
    [TestClass()]
    public class TestSupportTests {
        [TestMethod()]
        public void EventCodesTest() {
            String ec = String.Empty;
            foreach (FieldInfo fi in typeof(EventCodes).GetFields()) ec += fi.GetValue(null) + "_";
            Console.WriteLine($"EventCodes       : '{ec}'{Environment.NewLine}");
            foreach (FieldInfo fi in typeof(EventCodes).GetFields()) {
                Console.WriteLine($"Remove EventCode : '{fi.GetValue(null)}'");
                ec = ec.Replace((String)fi.GetValue(null), String.Empty);
                Console.WriteLine($"EventCodes       : '{ec}'");
            }
            ec = ec.Replace("_", String.Empty);
            Assert.AreEqual(ec, String.Empty);
        }

        [TestMethod()]
        public void EvaluateTestResultTest() {
            String eventCode = String.Empty;
            Dictionary<String, Test> tests = Test.Get();
            Assert.AreEqual(tests.Count, 10);
            foreach (KeyValuePair<String, Test> t in tests) Assert.AreEqual(t.Value.Result, EventCodes.UNSET, false);
            String ID;
            Exception e;

            //   - LimitLow = LimitHigh = String.Empty.
            ID = "ID0";
            Assert.AreEqual(tests[ID].LimitLow, String.Empty, false);
            Assert.AreEqual(tests[ID].LimitHigh, String.Empty, false);
            e = Assert.ThrowsException<Exception>(() => TestTasks.EvaluateTestResult(tests[ID], out eventCode));
            Assert.AreEqual(e.Message, $"Invalid limits; App.config TestElement ID '{ID}' has LimitLow = String.Empty && LimitHigh = String.Empty");
            Console.WriteLine(e.Message);
            Assert.AreEqual(eventCode, EventCodes.ERROR, false);

            //   - LimitLow = String.Empty,	LimitHigh ≠ String.Empty, but won't parse to Double
            ID = "ID1";
            Assert.AreEqual(tests[ID].LimitLow, String.Empty, false);
            Assert.AreNotEqual(tests[ID].LimitHigh, String.Empty, false);
            Assert.IsFalse(Double.TryParse(tests[ID].LimitHigh, out _));
            e = Assert.ThrowsException<Exception>(() => TestTasks.EvaluateTestResult(tests[ID], out eventCode));
            Assert.AreEqual(e.Message, ($"Invalid limits; App.config TestElement ID '{ID}' has LimitLow = String.Empty && LimitHigh ≠ String.Empty && LimitHigh ≠ System.Double"));
            Console.WriteLine(e.Message);
            Assert.AreEqual(eventCode, EventCodes.ERROR, false);

            //   - LimitHigh = String.Empty, LimitLow  ≠ String.Empty, but won't parse to Double.
            ID = "ID2";
            Assert.AreEqual(tests[ID].LimitHigh, String.Empty, false);
            Assert.AreNotEqual(tests[ID].LimitLow, String.Empty, false);
            Assert.IsFalse(Double.TryParse(tests[ID].LimitLow, out _));
            e = Assert.ThrowsException<Exception>(() => TestTasks.EvaluateTestResult(tests[ID], out eventCode));
            Assert.AreEqual(e.Message, $"Invalid limits; App.config TestElement ID '{ID}' has LimitHigh = String.Empty && LimitLow ≠ String.Empty && LimitLow ≠ System.Double");
            Console.WriteLine(e.Message);
            Assert.AreEqual(eventCode, EventCodes.ERROR, false);

            //   - LimitLow ≠ String.Empty,	LimitHigh ≠ String.Empty, neither parse to Double, & LimitLow ≠ LimitHigh.
            ID = "ID3";
            Assert.AreNotEqual(tests[ID].LimitLow, String.Empty, false);
            Assert.AreNotEqual(tests[ID].LimitHigh, String.Empty, false);
            Assert.IsFalse(Double.TryParse(tests[ID].LimitLow, out _));
            Assert.IsFalse(Double.TryParse(tests[ID].LimitHigh, out _));
            Assert.AreNotEqual(tests[ID].LimitLow, tests[ID].LimitHigh, false);
            e = Assert.ThrowsException<Exception>(() => TestTasks.EvaluateTestResult(tests[ID], out eventCode));
            Assert.AreEqual(e.Message, $"Invalid limits; App.config TestElement ID '{ID}' has LimitLow ≠ LimitHigh && LimitLow ≠ String.Empty && LimitHigh ≠ String.Empty && LimitLow ≠ System.Double && LimitHigh ≠ System.Double");
            Console.WriteLine(e.Message);
            Assert.AreEqual(eventCode, EventCodes.ERROR, false);

            //   - LimitLow & LimitHigh both parse to Doubles; both low & high limits.
            ID = "ID4";
            Assert.AreEqual(tests[ID].LimitLow, "0", false);
            Assert.AreEqual(tests[ID].LimitHigh, "1", false);
            Assert.IsTrue(Double.TryParse(tests[ID].LimitLow, out _));
            Assert.IsTrue(Double.TryParse(tests[ID].LimitHigh, out _));
            tests[ID].Measurement = "-0.5";
            TestTasks.EvaluateTestResult(tests[ID], out eventCode);
            Assert.AreEqual(eventCode, EventCodes.FAIL);
            tests[ID].Measurement = tests[ID].LimitLow;
            TestTasks.EvaluateTestResult(tests[ID], out eventCode);
            Assert.AreEqual(eventCode, EventCodes.PASS);
            tests[ID].Measurement = "0.5";
            TestTasks.EvaluateTestResult(tests[ID], out eventCode);
            Assert.AreEqual(eventCode, EventCodes.PASS);
            tests[ID].Measurement = tests[ID].LimitHigh;
            TestTasks.EvaluateTestResult(tests[ID], out eventCode);
            Assert.AreEqual(eventCode, EventCodes.PASS);
            tests[ID].Measurement = "1.5";
            TestTasks.EvaluateTestResult(tests[ID], out eventCode);
            Assert.AreEqual(eventCode, EventCodes.FAIL);
            tests[ID].Measurement = "Measurement";
            e = Assert.ThrowsException<Exception>(() => TestTasks.EvaluateTestResult(tests[ID], out eventCode));
            Assert.AreEqual(e.Message, $"Invalid measurement; App.config TestElement ID '{ID}' Measurement '{tests[ID].Measurement}' ≠ System.Double");
            Console.WriteLine(e.Message);
            Assert.AreEqual(eventCode, EventCodes.ERROR, false);

            //   - LimitLow is allowed to be > LimitHigh if both parse to Double.
            //     This simply excludes a range of measurements from passing, rather than including a range from passing.
            ID = "ID5";
            Assert.AreEqual(tests[ID].LimitLow, "1", false);
            Assert.AreEqual(tests[ID].LimitHigh, "0", false);
            Assert.IsTrue(Double.TryParse(tests[ID].LimitLow, out _));
            Assert.IsTrue(Double.TryParse(tests[ID].LimitHigh, out _));
            tests[ID].Measurement = "-0.5";
            TestTasks.EvaluateTestResult(tests[ID], out eventCode);
            Assert.AreEqual(eventCode, EventCodes.PASS);
            tests[ID].Measurement = tests[ID].LimitLow;
            TestTasks.EvaluateTestResult(tests[ID], out eventCode);
            Assert.AreEqual(eventCode, EventCodes.PASS);
            tests[ID].Measurement = "0.5";
            TestTasks.EvaluateTestResult(tests[ID], out eventCode);
            Assert.AreEqual(eventCode, EventCodes.FAIL);
            tests[ID].Measurement = tests[ID].LimitHigh;
            TestTasks.EvaluateTestResult(tests[ID], out eventCode);
            Assert.AreEqual(eventCode, EventCodes.PASS);
            tests[ID].Measurement = "1.5";
            TestTasks.EvaluateTestResult(tests[ID], out eventCode);
            Assert.AreEqual(eventCode, EventCodes.PASS);

            //   - LimitLow is allowed to be = LimitHigh if both parse to Double.
            ID = "ID6";
            Assert.AreEqual(tests[ID].LimitLow, "1", false);
            Assert.AreEqual(tests[ID].LimitHigh, "1", false);
            Assert.IsTrue(Double.TryParse(tests[ID].LimitLow, out _));
            Assert.IsTrue(Double.TryParse(tests[ID].LimitHigh, out _));
            tests[ID].Measurement = "0.5";
            TestTasks.EvaluateTestResult(tests[ID], out eventCode);
            Assert.AreEqual(eventCode, EventCodes.FAIL);
            tests[ID].Measurement = tests[ID].LimitLow;
            TestTasks.EvaluateTestResult(tests[ID], out eventCode);
            Assert.AreEqual(eventCode, EventCodes.PASS);

            //   - LimitLow parses to Double, LimitHigh = String.Empty; only low limit, no high.
            ID = "ID7";
            Assert.AreEqual(tests[ID].LimitLow, "0", false);
            Assert.AreEqual(tests[ID].LimitHigh, String.Empty, false);
            Assert.IsTrue(Double.TryParse(tests[ID].LimitLow, out _));
            Assert.IsFalse(Double.TryParse(tests[ID].LimitHigh, out _));
            tests[ID].Measurement = "-0.5";
            TestTasks.EvaluateTestResult(tests[ID], out eventCode);
            Assert.AreEqual(eventCode, EventCodes.FAIL);
            tests[ID].Measurement = tests[ID].LimitLow;
            TestTasks.EvaluateTestResult(tests[ID], out eventCode);
            Assert.AreEqual(eventCode, EventCodes.PASS);
            tests[ID].Measurement = "0.5";
            TestTasks.EvaluateTestResult(tests[ID], out eventCode);
            Assert.AreEqual(eventCode, EventCodes.PASS);
            tests[ID].Measurement = "Measurement";
            e = Assert.ThrowsException<Exception>(() => TestTasks.EvaluateTestResult(tests[ID], out eventCode));
            Assert.AreEqual(e.Message, $"Invalid measurement; App.config TestElement ID '{ID}' Measurement '{tests[ID].Measurement}' ≠ System.Double");
            Console.WriteLine(e.Message);
            Assert.AreEqual(eventCode, EventCodes.ERROR, false);

            //   - LimitLow = String.Empty, LimitHigh parses to Double; no low limit, only high.
            ID = "ID8";
            Assert.AreEqual(tests[ID].LimitLow, String.Empty, false);
            Assert.AreEqual(tests[ID].LimitHigh, "1", false);
            Assert.IsFalse(Double.TryParse(tests[ID].LimitLow, out _));
            Assert.IsTrue(Double.TryParse(tests[ID].LimitHigh, out _));
            tests[ID].Measurement = "0.5";
            TestTasks.EvaluateTestResult(tests[ID], out eventCode);
            Assert.AreEqual(eventCode, EventCodes.PASS);
            tests[ID].Measurement = tests[ID].LimitHigh;
            TestTasks.EvaluateTestResult(tests[ID], out eventCode);
            Assert.AreEqual(eventCode, EventCodes.PASS);
            tests[ID].Measurement = "1.5";
            TestTasks.EvaluateTestResult(tests[ID], out eventCode);
            Assert.AreEqual(eventCode, EventCodes.FAIL);
            tests[ID].Measurement = "Measurement";
            e = Assert.ThrowsException<Exception>(() => TestTasks.EvaluateTestResult(tests[ID], out eventCode));
            Assert.AreEqual(e.Message, $"Invalid measurement; App.config TestElement ID '{ID}' Measurement '{tests[ID].Measurement}' ≠ System.Double");
            Console.WriteLine(e.Message);
            Assert.AreEqual(eventCode, EventCodes.ERROR, false);

            //   - LimitLow = LimitHigh, both ≠ String.Empty, and neither parse to Double.
            //     This is to verify checksums or CRCs, or to read String contents from memory, or from a file, etc.
            ID = "ID9";
            Assert.AreEqual(tests[ID].LimitLow, "Limit", false);
            Assert.AreEqual(tests[ID].LimitHigh, "Limit", false);
            Assert.IsFalse(Double.TryParse(tests[ID].LimitLow, out _));
            Assert.IsFalse(Double.TryParse(tests[ID].LimitHigh, out _));
            tests[ID].Measurement = tests[ID].LimitLow;
            TestTasks.EvaluateTestResult(tests[ID], out eventCode);
            Assert.AreEqual(eventCode, EventCodes.PASS);
            tests[ID].Measurement = "1";
            TestTasks.EvaluateTestResult(tests[ID], out eventCode);
            Assert.AreEqual(eventCode, EventCodes.FAIL);

            //   - LimitLow = LimitHigh = CUSTOM.
            //     For custom Tests.
            ID = "ID10";
            Assert.AreEqual(tests[ID].LimitLow, "CUSTOM", false);
            Assert.AreEqual(tests[ID].LimitHigh, "CUSTOM", false);
            Assert.IsFalse(Double.TryParse(tests[ID].LimitLow, out _));
            Assert.IsFalse(Double.TryParse(tests[ID].LimitHigh, out _));
            foreach (FieldInfo fi in typeof(EventCodes).GetFields()) {
                tests[ID].Measurement = (String)fi.GetValue(null);
                TestTasks.EvaluateTestResult(tests[ID], out eventCode);
                Assert.AreEqual(eventCode, fi.GetValue(null));
            }
            tests[ID].Measurement = "This Measurement should cause an Exception.";
            e = Assert.ThrowsException<Exception>(() => TestTasks.EvaluateTestResult(tests[ID], out eventCode));
            Assert.AreEqual(e.Message, $"Invalid CUSTOM measurement; App.config TestElement ID '{ID}' Measurement '{tests[ID].Measurement}' didn't return valid EventCode.");
            TestTasks.EvaluateTestResult(tests[ID], out eventCode);
            Console.WriteLine(e.Message);
            Assert.AreEqual(eventCode, EventCodes.ERROR);
        }

        [TestMethod()]
        public void EvaluateUUTResultTest() {
            DialogResult dr = MessageBox.Show($"Select Group ID0", $"Select Group ID0", MessageBoxButtons.OKCancel);
            if (dr == DialogResult.Cancel) Assert.Inconclusive();
            Config config = Config.Get();
            foreach (KeyValuePair<String, Test> t in config.Tests) t.Value.Result = EventCodes.PASS;
            Assert.IsFalse(config.Group.Required);
            Assert.AreEqual(TestTasks.EvaluateUUTResult(config), EventCodes.UNSET);

            dr = MessageBox.Show($"Select Group ID1", $"Select Group ID1", MessageBoxButtons.OKCancel);
            if (dr == DialogResult.Cancel) Assert.Inconclusive();
            config = Config.Get();
            Assert.IsTrue(config.Group.Required);
            foreach (KeyValuePair<String, Test> t in config.Tests) Assert.AreEqual(t.Value.Result, EventCodes.UNSET);
          
            foreach (KeyValuePair<String, Test> t in config.Tests) t.Value.Result = EventCodes.ERROR;
            Assert.AreEqual(TestTasks.EvaluateUUTResult(config), EventCodes.ERROR);
            foreach (KeyValuePair<String, Test> t in config.Tests) {
                // If any result is ERROR, always evaluate to ERROR.
                t.Value.Result = EventCodes.UNSET;
                Assert.AreEqual(TestTasks.EvaluateUUTResult(config), EventCodes.ERROR);
                t.Value.Result = EventCodes.ABORT;
                Assert.AreEqual(TestTasks.EvaluateUUTResult(config), EventCodes.ERROR);
                t.Value.Result = EventCodes.FAIL;
                Assert.AreEqual(TestTasks.EvaluateUUTResult(config), EventCodes.ERROR);
                t.Value.Result = EventCodes.PASS;
                Assert.AreEqual(TestTasks.EvaluateUUTResult(config), EventCodes.ERROR);
                t.Value.Result = EventCodes.ERROR;
            }

            foreach (KeyValuePair<String, Test> t in config.Tests) t.Value.Result = EventCodes.UNSET;
            Assert.AreEqual(TestTasks.EvaluateUUTResult(config), EventCodes.ERROR);
            foreach (KeyValuePair<String, Test> t in config.Tests) {
                // In the absence of ERROR, any result = UNSET evaluates to ERROR.
                t.Value.Result = EventCodes.FAIL;
                Assert.AreEqual(TestTasks.EvaluateUUTResult(config), EventCodes.ERROR);
                t.Value.Result = EventCodes.PASS;
                Assert.AreEqual(TestTasks.EvaluateUUTResult(config), EventCodes.ERROR);
                t.Value.Result = EventCodes.ABORT;
                Assert.AreEqual(TestTasks.EvaluateUUTResult(config), EventCodes.ABORT);
                t.Value.Result = EventCodes.UNSET;
            }

            foreach (KeyValuePair<String, Test> t in config.Tests) t.Value.Result = EventCodes.ABORT;
            Assert.AreEqual(TestTasks.EvaluateUUTResult(config), EventCodes.ABORT);
            foreach (KeyValuePair<String, Test> t in config.Tests) {
                // In the absence of ERROR & UNSET, any result = ABORT evaluates to ABORT.
                t.Value.Result = EventCodes.FAIL;
                Assert.AreEqual(TestTasks.EvaluateUUTResult(config), EventCodes.ABORT);
                t.Value.Result = EventCodes.PASS;
                Assert.AreEqual(TestTasks.EvaluateUUTResult(config), EventCodes.ABORT);
                t.Value.Result = EventCodes.ABORT;
            }

            foreach (KeyValuePair<String, Test> t in config.Tests) t.Value.Result = EventCodes.FAIL;
            Assert.AreEqual(TestTasks.EvaluateUUTResult(config), EventCodes.FAIL);
            foreach (KeyValuePair<String, Test> t in config.Tests) {
                // In the absence of ERROR, UNSET and ABORT, any result = FAIL evaluates to FAIL.
                t.Value.Result = EventCodes.PASS;
                Assert.AreEqual(TestTasks.EvaluateUUTResult(config), EventCodes.FAIL);
                t.Value.Result = EventCodes.FAIL;
            }

            foreach (KeyValuePair<String, Test> t in config.Tests) t.Value.Result = EventCodes.PASS;
            Assert.AreEqual(TestTasks.EvaluateUUTResult(config), EventCodes.PASS);
            // All Test.Result values must be EventCode.PASS for EvaluateUUTResult() to return EventCode.PASS.
            foreach (KeyValuePair<String, Test> t in config.Tests) {
                t.Value.Result = EventCodes.ERROR;
                Assert.AreEqual(TestTasks.EvaluateUUTResult(config), EventCodes.ERROR);
                t.Value.Result = EventCodes.UNSET;
                Assert.AreEqual(TestTasks.EvaluateUUTResult(config), EventCodes.ERROR);
                t.Value.Result = EventCodes.ABORT;
                Assert.AreEqual(TestTasks.EvaluateUUTResult(config), EventCodes.ABORT);
                t.Value.Result = EventCodes.FAIL;
                Assert.AreEqual(TestTasks.EvaluateUUTResult(config), EventCodes.FAIL);
                t.Value.Result = EventCodes.PASS;
            }

            foreach (KeyValuePair<String, Test> t in config.Tests) t.Value.Result = EventCodes.PASS;
            foreach (KeyValuePair<String, Test> t in config.Tests) {
                t.Value.Result = "This result should throw an Exception";
                Assert.ThrowsException<Exception>(() => TestTasks.EvaluateUUTResult(config));
                t.Value.Result = EventCodes.PASS;
                Assert.AreEqual(TestTasks.EvaluateUUTResult(config), EventCodes.PASS);
            }
        }
    }
}
