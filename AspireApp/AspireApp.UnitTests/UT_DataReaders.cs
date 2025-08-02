using System.Diagnostics;
using AspireApp.Libraries;
using AspireApp.Libraries.DataSource;
using AspireApp.Libraries.Enums;
using csmatio;

namespace AspireApp.UnitTests
{
    [TestClass]
    public class UT_DataReaders
    {
        [TestMethod]
        public void Test_RawDataReader()
        {
            // Read a .xts TestSuite file
            string filePath = @"C:\Redlen\TEST LOG\MINI MODULE\Canon\M40190\Test 001\Raw Test Data\M40190\";
            string sensor = "A0";
            //string fileName = @"UNIFORMITY\CANON_MM_UNIFORMITY_M40190-XX_2020_12_19__09_40_38.mat"; // 1.49 MB
            //string fileName = @"DYNAMIC\CANON_MM_DYNAMIC_5MA_M40190-XX_2020_12_19__09_40_38.mat"; // 48.51 MB
            //string fileName = @"ENERGY_CAL\xray-Raw-K-edge_1000_ENERGY_CALIBRATION_M40190-XX_2020_12_19__09_40_38.mat"; // 2.97 MB
            //string fileName = @"ENERGY_CAL\xray-Pb-K-edge_1000_ENERGY_CALIBRATION_M40190-XX_2020_12_19__09_40_38.mat"; // 0.60 MB
            //string fileName = @"ENERGY_CAL\xray-CeO2-K-edge_1000_ENERGY_CALIBRATION_M40190-XX_2020_12_19__09_40_38.mat"; //
            //string fileName = @"SPECTRUM\xray_1000_SPECTRUM_Cu_M40190-XX_2020_12_19__09_40_38.mat"; // 3.59 MB
            string fileName = @"STABILITY\CANON_MM_STABILITY_7MA_M40190-XX_Run000_2020_12_19__09_40_38.mat"; // 67.59 MB
            //string fileName = @"STABILITY\CANON_MM_STABILITY_25MA_M40190-XX_2020_12_19__09_40_38.mat"; //  72.62 MB

            //var fileNames = new List<string>() { fileName1.Replace("XX", sensor), fileName2.Replace("XX", sensor), fileName3.Replace("XX", sensor) };

            var fileNames = new List<string>() { fileName.Replace("XX", sensor) };
            var oResult = new string[1];
            DateTime dtStart, dtEnd;
            decimal decRatio = 0;
            Dictionary<enmDerivedData, Array> dictDerivedData = new ();

            for (int k = 0; k < fileNames.Count; k++)
            {
                var reader = new RawDataReader(filePath, fileNames[k]);
                Array arrData = ((Array)reader.Data).Squeeze();
                var engine = new DataSourceEngine("D-Number Stability");
                dictDerivedData = engine.GetDerivedDataFromFile(enmTestType.stability, arrData);
            }
            Debug.Assert(dictDerivedData.Count != 0);
        }
    }
}