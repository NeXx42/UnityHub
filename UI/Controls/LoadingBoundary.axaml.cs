using System;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Markup.Xaml;

namespace UI.Controls;

public partial class LoadingBoundary : ContentControl
{
    private TaskCompletionSource setupTask;

    private Grid? _cont;
    private Grid? _loading;

    public LoadingBoundary()
    {
        setupTask = new TaskCompletionSource();

        InitializeComponent();
    }

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);

        _cont = e.NameScope.Find<Grid>("cont");
        _loading = e.NameScope.Find<Grid>("loading");

        setupTask.SetResult();
    }

    public async Task Load(Func<Task> toLoad)
    {
        await InternalLoad(toLoad);
    }

    public async Task<T?> Load<T>(Func<Task<T>> toLoad)
    {
        T? res = default;

        await InternalLoad(async () =>
        {
            res = await toLoad();
        });

        return res;
    }

    private async Task InternalLoad(Func<Task> loader)
    {
        await setupTask.Task;

        if (_loading != null) _loading.IsVisible = true;
        if (_cont != null) _cont.IsVisible = false;

        try
        {
            await loader();
        }
        finally
        {
            if (_loading != null) _loading.IsVisible = false;
            if (_cont != null) _cont.IsVisible = true;
        }
    }
}