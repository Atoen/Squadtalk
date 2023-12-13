namespace Shared.Communication;

public interface ICommunicationTab
{
    bool Selected { get; }
    
    string Name { get; }
}