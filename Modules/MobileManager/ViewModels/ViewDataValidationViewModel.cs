using Gijima.IOBM.Infrastructure.Events;
using Gijima.IOBM.Infrastructure.Structs;
using Gijima.IOBM.MobileManager.Common.Events;
using Gijima.IOBM.MobileManager.Common.Structs;
using Gijima.IOBM.MobileManager.Model.Data;
using Gijima.IOBM.MobileManager.Model.Models;
using Gijima.IOBM.MobileManager.Security;
using Prism.Commands;
using Prism.Events;
using Prism.Mvvm;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace Gijima.IOBM.MobileManager.ViewModels
{
    public class ViewDataValidationViewModel : BindableBase, IDataErrorInfo
    {
        #region Properties & Attributes

        private ValidationRuleModel _model = null;
        private IEventAggregator _eventAggregator;
        private SecurityHelper _securityHelper = null;
        private string _billingPeriod = string.Format("{0}{1}", DateTime.Now.Month.ToString().PadLeft(2, '0'), DateTime.Now.Year);

        #region Commands

        public DelegateCommand StartValidationCommand { get; set; }
        public DelegateCommand ApplyRuleFixCommand { get; set; }
        public DelegateCommand ManualFixCommand { get; set; }

        #endregion

        #region Properties

        /// <summary>
        /// The validation group progessbar description
        /// </summary>
        public string ValidationGroupDescription
        {
            get { return _validationGroupDescription; }
            set { SetProperty(ref _validationGroupDescription, value); }
        }
        private string _validationGroupDescription;

        /// <summary>
        /// The validation group progessbar value
        /// </summary>
        public int ValidationGroupProgress
        {
            get { return _validationGroupProgress; }
            set { SetProperty(ref _validationGroupProgress, value); }
        }
        private int _validationGroupProgress;

        /// <summary>
        /// The validation entity progessbar description
        /// </summary>
        public string ValidationEntityDescription
        {
            get { return _validationEntityDescription; }
            set { SetProperty(ref _validationEntityDescription, value); }
        }
        private string _validationEntityDescription;

        /// <summary>
        /// The validation entity progessbar value
        /// </summary>
        public int ValidationEntityProgress
        {
            get { return _validationEntityProgress; }
            set { SetProperty(ref _validationEntityProgress, value); }
        }
        private int _validationEntityProgress;

        /// <summary>
        /// The validation data rule progessbar description
        /// </summary>
        public string ValidationDataRuleDescription
        {
            get { return _validationDataRuleDescription; }
            set { SetProperty(ref _validationDataRuleDescription, value); }
        }
        private string _validationDataRuleDescription;

        /// <summary>
        /// The validation data rule progessbar value
        /// </summary>
        public int ValidationDataRuleProgress
        {
            get { return _validationDataRuleProgress; }
            set { SetProperty(ref _validationDataRuleProgress, value); }
        }
        private int _validationDataRuleProgress;

        /// <summary>
        /// The validation rule entity progessbar description
        /// </summary>
        public string ValidationRuleEntityDescription
        {
            get { return _validationRuleEntityDescription; }
            set { SetProperty(ref _validationRuleEntityDescription, value); }
        }
        private string _validationRuleEntityDescription;

        /// <summary>
        /// The validation rule entity progessbar value
        /// </summary>
        public int ValidationRuleEntityProgress
        {
            get { return _validationRuleEntityProgress; }
            set { SetProperty(ref _validationRuleEntityProgress, value); }
        }
        private int _validationRuleEntityProgress;

        /// <summary>
        /// The page description to display on billing wizard page
        /// </summary>
        public string BillingWizardPageDescription
        {
            get { return _billingWizardPageDescription; }
            set { SetProperty(ref _billingWizardPageDescription, value); }
        }
        private string _billingWizardPageDescription = string.Empty;

        /// <summary>
        /// The instruction to display on billing wizard page
        /// </summary>
        public string BillingWizardPageInstruction
        {
            get { return _billingWizardPageInstruction; }
            set { SetProperty(ref _billingWizardPageInstruction, value); }
        }
        private string _billingWizardPageInstruction = string.Empty;

        /// <summary>
        /// The number of validation groups
        /// </summary>
        public int ValidationGroupCount
        {
            get { return _validationGroupCount; }
            set { SetProperty(ref _validationGroupCount, value); }
        }
        private int _validationGroupCount;

        /// <summary>
        /// The number of entities tpvalidate
        /// </summary>
        public int ValidationEntityCount
        {
            get { return _validationEntityCount; }
            set { SetProperty(ref _validationEntityCount, value); }
        }
        private int _validationEntityCount;

        /// <summary>
        /// The number of validation rules
        /// </summary>
        public int ValidationDataRuleCount
        {
            get { return _validationDataRuleCount; }
            set { SetProperty(ref _validationDataRuleCount, value); }
        }
        private int _validationDataRuleCount;
        
        /// <summary>
        /// The number of validation rules
        /// </summary>
        public int ValidationRuleEntityCount
        {
            get { return _validationRuleEntityCount; }
            set { SetProperty(ref _validationRuleEntityCount, value); }
        }
        private int _validationRuleEntityCount;

        /// <summary>
        /// The number of validation groups that passed validation
        /// </summary>
        public int ValidationGroupsPassed
        {
            get { return _validationGroupsPassed; }
            set { SetProperty(ref _validationGroupsPassed, value); }
        }
        private int _validationGroupsPassed;

        /// <summary>
        /// The number of validation groups that failed validation
        /// </summary>
        public int ValidationGroupsFailed
        {
            get { return _validationGroupsFailed; }
            set { SetProperty(ref _validationGroupsFailed, value); }
        }
        private int _validationGroupsFailed;

        /// <summary>
        /// The number of validation entities that passed validation
        /// </summary>
        public int ValidationEntitiesPassed
        {
            get { return _validationEntitiesPassed; }
            set { SetProperty(ref _validationEntitiesPassed, value); }
        }
        private int _validationEntitiesPassed;

        /// <summary>
        /// The number of validation entities that failed validation
        /// </summary>
        public int ValidationEntitiesFailed
        {
            get { return _validationEntitiesFailed; }
            set { SetProperty(ref _validationEntitiesFailed, value); }
        }
        private int _validationEntitiesFailed;

        /// <summary>
        /// The number of validation data rules that passed validation
        /// </summary>
        public int ValidationDataRulesPassed
        {
            get { return _validationDataRulesPassed; }
            set { SetProperty(ref _validationDataRulesPassed, value); }
        }
        private int _validationDataRulesPassed;

        /// <summary>
        /// The number of validation data rules that failed validation
        /// </summary>
        public int ValidationDataRulesFailed
        {
            get { return _validationDataRulesFailed; }
            set { SetProperty(ref _validationDataRulesFailed, value); }
        }
        private int _validationDataRulesFailed;

        /// <summary>
        /// The number of validation data rules that passed validation
        /// </summary>
        public int ValidationRuleEntitiesPassed
        {
            get { return _validationRuleEntitiesPassed; }
            set { SetProperty(ref _validationRuleEntitiesPassed, value); }
        }
        private int _validationRuleEntitiesPassed;

        /// <summary>
        /// The number of validation data rules that failed validation
        /// </summary>
        public int ValidationRuleEntitiesFailed
        {
            get { return _validationRuleEntitiesFailed; }
            set { SetProperty(ref _validationRuleEntitiesFailed, value); }
        }
        private int _validationRuleEntitiesFailed;

        /// <summary>
        /// The selected exceptions
        /// </summary>
        public string SelectedExceptions
        {
            get { return _selectedExceptions; }
            set
            {
                SetProperty(ref _selectedExceptions, value);
                PopulateSelectedExceptions(value);
            }
        }
        private string _selectedExceptions;

        /// <summary>
        /// The current selected exception
        /// </summary>
        private ValidationRuleException SelectedException
        {
            get { return _selectedException; }
            set { SetProperty(ref _selectedException, value); }
        }
        private ValidationRuleException _selectedException = null;

        /// <summary>
        /// Indicate if the current billing process completed
        /// </summary>
        public bool BillingProcessCompleted
        {
            get { return _billingProcessCompleted; }
            set { SetProperty(ref _billingProcessCompleted, value); }
        }
        private bool _billingProcessCompleted = false;

        /// <summary>
        /// The selected exceptions to fix
        /// </summary>
        private ObservableCollection<ValidationRuleException> ExceptionsToFix
        {
            get { return _exceptionsToFix; }
            set { SetProperty(ref _exceptionsToFix, value); }
        }
        private ObservableCollection<ValidationRuleException> _exceptionsToFix = null;

        /// <summary>
        /// The collection of validation groups from the ValidationRuleGroup enum
        /// </summary>
        public ObservableCollection<string> ValidationGroupCollection
        {
            get { return _categoryCollection; }
            set { SetProperty(ref _categoryCollection, value); }
        }
        private ObservableCollection<string> _categoryCollection = null;

        /// <summary>
        /// The collection of validation rules from the database
        /// </summary>
        public ObservableCollection<ValidationRule> ValidationRuleCollection
        {
            get { return _validationRuleCollection; }
            set { SetProperty(ref _validationRuleCollection, value); }
        }
        private ObservableCollection<ValidationRule> _validationRuleCollection = null;

        /// <summary>
        /// The collection of data validation exception Info
        /// </summary>
        public ObservableCollection<ValidationRuleException> ValidationErrorCollection
        {
            get { return _validationErrorCollection; }
            set { SetProperty(ref _validationErrorCollection, value); }
        }
        private ObservableCollection<ValidationRuleException> _validationErrorCollection = null;
       
        #region View Lookup Data Collections

        #endregion

        #region Required Fields

        #endregion

        #region Input Validation

        /// <summary>
        /// Input validate error message
        /// </summary>
        public string Error
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public object Dispatcher { get; private set; }

        /// <summary>
        /// Input validation properties
        /// </summary>
        /// <param name="columnName"></param>
        /// <returns></returns>
        public string this[string columnName]
        {
            get
            {
                string result = string.Empty;
                return result;
            }
        }

        #endregion

        #endregion

        #endregion

        #region Event Handlers

        /// <summary>
        /// This event gets received to update the progressbar
        /// </summary>
        /// <param name="sender">The error message.</param>
        private void ProgressBarInfo_Event(object sender)
        {
            Application.Current.Dispatcher.Invoke(() => { UpdateProgressBarValues(sender); });
        }

        /// <summary>
        /// This event gets received to display the validation result info
        /// </summary>
        /// <param name="sender">The error message.</param>
        private void DataValiationResult_Event(object sender)
        {
            Application.Current.Dispatcher.Invoke(() => { DisplayDataValidationResults(sender); });
        }

        /// <summary>
        /// This event gets received to enable the next button 
        /// when the process completed
        /// </summary>
        /// <param name="sender">The error message.</param>
        private void BillingProcessCompleted_Event(BillingExecutionState sender)
        {
            if (sender == BillingExecutionState.DataValidation)
                BillingProcessCompleted = true;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Constructor
        /// </summary>
        public ViewDataValidationViewModel(IEventAggregator eventAggregator)
        {
            _eventAggregator = eventAggregator;
            _securityHelper = new SecurityHelper(eventAggregator);
            InitialiseBillingView();
        }

        /// <summary>
        /// Initialise all the view dependencies
        /// </summary>
        private async void InitialiseBillingView()
        {
            _model = new ValidationRuleModel(_eventAggregator);
            InitialiseViewControls();

            // Initialise the view commands
            StartValidationCommand = new DelegateCommand(ExecuteStartValidation, CanStartValidation).ObservesProperty(() => ValidationRuleCollection)
                                                                                                    .ObservesProperty(() => BillingProcessCompleted);
            ApplyRuleFixCommand = new DelegateCommand(ExecuteApplyRuleFix, CanApplyRuleFix).ObservesProperty(() => ExceptionsToFix);
            ManualFixCommand = new DelegateCommand(ExecuteManualFix, CanManualFix).ObservesProperty(() => ExceptionsToFix);

            // Subscribe to this event to update the progressbar
            _eventAggregator.GetEvent<ProgressBarInfoEvent>().Subscribe(ProgressBarInfo_Event, true);

            // Subscribe to this event to display the data validation errors
            _eventAggregator.GetEvent<DataValiationResultEvent>().Subscribe(DataValiationResult_Event, true);

            // Subscribe to this event to lock the completed process
            // and enable functionality to move to the process completed
            _eventAggregator.GetEvent<BillingProcessCompletedEvent>().Subscribe(BillingProcessCompleted_Event, true);

            // Load the view data
            ReadValidationRuleGroups();
            await ReadValidationRuleExceptionsAsync();
        }

        /// <summary>
        /// Set default values to view properties
        /// </summary>
        private void InitialiseViewControls()
        {
            BillingWizardPageInstruction = "Please start the billing data validation process to validate all the data rules configured for each data validation group.";
            ValidationGroupDescription = string.Format("Validating data group - {0} of {1}", 0, ValidationGroupCollection != null ? ValidationGroupCollection.Count : 0);
            ValidationEntityDescription = string.Format("Validating entity - {0} of {1}", 0, 0);
            ValidationDataRuleDescription = string.Format("Validating data rule - {0} of {1}", 0, 0);
            ValidationRuleEntityDescription = string.Format("Validating data rule entity - {0} of {1}", 0, 0);
            ValidationGroupProgress = ValidationEntityProgress = ValidationDataRuleProgress = ValidationRuleEntityProgress = 0;
            ValidationGroupCount = ValidationEntityCount = ValidationDataRuleCount = ValidationRuleEntityCount = 1;
            ValidationGroupsPassed = ValidationEntitiesPassed = ValidationDataRulesPassed = ValidationRuleEntitiesPassed = 0;
            ValidationGroupsFailed = ValidationEntitiesFailed = ValidationDataRulesFailed = ValidationRuleEntitiesFailed = 0;
            ValidationErrorCollection = null;
        }

        /// <summary>
        /// Load all the validation rule groups from the database
        /// </summary>
        private void ReadValidationRuleGroups()
        {
            try
            {
                ValidationGroupCollection = new ObservableCollection<string>();

                foreach (ValidationRuleGroup source in Enum.GetValues(typeof(ValidationRuleGroup)))
                {
                    if (source != ValidationRuleGroup.None)
                        ValidationGroupCollection.Add(source.ToString());
                }
            }
            catch (Exception ex)
            {
                _eventAggregator.GetEvent<MessageEvent>().Publish(ex);
            }
        }

        /// <summary>
        /// Load all the validation rules based on the selected group from the database
        /// </summary>
        /// <param name="validationGroup">The valiation group to read the rules for.</param>
        private async Task ReadValidationRulesAsync(string validationGroup)
        {
            try
            {
                ValidationRuleCollection = await Task.Run(() => _model.ReadValidationRules((ValidationRuleGroup)Enum.Parse(typeof(ValidationRuleGroup), validationGroup)));
            }
            catch (Exception ex)
            {
                _eventAggregator.GetEvent<MessageEvent>().Publish(ex);
            }
        }

        /// <summary>
        /// Load all the validation rule exceptions if any from the database
        /// </summary>
        private async Task ReadValidationRuleExceptionsAsync()
        {
            try
            {
                ValidationErrorCollection = await Task.Run(() => new ValidationRuleExceptionModel(_eventAggregator).ReadValidationRuleExceptions(_billingPeriod));
            }
            catch (Exception ex)
            {
                _eventAggregator.GetEvent<MessageEvent>().Publish(ex);
            }
        }

        /// <summary>
        /// Add the data validation error to the validation exception listview
        /// </summary>
        /// <param name="resultInfo">The data validation result info.</param>
        private void DisplayDataValidationResults(object validationResultInfo)
        {
            try
            {
                if (ValidationErrorCollection == null)
                    ValidationErrorCollection = new ObservableCollection<ValidationRuleException>();

                ValidationRuleException resultInfo = (ValidationRuleException)validationResultInfo;

                if (resultInfo.Result == true)
                {
                    ValidationRuleEntitiesPassed++;
                }
                else
                { 
                    ValidationRuleEntitiesFailed++;
                    ValidationErrorCollection.Add(resultInfo);
                }
            }
            catch (Exception ex)
            {
                _eventAggregator.GetEvent<MessageEvent>().Publish(ex);
            }
        }

        /// <summary>
        /// Update the progressbar values
        /// </summary>
        /// <param name="progressBarInfo">The progressbar info.</param>
        private void UpdateProgressBarValues(object progressBarInfo)
        {
            try
            {
                ProgressBarInfo progressInfo = (ProgressBarInfo)progressBarInfo;
                ValidationRuleEntityCount = progressInfo.MaxValue;
                ValidationRuleEntityProgress = progressInfo.CurrentValue;
                ValidationRuleEntityDescription = string.Format("Validating data rule entity - {0} of {1}", ValidationRuleEntityProgress,
                                                                                                            progressInfo.MaxValue);
            }
            catch (Exception ex)
            {
                _eventAggregator.GetEvent<MessageEvent>().Publish(ex);
            }
        }

        /// <summary>
        /// Populate the selected exceptions to fix collection
        /// </summary>
        /// <param name="selectedExceptions">The Xceed CheckListBox selected items string.</param>
        private void PopulateSelectedExceptions(string selectedExceptions)
        {
            try
            {
                ObservableCollection<ValidationRuleException> resultInfosCollection= new ObservableCollection<ValidationRuleException>();
                ValidationRuleException resultInfo = null;

                // The Xceed CheckListBox return all the selected items in
                // a comma delimeted string, so get the number of selected 
                // items we need to split the string
                string[] exceptionsToFix = selectedExceptions.Split(',');

                // Find the last selected exception based on the exception message
                SelectedException = ValidationErrorCollection.Where(p => p.Message == exceptionsToFix.Last()).FirstOrDefault();

                // Convert all the string values to DataValidationResultInfo
                // and populate the ValidationErrorCollection
                foreach (string exception in exceptionsToFix)
                {
                    resultInfo = ValidationErrorCollection.Where(p => p.Message == exception).FirstOrDefault();

                    if (resultInfo != null)
                    {
                        resultInfosCollection.Add(resultInfo);
                    }
                }

                ExceptionsToFix = new ObservableCollection<ValidationRuleException>(resultInfosCollection);
            }
            catch (Exception ex)
            {
                _eventAggregator.GetEvent<MessageEvent>().Publish(ex);
            }
        }

        /// <summary>
        /// Create a new billing process history entry
        /// </summary>
        private async Task CreateBillingProcessHistoryAsync()
        {
            try
            {
                bool result = await Task.Run(() => new BillingProcessModel(_eventAggregator).CreateBillingProcessHistory(BillingExecutionState.DataValidation));

                // Publish this event to update the billing process history on the wizard's Info content
                _eventAggregator.GetEvent<BillingProcessHistoryEvent>().Publish(result);
            }
            catch (Exception ex)
            {
                _eventAggregator.GetEvent<MessageEvent>().Publish(ex);
            }
        }

        /// <summary>
        /// Create a new billing process history entry
        /// </summary>
        /// <param name="billingProcess">The billing process to complete.</param>/// 
        private async Task CompleteBillingProcessHistoryAsync(BillingExecutionState billingProcess)
        {
            try
            {
                bool result = await Task.Run(() => new BillingProcessModel(_eventAggregator).CompleteBillingProcessHistory(billingProcess, true));

                if (result)
                {
                    // Publish this event to update the billing process history on the wizard's Info content
                    _eventAggregator.GetEvent<BillingProcessHistoryEvent>().Publish(result);

                    // Publish this event to lock the completed process and enable 
                    // functinality to move to the next process
                    if (billingProcess == BillingExecutionState.DataValidation)
                        _eventAggregator.GetEvent<BillingProcessCompletedEvent>().Publish(BillingExecutionState.DataValidation);
                }
            }
            catch (Exception ex)
            {
                _eventAggregator.GetEvent<MessageEvent>().Publish(ex);
            }
        }       

        /// <summary>
        /// Save all the data rule exceptions to the database
        /// </summary>
        private async Task CreateValidationRuleExceptionsAsync()
        {
            try
            {               
                bool result = await Task.Run(() => new ValidationRuleExceptionModel(_eventAggregator).CreateValidationRuleExceptions(ValidationErrorCollection));
            }
            catch (Exception ex)
            {
                _eventAggregator.GetEvent<MessageEvent>().Publish(ex);
            }
        }

        #region Lookup Data Loading

        #endregion

        #region Command Execution

        /// <summary>
        /// Set view command buttons enabled/disabled state
        /// </summary>
        /// <returns></returns>
        private bool CanStartValidation()
        {
            return ValidationGroupCollection != null && ValidationGroupCollection.Count > 0  && BillingProcessCompleted == false ? true : false;
        }

        /// <summary>
        /// Execute when the start command button is clicked 
        /// </summary>
        private async void ExecuteStartValidation()
        {
            InitialiseViewControls();

            // Set the previous data validation process as complete
            await CompleteBillingProcessHistoryAsync(BillingExecutionState.Started);

            // Create a new history entry everytime the process get started
            await CreateBillingProcessHistoryAsync();

            // Set the group progressbar max value
            ValidationGroupCount = ValidationGroupCollection.Count;

            // Update the process progress values on the wizard's Info content
            _eventAggregator.GetEvent<BillingProcessEvent>().Publish(BillingExecutionState.DataValidation);

            // Disable the next buttton when the process gets started
            _eventAggregator.GetEvent<BillingProcessStartedEvent>().Publish(BillingExecutionState.DataValidation);

            foreach (string group in ValidationGroupCollection)
            {
                // Read the validation rules for the specified
                // group from the database
                await ReadValidationRulesAsync(group);

                // Set the validation group progresssbar description
                ++ValidationGroupProgress;
                ValidationGroupDescription = string.Format("Validating data group {0} - {1} of {2}", group.ToUpper(),
                                                                                                     ValidationGroupProgress,
                                                                                                     ValidationGroupCollection.Count);
                if (ValidationRuleCollection.Count > 0)
                {
                    int entityCount = ValidationRuleCollection.GroupBy(p => p.EntityID).Count();                    
                    int entityID = 0;

                    // Set the entity and data rule progressbars max values
                    ValidationEntityCount = entityCount;
                    ValidationDataRuleCount = ValidationRuleCollection.Count;

                    foreach (ValidationRule rule in ValidationRuleCollection)
                    {
                       // entityID = rule.EntityID;

                        // Validate the data rule and update
                        // the progress values accodingly
                        if (await Task.Run(() => _model.ValidateDataRule(rule)))
                            ValidationDataRulesPassed++;
                        else
                            ValidationDataRulesFailed++;

                        // Update progress values when the
                        // group entity change
                        if (entityID != rule.EntityID)
                        {
                            entityID = rule.EntityID;

                            // Set the validation entity progresssbar description
                            ++ValidationEntityProgress;
                            ValidationEntityDescription = string.Format("Validating {0} {1} - {2} of {3}", group.ToLower(),
                                                                                                           rule.EntityName.ToUpper(),
                                                                                                           ValidationEntityProgress,
                                                                                                           entityCount);                        
                            // Update the group entity progress values
                            if (ValidationDataRulesFailed == 0)
                                ValidationEntitiesPassed++;
                            else
                                ValidationEntitiesFailed++;
                        }

                        // Set the data rule progresssbar description
                        ++ValidationDataRuleProgress;
                        ValidationDataRuleDescription = string.Format("Validating data rule {0} - {1} of {2}", rule.RuleDataName.ToUpper(),
                                                                                                               ValidationDataRuleProgress,
                                                                                                               ValidationRuleCollection.Count);
                    }

                    // Update the validation group 
                    // progress values accodingly
                    if (ValidationEntitiesFailed == 0)
                        ValidationGroupsPassed++;
                    else
                        ValidationGroupsFailed++;

                    // if NO validations exceptions found the set 
                    // the data validation process as complete
                    // else save the exceptions to the database
                    if (ValidationErrorCollection.Count == 0)
                        await CompleteBillingProcessHistoryAsync(BillingExecutionState.DataValidation);
                    else
                        await CreateValidationRuleExceptionsAsync();


                }
            }
        }

        /// <summary>
        /// Set view command buttons enabled/disabled state
        /// </summary>
        /// <returns></returns>
        private bool CanApplyRuleFix()
        {
            return ExceptionsToFix != null && ExceptionsToFix.Any(p => p.CanApplyRule == true) && !ExceptionsToFix.Any(p => p.CanApplyRule == false);
        }

        /// <summary>
        /// Execute when the apply rule command button is clicked 
        /// </summary>
        private async void ExecuteApplyRuleFix()
        {
            try
            {
                // Apply the data rule to each exception and remove the fixed
                // exceptions to the exceptions collection                  
                foreach (ValidationRuleException exception in ExceptionsToFix)
                {
                    if (await Task.Run(() => _model.ApplyDataRule(exception)))
                        ValidationErrorCollection.Remove(exception);
                }
            }
            catch (Exception ex)
            {
                _eventAggregator.GetEvent<MessageEvent>().Publish(ex);
            }
        }

        /// <summary>
        /// Set view command buttons enabled/disabled state
        /// </summary>
        /// <returns></returns>
        private bool CanManualFix()
        {
            return ExceptionsToFix != null && ExceptionsToFix.Count == 1 && !ExceptionsToFix.Any(p => p.CanApplyRule == true);
        }

        /// <summary>
        /// Execute when the manual fix command button is clicked 
        /// </summary>
        private void ExecuteManualFix()
        {
            if (_selectedException != null)
                _eventAggregator.GetEvent<SearchResultEvent>().Publish(_selectedException.EntityID);
        }

        #endregion

        #endregion
    }
}
