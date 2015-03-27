using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using VITacademics.Managers;
using VITacademics.UIControls;


namespace VITacademics.Helpers
{

    public enum ControlTypeCodes
    {
        Overview = 0,
        BasicTimetable = 1,
        EnhancedTimetable = 2,
        CourseInfo = 3
    }

    public class ControlManager : IManageable
    {

        #region Static Properties and Contructor

        private static readonly Dictionary<Type, ControlTypeCodes> _controlTypeDictionary;

        static ControlManager()
        {
            _controlTypeDictionary = new Dictionary<Type, ControlTypeCodes>();
            _controlTypeDictionary.Add(typeof(UserOverviewControl), ControlTypeCodes.Overview);
            _controlTypeDictionary.Add(typeof(CourseInfoControl), ControlTypeCodes.CourseInfo);
            _controlTypeDictionary.Add(typeof(BasicTimetableControl), ControlTypeCodes.BasicTimetable);
            _controlTypeDictionary.Add(typeof(EnhancedTimetableControl), ControlTypeCodes.EnhancedTimetable);
        }

        public static ControlTypeCodes GetTypeCode(IProxiedControl control)
        {
            return _controlTypeDictionary[control.GetType()];
        }

        public static ControlTypeCodes GetTypeCode(Type proxiedControlType)
        {
            return _controlTypeDictionary[proxiedControlType];
        }

        #endregion

        #region Fields and Properties

        private List<int> _controlHistory;
        private List<Dictionary<string, object>> _stateHistory;
        private List<string> _paramterHistory;
        private IProxiedControl _currentControl;
        private string _currentParameter;
        private EventHandler<RequestEventArgs> _handler;

        public IProxiedControl CurrentControl
        {
            get
            {
                return _currentControl;
            }
        }
        public bool CanGoBack
        {
            get { return (_controlHistory.Count > 0); }
        }

        #endregion

        #region Constructor

        public ControlManager(EventHandler<RequestEventArgs> handler)
        {
            _handler = handler;
            ClearHistory();
        }

        #endregion

        #region Private Helper Methods

        private void ActionRequestedListener(object sender, RequestEventArgs e)
        {
            _handler(sender, e);
        }

        private void SaveCurrentControl()
        {
            _controlHistory.Add((int)GetTypeCode(_currentControl));
            _paramterHistory.Add(_currentParameter);
            _stateHistory.Add(_currentControl.SaveState());
        }

        private void LoadControl(int controlTypeCode, string parameter)
        {
            switch (controlTypeCode)
            {
                case (int)ControlTypeCodes.Overview:
                    _currentControl = new UserOverviewControl();
                    break;
                case (int)ControlTypeCodes.CourseInfo:
                    _currentControl = new CourseInfoControl();
                    break;
                case (int)ControlTypeCodes.BasicTimetable:
                    _currentControl = new BasicTimetableControl();
                    break;
                case (int)ControlTypeCodes.EnhancedTimetable:
                    _currentControl = new EnhancedTimetableControl();
                    break;
            }
            _currentControl.ActionRequested += ActionRequestedListener;
            _currentControl.GenerateView(parameter);
            _currentParameter = parameter;
        }

        private void RemoveLastControl()
        {
            int count = _controlHistory.Count;
            _controlHistory.RemoveAt(count - 1);
            _paramterHistory.RemoveAt(count - 1);
            _stateHistory.RemoveAt(count - 1);
        }

        #endregion

        #region Public Methods

        public void ClearHistory()
        {
            _controlHistory = new List<int>();
            _stateHistory = new List<Dictionary<string, object>>();
            _paramterHistory = new List<string>();
            _currentControl = null;
        }

        public void NavigateToControl(Type controlType, string parameter)
        {
            if (_currentControl != null)
            {
                SaveCurrentControl();
            }

            int controlCode = (int)GetTypeCode(controlType);
            LoadControl(controlCode, parameter);
        }

        public void ReturnToLastControl()
        {
            int count = _controlHistory.Count;
            if (count < 1)
                return;
            else
            {
                int controlTypeCode = _controlHistory[count - 1];
                string parameter = _paramterHistory[count - 1];
                var lastState = _stateHistory[count - 1];

                RemoveLastControl();

                LoadControl(controlTypeCode, parameter);
                _currentControl.LoadState(lastState);
            }
        }

        public void RefreshCurrentControl()
        {
            if (CurrentControl == null)
                throw new InvalidOperationException("The current control is not yet assigned. Call NavigateToControl() to start.");

            _currentControl.GenerateView(_currentParameter);
        }

        public Dictionary<string, object> SaveState()
        {
            List<int> controls = new List<int>(_controlHistory);
            List<Dictionary<string, object>> states = new List<Dictionary<string,object>>(_stateHistory);
            List<string> paramters = new List<string>(_paramterHistory);

            if (_currentControl != null)
            {
                controls.Add((int)GetTypeCode(_currentControl));
                paramters.Add(_currentParameter);
                states.Add(_currentControl.SaveState());
            }

            Dictionary<string, object> state = new Dictionary<string, object>();
            state.Add("controls", controls);
            state.Add("states", states);
            state.Add("parameters", paramters);
            return state;
        }

        public void LoadState(Dictionary<string, object> lastState)
        {
            try
            {
                _controlHistory = lastState["controls"] as List<int>;
                _stateHistory = lastState["states"] as List<Dictionary<string, object>>;
                _paramterHistory = lastState["parameters"] as List<string>;
            }
            catch
            {
                ClearHistory();
            }
        }

        #endregion

    }
}
