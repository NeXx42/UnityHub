using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Avalonia.Controls;

namespace UI.Helpers;

public class ReusableList<T> where T : UserControl
{
    private Panel parent;
    private List<T> cachedEntries;

    public delegate void DrawCallback<DATA>(T element, int index, DATA data);

    public ReusableList(Panel parent)
    {
        this.parent = parent;
        cachedEntries = new List<T>();
    }

    public async Task DrawAsync<DATA>(Func<Task<DATA[]>> inp, DrawCallback<DATA> draw)
    {
        var res = await inp();
        Draw(res, draw);
    }

    public void Draw<DATA>(ICollection<DATA> inp, DrawCallback<DATA> draw)
    {
        for (int i = 0; i < Math.Max(inp.Count, cachedEntries.Count); i++)
        {
            if (i >= inp.Count)
            {
                cachedEntries[i].IsVisible = false;
                continue;
            }

            if (i >= cachedEntries.Count)
                CreateEntry();

            cachedEntries[i].IsVisible = true;
            draw(cachedEntries[i], i, inp.ElementAt(i));
        }
    }

    private void CreateEntry()
    {
        T el = Activator.CreateInstance<T>();

        cachedEntries.Add(el);
        parent.Children.Add(el);
    }

    public T this[int i]
    {
        get => cachedEntries[i];
    }
}
