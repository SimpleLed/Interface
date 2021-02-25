using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace Tests
{
    [TestFixture()]
    public class ReleaseNumberTests
    {
        [Test()]
        public void EnsureThatOrderWorks()
        {
            List<ReleaseNumber> test = new List<ReleaseNumber>
            {
                new ReleaseNumber("1.0.0.0"),
                new ReleaseNumber("1.0.0.0001"),
                new ReleaseNumber("1.0.0.0002"),
                new ReleaseNumber("1.0.0.0010"),
                new ReleaseNumber("1.0.1.0"),
                new ReleaseNumber("1.1.0.0"),
            };

            var testReOrdered = test.OrderBy(x => Guid.NewGuid());

            var testSorted = testReOrdered.OrderBy(x => x).ToArray();

            var pseudo = test.Select(x => x.ToString()).OrderBy(x => x).Select(x => new ReleaseNumber(x)).ToArray();

            Debug.WriteLine(testSorted);

            for (int i = 0; i < testSorted.Length; i++)
            {
                Assert.That(testSorted[i].ToString() == pseudo[i].ToString());
            }

            
        }
    }
}