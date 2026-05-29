namespace Entain.Application.Interface.Service;

public interface ICallGroup<in TOperation>
{
    Task Join(TOperation value); 
    void Leave(); 
}