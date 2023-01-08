using TestLibrary.Config;
using TestLibrary.Instruments;
using Agilent.CommandExpert.ScpiNet.AgSCPI99_1_0;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Windows.Forms;

namespace TestLibraryTests.Instruments {
    [TestClass()]
    public class SCPI99Tests {
        public AgSCPI99 AG_SCPI99;
        public static Dictionary<String, Instrument> instruments;
        public static DialogResult dr;

        [ClassInitialize]
        public static void ClassInitialize(TestContext TestContext) {
            dr = MessageBox.Show("Power ON all Instruments", "Power all ON.", MessageBoxButtons.OKCancel);
            if (dr == DialogResult.Cancel) Assert.Inconclusive();
            instruments = Instrument.Get();
        }

        [TestMethod()]
        public void ResetTest() {
            foreach (KeyValuePair<String, Instrument> i in instruments) {
                Console.WriteLine(InstrumentTasks.GetMessage(i.Value));
                SCPI99.Reset(i.Value.Address);
                AG_SCPI99 = new AgSCPI99(i.Value.Address);
                AG_SCPI99.SCPI.STATus.QUEStionable.CONDition.Query(out Int32 ConditionRegister);
                AG_SCPI99.SCPI.TST.Query(out Int32 SelfTestResult);
                Assert.AreEqual(SelfTestResult, 0);
                Assert.AreEqual(ConditionRegister, 0);
            }
        }

        [TestMethod()]
        public void SelfTestTest() {
            Int32 SelfTestResult;
            foreach (KeyValuePair<String, Instrument> i in instruments) {
                Console.WriteLine(InstrumentTasks.GetMessage(i.Value));
                AG_SCPI99 = new AgSCPI99(i.Value.Address);
                AG_SCPI99.SCPI.RST.Command();
                SelfTestResult = SCPI99.SelfTest(i.Value.Address);
                Assert.AreEqual(SelfTestResult, 0);
            }
        }

        [TestMethod()]
        public void QuestionConditionTest() {
            Int32 ConditionRegister;
            foreach (KeyValuePair<String, Instrument> i in instruments) {
                Console.WriteLine(InstrumentTasks.GetMessage(i.Value));
                AG_SCPI99 = new AgSCPI99(i.Value.Address);
                AG_SCPI99.SCPI.RST.Command();
                ConditionRegister = SCPI99.QuestionCondition(i.Value.Address);
                Assert.AreEqual(ConditionRegister, 0);
            }
        }
    }
}