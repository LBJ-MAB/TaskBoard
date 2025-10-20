using Application;      // for ITaskService and InMemoryTaskService

namespace Test;

public class Tests
{
    [SetUp]
    public void Setup()
    {
        // make an instance of InMemoryTaskService()
    }

    [Test]
    public void Test1()
    {
        Assert.Pass();
    }
}