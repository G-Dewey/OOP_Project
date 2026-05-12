using ErrorOr;
using Microsoft.VisualStudio.TestPlatform.ObjectModel.Client;
using NUnit.Framework.Legacy;

namespace OOP_Project.nUnitTests
{
    public class JobShopTests
    {
        [Test]
        public void TestCreateJobshop()
        {
            ErrorOr<JobShop> validJobShop = JobShop.Create(TestData.ValidData);
            ErrorOr<JobShop> invalidJobShop = JobShop.Create(TestData.InvalidData);

            Assert.That(validJobShop.IsError == false && invalidJobShop.IsError == true);
        }

        [Test]
        public void TestEvaluateGene()
        {
            int fitness = TestData.TinyJobShop.EvaluateGene(TestData.TinyJobShop.BaseGene.ToArray());
            Assert.That(fitness > 0);
        }      
    }
}
