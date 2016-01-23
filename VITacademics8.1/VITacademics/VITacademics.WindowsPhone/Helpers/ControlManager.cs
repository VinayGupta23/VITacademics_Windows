using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using VITacademics.Managers;
using VITacademics.UIControls;


namespace VITacademics.Helpers
{

    public class ControlManager : IManageable
    {
        
        #region Static Constructor and Helpers

        delegate object InstanceCreator();

        private static readonly Dictionary<string, InstanceCreator> _controlFactory;

        static ControlManager()
        {
            _controlFactory = new Dictionary<string, InstanceCreator>();
            _controlFactory.Add(typeof(UserOverviewControl).FullName, () => new UserOverviewControl());
            _controlFactory.Add(typeof(BasicTimetableControl).FullName, () => new BasicTimetableControl());
            _controlFactory.Add(typeof(EnhancedTimetableControl).FullName, () => new EnhancedTimetableControl());
            _controlFactory.Add(typeof(CourseInfoControl).FullName, () => new CourseInfoControl());
            _controlFactory.Add(typeof(GradesControl).FullName, () => new GradesControl());
            _controlFactory.Add(typeof(AdvisorControl).FullName, () => new AdvisorControl());
        }

        private static IProxiedControl GetInstance(string fullTypeName)
        {
            return (IProxiedControl)_controlFactory[fullTypeName].Invoke();
        }

        #endregion

        #region Fields and Properties

        private List<string> _controlHistory;
        private List<Dictionary<string, object>> _stateHistory;
        private List<string> _paramterHistory;
        private EventHandler<RequestEventArgs> _handler;

        private IProxiedControl _currentControl;
        private string _currentParameter;

        public IProxiedControl CurrentControl
        {
            get { return _currentControl; }
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
            Clear();
        }

        #endregion

        #region Private Helper Methods

        private void ActionRequestedListener(object sender, RequestEventArgs e)
        {
            _handler(sender, e);
        }

        private void SaveCurrentControl()
        {
            _controlHistory.Add(_currentControl.GetType().FullName);
            _paramterHistory.Add(_currentParameter);
            _stateHistory.Add(_currentControl.SaveState());
        }

        private void LoadControl(string controlTypeName, string parameter, Dictionary<string, object> lastState = null)
        {
            _currentControl = GetInstance(controlTypeName);
            _currentControl.ActionRequested += ActionRequestedListener;
            _currentControl.LoadView(parameter, lastState);
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

        public void Clear()
        {
            _controlHistory = new List<string>();
            _stateHistory = new List<Dictionary<string, object>>();
            _paramterHistory = new List<string>();
            _currentControl = null;
            _currentParameter = null;
        }

        public void NavigateToControl(Type controlType, string parameter)
        {
            NavigateToControl(controlType.FullName, parameter);
        }

        public void NavigateToControl(string controlTypeName, string parameter)
        {
            if (_currentControl != null)
                SaveCurrentControl();
            LoadControl(controlTypeName, parameter);
        }

        public void ReturnToLastControl()
        {
            int count = _controlHistory.Count;
            if (count < 1)
                return;

            string controlTypeName = _controlHistory[count - 1];
            string parameter = _paramterHistory[count - 1];
            var lastState = _stateHistory[count - 1];

            RemoveLastControl();
            LoadControl(controlTypeName, parameter, lastState);
        }

        public void RefreshCurrentControl()
        {
            if (CurrentControl == null)
                throw new InvalidOperationException("The current control is not yet assigned. Call NavigateToControl() to start.");
            _currentControl.LoadView(_currentParameter);
        }

        public Dictionary<string, object> SaveState()
        {
            List<string> controls = new List<string>(_controlHistory);
            List<Dictionary<string, object>> states = new List<Dictionary<string, object>>(_stateHistory);
            List<string> paramters = new List<string>(_paramterHistory);

            if (_currentControl != null)
            {
                controls.Add(_currentControl.GetType().FullName);
                paramters.Add(_currentParameter);
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
                _controlHistory = lastState["controls"] as List<string>;
                _stateHistory = lastState["states"] as List<Dictionary<string, object>>;
                _paramterHistory = lastState["parameters"] as List<string>;
            }
            catch
            {
                Clear();
            }
        }

        #endregion

    }
}
