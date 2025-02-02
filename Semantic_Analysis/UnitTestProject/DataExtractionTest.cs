using Semantic_Analysis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UnitTestProject
{
    [TestClass]
    public class DataExtractionTest
    {
        private DataExtraction _dataExtraction = new DataExtraction();

        [TestInitialize]
        public void Setup()
        {
            _dataExtraction = new DataExtraction();
        }

        [TestMethod] 
        public void Test()
        {

        }
    }
}
