using QRCodeManager.Application.Constants;
using QRCodeManager.Infrastructure.Data;

namespace QRCodeManager.Tests;

public class DatabasePathProviderTests
{
    [Fact]
    public void DatabasePath_UsesLocalApplicationDataDirectory()
    {
        var sut = new DatabasePathProvider();

        var expectedRoot = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            ApplicationConstants.ApplicationName);
        var expectedDatabaseDirectory = Path.Combine(expectedRoot, ApplicationConstants.DatabaseDirectoryName);

        Assert.Equal(expectedRoot, sut.ApplicationDataDirectory);
        Assert.Equal(expectedDatabaseDirectory, sut.DatabaseDirectory);
        Assert.Equal(Path.Combine(expectedDatabaseDirectory, ApplicationConstants.DatabaseFileName), sut.DatabasePath);
        Assert.EndsWith(ApplicationConstants.InitialDatabaseFileName, sut.BundledInitialDatabasePath);
    }
}
