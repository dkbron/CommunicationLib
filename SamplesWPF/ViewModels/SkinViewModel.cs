﻿using MaterialDesignColors;
using MaterialDesignColors.ColorManipulation;
using MaterialDesignThemes.Wpf;
using Prism.Commands;
using Prism.Mvvm;
using System;
using System.Collections.Generic; 
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace SamplesWPF.ViewModels
{
    public class SkinViewModel : BindableBase
    {
        private readonly PaletteHelper paletteHelper = new PaletteHelper();  

        private bool _isDarkTheme;
        public bool IsDarkTheme
        {
            get => _isDarkTheme;
            set
            {
                if (SetProperty(ref _isDarkTheme, value))
                {
                    ModifyTheme(theme => theme.SetBaseTheme(value ? Theme.Dark : Theme.Light)); 
                }
                RaisePropertyChanged();
            }
        }  

        public SkinViewModel()
        { 
            ITheme theme = paletteHelper.GetTheme();
            ChangeHueCommand = new DelegateCommand<object>(ChangeHue);
            IsDarkTheme = theme.GetBaseTheme() == BaseTheme.Dark;
        }  


        public IEnumerable<ISwatch> Swatches { get; } = SwatchHelper.Swatches;

        public DelegateCommand<object> ChangeHueCommand { get; }
         
         
        private void ChangeHue(object? obj)
        {
            var hue = (Color)obj!;

            ITheme theme = paletteHelper.GetTheme();

            theme.PrimaryLight = new ColorPair(hue.Lighten());
            theme.PrimaryMid = new ColorPair(hue);
            theme.PrimaryDark = new ColorPair(hue.Darken());

            paletteHelper.SetTheme(theme);
        }

        private static void ModifyTheme(Action<ITheme> modificationAction)
        {
            var paletteHelper = new PaletteHelper();
            ITheme theme = paletteHelper.GetTheme();

            modificationAction?.Invoke(theme); 
            paletteHelper.SetTheme(theme);
        }
    }
}
