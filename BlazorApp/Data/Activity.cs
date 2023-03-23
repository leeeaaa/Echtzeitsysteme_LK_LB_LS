namespace BlazorApp.Data;

public class Activity
{
	public List<ISemaphoreOperations> Inputs
	{
		get;
		private set;
	}

	public List<ISemaphoreOperations> Outputs
	{
		get;
		private set;
	}

	public int ProcessDuration { private set; get; } = 1;

	public int ProcessTime { private set; get; } = 0;

	public bool CanProcess { get; private set; } = false;

	public string Name
	{
		get;
		private set;
	}

	public Activity(int processDuration, string name)
		: this(new(), new(), processDuration, name) { }

	public Activity(List<ISemaphoreOperations> inputs, List<ISemaphoreOperations> outputs,
		int processDuration, string name)
	{
		Inputs = inputs;
		Outputs = outputs;
		ProcessDuration = processDuration;
		Name = name;
	}


	public void AddInput(ISemaphoreOperations input)
	{
		Inputs.Add(input);
		input.Outputs.Add(this);
	}

	public void AddOutput(ISemaphoreOperations output)
	{
		Outputs.Add(output);
		output.Inputs.Add(this);
	}

	public void Consume()
	{
		if (ProcessTime == 0)
		{
			CanProcess = Inputs.TrueForAll(input => input.CanDecrement());
			if (CanProcess)
			{
				Inputs.ForEach(input => {input.Decrement();
				Console.WriteLine(input.Name);});
			}
		}
	}


	public void Process()
	{
		if (ProcessTime > 0 || CanProcess) ProcessTime++;
		if (ProcessTime == ProcessDuration)
		{
			Outputs.ForEach(output => { output.Increment(Name);
			Console.WriteLine(output.Name); });
			
			ProcessTime = 0;
		}
		CanProcess = false;
	}

	public bool IsActive { get => ProcessTime > 0 || CanProcess; }

}