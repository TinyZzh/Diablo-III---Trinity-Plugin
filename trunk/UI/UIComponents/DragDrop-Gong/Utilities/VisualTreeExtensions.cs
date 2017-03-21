﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Media3D;

namespace Trinity.UI.UIComponents.Utilities
{
    public static class VisualTreeExtensions
  {
    /// <summary>
    /// Gets the next ancestor element which is a drop target.
    /// </summary>
    /// <param name="element">The start element.</param>
    /// <returns>The first element which is a drop target.</returns>
    public static UIElement TryGetNextAncestorDropTargetElement(this UIElement element)
    {
      if (element == null)
      {
        return null;
      }
      var ancestor = element.GetVisualAncestor<UIElement>();
      while (ancestor != null)
      {
        if (ancestor.IsDropTarget())
        {
          return ancestor;
        }
        ancestor = ancestor.GetVisualAncestor<UIElement>();
      }
      return null;
    }

    internal static DependencyObject FindVisualTreeRoot(this DependencyObject d)
    {
      var current = d;
      var result = d;

      while (current != null) {
        result = current;
        if (current is Visual || current is Visual3D) {
          break;
        } else {
          // If we're in Logical Land then we must walk 
          // up the logical tree until we find a 
          // Visual/Visual3D to get us back to Visual Land.
          current = LogicalTreeHelper.GetParent(current);
        }
      }

      return result;
    }

    public static T GetVisualAncestor<T>(this DependencyObject d) where T : class
    {
      var item = VisualTreeHelper.GetParent(d.FindVisualTreeRoot());

      while (item != null) {
        var itemAsT = item as T;
        if (itemAsT != null) {
          return itemAsT;
        }
        item = VisualTreeHelper.GetParent(item);
      }

      return null;
    }

    public static DependencyObject GetVisualAncestor(this DependencyObject d, Type type)
    {
      var item = VisualTreeHelper.GetParent(d.FindVisualTreeRoot());

      while (item != null && type != null) {
        if (item.GetType() == type || item.GetType().IsSubclassOf(type)) {
          return item;
        }
        item = VisualTreeHelper.GetParent(item);
      }

      return null;
    }

    /// <summary>
    /// find the visual ancestor by type and go through the visual tree until the given itemsControl will be found
    /// </summary>
    public static DependencyObject GetVisualAncestor(this DependencyObject d, Type type, ItemsControl itemsControl)
    {
      var item = VisualTreeHelper.GetParent(d.FindVisualTreeRoot());
      DependencyObject lastFoundItemByType = null;

      while (item != null && type != null) {
        if (item == itemsControl) {
          return lastFoundItemByType;
        }
        if ((item.GetType() == type || item.GetType().IsSubclassOf(type))
            && (itemsControl == null || itemsControl.ItemContainerGenerator.IndexFromContainer(item) != -1)) {
          lastFoundItemByType = item;
        }
        item = VisualTreeHelper.GetParent(item);
      }

      return lastFoundItemByType;
    }

    public static T GetVisualDescendent<T>(this DependencyObject d) where T : DependencyObject
    {
      return d.GetVisualDescendents<T>().FirstOrDefault();
    }

    public static IEnumerable<T> GetVisualDescendents<T>(this DependencyObject d) where T : DependencyObject
    {
      var childCount = VisualTreeHelper.GetChildrenCount(d);

      for (var n = 0; n < childCount; n++) {
        var child = VisualTreeHelper.GetChild(d, n);

        if (child is T) {
          yield return (T)child;
        }

        foreach (var match in GetVisualDescendents<T>(child)) {
          yield return match;
        }
      }

      yield break;
    }
  }
}
