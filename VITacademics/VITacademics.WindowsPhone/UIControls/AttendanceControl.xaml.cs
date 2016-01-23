using Academics.DataModel;
using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using System.Collections.Generic;

namespace VITacademics.UIControls
{
    public sealed partial class AttendanceControl : UserControl, INotifyPropertyChanged
    {

        public event PropertyChangedEventHandler PropertyChanged;
        private double _attendancePercentage;
        private short _willAttendCount;
        private short _willMissCount;
        private ushort _totalClasses;
        private ushort _attendedClasses;
        private Attendance _attendance;
        private double _targetAttendance = 75;

        public List<KeyValuePair<DateTimeOffset, AttendanceStub>> Details
        {
            get;
            private set;
        }
        public double AttendancePercentage
        {
            get { return Math.Ceiling(_attendancePercentage); }
            set
            {
                _attendancePercentage = value;
                NotifyPropertyChanged();
                NotifyPropertyChanged("OverallStatusString");
                NotifyPropertyChanged("TargetClasses");
            }
        }
        private short WillAttendCount
        {
            get { return _willAttendCount; }
            set
            {
                if (value < 0) return;
                _willAttendCount = value;
                CalculateAttendance();
                NotifyPropertyChanged("WillAttendString");
            }
        }
        private short WillMissCount
        {
            get { return _willMissCount; }
            set
            {
                if (value < 0) return;
                _willMissCount = value;
                CalculateAttendance();
                NotifyPropertyChanged("WillMissString");
            }
        }
        public string OverallStatusString
        {
            get { return String.Format("You{0} have attended {1}\nout of {2} classes", 
                                    (WillAttendCount + WillMissCount == 0)?"":"'ll", _attendedClasses, _totalClasses); }
        }
        public string WillAttendString
        {
            get { return String.Format("Attend {0} class{1}", WillAttendCount, WillAttendCount == 1 ? "" : "es"); }
        }
        public string WillMissString
        {
            get { return String.Format("Miss {0} class{1}", WillMissCount, WillMissCount == 1 ? "" : "es"); }
        }
        public string TargetAttendanceText
        {
            get
            {
                return _targetAttendance.ToString("F0");
            }
        }
        public string TargetClasses
        {
            get
            {
                if (AttendancePercentage >= _targetAttendance)
                {
                    if (WillAttendCount == 0 && WillMissCount == 0)
                        return "You already have a higher percentage :)";
                    else
                        return "You will be above your target.";
                }
                else
                {
                    double target = (_targetAttendance - 1) / 100;
                    double req = (_attendedClasses - target * _totalClasses) / (target - 1);
                    if (double.IsInfinity(req) || double.IsNaN(req) || req < 0)
                        return "Sorry, you can't reach your target :/";
                    else
                        return String.Format("Need to attend at least {0} class hour{1}.", Math.Floor(req + 1), req == 1 ? "" : "s");
                }
            }
        }
        
        public AttendanceControl()
        {
            this.InitializeComponent();
            this.DataContextChanged += AttendanceControl_DataContextChanged;
        }

        private void AttendanceControl_DataContextChanged(FrameworkElement sender, DataContextChangedEventArgs args)
        {
            if (this.DataContext != null)
            {
                _attendance = this.DataContext as Attendance;
                Details = new List<KeyValuePair<DateTimeOffset, AttendanceStub>>(_attendance.Details.Count);
                foreach (AttendanceGroup group in _attendance.Details)
                    foreach (AttendanceStub stub in group.Details)
                        Details.Add(new KeyValuePair<DateTimeOffset, AttendanceStub>(group.ContextDate, stub));
                Details.Reverse();
                detailsList.DataContext = this;
                attendanceGrid.DataContext = this;
                ResetParamters();
            }
        }

        private void NotifyPropertyChanged([CallerMemberName]string propertyName = null)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }

        private void ResetParamters()
        {
            _totalClasses = _attendance.TotalClasses;
            _attendedClasses = _attendance.AttendedClasses;
            AttendancePercentage = _attendance.Percentage;
            _willAttendCount = 0;
            _willMissCount = 0;
        }

        private void CalculateAttendance()
        {
            _totalClasses = (ushort)(_attendance.TotalClasses + _willAttendCount + _willMissCount);
            _attendedClasses = (ushort)(_attendance.AttendedClasses + _willAttendCount);
            AttendancePercentage = 100 * (double)_attendedClasses / (double)_totalClasses;
        }

        private void ChangeTargetAttendence(string value)
        {
            try
            {
                double val = double.Parse(value);
                if (val > 100)
                    val = 100;
                _targetAttendance = Math.Round(val, 0);
            }
            catch { }
            finally
            {
                NotifyPropertyChanged("TargetAttendanceText");
                NotifyPropertyChanged("TargetClasses");
            }
        }

        private void AttendMinusButton_Click(object sender, RoutedEventArgs e)
        {
            WillAttendCount -= (short)_attendance.SingleClassLength;
        }

        private void AttendPlusButton_Click(object sender, RoutedEventArgs e)
        {
            WillAttendCount += (short)_attendance.SingleClassLength;
        }

        private void MissMinusButton_Click(object sender, RoutedEventArgs e)
        {
            WillMissCount -= (short)_attendance.SingleClassLength;
        }

        private void MissPlusButton_Click(object sender, RoutedEventArgs e)
        {
            WillMissCount += (short)_attendance.SingleClassLength;
        }

        private void TargetBox_LostFocus(object sender, RoutedEventArgs e)
        {
            ChangeTargetAttendence((sender as TextBox).Text);
        }
    }
}
