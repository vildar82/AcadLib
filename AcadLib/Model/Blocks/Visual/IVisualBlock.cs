namespace AcadLib.Blocks.Visual
{
    using System.Windows.Input;
    using System.Windows.Media;
    using JetBrains.Annotations;

    [PublicAPI]
    public interface IVisualBlock
    {
        string File { get; set; }

        string Group { get; set; }

        ImageSource Image { get; set; }

        string Name { get; set; }

        ICommand Redefine { get; set; }
    }
}