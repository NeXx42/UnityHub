using Avalonia;
using Avalonia.Headless;
using Avalonia.Headless.NUnit;
using Data.DataRepos;
using Logic;
using Logic.Editor;
using Models.Interfaces;
using NUnit.Framework;

namespace UI.Test;

public class Tests
{
    [SetUp]
    public void Setup()
    {
        DependencyManager.RegisterService<IDataRepository, MockDataRepo>();
        DependencyManager.RegisterService<IEditorLogic, EditorLogic_Linux>();
        DependencyManager.RegisterService<IProjectLogic, ProjectLogic>();
        DependencyManager.RegisterService<ITaggingLogic, TaggingLogic>();
        DependencyManager.RegisterService<IConfigLogic, ConfigLogic>();

        AppBuilder.Configure<App>()
            .UseHeadless(new AvaloniaHeadlessPlatformOptions { })
            .SetupWithoutStarting();
    }

    [Test]
    [AvaloniaTest]
    public void Test1()
    {
        var window = new MainWindow();
        Assert.That(window, Is.Not.Null);
    }
}
