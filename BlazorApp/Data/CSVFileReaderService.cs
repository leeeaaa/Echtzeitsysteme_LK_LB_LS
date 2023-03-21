using System.IO;
using System.Runtime.InteropServices.ComTypes;
using ExcelDataReader;
using System.Text;
using Microsoft.AspNetCore.Components.Forms;
using System.Xml.Linq;

namespace BlazorApp.Data;

public struct DiagramData{

	public DiagramData()
	{
		Tasks = new();
		Semaphores = new();
		Mutexes = new();
		CsvData = new();
	}

	public List<Task> Tasks { get; set; }
	public List<Semaphore> Semaphores { get; set; }
	public List<Mutex> Mutexes { get; set; }
	public List<List<string>> CsvData { get; set; }
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



		elements.FindAll(element => element.First() == "Task").ForEach(taskList => diagramData.Tasks.Add(CreateTaskFromList(taskList)));
		elements.FindAll(element => element.First() == "Activity").ForEach(activityList =>
		{
			var activity = CreateActivityCsvData(activityList);
			diagramData.Tasks.Find(task => task.Name == activityList[1])?.AddActivity(activity);
			activities.Add(activity);
		});

		elements.FindAll(element => element.First() == "Semaphore").ForEach(semaphoreList =>
		{
			var existingSemaphore = diagramData.Semaphores.Find(semaphore => semaphore.Name == semaphoreList[1]);
			if (existingSemaphore is null)
			{
				existingSemaphore = CreateSemaphoreFromCsvData(semaphoreList);
				diagramData.Semaphores.Add(existingSemaphore);
			}
			else
			{
				existingSemaphore.IncrementNumberInputs();
			}

			var outputActivity = activities.Find(activity => activity.Name == semaphoreList[3]);
			if(outputActivity?.Outputs.Find(output => output.Name == existingSemaphore.Name) is null) outputActivity?.AddOutput(existingSemaphore);

			var inputActivity = activities.Find(activity => activity.Name == semaphoreList[4]);
			if(inputActivity?.Inputs.Find(input => input.Name == existingSemaphore.Name) is null) inputActivity?.AddInput(existingSemaphore);

			if (inputActivity != null && outputActivity != null)
			{
				if (diagramData.Tasks.Find(task =>
					    task.Activities.Contains(inputActivity) && task.Activities.Contains(outputActivity)) !=
				    null) existingSemaphore.SetActivitySemaphore();
			}
		});

		elements.FindAll(element => element.First() == "Mutex").ForEach(mutexList =>
		{
			var mutex = CreateMutexFromCsvData(mutexList);
			diagramData.Mutexes.Add(mutex);

			for (int i = 2; i < mutexList.Count; i++)
			{
				var activity = activities.Find(activity => activity.Name == mutexList[i]);
				activity?.AddInput(mutex);
				activity?.AddOutput(mutex);
			}
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

	private static Activity CreateActivityCsvData(List<string> activityItems)
		=> new Activity(Int32.Parse(activityItems[3]), activityItems[2]);
	private static Task CreateTaskFromList(List<string> taskItems)
		=> new Task(taskItems[1]);

	private static Semaphore CreateSemaphoreFromCsvData(List<string> semaphoreItems)
		=> new Semaphore(Int32.Parse(semaphoreItems[2]), semaphoreItems[1]);

	private static Mutex CreateMutexFromCsvData(List<string> mutexItems)
		=> new Mutex(mutexItems[1]);
}