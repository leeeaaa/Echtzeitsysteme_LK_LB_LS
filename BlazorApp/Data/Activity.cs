namespace BlazorApp.Data;

public class Activity 
{
    public List<ISemaphoreOperations> Inputs {
        get;
        private set;
    }

    public List<ISemaphoreOperations> Outputs {
        get;
        private set;
    }

    private readonly int _processDuration;

    private int _processTime = 0;

    public bool CanProcess { get; private set; } = false;

    public string Name {
        get;
        private set;
    }

    public Activity(int processDuration, string name) 
        : this(new(), new(), processDuration, name){}

    public Activity(List<ISemaphoreOperations> inputs, List<ISemaphoreOperations> outputs,
        int processDuration, string name)
    {
        Inputs = inputs;
        Outputs = outputs;
        _processDuration = processDuration;
        Name = name;
    }


    public void AddInput(ISemaphoreOperations input) => Inputs.Add(input);

    public void AddOutput(ISemaphoreOperations output) => Outputs.Add(output);

    public void Consume()
    {
        if(_processTime == 0){
            Inputs.ForEach(input => {
                CanProcess = input.CanDecrement();
                if(CanProcess == false) return;
            });

            Inputs.ForEach(input => input.Decrement());
        }
    }


    public void Process()
    {
        if(_processTime > 0 || CanProcess) _processTime++;
        if(_processTime == _processDuration)
        {
            Outputs.ForEach(output => output.Increment());
            _processTime = 0;
        }
        CanProcess = false;
    }

    public bool IsActive() => _processTime > 0;

}