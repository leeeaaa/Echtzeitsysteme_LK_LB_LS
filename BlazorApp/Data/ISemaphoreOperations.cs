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

}