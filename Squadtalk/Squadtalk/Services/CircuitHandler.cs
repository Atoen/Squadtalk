using Microsoft.AspNetCore.Components.Server.Circuits;

namespace Squadtalk.Services;

public class MyCircuitHandler : CircuitHandler
{
    private readonly IMyCircuit _myCircuit;

    public MyCircuitHandler(IMyCircuit myCircuit)
    {
        _myCircuit = myCircuit;
    }
    
    public override Task OnCircuitOpenedAsync(Circuit circuit, CancellationToken cancellationToken)
    {
        _myCircuit.CurrentCircuit = circuit;
        return base.OnCircuitOpenedAsync(circuit, cancellationToken);
    }
}

public interface IMyCircuit
{
    Circuit CurrentCircuit { get; set; }
}

public class MyCircuit : IMyCircuit
{
    public Circuit? CurrentCircuit { get; set; }  
}