using Xunit;

namespace ArchivSoftware.Tests;

/// <summary>
/// Beispiel-Testklasse.
/// </summary>
public class SampleTest
{
    [Fact]
    public void Test_ShouldPass()
    {
        // Arrange
        var expected = true;
        
        // Act
        var actual = true;
        
        // Assert
        Assert.Equal(expected, actual);
    }
}
