﻿using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Windows.Automation;

namespace PurpleLib
{
    public class PurplePath
    {
        //This class is used to build and interpret purplepaths to find AutomationElements
        //These constants should be stored in the app.config
        private const String DELIMITER = "//";
        private const String BLANK = "!BLANK!";
        private readonly String _blankValue = BLANK;
        private readonly String _delimiter = DELIMITER;

        public PurplePath(String delimiter = DELIMITER, String blank = BLANK)
        {
            _delimiter = ConfigurationManager.AppSettings[DELIMITER];
            if (_delimiter == null)
            {
                _delimiter = delimiter;
            }
            _blankValue = ConfigurationManager.AppSettings[BLANK];
            if (_blankValue == null)
            {
                _blankValue = blank;
            }
        }

        public String getPurplePath(AutomationElement element)
        {
            //I was curious how UI automation would handle having more than one panel with a blank name from Inspect.exe when building this path
            //It was surpriseing to know that the TreeWalker handles that for us when we walk up the tree, and conversly down the tree.
            //TODO: handle title bars like LQP where the title name changes based on the file opened
            TreeWalker walker = TreeWalker.RawViewWalker;
            bool parentExists = true; //need to assume that there's a parent
            String path = element.Current.Name + _delimiter;
            AutomationElement parent;
            AutomationElement node = element;
            String purplePath = "";

            while (parentExists)
            {
                parent = walker.GetParent(node);
                if (parent != null)
                {
                    string parentName = parent.Current.Name;
                    if (parentName == "")
                    {
                        //We need to put something for blanks
                        parentName = _blankValue;
                    }
                    path += parentName + _delimiter;
                    node = parent;
                }
                else
                {
                    parentExists = false;
                }
            }
            //now try to build the path in the proper order
            string[] pathStrings = path.Split(_delimiter.ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
            Array.Reverse(pathStrings);
                //Reverse the order of the strings in the array so elements appear in top to bottom
            //this trims off the first level, since that's the root element or Desktop
            for (int x = 1; x < pathStrings.Count(); x++)
            {
                purplePath += _delimiter + pathStrings[x];
            }
            return purplePath;
        }

        public AutomationElement FindElement(String purplePath)
        {
            
            //This function will return a AutomationElement based on purple path provided
            var pathStrings = new List<string>(purplePath.Split(_delimiter.ToCharArray(), StringSplitOptions.RemoveEmptyEntries));

            AutomationElement element = AutomationElement.RootElement;
            TreeWalker walker = TreeWalker.RawViewWalker;
            AutomationElement node = element;
            Condition findCondition;

            for (int i = 0; i < pathStrings.Count(); i++)
            {
                if (pathStrings[i].Equals(_blankValue))
                {
                    pathStrings[i] = "";
                }
                findCondition = new PropertyCondition(AutomationElement.NameProperty, pathStrings[i],
                    PropertyConditionFlags.IgnoreCase);
               
                node = element.FindFirst(TreeScope.Children, findCondition);
                var nodename = node.Current.Name;
                if (node != null)
                {
                    element = node;
                }
            }
            return node;
        }

        public String getDelimiter()
        {
            return _delimiter;
        }
    }
}