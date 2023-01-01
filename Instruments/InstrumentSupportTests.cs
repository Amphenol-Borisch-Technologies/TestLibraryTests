using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using ABTTestLibrary.Instruments;

// NOTE: The quality of these MSTest unit tests vary, and some can certainly be improved.
namespace ABTTestLibraryTests.Instruments {
    [TestClass()]
    public class InstrumentTests {
        public static Dictionary<String, Instrument> instruments;
        public static DialogResult dr;

        [ClassInitialize]
        public static void ClassInitialize(TestContext TestContext) {
            dr = MessageBox.Show("Power ON all Instruments", "Power all ON.", MessageBoxButtons.OKCancel);
            if (dr == DialogResult.Cancel) Assert.Inconclusive();
            instruments = Instrument.Get();
        }

        [TestMethod()]
        public void InstrumentTestGetTest() {
            Dictionary<String, InstrumentTest> InstrumentTests = InstrumentTest.Get();
            Assert.AreEqual(instruments.Count(), InstrumentTests.Count());
            foreach (KeyValuePair<String, Instrument> i in instruments) {
                Assert.AreEqual(instruments[i.Key].ID, InstrumentTests[i.Key].ID, false);
                Assert.AreEqual(instruments[i.Key].Address, InstrumentTests[i.Key].Address, false);
                Assert.AreEqual(instruments[i.Key].Category, InstrumentTests[i.Key].Category, false);
                Assert.IsInstanceOfType(instruments[i.Key].Instance, typeof(Object));
                Assert.IsInstanceOfType(InstrumentTests[i.Key].Instance, typeof(Object));
                Assert.AreEqual(instruments[i.Key].Manufacturer, InstrumentTests[i.Key].Manufacturer, false);
                Assert.AreEqual(instruments[i.Key].Model, InstrumentTests[i.Key].Model, false);
                Console.WriteLine($"ID           : {instruments[i.Key].ID}");
                Console.WriteLine($"Address      : {instruments[i.Key].Address}");
                Console.WriteLine($"Category     : {instruments[i.Key].Category}");
                Console.WriteLine($"Manufacturer : {instruments[i.Key].Manufacturer}");
                Console.WriteLine($"Model        : {instruments[i.Key].Model}{Environment.NewLine}");
            }
        }
    }

    internal class InstrumentTest {
        public String ID { get; private set; }
        public String Address { get; private set; }
        public String Category { get; private set; }
        public object Instance { get; private set; }
        public String Manufacturer { get; private set; }
        public String Model { get; private set; }

        private InstrumentTest(String ID, String Address, String Category, String Manufacturer, String Model) {
            this.ID = ID;
            this.Address = Address;
            this.Category = Category;
            this.Instance = new Object();
            this.Manufacturer = Manufacturer;
            this.Model = Model;
        }

        internal static Dictionary<String, InstrumentTest> Get() {
            Dictionary<String, InstrumentTest> d = new Dictionary<String, InstrumentTest> {
                //{ "LOAD", new InstrumentTest("LOAD", "USB0::0x2A8D::0x3802::MY61001295::0::INSTR", "Electronic Load", "Keysight Technologies", "EL34143A") },
                //{ "MULTI_METER", new InstrumentTest("MULTI_METER", "TBD", "Multi-Meter", "Keysight Technologies", "34661A") },
                //{ "WAVE_GENERATOR", new InstrumentTest("WAVE_GENERATOR", "USB0::0x0957::0x2507::MY59003604::0::INSTR", "Waveform Generator", "Keysight Technologies", "33509B") },
                //{ "POWER_MAIN", new InstrumentTest("POWER_MAIN", "USB0::0x2A8D::0x3402::MY61002598::0::INSTR", "Power Supply", "Keysight Technologies", "E36234A") },
                { "POWER_PRELIMINARY", new InstrumentTest("POWER_PRELIMINARY", "USB0::0x2A8D::0x1802::MY61001696::0::INSTR", "Power Supply", "Keysight Technologies", "E36105B") },
                { "POWER_PRIMARY", new InstrumentTest("POWER_PRIMARY", "USB0::0x2A8D::0x1602::MY61001983::0::INSTR", "Power Supply", "Keysight Technologies", "E36103B") },
                { "POWER_SECONDARY", new InstrumentTest("POWER_SECONDARY", "USB0::0x2A8D::0x1602::MY61001958::0::INSTR", "Power Supply", "Keysight Technologies", "E36103B") }
            };
            return d;
        }
    }
}