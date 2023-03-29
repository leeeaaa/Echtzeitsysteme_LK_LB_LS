using System.IO;
using System.Runtime.InteropServices.ComTypes;
using ExcelDataReader;
using System.Text;
using Microsoft.AspNetCore.Components.Forms;
using System.Xml.Linq;
using static System.Runtime.InteropServices.JavaScript.JSType;
using System.Threading.Tasks;
using System.Diagnostics;

namespace BlazorApp.Data;

public struct DiagramData{

	public DiagramData()
	{
		Tasks = new();
		Semaphores = new();
		Mutexes = new();
		CsvData = new();
		Errors = new();
	}

	public List<Task> Tasks { get; set; }
	public List<Semaphore> Semaphores { get; set; }
	public List<Mutex> Mutexes { get; set; }
	public List<List<string>> CsvData { get; set; }
	public List<string> Errors { get; set; }
}

public class CsvFileReaderService
{

	public static System.Threading.Tasks.Task<DiagramData> ReadCsvFileToObjectAsync(System.IO.Stream iStream)
	{
		return System.Threading.Tasks.Task.Run(() =>
		{
			List<List<string>> elements = new();

			char separator = GetSeparator(iStream);

			iStream.Seek(0, SeekOrigin.Begin);
			using (var reader = new StreamReader(iStream))
			{
				while (!reader.EndOfStream)
				{
					var line = reader.ReadLine();
					var values = line?.Split(separator);

					elements.Add(values.ToList<string>());
				}
			}

			return ConvertCsvDataToDiagramData(elements);
		});
	}

	public static System.Threading.Tasks.Task<DiagramData> RestoreInitialState(DiagramData data)
	{
		return System.Threading.Tasks.Task.Run(() => ConvertCsvDataToDiagramData(data.CsvData));
	}

	private static DiagramData ConvertCsvDataToDiagramData(List<List<string>> elements)
	{
		var diagramData = new DiagramData();
		diagramData.CsvData = elements;
		List<Activity> activities = new();


		elements.FindAll(element => element.First() == "Task").ForEach(taskList => diagramData.Tasks.Add(CreateTaskFromList(taskList, diagramData.Errors)));


		elements.FindAll(element => element.First() == "Activity").ForEach(activityList =>
		{
			var activity = CreateActivityCsvData(activityList, diagramData.Errors);
			var task = diagramData.Tasks.Find(task => task.Name == activityList[1]);

			if (task is null)
			{
				diagramData.Errors.Add($"Activity '{activity.Name}' references non existing task '{activityList[1]}'");
			}
			else
			{
				task.AddActivity(activity);
			}

			activities.Add(activity);
		});

		elements.FindAll(element => element.First() == "Semaphore").ForEach(semaphoreList =>
		{
			var existingSemaphore = diagramData.Semaphores.Find(semaphore => semaphore.Name == semaphoreList[1]);
			if (existingSemaphore is null)
			{
				existingSemaphore = CreateSemaphoreFromCsvData(semaphoreList, diagramData.Errors);
				diagramData.Semaphores.Add(existingSemaphore);
			}
			else
			{
				existingSemaphore.IncrementNumberInputs();
			}

			if(semaphoreList.Count <= 3)
				diagramData.Errors.Add($"Semaphore '{existingSemaphore.Name}': Please specify a output activity!");

			else if (semaphoreList.Count <= 4)
				diagramData.Errors.Add($"Semaphore '{existingSemaphore.Name}': Please specify a input activity!");

			else
			{
				var outputActivity = activities.Find(activity => activity.Name == semaphoreList[3]);
				if (outputActivity is null)
				{
					diagramData.Errors.Add($"Semaphore '{existingSemaphore.Name}' references non existing activity '{semaphoreList[3]}'");
				}
				else
				{
					if (outputActivity.Outputs.Find(output => output.Name == existingSemaphore.Name) is null) outputActivity.AddOutput(existingSemaphore);
				}
				var inputActivity = activities.Find(activity => activity.Name == semaphoreList[4]);
				if (inputActivity is null)
				{
					diagramData.Errors.Add($"Semaphore '{existingSemaphore.Name}' references non existing activity '{semaphoreList[4]}'");
				}
				else
				{
					if (inputActivity.Inputs.Find(input => input.Name == existingSemaphore.Name) is null) inputActivity.AddInput(existingSemaphore);
				}

				if (inputActivity != null && outputActivity != null)
				{
					if (diagramData.Tasks.Find(task =>
						    task.Activities.Contains(inputActivity) && task.Activities.Contains(outputActivity)) !=
					    null) existingSemaphore.SetActivitySemaphore();
				}
			}
		});

		elements.FindAll(element => element.First() == "Mutex").ForEach(mutexList =>
		{
			var mutex = CreateMutexFromCsvData(mutexList, diagramData.Errors);
			diagramData.Mutexes.Add(mutex);

			for (int i = 2; i < mutexList.Count; i++)
			{
				var activityName = mutexList[i];
				if (activityName != "")
				{
					var activity = activities.Find(activity => activity.Name == activityName);

					if (activity is null)
					{
						diagramData.Errors.Add($"Mutex '{mutex.Name}' references non existing activity '{mutexList[i]}'!");
					}
					else
					{
						activity.AddInput(mutex);
						activity.AddOutput(mutex);
					}
				}
			}
		});

		if(diagramData.Tasks.Count == 0)
			diagramData.Errors.Add("No diagram to display!");

		diagramData.Tasks.ForEach(task =>
		{
			if(task.Activities.Count == 0)
				diagramData.Errors.Add($"Task '{task.Name}' has no activity!");
		});

		return diagramData;
	}

	private static char GetSeparator(System.IO.Stream iStream)
	{
		char usedSeparator = ',';
		List<char> separators = new List<char>{',', ';'};
		iStream.Seek(0, SeekOrigin.Begin);
		using var reader = new StreamReader(iStream, leaveOpen:true);
		var line1 = reader.ReadLine();
		var line2 = reader.ReadLine();
		foreach (var separator in separators)
		{
			if (line1.Contains(separator))
			{
				if (line1.Split(separator).Length == line2.Split(separator).Length)
				{
					usedSeparator = separator;
					break;
				}
			}
		}

		return usedSeparator;
	}

	private static Activity CreateActivityCsvData(List<string> activityItems, List<string> errors)
	{
		if (activityItems.Count >= 2 && activityItems[2] == "")
			errors.Add("No name specified for activity!");
		var name = activityItems[2];


		var processDuration = 0;
		if (activityItems.Count >= 3 && !Int32.TryParse(activityItems[3], out processDuration))
		{
			errors.Add($"'{activityItems[3]}' is no valid duration for activity '{name}'!");
		}
		else
		{
			if (processDuration == 0)
				errors.Add($"Process duration on activity '{name}' can't be 0!");
		}

		return new Activity(processDuration, name);
	}

	private static Task CreateTaskFromList(List<string> taskItems, List<string> errors)
	{
		var taskName = "";
		if (taskItems.Count >=1 && taskItems[1] == "")
			errors.Add("No name specified for task!");
		else
		{
			taskName = taskItems[1];
		}

		return new Task(taskName);
	}

	private static Semaphore CreateSemaphoreFromCsvData(List<string> semaphoreItems, List<string> errors)
	{
		if (semaphoreItems.Count >= 1 && semaphoreItems[1] == "")
			errors.Add("No name specified for semaphore!");

		var name = semaphoreItems[1];

		var state = 0;
		if (semaphoreItems.Count >= 2 && !Int32.TryParse(semaphoreItems[2], out state))
			errors.Add($"'{semaphoreItems[2]}' is no valid state for semaphore '{name}'!");
		else
		{
			if(state < 0)
				errors.Add($"State of semaphore '{name}' can't be below 0!");
		}
		
		return new Semaphore(state, name);
	}

	private static Mutex CreateMutexFromCsvData(List<string> mutexItems, List<string> errors)
	{
		var name = "";

		if (mutexItems.Count >= 1 && mutexItems[1] == "")
			errors.Add("No name specified for mutex!");
		else
		{
			name = mutexItems[1];
		}

		return new Mutex(name);
	}

}