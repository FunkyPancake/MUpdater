using System;
using Microsoft.VisualBasic;

namespace CanUpdaterGui;

public class CanFramesCollection :IObservable<Collection>
{
    
    public IDisposable Subscribe(IObserver<Collection> observer)
    {
        throw new NotImplementedException();
    }
}