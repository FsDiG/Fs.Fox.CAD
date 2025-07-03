using Autodesk.AutoCAD.Customization;

namespace ThCADExtension
{
    public static class CustomizationExtensions
    {
        public static RibbonTabSource AddNewTab(
            this CustomizationSection instance,
            string name,
            string text = null)
        {
            if (text == null)
                text = name;

            var ribbonRoot            = instance.MenuGroup.RibbonRoot;
            var id                    = "tab" + name;

            RibbonTabSource ribbonTabSource = null;
            if (ribbonRoot.FindTab(id) == null)
            {
                ribbonTabSource = new RibbonTabSource(ribbonRoot)
                {
                    Id = id,
                    Name = name,
                    Text = text,
                    ElementID = id
                };

                ribbonRoot.RibbonTabSources.Add(ribbonTabSource);
            }
            return ribbonTabSource;
        }

        public static RibbonPanelSource AddNewPanel(
            this RibbonTabSource instance,
            string name,
            string text = null)
        {
            if (text == null)
                text = name;

            var ribbonRoot              = instance.CustomizationSection.MenuGroup.RibbonRoot;
            var panels                  = ribbonRoot.RibbonPanelSources;
            var id                      = "pnl" + name;
            var ribbonPanelSource       = new RibbonPanelSource(ribbonRoot);

            ribbonPanelSource.Name      = name;
            ribbonPanelSource.Text      = text;
            ribbonPanelSource.Id        = id;
            ribbonPanelSource.ElementID = id;

            panels.Add(ribbonPanelSource);

            var ribbonPanelSourceReference     = new RibbonPanelSourceReference(instance);

            ribbonPanelSourceReference.PanelId = ribbonPanelSource.ElementID;

            instance.Items.Add(ribbonPanelSourceReference);

            return ribbonPanelSource;
        }

        public static RibbonRow AddNewRibbonRow(this RibbonPanelSource instance)
        {
            var row = new RibbonRow(instance);

            instance.Items.Add(row);

            return row;
        }

        public static RibbonRow AddNewRibbonRow(this RibbonRowPanel instance)
        {
            var row = new RibbonRow(instance);

            instance.Items.Add(row);

            return row;
        }

        public static RibbonRowPanel AddNewPanel(this RibbonRow instance)
        {
            var row = new RibbonRowPanel(instance);

            instance.Items.Add(row);

            return row;
        }

        public static RibbonCommandButton AddNewButton(
            this RibbonRow instance,
            string text,
            string commandFriendlyName,
            string command,
            string commandDescription,
            string smallImagePath,
            string largeImagePath,
            RibbonButtonStyle style)
        {
            var button = NewButton(instance,
                                   text,
                                   commandFriendlyName,
                                   command, commandDescription,
                                   smallImagePath,
                                   largeImagePath,
                                   style);

            instance.Items.Add(button);

            return button;
        }

        public static RibbonCommandButton AddNewButton(
            this RibbonSplitButton instance,
            string text,
            string commandFriendlyName,
            string command,
            string commandDescription,
            string smallImagePath,
            string largeImagePath,
            RibbonButtonStyle style)
        {
            var button = NewButton(instance,
                                   text,
                                   commandFriendlyName,
                                   command, commandDescription,
                                   smallImagePath,
                                   largeImagePath,
                                   style);

            instance.Items.Add(button);

            return button;
        }

        public static RibbonSplitButton AddNewSplitButton(this RibbonRow                  instance,
                                                               string                     text,
                                                               RibbonSplitButtonBehavior  behavior,
                                                               RibbonSplitButtonListStyle listStyle,
                                                               RibbonButtonStyle          style)
        {
            var button         = new RibbonSplitButton(instance);

            button.Text        = text;
            button.Behavior    = behavior;
            button.ListStyle   = listStyle;
            button.ButtonStyle = style;

            instance.Items.Add(button);

            return button;
        }

        public static RibbonSeparator AddNewSeparator(this RibbonRow instance,
                                                           RibbonSeparatorStyle style = RibbonSeparatorStyle.Line)
        {
            var separator = new RibbonSeparator(instance);

            separator.SeparatorStyle = style;

            instance.Items.Add(separator);

            return separator;
        }

        public static RibbonSeparator AddNewSeparator(this RibbonSplitButton    instance,
                                                           RibbonSeparatorStyle style = RibbonSeparatorStyle.Line)
        {
            var separator = new RibbonSeparator(instance);

            separator.SeparatorStyle = style;

            instance.Items.Add(separator);

            return separator;
        }

        private static RibbonCommandButton NewButton(RibbonItem        parent,
                                                     string            text,
                                                     string            commandFriendlyName,
                                                     string            command,
                                                     string            commandDescription,
                                                     string            smallImagePath,
                                                     string            largeImagePath,
                                                     RibbonButtonStyle style)
        {
            var customizationSection = parent.CustomizationSection;
            var macroGroups          = customizationSection.MenuGroup.MacroGroups;

            MacroGroup macroGroup;

            if (macroGroups.Count == 0)
                macroGroup = new MacroGroup("MacroGroup", customizationSection.MenuGroup);
            else
                macroGroup = macroGroups[0];

            var button          = new RibbonCommandButton(parent);
            button.Text         = text;

            var commandMacro    = "^C^C_" + command;
            var commandId       = "ID_"   + command;
            var buttonId        = "btn"   + command;
            var labelId         = "lbl"   + command;

            var menuMacro       = macroGroup.CreateMenuMacro(commandFriendlyName,
                                                             commandMacro,
                                                             commandId,
                                                             commandDescription,
                                                             MacroType.Any,
                                                             smallImagePath,
                                                             largeImagePath,
                                                             labelId);
            var macro           = menuMacro.macro;

            macro.CLICommand    = command;

            button.MacroID      = menuMacro.ElementID;
            button.ButtonStyle  = style;

            return button;
        }
    }
}
