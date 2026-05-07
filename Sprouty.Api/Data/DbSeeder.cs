using Sprouty.Api.Models;

namespace Sprouty.Api.Data;

public static class DbSeeder
{
    public static void Seed(AppDbContext db)
    {
        if (!db.Institutions.Any())
        {
            db.Institutions.AddRange(
                new Institution
                {
                    Name = "Harmony Music Academy",
                    Category = "Music",
                    Description = "Premier music academy nurturing talent across genres.",
                    Location = "Chennai, India",
                    Courses = "[\"Vocals\",\"Guitar\",\"Piano\",\"Drums\"]",
                    Achievements = "[\"100+ stage performers\",\"5 international scholarships\"]",
                    SuccessStories = "[\"Aakash won National Idol 2023\"]"
                },
                new Institution
                {
                    Name = "Canvas Art Studio",
                    Category = "Art",
                    Description = "Where every brushstroke tells a story.",
                    Location = "Kuzhithurai, India",
                    Courses = "[\"Watercolor\",\"Oil Painting\",\"Sketching\"]",
                    Achievements = "[\"50+ exhibitions hosted\"]"
                },
                new Institution
                {
                    Name = "Rhythm Dance Hub",
                    Category = "Dance",
                    Description = "Move to your own rhythm.",
                    Location = "Bengaluru, India",
                    Courses = "[\"Classical\",\"Hip-Hop\",\"Contemporary\"]"
                },
                new Institution
                {
                    Name = "CodeCraft Academy",
                    Category = "Code",
                    Description = "Building tomorrow's developers today.",
                    Location = "Hyderabad, India",
                    Courses = "[\"React\",\"C#\",\"Python\",\"Cloud\"]",
                    Achievements = "[\"200+ alumni placed\"]"
                },
                new Institution
                {
                    Name = "Lens Photography Institute",
                    Category = "Photography",
                    Description = "Capture moments. Create memories.",
                    Location = "Mumbai, India",
                    Courses = "[\"DSLR Basics\",\"Editing\",\"Portrait\"]"
                }
            );
            db.SaveChanges();
        }

        if (!db.Users.Any(u => u.Email == "team@sprouty.app"))
        {
            var sprouty = new User
            {
                Name = "Sprouty Team",
                Email = "team@sprouty.app",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("SproutyTeam!2026"),
                Headline = "Inspiring creators every day 🌱",
                Categories = "Art,Music,Dance,Code,Photography,Writing,Fitness",
                Location = "Sprouty HQ"
            };
            db.Users.Add(sprouty);
            db.SaveChanges();

            db.Posts.AddRange(
                new Post
                {
                    UserId = sprouty.Id,
                    Content = "Trending in Music 🎵 — Practice 10 minutes daily and watch the magic unfold.",
                    Category = "Music",
                    MediaType = "none",
                    ApplaudCount = 24
                },
                new Post
                {
                    UserId = sprouty.Id,
                    Content = "Inspiration ✨ — Every expert was once a beginner. Start your 21-day challenge today!",
                    Category = "Art",
                    MediaType = "none",
                    ApplaudCount = 41
                }
            );
            db.SaveChanges();
        }
    }
}
