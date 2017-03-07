using Zeta.Bot.Navigation;

namespace QuestTools.Navigation
{
    public class GridProvider
    {
        public static MainGridProvider MainGridProvider { get { return (MainGridProvider)Navigator.SearchGridProvider; } }
    }
}
