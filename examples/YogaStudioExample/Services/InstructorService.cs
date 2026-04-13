namespace YogaStudioExample.Services;

using System.Text.Json;
using Models;

public sealed class InstructorService
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
    };

    private readonly Lazy<InstructorData> _data;

    public InstructorService(IWebHostEnvironment env)
    {
        _data = new Lazy<InstructorData>(() =>
        {
            var path = Path.Combine(env.ContentRootPath, "Data", "instructors.json");
            var json = File.ReadAllText(path);
            return JsonSerializer.Deserialize<InstructorData>(json, JsonOptions)
                   ?? new InstructorData([]);
        });
    }

    public List<InstructorProfile> GetAll() => _data.Value.Instructors;

    public InstructorProfile? GetBySlug(string slug) =>
        _data.Value.Instructors.FirstOrDefault(i => i.Slug.Equals(slug, StringComparison.OrdinalIgnoreCase));

    public InstructorProfile? GetById(string id) =>
        _data.Value.Instructors.FirstOrDefault(i => i.Id == id);
}