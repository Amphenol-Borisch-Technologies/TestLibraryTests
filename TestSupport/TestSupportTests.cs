using System;
using System.Collections.Generic;
using System.Windows.Forms;
using TestLibrary.Config;
using TestLibrary.TestSupport;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Reflection;

namespace TestLibraryTests.TestSupport {
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
            Assert.AreEqual(tests.Count, 4);
            foreach (KeyValuePair<String, Test> t in tests) Assert.AreEqual(t.Value.Result, EventCodes.UNSET, false);
            String ID;
            TestNumerical tn;

            //   - LimitLow & LimitHigh both parse to Doubles; both low & high limits.
            ID = "ID0";
            tn = (TestNumerical)tests[ID].ClassObject;
            Assert.AreEqual(tn.Low, 0);
            Assert.AreEqual(tn.High, 1);
            tests[ID].Measurement = "-0.5";
            eventCode = TestTasks.EvaluateTestResult(tests[ID]);
            Assert.AreEqual(eventCode, EventCodes.FAIL);
            tests[ID].Measurement = tn.Low.ToString();
            eventCode = TestTasks.EvaluateTestResult(tests[ID]);
            Assert.AreEqual(eventCode, EventCodes.PASS);
            tests[ID].Measurement = "0.5";
            eventCode = TestTasks.EvaluateTestResult(tests[ID]);
            Assert.AreEqual(eventCode, EventCodes.PASS);
            tests[ID].Measurement = tn.High.ToString();
            eventCode = TestTasks.EvaluateTestResult(tests[ID]);
            Assert.AreEqual(eventCode, EventCodes.PASS);
            tests[ID].Measurement = "1.5";
            eventCode = TestTasks.EvaluateTestResult(tests[ID]);
            Assert.AreEqual(eventCode, EventCodes.FAIL);
            tests[ID].Measurement = "Measurement";

            ID = "ID1";
            tn = (TestNumerical)tests[ID].ClassObject;
            Assert.AreEqual(tn.Low, 1);
            Assert.AreEqual(tn.High, 1);
            tests[ID].Measurement = "0.5";
            eventCode = TestTasks.EvaluateTestResult(tests[ID]);
            Assert.AreEqual(eventCode, EventCodes.FAIL);
            tests[ID].Measurement = tn.Low.ToString();
            eventCode = TestTasks.EvaluateTestResult(tests[ID]);
            Assert.AreEqual(eventCode, EventCodes.PASS);

            ID = "ID2";
            TestTextual tt = (TestTextual)tests[ID].ClassObject;
            Assert.AreEqual(tt.Text, "Text2", false);
            tests[ID].Measurement = tt.Text;
            eventCode = TestTasks.EvaluateTestResult(tests[ID]);
            Assert.AreEqual(eventCode, EventCodes.PASS);
            tests[ID].Measurement = "This Measurement should cause a failure.";
            eventCode = TestTasks.EvaluateTestResult(tests[ID]);
            Assert.AreEqual(eventCode, EventCodes.FAIL);

            ID = "ID3";
            TestCustomizable tc = (TestCustomizable)tests[ID].ClassObject;
            Assert.AreEqual(tc.Arguments["Custom3"], "Custom3", false);
            foreach (FieldInfo fi in typeof(EventCodes).GetFields()) {
                tests[ID].Measurement = (String)fi.GetValue(null);
                eventCode = TestTasks.EvaluateTestResult(tests[ID]);
                Assert.AreEqual(eventCode, fi.GetValue(null));
            }
            tests[ID].Measurement = "This Measurement should cause an Failure.";
        }

        [TestMethod()]
        public void EvaluateUUTResultTest() {
            DialogResult dr = MessageBox.Show($"Select Group 'ID0', where Required = 'False'", $"Select Group 'ID0'", MessageBoxButtons.OKCancel);
            if (dr == DialogResult.Cancel) Assert.Inconclusive();
            ConfigTest configTest = ConfigTest.Get();
            foreach (KeyValuePair<String, Test> t in configTest.Tests) t.Value.Result = EventCodes.PASS;
            Assert.IsFalse(configTest.Group.Required);
            Assert.AreEqual(TestTasks.EvaluateUUTResult(configTest), EventCodes.UNSET);

            dr = MessageBox.Show($"Select Group 'ID1', where Required = 'True'", $"Select Group 'ID1'", MessageBoxButtons.OKCancel);
            if (dr == DialogResult.Cancel) Assert.Inconclusive();
            configTest = ConfigTest.Get();
            Assert.IsTrue(configTest.Group.Required);
            foreach (KeyValuePair<String, Test> t in configTest.Tests) Assert.AreEqual(t.Value.Result, EventCodes.UNSET);
          
            foreach (KeyValuePair<String, Test> t in configTest.Tests) t.Value.Result = EventCodes.ERROR;
            Assert.AreEqual(TestTasks.EvaluateUUTResult(configTest), EventCodes.ERROR);
            foreach (KeyValuePair<String, Test> t in configTest.Tests) {
                // If any result is ERROR, always evaluate to ERROR.
                t.Value.Result = EventCodes.UNSET;
                Assert.AreEqual(TestTasks.EvaluateUUTResult(configTest), EventCodes.ERROR);
                t.Value.Result = EventCodes.CANCEL;
                Assert.AreEqual(TestTasks.EvaluateUUTResult(configTest), EventCodes.ERROR);
                t.Value.Result = EventCodes.FAIL;
                Assert.AreEqual(TestTasks.EvaluateUUTResult(configTest), EventCodes.ERROR);
                t.Value.Result = EventCodes.PASS;
                Assert.AreEqual(TestTasks.EvaluateUUTResult(configTest), EventCodes.ERROR);
                t.Value.Result = EventCodes.ERROR;
            }

            foreach (KeyValuePair<String, Test> t in configTest.Tests) t.Value.Result = EventCodes.UNSET;
            Assert.ThrowsException<InvalidOperationException>(() => TestTasks.EvaluateUUTResult(configTest));
            foreach (KeyValuePair<String, Test> t in configTest.Tests) {
                // In the absence of ERROR & CANCEL, any result = UNSET throws exception.
                t.Value.Result = EventCodes.FAIL;
                Assert.ThrowsException<InvalidOperationException>(() => TestTasks.EvaluateUUTResult(configTest));
                t.Value.Result = EventCodes.PASS;
                Assert.ThrowsException<InvalidOperationException>(() => TestTasks.EvaluateUUTResult(configTest));
                t.Value.Result = EventCodes.CANCEL;
                Assert.AreEqual(TestTasks.EvaluateUUTResult(configTest), EventCodes.CANCEL);
                t.Value.Result = EventCodes.UNSET;
            }

            foreach (KeyValuePair<String, Test> t in configTest.Tests) t.Value.Result = EventCodes.CANCEL;
            Assert.AreEqual(TestTasks.EvaluateUUTResult(configTest), EventCodes.CANCEL);
            foreach (KeyValuePair<String, Test> t in configTest.Tests) {
                // In the absence of ERROR & UNSET, any result = CANCEL evaluates to CANCEL.
                t.Value.Result = EventCodes.FAIL;
                Assert.AreEqual(TestTasks.EvaluateUUTResult(configTest), EventCodes.CANCEL);
                t.Value.Result = EventCodes.PASS;
                Assert.AreEqual(TestTasks.EvaluateUUTResult(configTest), EventCodes.CANCEL);
                t.Value.Result = EventCodes.CANCEL;
            }

            foreach (KeyValuePair<String, Test> t in configTest.Tests) t.Value.Result = EventCodes.FAIL;
            Assert.AreEqual(TestTasks.EvaluateUUTResult(configTest), EventCodes.FAIL);
            foreach (KeyValuePair<String, Test> t in configTest.Tests) {
                // In the absence of ERROR, UNSET and CANCEL, any result = FAIL evaluates to FAIL.
                t.Value.Result = EventCodes.PASS;
                Assert.AreEqual(TestTasks.EvaluateUUTResult(configTest), EventCodes.FAIL);
                t.Value.Result = EventCodes.FAIL;
            }

            foreach (KeyValuePair<String, Test> t in configTest.Tests) t.Value.Result = EventCodes.PASS;
            Assert.AreEqual(TestTasks.EvaluateUUTResult(configTest), EventCodes.PASS);
            // All Test.Result values must be EventCode.PASS for EvaluateUUTResult() to return EventCode.PASS.
            foreach (KeyValuePair<String, Test> t in configTest.Tests) {
                t.Value.Result = EventCodes.ERROR;
                Assert.AreEqual(TestTasks.EvaluateUUTResult(configTest), EventCodes.ERROR);
                t.Value.Result = EventCodes.UNSET;
                Assert.ThrowsException<InvalidOperationException>(() => TestTasks.EvaluateUUTResult(configTest));
                t.Value.Result = EventCodes.CANCEL;
                Assert.AreEqual(TestTasks.EvaluateUUTResult(configTest), EventCodes.CANCEL);
                t.Value.Result = EventCodes.FAIL;
                Assert.AreEqual(TestTasks.EvaluateUUTResult(configTest), EventCodes.FAIL);
                t.Value.Result = EventCodes.PASS;
            }

            foreach (KeyValuePair<String, Test> t in configTest.Tests) t.Value.Result = EventCodes.PASS;
            foreach (KeyValuePair<String, Test> t in configTest.Tests) {
                t.Value.Result = "This result should throw an Exception";
                Assert.ThrowsException<NotImplementedException>(() => TestTasks.EvaluateUUTResult(configTest));
                t.Value.Result = EventCodes.PASS;
                Assert.AreEqual(TestTasks.EvaluateUUTResult(configTest), EventCodes.PASS);
            }
        }
    }
}
