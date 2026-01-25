namespace UnoGame.Core.DTO.Drawing;

public class PublicDrawResult
{
    public int Requested { get; set; }
    public int Drawn { get; set; }
    public bool Completed => Drawn == Requested;

    public int? ReshuffleIndex { get; set; }
}