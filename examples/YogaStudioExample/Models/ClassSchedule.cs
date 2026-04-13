namespace YogaStudioExample.Models;

public record ScheduleEntry(
    string Id,
    string ClassName,
    string InstructorId,
    string DayOfWeek,
    string StartTime,
    string EndTime,
    string ClassType,
    string Level,
    string Description,
    string Room,
    int MaxCapacity);

public record ScheduleData(List<ScheduleEntry> Classes);