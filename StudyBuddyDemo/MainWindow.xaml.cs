/*
------------------------------------------------------------------------------------------
Project: Study Buddy Demo
Purpose: A demo of the Study Buddy program.
==========================================================================================
Program Description:
Study Buddy is a focus-based productivity tool that incorporates a minigame to improve the 
user’s engagement with their own study plan. Its purpose is to help users who struggle with 
staying on task by introducing a rewards system they can become invested in. In academia in 
particular, bad grades are a major source of negative reinforcement. The main source of 
positive reinforcement is gaining skills and experience to help the student in their later 
career. However, these are usually not immediately noticeable or useful, so most of a student’s 
motivation comes from the fear of poor and failing grades on assessments. Our aim with this 
project is to introduce a source of positive reinforcement for users who need more of it in 
order to succeed in their studies.
------------------------------------------------------------------------------------------
Author:  Shailendra Singh, Adam Scott, Riley Huston, Christine Nguyen, Terry Tran, 
Tatiana Olenciuc, Alex Lau
Version  2022-04-05
------------------------------------------------------------------------------------------
*/

using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using System;
using System.Diagnostics;
using System.Threading;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using PInvoke;
using System.Text;
using Newtonsoft.Json;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace StudyBuddyDemo
{
    /// <summary>
    /// An empty window that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainWindow : Window
    {
        //Datafield
        private static int WIDTH = 1280;
        private static int HEIGHT = 720;
        private StudySession StudyState;
        private Pet PetState;

        //Window setting variables
        private OverlappedPresenter _presenter;

        public MainWindow()
        {
            //Check the user's roaming data and check if it has the folder for Study Buddy
            string userPath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            string studyBuddySavesPath = Path.Combine(userPath, @"Study Buddy Saves");

            //If it isn't there, make the folder
            bool directoryExists = Directory.Exists(studyBuddySavesPath);
            if (!directoryExists)
            {
                Directory.CreateDirectory(studyBuddySavesPath);
            }

            //Check if the folder for day records exist
            string dateRecordSavePaths = Path.Combine(studyBuddySavesPath, @"Date Records");
            directoryExists = Directory.Exists(dateRecordSavePaths);
            if(!directoryExists)
            {
                Directory.CreateDirectory(dateRecordSavePaths);
            }

            //Set the size of window taking into account DPI (https://stackoverflow.com/questions/67169712/winui-3-0-reunion-0-5-window-size)
            IntPtr hWnd = WinRT.Interop.WindowNative.GetWindowHandle(this);
            var windowId = Microsoft.UI.Win32Interop.GetWindowIdFromWindow(hWnd);
            var appWindow = Microsoft.UI.Windowing.AppWindow.GetFromWindowId(windowId);
            double dpi = (double)User32.GetDpiForWindow(hWnd);
            double scaling = dpi / 96;
            appWindow.Resize(new Windows.Graphics.SizeInt32 { Width = (int)(WIDTH * scaling), Height = (int)(HEIGHT * scaling) });

            //Fix the size of the window (https://github.com/microsoft/WindowsAppSDK/discussions/1694)
            _presenter = appWindow.Presenter as OverlappedPresenter;
            _presenter.IsResizable = false;
            _presenter.IsMaximizable = false;

            //Set the title of app
            Title = $"Study Buddy Demo";

            //Initialize the study session
            StudyState = new StudySession();

            //Load the Pet object
            PetState = new Pet();

            //Display window
            this.InitializeComponent();
            StatusReport.Text = PetState.ToString();
        }

        //Methods------------------------------------------------------------------------------------------------------------------
        /// <summary>
        /// This function will retrieve the list of processes blacklisted
        /// by the user, put each name in an array of strings and return
        /// it.
        /// </summary>
        /// <returns>An array of the names of the blacklisted processes</returns>
        private string[] GetBlacklistedProcesses()
        {
            string[] blacklistedProcesses = { "chrome", "Discord", "Spotify" };
            return blacklistedProcesses;
        }

        /// <summary>
        /// Choose which study thread to start and kick start it.
        /// </summary>
        public void StartStudying()
        {
            //Get the mode for the studying session
            StudyState.IsInFocusMode = FocusSelect.IsOn;

            //Start the timer
            StudyState.Timer.Restart();

            //Focus thread
            if (StudyState.IsInFocusMode)
            {
                //Initialize the study thread
                StudyState.StudyThread = new Thread(FocusModeThread);

                //Give user message
                StatusReport.Text = "Focus Mode Engaged! It is time to work both hard and smart. Good luck...";
            }

            //Casual mode
            else
            {
                //Initialize the study thread
                StudyState.StudyThread = new Thread(CasualModeThread);

                //Give user message
                StatusReport.Text = "Casual Mode Engaged! Distractions:\n";
            }

            //Start the study thread
            StudyState.StudyThreadRunning = true;
            StudyState.StudyThread.Start();

            //Setup UI for studying
            FocusSelect.IsEnabled = false;
            StudyButton.Content = "Stop Studying";
        }

        /// <summary>
        /// The thread which manages the focus mode
        /// </summary>
        private void FocusModeThread()
        {
            //Initialize variables
            Process[] processKillList;
            string[] blacklist = GetBlacklistedProcesses();

            //Study loop
            while (StudyState.StudyThreadRunning)
            {
                //Go through every process in the blacklist and search for open processes
                foreach (string blacklistedProcess in blacklist)
                {
                    //Get list of processes to potentially be killed
                    processKillList = Process.GetProcessesByName(blacklistedProcess);

                    //Check if there are any processes to kill, a dialog box is not open and that the study thread is still running
                    if (processKillList.Length != 0)
                    {
                        //Make window stay on top
                        _presenter.IsAlwaysOnTop = true;

                        //Pause the timer
                        StudyState.Timer.Stop();

                        //Bring up the dialog box to ask the user if they wish to continue studying or not
                        StudyState.DialogResult = ContentDialogResult.None;
                        this.DispatcherQueue.TryEnqueue(async () =>
                        {
                            //Ask if the user is sure they want to kill the process
                            ContentDialog processKillConfirm = new ContentDialog()
                            {
                                Title = "Distraction Detected",
                                Content = $"Are you sure you want to terminate {blacklistedProcess}?",
                                PrimaryButtonText = "Terminate process and continue studying",
                                SecondaryButtonText = "Stop studying"
                            };

                            processKillConfirm.XamlRoot = this.Content.XamlRoot;
                            StudyState.DialogResult = await processKillConfirm.ShowAsync();
                        });

                        //Wait for someone to select an answer for the dialog box
                        while (StudyState.DialogResult == ContentDialogResult.None) ;

                        //Stop making the window overlayed over everything as the dialog is over
                        _presenter.IsAlwaysOnTop = false;

                        //Resume the timer
                        StudyState.Timer.Start();

                        //User chose to continue studying. Therefore, kill the blacklist proceses
                        if (StudyState.DialogResult == ContentDialogResult.Primary)
                        {
                            //Kill all of the processes
                            foreach (Process process in processKillList)
                            {
                                try
                                {
                                    process.Kill();
                                }

                                catch
                                {

                                }
                            }
                        }

                        //User chose to end study session
                        else if (StudyState.DialogResult == ContentDialogResult.Secondary)
                        {
                            //Record final time studied
                            TimeSpan timeStudied = StudyState.Timer.Elapsed;

                            //Reset the timer
                            StudyState.Timer.Reset();

                            //Give the pet coins earned
                            long coinsEarned = GivePetCoins(timeStudied);
                            
                            //Update the today's record
                            DayRecord todayRecord = new DayRecord();
                            todayRecord.AddTimeStudied(timeStudied);
                            todayRecord.IncrementFunds(coinsEarned);

                            //Signal the study thread to stop
                            StudyState.StudyThreadRunning = false;

                            //Reset UI
                            this.DispatcherQueue.TryEnqueue(() =>
                            {
                                FocusSelect.IsEnabled = true;
                                StudyButton.Content = "Start Studying";
                                StatusReport.Text = $"Total time studying {timeStudied}.\n Earned {coinsEarned} Coins";
                            });

                            //Break out of foreach loop to avoid the thread continuing to try to end processes
                            break;
                        }
                    }
                }
            }
        }

        /// <summary>
        /// The thread which manages the casual mode
        /// </summary>
        private void CasualModeThread()
        {
            //Initialize variables
            Process[] unauthorizedProcesses;
            string[] blacklist = GetBlacklistedProcesses();
            int numberOfUnauthorizedProcessRunning = 0;
            Dictionary<string, bool> processesThatDistractedUser = new Dictionary<string, bool>();

            //Setup dictionary to record which processes distracted the user
            foreach(string blacklistedProcess in blacklist)
            {
                processesThatDistractedUser.Add(blacklistedProcess, false);
            }

            //Study loop
            while(StudyState.StudyThreadRunning)
            {
                //Go through every process in the blacklist and search for open processes
                foreach (string blacklistedProcess in blacklist)
                {
                    //Get list of unauthorized processes
                    unauthorizedProcesses = Process.GetProcessesByName(blacklistedProcess);

                    //If this list isn't empty, mark this process as a process that distracted user and enable distraction timer
                    if (unauthorizedProcesses.Length != 0)
                    {
                        if(!processesThatDistractedUser[blacklistedProcess])
                        {
                            processesThatDistractedUser[blacklistedProcess] = true;
                            
                            this.DispatcherQueue.TryEnqueue(() =>
                            {
                                StatusReport.Text += $"- {blacklistedProcess}\n";
                            });

                            if (numberOfUnauthorizedProcessRunning == 0)
                            {
                                StudyState.DistractedTimer.Start();
                            }

                            numberOfUnauthorizedProcessRunning++;
                        }
                    }

                    else
                    {
                        if (processesThatDistractedUser[blacklistedProcess])
                        {
                            numberOfUnauthorizedProcessRunning--;
                            processesThatDistractedUser[blacklistedProcess] = false;

                            if (numberOfUnauthorizedProcessRunning == 0)
                            {
                                StudyState.DistractedTimer.Stop();
                            }
                        }
                    }
                }
            }
        }

        private long GivePetCoins(TimeSpan timeStudied)
        {
            //Calculate total number of coins
            long coinsEarned = (long)timeStudied.TotalMinutes;

            //Give pet coins
            PetState.UpdateFunds(coinsEarned);

            //Return the new coins earned
            return coinsEarned;
        }

        //Event Handlers-----------------------------------------------------------------------------------------------------------
        private void StudyButton_Click(object sender, RoutedEventArgs e)
        {
            //Check if the user is not already studying
            if (!StudyState.StudyThreadRunning)
            {
                StartStudying();
            }

            //User clicked stop studying button
            else
            {
                if(StudyState.IsInFocusMode)
                {
                    //Record final time studied
                    TimeSpan timeStudied = StudyState.Timer.Elapsed;

                    //Reset the timer
                    StudyState.Timer.Reset();

                    //Give the pet coins earned
                    long coinsEarned = GivePetCoins(timeStudied);

                    //Update the today's record
                    DayRecord todayRecord = new DayRecord();
                    todayRecord.AddTimeStudied(timeStudied);
                    todayRecord.IncrementFunds(coinsEarned);

                    //Stop the study thread
                    StudyState.StudyThreadRunning = false;
                    StudyState.StudyThread.Join();

                    //Reset UI
                    FocusSelect.IsEnabled = true;
                    StudyButton.Content = "Start Studying";
                    string statusReportText = $"Total time studying {timeStudied}.\n" +
                                              $"Earned {coinsEarned} Coins";
                    StatusReport.Text = statusReportText;
                }

                else
                {
                    //Record net time studied
                    TimeSpan timeStudied = StudyState.Timer.Elapsed;
                    TimeSpan distractedTime = StudyState.DistractedTimer.Elapsed;
                    TimeSpan netTimeStudied = timeStudied - distractedTime;

                    //Reset the timer
                    StudyState.Timer.Reset();
                    StudyState.DistractedTimer.Reset();

                    //Give the pet coins earned
                    long coinsEarned = GivePetCoins(netTimeStudied);

                    //Update the today's record
                    DayRecord todayRecord = new DayRecord();
                    todayRecord.AddTimeStudied(timeStudied);
                    todayRecord.IncrementFunds(coinsEarned);

                    //Stop the study thread
                    StudyState.StudyThreadRunning = false;
                    StudyState.StudyThread.Join();

                    //Reset UI
                    FocusSelect.IsEnabled = true;
                    StudyButton.Content = "Start Studying";
                    string statusReportText = $"Total time studying: {timeStudied}\n" +
                                              $"Time Distracted: {distractedTime}\n" +
                                              $"Net Time: {netTimeStudied}\n" +
                                              $"Earned {coinsEarned} Coins";
                    StatusReport.Text = statusReportText;
                }
            }

        }

        private void DateReviewPicker_DateChanged(CalendarDatePicker sender, CalendarDatePickerDateChangedEventArgs args)
        {
            //Get currently selected date offset
            DateTimeOffset? selectedDateOffset = DateReviewPicker.Date;
            
            //Ensure it has a value
            if(selectedDateOffset.HasValue)
            {
                //When it does, convert this value to a DateOnly
                DateOnly selectedDate = DateOnly.FromDateTime(selectedDateOffset.Value.Date);

                //Try to retrieve record for selected date
                string userPath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
                string studyBuddySavesPath = Path.Combine(userPath, @"Study Buddy Saves");
                string dateRecordSavePaths = Path.Combine(studyBuddySavesPath, @"Date Records");
                string dayDatePath = Path.Combine(dateRecordSavePaths, $@"{selectedDate.ToString("MM-dd-yyyy")}.json");

                //If found, output info to status report
                if(File.Exists(dayDatePath))
                {
                    //Deserialize file into object
                    string dayRecordString = File.ReadAllText(dayDatePath);
                    DayRecord dayObject = JsonConvert.DeserializeObject<DayRecord>(dayRecordString);

                    //Output to the StatusReport
                    StatusReport.Text = dayObject.ToString();
                }

                //If not found, inform user
                else
                {
                    StatusReport.Text = $"No information for {selectedDate.ToLongDateString()}";
                }
            }
        }
    }
}
