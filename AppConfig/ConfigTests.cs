using System;
using System.Collections.Generic;
using System.Windows.Forms;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ABTTestLibrary.Config;
using ABTTestLibrary.TestSupport;
// Also indirectly tests App.config & Config.cs.

namespace ABTTestLibraryTests.AppConfig {
    [TestClass()]
    public class ConfigTests {
        [TestMethod()]
        public void LoggerGetTest() {
            LoggerSubTest(Logger.Get());
        }

        private void LoggerSubTest(Logger logger) {
            Assert.IsTrue(logger.FileEnabled);
            Assert.AreEqual(logger.FilePath, "LOGGER_FilePath", false);
            Assert.AreEqual(logger.SQLConnectionString, String.Empty);
            Assert.IsFalse(logger.SQLEnabled);
        }

        [TestMethod()]
        public void UUTGetTest() {
            UUTSubTest(UUT.Get());
        }

        private void UUTSubTest(UUT uut) {
            Assert.AreEqual(uut.Customer, "UUT_Customer", false);
            Assert.AreEqual(uut.Type, "UUT_Type", false);
            Assert.AreEqual(uut.Number, "UUT_Number", false);
            Assert.AreEqual(uut.Revision, "UUT_Revision", false);
            Assert.AreEqual(uut.Description, "UUT_Description", false);
            Assert.AreEqual(uut.TestSpecification, "UUT_TestSpecification", false);
            Assert.AreEqual(uut.SerialNumber, String.Empty, false);
            Assert.AreEqual(uut.EventCode, EventCodes.UNSET, false);
        }

        [TestMethod()]
        public void GroupGetTest() {
            GroupSubTest(Group.Get());
        }

        private void GroupSubTest(Dictionary<String, Group> Groups) {
            Assert.AreEqual(Groups.Count, 3);
            Int32 i = 0;
            foreach (KeyValuePair<String, Group> g in Groups) {
                Assert.AreEqual(g.Value.ID, $"ID{i}", false);
                if (i == 0) Assert.IsFalse(g.Value.Required);
                else Assert.IsTrue(g.Value.Required);
                Assert.AreEqual(g.Value.Summary, $"Summary{i}", false);
                Assert.AreEqual(g.Value.Detail, $"Detail{i}", false);
                i++;
            }
        }

        [TestMethod()]
        public void TestGetTest() {
            TestSubTest(Test.Get());
        }

        private void TestSubTest(Dictionary<String, Test> tests) {
            Assert.AreEqual(tests.Count, 10);
            Int32 i = 0;
            foreach (KeyValuePair<String, Test> t in tests) {
                Assert.AreEqual(t.Value.ID, $"ID{i}", false);
                Assert.AreEqual(t.Value.Summary, $"Summary{i}", false);
                Assert.AreEqual(t.Value.Detail, $"Detail{i}", false);
                // Assert.AreEqual(Tests[t.Key].LimitLow, $"LimitLow{i}", false);
                // Assert.AreEqual(Tests[t.Key].LimitHigh, $"LimitHigh{i}", false);
                // LimitLow & LimitHigh are tested in class TestSupportTests, method EvaluateTestResultTest
                if (t.Key == "ID9") Assert.AreEqual(t.Value.Units, $"N/A", false);
                else Assert.AreEqual(t.Value.Units, $"Units{i}", false);
                Assert.AreEqual(t.Value.UnitType, $"UnitType{i}", false);
                Assert.AreEqual(t.Value.Result, EventCodes.UNSET, false);
                i++;
            }
        }

        [TestMethod()]
        public void GroupSelectTests() {
            Dictionary<String, Group> Groups = Group.Get();
            DialogResult dr;
            foreach (KeyValuePair<String, Group> g in Groups) {
                dr = MessageBox.Show($"Select Group {g.Key}", $"Select Group {g.Key}", MessageBoxButtons.OKCancel);
                if (dr == DialogResult.Cancel) Assert.Inconclusive();
                String gs = GroupSelect.Get(Groups);
                Assert.AreEqual(gs, g.Key);
            }
        }

        [TestMethod()]
        public void ConfigLibGetTest() {
            ConfigLib configLib = ConfigLib.Get();
            Assert.IsInstanceOfType(configLib.Logger, typeof(Logger));
            Assert.IsInstanceOfType(configLib.UUT, typeof(UUT));
        }

        [TestMethod()]
        public void ConfigTestGetTest() {
            ConfigTest configTest = ConfigTest.Get();
            Assert.IsInstanceOfType(configTest.Group, typeof(Group));
            Assert.IsInstanceOfType(configTest.Tests, typeof(Dictionary<String, Test>));
            Console.WriteLine($"config.Group.ID='{configTest.Group.ID}'");
            foreach (KeyValuePair<String, Test> t in configTest.Tests) Console.WriteLine($"ID='{t.Value.ID}', Summary='{t.Value.Summary}', Detail='{t.Value.Detail}'");
        }
    }
}