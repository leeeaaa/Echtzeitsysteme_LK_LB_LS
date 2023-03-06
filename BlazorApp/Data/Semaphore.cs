using System.Diagnostics;

namespace BlazorApp.Data;

public class Semaphore : ISemaphoreOperations
{

    public string Name {
        get;
        set;
    }

    private int _state;
    public bool IsActivitySemaphore{
        get;
        private set;
    }

    public int NumberInputs {
        get;
        private set;
    }

    public Semaphore(int state, string name) : this(state, false, 1, name){}

    public Semaphore(int state, bool isActivitySemaphore, int numberInputs, string name){
        _state = state;
        IsActivitySemaphore = isActivitySemaphore;
        NumberInputs = numberInputs;
        Name = name;
    }

    public void IncrementNumberInputs() => NumberInputs++;

    public void SetActivitySemaphore() => IsActivitySemaphore = true;

    public bool CanDecrement()
    {
        return _state > 0;
    }

    public void Decrement()
    {
        _state--;
        Debug.Assert(_state >= 0);
	}

    public void Increment()
    {
        _state++;
    }
}