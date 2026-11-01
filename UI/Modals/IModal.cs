using System;
using System.Threading.Tasks;
using UI.Controls;

namespace UI.Modals;

public interface IModal
{
    public ModalContainer setContainer { set; }
    public bool canDismiss { get; }
}

public interface IModalAutoWait : IModal
{
    public Task Launch();
}