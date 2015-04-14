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

        private static readonly Dictionary<Type, ControlTypeCodes> _typeCodeDictionary;

        private static Dictionary<Type, ControlTypeCodes> TypeCodeDictionary { get { return _typeCodeDictionary; } }

        static ControlManager()
        {
            _typeCodeDictionary = new Dictionary<Type, ControlTypeCodes>();
            _typeCodeDictionary.Add(typeof(UserOverviewControl), ControlTypeCodes.Overview);
            _typeCodeDictionary.Add(typeof(CourseInfoControl), ControlTypeCodes.CourseInfo);
            _typeCodeDictionary.Add(typeof(BasicTimetableControl), ControlTypeCodes.BasicTimetable);
            _typeCodeDictionary.Add(typeof(EnhancedTimetableControl), ControlTypeCodes.EnhancedTimetable);
        }

        #endregion

        #region Fields and Properties

        private List<int> _controlHistory;
        private List<Dictionary<string, object>> _stateHistory;
        private List<string> _paramterHistory;

        private EventHandler<RequestEventArgs> _handler;

        private IProxiedControl _currentControl;
        private ControlTypeCodes _currentControlCode;
        private string _currentParameter;

        private string CurrentParameter
        {
            get { return _currentParameter; }
        }
        public IProxiedControl CurrentControl
        {
            get { return _currentControl; }
        }
        public ControlTypeCodes CurrentControlCode
        {
            get
            {
                if (_currentControl == null)
                    throw new InvalidOperationException();
                return _currentControlCode;
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
            _controlHistory.Add((int)CurrentControlCode);
            _paramterHistory.Add(CurrentParameter);
            _stateHistory.Add(CurrentControl.SaveState());
        }

        private void LoadControl(ControlTypeCodes controlTypeCode, string parameter)
        {
            switch (controlTypeCode)
            {
                case ControlTypeCodes.Overview:
                    _currentControl = new UserOverviewControl();
                    break;
                case ControlTypeCodes.CourseInfo:
                    _currentControl = new CourseInfoControl();
                    break;
                case ControlTypeCodes.BasicTimetable:
                    _currentControl = new BasicTimetableControl();
                    break;
                case ControlTypeCodes.EnhancedTimetable:
                    _currentControl = new EnhancedTimetableControl();
                    break;
            }
            _currentControlCode = controlTypeCode;
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

        // Complete
        public void ClearHistory()
        {
            _controlHistory = new List<int>();
            _stateHistory = new List<Dictionary<string, object>>();
            _paramterHistory = new List<string>();
            _currentControl = null;
        }

        public void NavigateToControl(Type controlType, string parameter)
        {
            NavigateToControl(TypeCodeDictionary[controlType], parameter);
        }

        public void NavigateToControl(ControlTypeCodes typeCode, string parameter)
        {
            if (_currentControl != null)
                SaveCurrentControl();

            LoadControl(typeCode, parameter);
        }

        public void ReturnToLastControl()
        {
            int count = _controlHistory.Count;
            if (count < 1)
                return;
            else
            {
                ControlTypeCodes controlTypeCode = (ControlTypeCodes)_controlHistory[count - 1];
                string parameter = _paramterHistory[count - 1];
                var lastState = _stateHistory[count - 1];

                RemoveLastControl();

                LoadControl(controlTypeCode, parameter);
                CurrentControl.LoadState(lastState);
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
                controls.Add((int)CurrentControlCode);
                paramters.Add(CurrentParameter);
                states.Add(CurrentControl.SaveState());
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
