namespace YogaStudioExample.Models;

public record InstructorProfile(
    string Id,
    string Name,
    string Slug,
    string Title,
    string Bio,
    string[] Specialties,
    string PhotoUrl,
    int YearsExperience,
    string[] Certifications,
    string Quote);

public record InstructorData(List<InstructorProfile> Instructors);
