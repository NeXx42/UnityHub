using System;
using System.Threading.Tasks;
using UI.Controls;

namespace UI.Modals;

public interface IModal
{
    public ModalContainer setContainer { set; }
}
