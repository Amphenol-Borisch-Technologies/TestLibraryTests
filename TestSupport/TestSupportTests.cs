using System;
using System.Collections.Generic;
using System.Windows.Forms;
using ABTTestLibrary.AppConfig;
using ABTTestLibrary.TestSupport;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Reflection;

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
            Dictionary<String, Test> Tests = Test.Get();
            Assert.AreEqual(Tests.Count, 10);
            foreach (KeyValuePair<String, Test> t in Tests) Assert.AreEqual(t.Value.Result, EventCodes.UNSET, false);
            String ID;
            Exception e;

            //   - LimitLow = LimitHigh = String.Empty.
            ID = "ID0";
            Assert.AreEqual(Tests[ID].LimitLow, String.Empty, false);
            Assert.AreEqual(Tests[ID].LimitHigh, String.Empty, false);
            e = Assert.ThrowsException<Exception>(() => TestTasks.EvaluateTestResult(Tests[ID], out eventCode));
            Assert.AreEqual(e.Message, $"Invalid limits; App.config TestElement ID '{ID}' has LimitLow = String.Empty && LimitHigh = String.Empty");
            Console.WriteLine(e.Message);
            Assert.AreEqual(eventCode, EventCodes.ERROR, false);

            //   - LimitLow = String.Empty,	LimitHigh ≠ String.Empty, but won't parse to Double
            ID = "ID1";
            Assert.AreEqual(Tests[ID].LimitLow, String.Empty, false);
            Assert.AreNotEqual(Tests[ID].LimitHigh, String.Empty, false);
            Assert.IsFalse(Double.TryParse(Tests[ID].LimitHigh, out _));
            e = Assert.ThrowsException<Exception>(() => TestTasks.EvaluateTestResult(Tests[ID], out eventCode));
            Assert.AreEqual(e.Message, ($"Invalid limits; App.config TestElement ID '{ID}' has LimitLow = String.Empty && LimitHigh ≠ String.Empty && LimitHigh ≠ System.Double"));
            Console.WriteLine(e.Message);
            Assert.AreEqual(eventCode, EventCodes.ERROR, false);

            //   - LimitHigh = String.Empty, LimitLow  ≠ String.Empty, but won't parse to Double.
            ID = "ID2";
            Assert.AreEqual(Tests[ID].LimitHigh, String.Empty, false);
            Assert.AreNotEqual(Tests[ID].LimitLow, String.Empty, false);
            Assert.IsFalse(Double.TryParse(Tests[ID].LimitLow, out _));
            e = Assert.ThrowsException<Exception>(() => TestTasks.EvaluateTestResult(Tests[ID], out eventCode));
            Assert.AreEqual(e.Message, $"Invalid limits; App.config TestElement ID '{ID}' has LimitHigh = String.Empty && LimitLow ≠ String.Empty && LimitLow ≠ System.Double");
            Console.WriteLine(e.Message);
            Assert.AreEqual(eventCode, EventCodes.ERROR, false);

            //   - LimitLow ≠ String.Empty,	LimitHigh ≠ String.Empty, neither parse to Double, & LimitLow ≠ LimitHigh.
            ID = "ID3";
            Assert.AreNotEqual(Tests[ID].LimitLow, String.Empty, false);
            Assert.AreNotEqual(Tests[ID].LimitHigh, String.Empty, false);
            Assert.IsFalse(Double.TryParse(Tests[ID].LimitLow, out _));
            Assert.IsFalse(Double.TryParse(Tests[ID].LimitHigh, out _));
            Assert.AreNotEqual(Tests[ID].LimitLow, Tests[ID].LimitHigh, false);
            e = Assert.ThrowsException<Exception>(() => TestTasks.EvaluateTestResult(Tests[ID], out eventCode));
            Assert.AreEqual(e.Message, $"Invalid limits; App.config TestElement ID '{ID}' has LimitLow ≠ LimitHigh && LimitLow ≠ String.Empty && LimitHigh ≠ String.Empty && LimitLow ≠ System.Double && LimitHigh ≠ System.Double");
            Console.WriteLine(e.Message);
            Assert.AreEqual(eventCode, EventCodes.ERROR, false);

            //   - LimitLow & LimitHigh both parse to Doubles; both low & high limits.
            ID = "ID4";
            Assert.AreEqual(Tests[ID].LimitLow, "0", false);
            Assert.AreEqual(Tests[ID].LimitHigh, "1", false);
            Assert.IsTrue(Double.TryParse(Tests[ID].LimitLow, out _));
            Assert.IsTrue(Double.TryParse(Tests[ID].LimitHigh, out _));
            Tests[ID].Measurement = "-0.5";
            TestTasks.EvaluateTestResult(Tests[ID], out eventCode);
            Assert.AreEqual(eventCode, EventCodes.FAIL);
            Tests[ID].Measurement = Tests[ID].LimitLow;
            TestTasks.EvaluateTestResult(Tests[ID], out eventCode);
            Assert.AreEqual(eventCode, EventCodes.PASS);
            Tests[ID].Measurement = "0.5";
            TestTasks.EvaluateTestResult(Tests[ID], out eventCode);
            Assert.AreEqual(eventCode, EventCodes.PASS);
            Tests[ID].Measurement = Tests[ID].LimitHigh;
            TestTasks.EvaluateTestResult(Tests[ID], out eventCode);
            Assert.AreEqual(eventCode, EventCodes.PASS);
            Tests[ID].Measurement = "1.5";
            TestTasks.EvaluateTestResult(Tests[ID], out eventCode);
            Assert.AreEqual(eventCode, EventCodes.FAIL);
            Tests[ID].Measurement = "Measurement";
            e = Assert.ThrowsException<Exception>(() => TestTasks.EvaluateTestResult(Tests[ID], out eventCode));
            Assert.AreEqual(e.Message, $"Invalid measurement; App.config TestElement ID '{ID}' Measurement '{Tests[ID].Measurement}' ≠ System.Double");
            Console.WriteLine(e.Message);
            Assert.AreEqual(eventCode, EventCodes.ERROR, false);

            //   - LimitLow is allowed to be > LimitHigh if both parse to Double.
            //     This simply excludes a range of measurements from passing, rather than including a range from passing.
            ID = "ID5";
            Assert.AreEqual(Tests[ID].LimitLow, "1", false);
            Assert.AreEqual(Tests[ID].LimitHigh, "0", false);
            Assert.IsTrue(Double.TryParse(Tests[ID].LimitLow, out _));
            Assert.IsTrue(Double.TryParse(Tests[ID].LimitHigh, out _));
            Tests[ID].Measurement = "-0.5";
            TestTasks.EvaluateTestResult(Tests[ID], out eventCode);
            Assert.AreEqual(eventCode, EventCodes.PASS);
            Tests[ID].Measurement = Tests[ID].LimitLow;
            TestTasks.EvaluateTestResult(Tests[ID], out eventCode);
            Assert.AreEqual(eventCode, EventCodes.PASS);
            Tests[ID].Measurement = "0.5";
            TestTasks.EvaluateTestResult(Tests[ID], out eventCode);
            Assert.AreEqual(eventCode, EventCodes.FAIL);
            Tests[ID].Measurement = Tests[ID].LimitHigh;
            TestTasks.EvaluateTestResult(Tests[ID], out eventCode);
            Assert.AreEqual(eventCode, EventCodes.PASS);
            Tests[ID].Measurement = "1.5";
            TestTasks.EvaluateTestResult(Tests[ID], out eventCode);
            Assert.AreEqual(eventCode, EventCodes.PASS);

            //   - LimitLow is allowed to be = LimitHigh if both parse to Double.
            ID = "ID6";
            Assert.AreEqual(Tests[ID].LimitLow, "1", false);
            Assert.AreEqual(Tests[ID].LimitHigh, "1", false);
            Assert.IsTrue(Double.TryParse(Tests[ID].LimitLow, out _));
            Assert.IsTrue(Double.TryParse(Tests[ID].LimitHigh, out _));
            Tests[ID].Measurement = "0.5";
            TestTasks.EvaluateTestResult(Tests[ID], out eventCode);
            Assert.AreEqual(eventCode, EventCodes.FAIL);
            Tests[ID].Measurement = Tests[ID].LimitLow;
            TestTasks.EvaluateTestResult(Tests[ID], out eventCode);
            Assert.AreEqual(eventCode, EventCodes.PASS);

            //   - LimitLow parses to Double, LimitHigh = String.Empty; only low limit, no high.
            ID = "ID7";
            Assert.AreEqual(Tests[ID].LimitLow, "0", false);
            Assert.AreEqual(Tests[ID].LimitHigh, String.Empty, false);
            Assert.IsTrue(Double.TryParse(Tests[ID].LimitLow, out _));
            Assert.IsFalse(Double.TryParse(Tests[ID].LimitHigh, out _));
            Tests[ID].Measurement = "-0.5";
            TestTasks.EvaluateTestResult(Tests[ID], out eventCode);
            Assert.AreEqual(eventCode, EventCodes.FAIL);
            Tests[ID].Measurement = Tests[ID].LimitLow;
            TestTasks.EvaluateTestResult(Tests[ID], out eventCode);
            Assert.AreEqual(eventCode, EventCodes.PASS);
            Tests[ID].Measurement = "0.5";
            TestTasks.EvaluateTestResult(Tests[ID], out eventCode);
            Assert.AreEqual(eventCode, EventCodes.PASS);
            Tests[ID].Measurement = "Measurement";
            e = Assert.ThrowsException<Exception>(() => TestTasks.EvaluateTestResult(Tests[ID], out eventCode));
            Assert.AreEqual(e.Message, $"Invalid measurement; App.config TestElement ID '{ID}' Measurement '{Tests[ID].Measurement}' ≠ System.Double");
            Console.WriteLine(e.Message);
            Assert.AreEqual(eventCode, EventCodes.ERROR, false);

            //   - LimitLow = String.Empty, LimitHigh parses to Double; no low limit, only high.
            ID = "ID8";
            Assert.AreEqual(Tests[ID].LimitLow, String.Empty, false);
            Assert.AreEqual(Tests[ID].LimitHigh, "1", false);
            Assert.IsFalse(Double.TryParse(Tests[ID].LimitLow, out _));
            Assert.IsTrue(Double.TryParse(Tests[ID].LimitHigh, out _));
            Tests[ID].Measurement = "0.5";
            TestTasks.EvaluateTestResult(Tests[ID], out eventCode);
            Assert.AreEqual(eventCode, EventCodes.PASS);
            Tests[ID].Measurement = Tests[ID].LimitHigh;
            TestTasks.EvaluateTestResult(Tests[ID], out eventCode);
            Assert.AreEqual(eventCode, EventCodes.PASS);
            Tests[ID].Measurement = "1.5";
            TestTasks.EvaluateTestResult(Tests[ID], out eventCode);
            Assert.AreEqual(eventCode, EventCodes.FAIL);
            Tests[ID].Measurement = "Measurement";
            e = Assert.ThrowsException<Exception>(() => TestTasks.EvaluateTestResult(Tests[ID], out eventCode));
            Assert.AreEqual(e.Message, $"Invalid measurement; App.config TestElement ID '{ID}' Measurement '{Tests[ID].Measurement}' ≠ System.Double");
            Console.WriteLine(e.Message);
            Assert.AreEqual(eventCode, EventCodes.ERROR, false);

            //   - LimitLow = LimitHigh, both ≠ String.Empty, and neither parse to Double.
            //     This is to verify checksums or CRCs, or to read String contents from memory, or from a file, etc.
            ID = "ID9";
            Assert.AreEqual(Tests[ID].LimitLow, "Limit", false);
            Assert.AreEqual(Tests[ID].LimitHigh, "Limit", false);
            Assert.IsFalse(Double.TryParse(Tests[ID].LimitLow, out _));
            Assert.IsFalse(Double.TryParse(Tests[ID].LimitHigh, out _));
            Tests[ID].Measurement = Tests[ID].LimitLow;
            TestTasks.EvaluateTestResult(Tests[ID], out eventCode);
            Assert.AreEqual(eventCode, EventCodes.PASS);
            Tests[ID].Measurement = "1";
            TestTasks.EvaluateTestResult(Tests[ID], out eventCode);
            Assert.AreEqual(eventCode, EventCodes.FAIL);
        }

        [TestMethod()]
        public void EvaluateUUTResultTest() {
            DialogResult dr = MessageBox.Show($"Select Group ID0", $"Select Group ID0", MessageBoxButtons.OKCancel);
            if (dr == DialogResult.Cancel) Assert.Inconclusive();
            Config Config = Config.Get();
            foreach (KeyValuePair<String, Test> t in Config.Tests) t.Value.Result = EventCodes.PASS;
            Assert.IsFalse(Config.Group.Required);
            Assert.AreEqual(TestTasks.EvaluateUUTResult(Config), EventCodes.UNSET);

            dr = MessageBox.Show($"Select Group ID1", $"Select Group ID1", MessageBoxButtons.OKCancel);
            if (dr == DialogResult.Cancel) Assert.Inconclusive();
            Config = Config.Get();
            Assert.IsTrue(Config.Group.Required);
            foreach (KeyValuePair<String, Test> t in Config.Tests) Assert.AreEqual(t.Value.Result, EventCodes.UNSET);
          
            foreach (KeyValuePair<String, Test> t in Config.Tests) t.Value.Result = EventCodes.ERROR;
            Assert.AreEqual(TestTasks.EvaluateUUTResult(Config), EventCodes.ERROR);
            foreach (KeyValuePair<String, Test> t in Config.Tests) {
                // If any result is ERROR, always evaluate to ERROR.
                t.Value.Result = EventCodes.UNSET;
                Assert.AreEqual(TestTasks.EvaluateUUTResult(Config), EventCodes.ERROR);
                t.Value.Result = EventCodes.ABORT;
                Assert.AreEqual(TestTasks.EvaluateUUTResult(Config), EventCodes.ERROR);
                t.Value.Result = EventCodes.FAIL;
                Assert.AreEqual(TestTasks.EvaluateUUTResult(Config), EventCodes.ERROR);
                t.Value.Result = EventCodes.PASS;
                Assert.AreEqual(TestTasks.EvaluateUUTResult(Config), EventCodes.ERROR);
                t.Value.Result = EventCodes.ERROR;
            }

            foreach (KeyValuePair<String, Test> t in Config.Tests) t.Value.Result = EventCodes.UNSET;
            Assert.AreEqual(TestTasks.EvaluateUUTResult(Config), EventCodes.ERROR);
            foreach (KeyValuePair<String, Test> t in Config.Tests) {
                // In the absence of ERROR, any result = UNSET evaluates to ERROR.
                t.Value.Result = EventCodes.FAIL;
                Assert.AreEqual(TestTasks.EvaluateUUTResult(Config), EventCodes.ERROR);
                t.Value.Result = EventCodes.PASS;
                Assert.AreEqual(TestTasks.EvaluateUUTResult(Config), EventCodes.ERROR);
                t.Value.Result = EventCodes.ABORT;
                Assert.AreEqual(TestTasks.EvaluateUUTResult(Config), EventCodes.ABORT);
                t.Value.Result = EventCodes.UNSET;
            }

            foreach (KeyValuePair<String, Test> t in Config.Tests) t.Value.Result = EventCodes.ABORT;
            Assert.AreEqual(TestTasks.EvaluateUUTResult(Config), EventCodes.ABORT);
            foreach (KeyValuePair<String, Test> t in Config.Tests) {
                // In the absence of ERROR & UNSET, any result = ABORT evaluates to ABORT.
                t.Value.Result = EventCodes.FAIL;
                Assert.AreEqual(TestTasks.EvaluateUUTResult(Config), EventCodes.ABORT);
                t.Value.Result = EventCodes.PASS;
                Assert.AreEqual(TestTasks.EvaluateUUTResult(Config), EventCodes.ABORT);
                t.Value.Result = EventCodes.ABORT;
            }

            foreach (KeyValuePair<String, Test> t in Config.Tests) t.Value.Result = EventCodes.FAIL;
            Assert.AreEqual(TestTasks.EvaluateUUTResult(Config), EventCodes.FAIL);
            foreach (KeyValuePair<String, Test> t in Config.Tests) {
                // In the absence of ERROR, UNSET and ABORT, any result = FAIL evaluates to FAIL.
                t.Value.Result = EventCodes.PASS;
                Assert.AreEqual(TestTasks.EvaluateUUTResult(Config), EventCodes.FAIL);
                t.Value.Result = EventCodes.FAIL;
            }

            foreach (KeyValuePair<String, Test> t in Config.Tests) t.Value.Result = EventCodes.PASS;
            Assert.AreEqual(TestTasks.EvaluateUUTResult(Config), EventCodes.PASS);
            // All Test.Result values must be EventCode.PASS for EvaluateUUTResult() to return EventCode.PASS.
            foreach (KeyValuePair<String, Test> t in Config.Tests) {
                t.Value.Result = EventCodes.ERROR;
                Assert.AreEqual(TestTasks.EvaluateUUTResult(Config), EventCodes.ERROR);
                t.Value.Result = EventCodes.UNSET;
                Assert.AreEqual(TestTasks.EvaluateUUTResult(Config), EventCodes.ERROR);
                t.Value.Result = EventCodes.ABORT;
                Assert.AreEqual(TestTasks.EvaluateUUTResult(Config), EventCodes.ABORT);
                t.Value.Result = EventCodes.FAIL;
                Assert.AreEqual(TestTasks.EvaluateUUTResult(Config), EventCodes.FAIL);
                t.Value.Result = EventCodes.PASS;
            }

            foreach (KeyValuePair<String, Test> t in Config.Tests) t.Value.Result = EventCodes.PASS;
            foreach (KeyValuePair<String, Test> t in Config.Tests) {
                t.Value.Result = "This result should throw an Exception";
                Assert.ThrowsException<Exception>(() => TestTasks.EvaluateUUTResult(Config));
                t.Value.Result = EventCodes.PASS;
                Assert.AreEqual(TestTasks.EvaluateUUTResult(Config), EventCodes.PASS);
            }
        }
    }
}
