namespace BlazorApp.Data;

public class Mutex : ISemaphoreOperations
{

    private int _state = 1;

    public Mutex(){

    }

    public bool canDecrement()
    {
        return _state > 0;
    }

    public void decrement()
    {
        _state--;
    }

    public void increment()
    {
        _state++;
    }
}