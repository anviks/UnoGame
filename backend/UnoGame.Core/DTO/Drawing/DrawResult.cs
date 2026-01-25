namespace UnoGame.Core.DTO.Drawing;

public class DrawResult
{
    public int Requested { get; set; }
    public int Drawn => DrawnCards.Count;
    public bool Completed => Drawn == Requested;

    public List<DrawnCard> DrawnCards { get; set; } = [];

    public int? ReshuffleIndex { get; set; }
}