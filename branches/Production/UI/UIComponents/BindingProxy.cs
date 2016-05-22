﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Trinity.UIComponents
{
    /// <summary>
    /// Add Proxy <ut:BindingProxy x:Key="Proxy" Data="{Binding}" /> to Resources
    /// Bind like <Element Property="{Binding Data.MyValue, Source={StaticResource Proxy}}" />   
    /// </summary>
    public class BindingProxy : Freezable
    {
        #region Overrides of Freezable

        protected override Freezable CreateInstanceCore()
        {
            return new BindingProxy();
        }

        #endregion

        public object Data
        {
            get { return (object)GetValue(DataProperty); }
            set { SetValue(DataProperty, value); }
        }

        public static readonly DependencyProperty DataProperty =
         DependencyProperty.Register("Data", typeof(object), typeof(BindingProxy));
    }
}
