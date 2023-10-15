using FluentAssertions;
using Harmonize.Common.Models;

namespace Harmonize.CommonTest.Models;
public class JobItemTests
{
    [Fact]
    public void Given_a_new_JobItem_with_a_TestJob_properties_are_set_correctly_and_GetJob_returns_correct_object()
    {
        //Arrange
        var testJob = new TestJob() { Name = "Test", Age = 1 };
        var creatorId = Guid.NewGuid();

        //Act
        var jobItem = new JobItem(testJob, "Test", creatorId);
        var job = jobItem.GetJob<TestJob>();

        //Assert
        jobItem.JobType.Should().Be("TestJob");
        jobItem.Job.Should().Be("{\"Name\":\"Test\",\"Age\":1}");
        jobItem.CreatorName.Should().Be("Test");
        jobItem.CreatorId.Should().Be(creatorId);
        job.Should().BeEquivalentTo(testJob);
    }

    [Fact]
    public void Given_a_new_JobItem_with_a_string_properties_are_set_correctly_and_GetJob_returns_correct_string()
    {
        //Arrange
        var testJob = "This is a test string";
        var creatorId = Guid.NewGuid();

        //Act
        var jobItem = new JobItem(testJob, "Test", creatorId);
        var job = jobItem.GetJob<string>();

        //Assert
        jobItem.JobType.Should().Be("String");
        jobItem.Job.Should().Be(testJob);
        jobItem.CreatorName.Should().Be("Test");
        jobItem.CreatorId.Should().Be(creatorId);
        job.Should().BeEquivalentTo(testJob);
    }
}

public struct TestJob
{
    public string? Name { get; set; }
    public int Age { get; set; }
}
