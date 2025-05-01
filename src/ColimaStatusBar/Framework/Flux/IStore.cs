namespace ColimaStatusBar.Framework.Flux;

public interface ICommand;

public interface IStore
{
    Task Handle(ICommand command);
}
