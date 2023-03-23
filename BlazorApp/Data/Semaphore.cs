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

	public Queue<String> LastIncrementActivity { get; set; } = new();

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

    public void Increment(string activityName = "")
    {
	    LastIncrementActivity.Enqueue(activityName);
		State++;
	}

    public void Decrement()
    {
        State--;
        if(LastIncrementActivity.Count > 0)
			_ = LastIncrementActivity.Dequeue();
        Debug.Assert(State >= 0);
	}

}