using System;
using System.IO;
using Commands;
using Common;
using NUnit.Framework;

namespace Tests.CommandsTests
{
    [TestFixture]
    public class TestInit
    {
        [Test]
        public void CreateCementDir()
        {
            using (var tmp = new TempDirectory())
            {
                using (new DirectoryJumper(tmp.Path))
                {
                    new Init().Run(new[] {"init"});
                    var path = Path.Combine(tmp.Path, DirectoryHelper.CementDirectory);
                    Console.WriteLine($@"Assert path '{path}'");
                    Assert.That(Directory.Exists(path));
                }
            }
        }
    }
}