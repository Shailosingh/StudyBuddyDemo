using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.UI.Xaml.Controls;

namespace StudyBuddyDemo
{
    public class StudySession
    {
        //Datafields
        public bool IsInFocusMode { get; set; }
        public bool StudyThreadRunning { get; set; }
        public Thread StudyThread { get; set; }
        public Stopwatch Timer { get; set; }
        public ContentDialogResult DialogResult { get; set; }

        //Constructor
        public StudySession()
        {
            IsInFocusMode = true;
            StudyThreadRunning = false;
            Timer = new Stopwatch();
            DialogResult = ContentDialogResult.None;
        }
    }
}
