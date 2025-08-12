using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HASS.Agent.Base.Models;
using HASS.Agent.Contracts.Models.Update;

namespace HASS.Agent.Test.Contracts.Models;

[TestClass]
public class AgentVersionTest
{
    [TestMethod]
    public void TestParsing()
    {
        var version = new AgentVersion();
        var parseResult = version.Parse("1.0.3");
        Assert.IsTrue(parseResult);
        Assert.AreEqual(1, version.Base.Major);
        Assert.AreEqual(0, version.Base.Minor);
        Assert.AreEqual(3, version.Base.Build);
        Assert.IsFalse(version.IsBeta);
        Assert.IsFalse(version.IsNightly);
    }

    [TestMethod]
    public void TestParsingInvalid()
    {
        var version = new AgentVersion();
        var parseResult = version.Parse("x1.0.3");
        Assert.IsFalse(parseResult);
    }

    [TestMethod]
    public void TestParsingBeta()
    {
        var version = new AgentVersion();
        var parseResult = version.Parse("5.7.1-beta1.0");
        Assert.IsTrue(parseResult);
        Assert.AreEqual(5, version.Base.Major);
        Assert.AreEqual(7, version.Base.Minor);
        Assert.AreEqual(1, version.Base.Build);
        Assert.IsTrue(version.IsBeta);
        Assert.IsFalse(version.IsNightly);
        Assert.AreEqual(1, version.Additional.Major);
        Assert.AreEqual(0, version.Additional.Minor);
        Assert.AreEqual(-1, version.Additional.Build);
    }

    [TestMethod]
    public void TestParsingBeta2()
    {
        var version = new AgentVersion();
        var parseResult = version.Parse("5.7.1-beta4.5.3");
        Assert.IsTrue(parseResult);
        Assert.AreEqual(5, version.Base.Major);
        Assert.AreEqual(7, version.Base.Minor);
        Assert.AreEqual(1, version.Base.Build);
        Assert.IsTrue(version.IsBeta);
        Assert.IsFalse(version.IsNightly);
        Assert.AreEqual(4, version.Additional.Major);
        Assert.AreEqual(5, version.Additional.Minor);
        Assert.AreEqual(3, version.Additional.Build);
    }

    [TestMethod]
    public void TestParsingBetaInvalid()
    {
        var version = new AgentVersion();
        var parseResult = version.Parse("1.0.3-beta1");
        Assert.IsFalse(parseResult);
    }

    [TestMethod]
    public void TestParsingNightly()
    {
        var version = new AgentVersion();
        var parseResult = version.Parse("3.8.2-nightly5.4");
        Assert.IsTrue(parseResult);
        Assert.AreEqual(3, version.Base.Major);
        Assert.AreEqual(8, version.Base.Minor);
        Assert.AreEqual(2, version.Base.Build);
        Assert.IsFalse(version.IsBeta);
        Assert.IsTrue(version.IsNightly);
        Assert.AreEqual(5, version.Additional.Major);
        Assert.AreEqual(4, version.Additional.Minor);
        Assert.AreEqual(-1, version.Additional.Build);
    }

    [TestMethod]
    public void TestParsingNightly2()
    {
        var version = new AgentVersion();
        var parseResult = version.Parse("6.6.3-nightly1.3.8");
        Assert.IsTrue(parseResult);
        Assert.AreEqual(6, version.Base.Major);
        Assert.AreEqual(6, version.Base.Minor);
        Assert.AreEqual(3, version.Base.Build);
        Assert.IsFalse(version.IsBeta);
        Assert.IsTrue(version.IsNightly);
        Assert.AreEqual(1, version.Additional.Major);
        Assert.AreEqual(3, version.Additional.Minor);
        Assert.AreEqual(8, version.Additional.Build);
    }

    [TestMethod]
    public void TestParsingNightlyInvalid()
    {
        var version = new AgentVersion();
        var parseResult = version.Parse("4.7.5-nightly");
        Assert.IsFalse(parseResult);
    }

    [TestMethod]
    public void TestParsingCustomTag()
    {
        var version = new AgentVersion();
        var parseResult = version.Parse("7.3.5-custom21.37");
        Assert.IsTrue(parseResult);
        Assert.AreEqual(7, version.Base.Major);
        Assert.AreEqual(3, version.Base.Minor);
        Assert.AreEqual(5, version.Base.Build);
        Assert.IsFalse(version.IsBeta);
        Assert.IsFalse(version.IsNightly);
        Assert.AreEqual("custom", version.Tag);
        Assert.AreEqual(21, version.Additional.Major);
        Assert.AreEqual(37, version.Additional.Minor);
        Assert.AreEqual(-1, version.Additional.Build);
    }

    [TestMethod]
    public void TestComparison()
    {
        var newer = new AgentVersion("5.2.3");
        var older = new AgentVersion("2.8.9");
        var older2 = new AgentVersion("2.8.9");
        Assert.AreEqual(VersionComparison.Newer, older.CompareTo(newer));
        Assert.AreEqual(VersionComparison.Older, newer.CompareTo(older));
        Assert.AreEqual(VersionComparison.Equal, older.CompareTo(older2));
    }

    [TestMethod]
    public void TestComparisonBeta()
    {
        var newer = new AgentVersion("5.2.3");
        var newerBeta = new AgentVersion("5.2.3-beta3.1.3");
        var older = new AgentVersion("2.8.9");
        var olderBeta = new AgentVersion("2.8.9-beta1.0");
        Assert.AreEqual(VersionComparison.Newer, newer.CompareTo(newerBeta));
        Assert.AreEqual(VersionComparison.Newer, older.CompareTo(olderBeta));
        Assert.AreEqual(VersionComparison.Older, newer.CompareTo(olderBeta));
    }

    [TestMethod]
    public void TestToString()
    {
        var versionString = "5.2.3-beta3.1.3";
        var version = new AgentVersion(versionString);
        Assert.AreEqual(versionString, version.ToString());
    }
}
