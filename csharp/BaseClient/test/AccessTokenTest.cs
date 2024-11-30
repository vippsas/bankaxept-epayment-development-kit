using System;
using System.IO;
using System.Reflection;
using System.Text.RegularExpressions;
using Microsoft.VisualStudio.TestPlatform.PlatformAbstractions;
using NUnit.Framework;

namespace BankAxept.Epayment.Client.Base.AccessToken
{
    public class AccessTokenTest
    {
        private readonly DateTime fixedTime = DateTime.UtcNow;

        public AccessTokenTest() {}

        [Test]
        public void ShouldParseJson() {
            var parsedToken = AccessToken.Parse (ReadTestData("resources/token-response.json"), fixedTime);
            Assert.That("a-token", Is.EqualTo(parsedToken.GetToken()));
            Assert.That(fixedTime.AddSeconds(3600), Is.EqualTo(parsedToken.GetExpiry()));
        }

        [Test]
        public void ShouldParseJsonDifferentOrderOtherFields() {
            var parsedToken = AccessToken.Parse (ReadTestData ("resources/token-response2.json"), fixedTime);
            Assert.That("a-token", Is.EqualTo(parsedToken.GetToken()));
            Assert.That(fixedTime.AddSeconds(3600), Is.EqualTo(parsedToken.GetExpiry()));
        }

        [Test]
        public void ShouldThrowIfUnparsable() {
            Assert.Throws<ArgumentException>(() => AccessToken.Parse("garbage"));
        }

        private string ReadTestData(string filename) {
            // tests executed from assembly path at /test/bin/Debug/net8.0
            var path = Path.GetFullPath($"{Directory.GetCurrentDirectory()}/../../../{filename}");
            return File.ReadAllText(path);
        }
    }

}