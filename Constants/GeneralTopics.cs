namespace VideoGenerator.AllConstants;

public static partial class Constants
{
    private static readonly List<string> _generalTopics =
        [
            "Health and medicine",
            "Travel and tourism",
            "Education and self - improvement",
            "Technology and science",
            "Arts and culture",
            "Finance and investments",
            "Entertainment and gaming",
            "Fashion and style",
            "Pets and animals",
            "Food and culinary",
            "Psychology and psychotherapy",
            "Sports and fitness",
            "Home improvement and construction",
            "Music and movies",
            "Design and architecture",
            "Software engineering and computer science",
            "Politics"
        ];

    public static IReadOnlyCollection<string> GeneralTopics => _generalTopics;
}