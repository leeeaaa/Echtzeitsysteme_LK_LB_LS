using System.Diagnostics;

namespace BlazorApp.Data;

public class Mutex : ISemaphoreOperations
{

    public string Name {
        get;
        set;
    }
	public List<Activity> Inputs { get; set; } = new List<Activity>();
	public List<Activity> Outputs { get; set; } = new List<Activity>();

	private int _state = 1;

    public Mutex(string name){
        Name = name;
    }

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