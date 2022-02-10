using SpinCore.Handlers;
using SpinCore.Handlers.UI;
using SMU.Utilities;
using System;

namespace SRXDModifiers
{
    class ContextMenu
    {

        public static CustomContextMenu modifiersContextMenu;

        public static CustomDropDown speedDropdown;
        public static CustomCheckbox autoPlay;
        public static CustomCheckbox survivalMode;

        public static Bindable<CustomDropDown.DropDownOption> dropDownBindables = new Bindable<CustomDropDown.DropDownOption>();
        public static Bindable<bool> autoPlayBindable = new Bindable<bool>();
        public static Bindable<bool> survivalModeBindable = new Bindable<bool>();

        public static void Setup()
        {

            InstanceHandler.OnCustomLevelsOpen += delegate
            {

                modifiersContextMenu = new CustomContextMenu("Modifiers", InstanceHandler.XDCustomLevelSelectMenuInstance, 10);

                string[] options = { "x0.8", "x1.0", "x1.25", "x1.5" };

                dropDownBindables.Bind(onDropdownSelection());
                autoPlayBindable.Bind(onAutoPlayCheckboxChange());
                survivalModeBindable.Bind(onSurvivalModeCheckboxChange());



                speedDropdown = new CustomDropDown("Playback Speed", dropDownBindables, modifiersContextMenu, options, 150);
                autoPlay = new CustomCheckbox("AutoPlay", autoPlayBindable, modifiersContextMenu, 260);
                survivalMode = new CustomCheckbox("Survival Mode", survivalModeBindable, modifiersContextMenu, 260);

            };

        }

        public static void openMenu()
        {
            if(!modifiersContextMenu.isOpen && modifiersContextMenu != null)
            {
                modifiersContextMenu.OpenMenu();
            }
            else
            {
                modifiersContextMenu.CloseMenu();
            }
        }

        public static Action<CustomDropDown.DropDownOption> onDropdownSelection() => delegate 
        {

            SRXDModifiers.PlaybackSpeed.updatePlaybackSpeed(Convert.ToDouble(dropDownBindables.Value.OptionName.Substring(1)));

        };
        public static Action<bool> onAutoPlayCheckboxChange() => delegate
        {

            SRXDModifiers.AutoPlay.enabled = autoPlayBindable.Value;

        };

        public static Action<bool> onSurvivalModeCheckboxChange() => delegate
        {

            SRXDModifiers.SurvivalMode.enabled = survivalModeBindable.Value;

        };

    }
}
