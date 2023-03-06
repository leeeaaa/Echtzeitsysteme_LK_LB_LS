using System.Diagnostics;

namespace BlazorApp.Data;

public class Mutex : ISemaphoreOperations
{

    public string Name {
        get;
        private set;
    }

    public int State { get; private set; } = 1;

    public List<Activity> Inputs { get; set; } = new List<Activity>();
	public List<Activity> Outputs { get; set; } = new List<Activity>();

	public Mutex(string name){
        Name = name;
    }

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