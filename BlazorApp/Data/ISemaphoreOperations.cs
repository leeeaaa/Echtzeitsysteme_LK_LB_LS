namespace BlazorApp.Data;

public interface ISemaphoreOperations
{

    void Increment(string activityName = "");

    void Decrement();

    bool CanDecrement();

    string Name {
        get;
    }

    int State
    {
	    get;
    }

    List<Activity> Inputs { get; set; }
    List<Activity> Outputs { get; set;}

}