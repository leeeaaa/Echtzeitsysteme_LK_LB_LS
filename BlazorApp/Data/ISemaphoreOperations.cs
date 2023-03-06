namespace BlazorApp.Data;

public interface ISemaphoreOperations
{

    void Increment();

    void Decrement();

    bool CanDecrement();

    string Name {
        get;
        set;
    }

    List<Activity> Inputs { get; set; }
    List<Activity> Outputs { get; set;}

}