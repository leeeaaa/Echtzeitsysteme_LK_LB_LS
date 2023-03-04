namespace BlazorApp.Data;

public interface ISemaphoreOperations
{

    void increment();

    void decrement();

    bool canDecrement();

}