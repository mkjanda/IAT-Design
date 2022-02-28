namespace IATClient
{
    public interface IAlternationGroupItem
    {
        bool IsInstructionBlock { get; }
        bool IsIATBlock { get; }
        bool IsIATPracticeBlock { get; }
        bool HasAlternateItem { get; }
        bool IsSurvey { get; }
        object Self { get; }
        AlternationGroup AlternationGroup { get; set; }
    }
}
