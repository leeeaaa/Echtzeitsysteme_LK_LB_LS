using System.Diagnostics;

namespace BlazorApp.Data;

public class Semaphore : ISemaphoreOperations
{

    public string Name {
        get;
        private set;
    }

    public int State { get; private set; }

    public bool IsActivitySemaphore{
        get;
        private set;
    }

    public int NumberInputs {
        get;
        private set;
    }
	public List<Activity> Inputs { get; set; } = new List<Activity>();
	public List<Activity> Outputs { get; set; } = new List<Activity>();

	public Semaphore(int state, string name) : this(state, false, 1, name){}

    public Semaphore(int state, bool isActivitySemaphore, int numberInputs, string name){
        State = state;
        IsActivitySemaphore = isActivitySemaphore;
        NumberInputs = numberInputs;
        Name = name;
    }

    public void IncrementNumberInputs() => NumberInputs++;

    public void SetActivitySemaphore() => IsActivitySemaphore = true;

    public bool CanDecrement()
    {
        return State > 0;
    }

    public void Decrement()
    {
        State--;
        Debug.Assert(State >= 0);
	}

    public void Increment()
    {
        State++;
    }
}