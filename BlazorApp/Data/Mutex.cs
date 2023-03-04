namespace BlazorApp.Data;

public class Mutex : ISemaphoreOperations
{

    private int _state = 1;

    public Mutex(){

    }

    public bool CanDecrement()
    {
        return _state > 0;
    }

    public void Decrement()
    {
        _state--;
    }

    public void Increment()
    {
        _state++;
    }
}