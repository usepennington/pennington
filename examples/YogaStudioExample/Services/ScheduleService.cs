namespace YogaStudioExample.Services;

using System.Text.Json;
using YogaStudioExample.Models;

public sealed class ScheduleService
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
    };

    private readonly Lazy<ScheduleData> _data;

    public ScheduleService(IWebHostEnvironment env)
    {
        _data = new Lazy<ScheduleData>(() =>
        {
            var path = Path.Combine(env.ContentRootPath, "Data", "schedule.json");
            var json = File.ReadAllText(path);
            return JsonSerializer.Deserialize<ScheduleData>(json, JsonOptions)
                   ?? new ScheduleData([]);
        });
    }

    public List<ScheduleEntry> GetAllClasses() => _data.Value.Classes;

    public List<ScheduleEntry> GetClassesForDay(string day) =>
        _data.Value.Classes.Where(c => c.DayOfWeek.Equals(day, StringComparison.OrdinalIgnoreCase)).ToList();

    public ScheduleEntry? GetClassById(string id) =>
        _data.Value.Classes.FirstOrDefault(c => c.Id == id);

    public List<ScheduleEntry> GetClassesByInstructor(string instructorId) =>
        _data.Value.Classes.Where(c => c.InstructorId == instructorId).ToList();

    public List<string> GetClassTypes() =>
        _data.Value.Classes.Select(c => c.ClassType).Distinct().OrderBy(t => t).ToList();

    public List<string> GetLevels() =>
        _data.Value.Classes.Select(c => c.Level).Distinct().OrderBy(l => l).ToList();

    private static readonly string[] DayOrder =
        ["Monday", "Tuesday", "Wednesday", "Thursday", "Friday", "Saturday", "Sunday"];

    public List<(string Day, List<ScheduleEntry> Classes)> GetWeeklySchedule() =>
        DayOrder
            .Select(day => (day, GetClassesForDay(day).OrderBy(c => c.StartTime).ToList()))
            .Where(g => g.Item2.Count > 0)
            .ToList();
}
