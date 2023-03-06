namespace Fohhn.FohhnNet.Demo.Xpanels.Pages
{
    public class Page
    {
        protected readonly PanelBase Panel;
        private readonly uint _join;

        public Page(PanelBase panel, uint join)
        {
            Panel = panel;
            _join = join;
        }

        public void Show()
        {
            Panel.SetDigital(_join, true);
        }
        public void Hide()
        {
            Panel.SetDigital(_join, false);
        }
    }
}