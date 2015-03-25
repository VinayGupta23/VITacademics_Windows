using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VITacademics.UIControls;


namespace VITacademics.Managers
{

    public class ControlManager : IManageable
    {

        #region Static Properties and Contructor

        private static readonly Dictionary<Type, int> _controlTypeDictionary;
        public static ReadOnlyDictionary<Type, int> ControlTypeDictionary
        {
            get;
            private set;
        }

        static ControlManager()
        {
            _controlTypeDictionary = new Dictionary<Type, int>();
            _controlTypeDictionary.Add(typeof(UserOverviewControl), 0);
            _controlTypeDictionary.Add(typeof(CourseInfoControl), 1);
            _controlTypeDictionary.Add(typeof(BasicTimetableControl), 2);
            _controlTypeDictionary.Add(typeof(EnhancedTimetableControl), 3);
            ControlTypeDictionary = new ReadOnlyDictionary<Type, int>(_controlTypeDictionary);
        }

        #endregion

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

        public ControlManager(EventHandler<RequestEventArgs> handler)
        {
            _handler = handler;
            ClearHistory();
        }

        private void ActionRequestedListener(object sender, RequestEventArgs e)
        {
            _handler(sender, e);
        }

        private void SaveCurrentControl()
        {
            _controlHistory.Add(ControlTypeDictionary[_currentControl.GetType()]);
            _paramterHistory.Add(_currentParameter);
            _stateHistory.Add(_currentControl.SaveState());
        }

        private void LoadCurrentControl(int controlTypeCode, string parameter)
        {
            switch (controlTypeCode)
            {
                case 0:
                    _currentControl = new UserOverviewControl();
                    break;
                case 1:
                    _currentControl = new CourseInfoControl();
                    break;
                case 2:
                    _currentControl = new BasicTimetableControl();
                    break;
                case 3:
                    _currentControl = new EnhancedTimetableControl();
                    break;
            }
            _currentControl.ActionRequested += ActionRequestedListener;
            _currentControl.GenerateView(parameter);
            _currentParameter = parameter;
        }

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

            int controlTypeCode = ControlTypeDictionary[controlType];
            LoadCurrentControl(controlTypeCode, parameter);
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

                _controlHistory.RemoveAt(count - 1);
                _paramterHistory.RemoveAt(count - 1);
                _stateHistory.RemoveAt(count - 1);

                LoadCurrentControl(controlTypeCode, parameter);
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
            if (_currentControl != null)
            {
                SaveCurrentControl();
            } 
            Dictionary<string, object> state = new Dictionary<string, object>();
            state.Add("controls", _controlHistory);
            state.Add("states", _stateHistory);
            state.Add("parameters", _paramterHistory);
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
    }
}
