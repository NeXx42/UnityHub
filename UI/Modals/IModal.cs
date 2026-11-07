using System;
using System.Threading;
using System.Threading.Tasks;
using UI.Controls;

namespace UI.Modals;

public interface IModal
{
    public ModalContainer? container { get; set; }
}

public interface IModalAutoWait : IModal
{
    public Task Launch(CancellationToken token);
}