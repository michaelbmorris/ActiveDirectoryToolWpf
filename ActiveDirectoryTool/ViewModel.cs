﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data;
using System.Deployment.Application;
using System.Diagnostics;
using System.DirectoryServices.AccountManagement;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Extensions.CollectionExtensions;
using Extensions.PrimitiveExtensions;
using GalaSoft.MvvmLight.CommandWpf;
using static System.Deployment.Application.ApplicationDeployment;
using static ActiveDirectoryTool.QueryType;

namespace ActiveDirectoryTool
{
    internal class ViewModel : INotifyPropertyChanged
    {
        private const string ComputerDistinguishedName =
            "ComputerDistinguishedName";

        private const string ContainerGroupDistinguishedName =
            "ContainerGroupDistinguishedName";

        private const string DirectReportDistinguishedName =
            "DirectReportDistinguishedName";

        private const string GroupDistinguishedName = "GroupDistinguishedName";
        private const string HelpFile = "ActiveDirectoryToolHelp.chm";

        private const string ManagerDistinguishedName =
            "ManagerDistinguishedName";

        private const string UserDistinguishedName = "UserDistinguishedName";

        private readonly MenuItem _computerGetGroupsMenuItem;
        private readonly MenuItem _computerGetSummaryMenuItem;
        private readonly MenuItem _containerGroupGetComputersMenuItem; //TODO
        private readonly MenuItem _containerGroupGetSummaryMenuItem; //TODO
        private readonly MenuItem _containerGroupGetUsersDirectReportsMenuItem; //TODO
        private readonly MenuItem _containerGroupGetUsersGroupsMenuItem; //TODO 
        private readonly MenuItem _containerGroupGetUsersMenuItem; //TODO
        private readonly MenuItem _directReportGetDirectReportsMenuItem;
        private readonly MenuItem _directReportGetGroupsMenuItem;
        private readonly MenuItem _directReportGetSummaryMenuItem;
        private readonly MenuItem _groupGetComputersMenuItem;
        private readonly MenuItem _groupGetSummaryMenuItem;
        private readonly MenuItem _groupGetUsersDirectReportsMenuItem;
        private readonly MenuItem _groupGetUsersGroupsMenuItem;
        private readonly MenuItem _groupGetUsersMenuItem;
        private readonly MenuItem _userGetDirectReportsMenuItem;
        private readonly MenuItem _userGetGroupsMenuItem;
        private readonly MenuItem _userGetSummaryMenuItem;

        private AboutWindow _aboutWindow;
        private Visibility _cancelButtonVisibility;
        private bool _computerSearchIsChecked;
        private List<MenuItem> _contextMenuItems;
        private Visibility _contextMenuVisibility;
        private DataView _data;
        private bool _groupSearchIsChecked;
        private string _messageContent;
        private Visibility _messageVisibility;
        private Visibility _progressBarVisibility;
        private ObservableCollection<Query> _queries;
        private string _searchText;
        private bool _userSearchIsChecked;
        private bool _viewIsEnabled;

        public ViewModel()
        {
            SetViewVariables();
            try
            {
                RootScope = new ScopeFetcher().Scope;
            }
            catch (PrincipalServerDownException)
            {
                ShowMessage(
                    "The Active Directory server could not be contacted.");
            }
            Queries = new ObservableCollection<Query>();
            _computerGetGroupsMenuItem = new MenuItem
            {
                Header = "Computer - Get Groups",
                Command = GetComputerGroupsCommand
            };
            _computerGetSummaryMenuItem = new MenuItem
            {
                Header = "Computer - Get Summary",
                Command = GetComputerSummaryCommand
            };

            _directReportGetDirectReportsMenuItem = new MenuItem
            {
                Header = "Direct Report - Get Direct Reports",
                Command = GetDirectReportDirectReportsCommand
            };
            _directReportGetGroupsMenuItem = new MenuItem
            {
                Header = "Direct Report - Get Groups",
                Command = GetDirectReportGroupsCommand
            };
            _directReportGetSummaryMenuItem = new MenuItem
            {
                Header = "Direct Report - Get Summary",
                Command = GetDirectReportSummaryCommand
            };
            _groupGetComputersMenuItem = new MenuItem
            {
                Header = "Group - Get Computers",
                Command = GetGroupComputersCommand
            };
            _groupGetUsersMenuItem = new MenuItem
            {
                Header = "Group - Get Users",
                Command = GetGroupUsersCommand
            };
            _groupGetUsersDirectReportsMenuItem = new MenuItem
            {
                Header = "Group - Get Users' Direct Reports",
                Command = GetGroupUsersDirectReportsCommand
            };
            _groupGetUsersGroupsMenuItem = new MenuItem
            {
                Header = "Group - Get Users' Groups",
                Command = GetGroupUsersGroupsCommand
            };
            _groupGetSummaryMenuItem = new MenuItem
            {
                Header = "Group - Get Summary",
                Command = GetGroupSummaryCommand
            };
            _userGetDirectReportsMenuItem = new MenuItem
            {
                Header = "User - Get Direct Reports",
                Command = GetUserDirectReportsCommand
            };
            _userGetGroupsMenuItem = new MenuItem
            {
                Header = "User - Get Groups",
                Command = GetUserGroupsCommand
            };
            _userGetSummaryMenuItem = new MenuItem
            {
                Header = "User - Get Summary",
                Command = GetUserSummaryCommand
            };
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public Visibility CancelButtonVisibility
        {
            get
            {
                return _cancelButtonVisibility;
            }
            set
            {
                _cancelButtonVisibility = value;
                NotifyPropertyChanged();
            }
        }

        public ICommand CancelCommand { get; private set; }

        public bool ComputerSearchIsChecked
        {
            get
            {
                return _computerSearchIsChecked;
            }
            set
            {
                _computerSearchIsChecked = value;
                NotifyPropertyChanged();
            }
        }

        public List<MenuItem> ContextMenuItems
        {
            get
            {
                return _contextMenuItems;
            }
            private set
            {
                _contextMenuItems = value;
                ContextMenuVisibility = _contextMenuItems.IsNullOrEmpty()
                    ? Visibility.Hidden
                    : Visibility.Visible;
                NotifyPropertyChanged();
            }
        }

        public Visibility ContextMenuVisibility
        {
            get
            {
                return _contextMenuVisibility;
            }
            set
            {
                _contextMenuVisibility = value;
                NotifyPropertyChanged();
            }
        }

        public Scope CurrentScope { get; set; }

        public DataView Data
        {
            get
            {
                return _data;
            }
            private set
            {
                _data = value;
                NotifyPropertyChanged();
            }
        }

        public ICommand GetOuComputersCommand { get; private set; }

        public ICommand GetOuGroupsCommand { get; private set; }

        public ICommand GetOuGroupsUsersCommand { get; private set; }

        public ICommand GetOuUsersCommand { get; private set; }

        public ICommand GetOuUsersDirectReportsCommand { get; private set; }

        public ICommand GetOuUsersGroupsCommand { get; private set; }

        public bool GroupSearchIsChecked
        {
            get
            {
                return _groupSearchIsChecked;
            }
            set
            {
                _groupSearchIsChecked = value;
                NotifyPropertyChanged();
            }
        }

        public string MessageContent
        {
            get
            {
                return _messageContent;
            }
            set
            {
                _messageContent = value;
                NotifyPropertyChanged();
            }
        }

        public Visibility MessageVisibility
        {
            get
            {
                return _messageVisibility;
            }
            set
            {
                _messageVisibility = value;
                NotifyPropertyChanged();
            }
        }

        public ICommand OpenAboutWindowCommand { get; private set; }
        public ICommand OpenHelpWindowCommand { get; private set; }
        public ICommand PreviousQueryCommand { get; private set; }

        public Visibility ProgressBarVisibility
        {
            get
            {
                return _progressBarVisibility;
            }
            set
            {
                _progressBarVisibility = value;
                NotifyPropertyChanged();
            }
        }

        public ObservableCollection<Query> Queries
        {
            get
            {
                return _queries;
            }
            set
            {
                _queries = value;
                NotifyPropertyChanged();
            }
        }

        public Scope RootScope { get; }
        public ICommand SearchCommand { get; private set; }
        public ICommand SearchOuCommand { get; private set; }

        public string SearchText
        {
            get
            {
                return _searchText;
            }
            set
            {
                _searchText = value;
                NotifyPropertyChanged();
            }
        }

        public ICommand SelectionChangedCommand { get; private set; }

        public bool UserSearchIsChecked
        {
            get
            {
                return _userSearchIsChecked;
            }
            set
            {
                _userSearchIsChecked = value;
                NotifyPropertyChanged();
            }
        }

        public string Version { get; private set; }

        public bool ViewIsEnabled
        {
            get
            {
                return _viewIsEnabled;
            }
            private set
            {
                _viewIsEnabled = value;
                NotifyPropertyChanged();
            }
        }

        public ICommand WriteToFileCommand { get; private set; }
        private ICommand GetComputerGroupsCommand { get; set; }
        private ICommand GetComputerSummaryCommand { get; set; }
        private ICommand GetContainerGroupComputersCommand { get; set; } //TODO
        private ICommand GetContainerGroupSummaryCommand { get; set; } //TODO
        private ICommand GetContainerGroupUsersCommand { get; set; } //TODO
        private ICommand GetContainerGroupUsersDirectReportsCommand { get; set; } //TODO
        private ICommand GetContainerGroupUsersGroupsCommand { get; set; } //TODO
        private ICommand GetDirectReportDirectReportsCommand { get; set; }
        private ICommand GetDirectReportGroupsCommand { get; set; }
        private ICommand GetDirectReportSummaryCommand { get; set; }
        private ICommand GetGroupComputersCommand { get; set; }
        private ICommand GetGroupSummaryCommand { get; set; }
        private ICommand GetGroupUsersCommand { get; set; }
        private ICommand GetGroupUsersDirectReportsCommand { get; set; }
        private ICommand GetGroupUsersGroupsCommand { get; set; }
        private ICommand GetUserDirectReportsCommand { get; set; }
        private ICommand GetUserGroupsCommand { get; set; }
        private ICommand GetUserSummaryCommand { get; set; }
        private IList SelectedDataRowViews { get; set; }

        private static string GetComputerDistinguishedName(
            DataRowView dataRowView)
        {
            return dataRowView[ComputerDistinguishedName].ToString();
        }

        private static string GetDirectReportDistinguishedName(
            DataRowView dataRowView)
        {
            return dataRowView[DirectReportDistinguishedName].ToString();
        }

        private static string GetGroupDistinguishedName(
            DataRowView dataRowView)
        {
            return dataRowView[GroupDistinguishedName].ToString();
        }

        private static string GetUserDistinguishedName(DataRowView dataRowView)
        {
            return dataRowView[UserDistinguishedName].ToString();
        }

        private static void OpenHelpWindowCommandExecute() => Process.Start(
            HelpFile);

        private void CancelCommandExecute()
        {
            Queries.Peek()?.Cancel();
            ResetQuery();
        }

        private void FinishTask()
        {
            ProgressBarVisibility = Visibility.Hidden;
            CancelButtonVisibility = Visibility.Hidden;
            ViewIsEnabled = true;
        }

        private List<MenuItem> GenerateContextMenuItems()
        {
            var contextMenuItems = new List<MenuItem>();
            if (Queries.Peek() == null) return null;
            if (Data.Table.Columns.Contains(ComputerDistinguishedName))
            {
                contextMenuItems.AddRange(GenerateComputerContextMenuItems());
            }
            if (Data.Table.Columns.Contains(ContainerGroupDistinguishedName))
            {
                // TODO
            }
            if (Data.Table.Columns.Contains(DirectReportDistinguishedName))
            {
                contextMenuItems.Add(_directReportGetDirectReportsMenuItem);
                contextMenuItems.Add(_directReportGetGroupsMenuItem);
                contextMenuItems.Add(_directReportGetSummaryMenuItem);
            }
            if (Data.Table.Columns.Contains(GroupDistinguishedName))
            {
                contextMenuItems.AddRange(GenerateGroupContextMenuItems());
            }
            if (Data.Table.Columns.Contains(ManagerDistinguishedName))
            {
                // TODO
            }
            if (!Data.Table.Columns.Contains(UserDistinguishedName))
                return contextMenuItems;
            contextMenuItems.AddRange(GenerateUserContextMenuItems());
            return contextMenuItems;
        }

        private IEnumerable<MenuItem> GenerateComputerContextMenuItems()
        {
            var computerContextMenuItems = new List<MenuItem>();
            var queryType = Queries.Peek().QueryType;
            if (queryType != ComputersGroups)
                computerContextMenuItems.Add(_computerGetGroupsMenuItem);
            if (queryType != ComputersSummaries)
                computerContextMenuItems.Add(_computerGetSummaryMenuItem);
            return computerContextMenuItems;
        }

        private IEnumerable<MenuItem> GenerateGroupContextMenuItems()
        {
            var groupContextMenuItems = new List<MenuItem>();
            var queryType = Queries.Peek().QueryType;
            if (queryType != GroupsComputers)
                groupContextMenuItems.Add(_groupGetComputersMenuItem);
            if (queryType != GroupsSummaries)
                groupContextMenuItems.Add(_groupGetSummaryMenuItem);
            if (queryType != GroupsUsers)
                groupContextMenuItems.Add(_groupGetUsersMenuItem);
            if (queryType != GroupsUsersDirectReports)
                groupContextMenuItems.Add(_groupGetUsersDirectReportsMenuItem);
            if (queryType != GroupsUsersGroups)
                groupContextMenuItems.Add(_groupGetUsersGroupsMenuItem);
            return groupContextMenuItems;
        }

        private IEnumerable<MenuItem> GenerateUserContextMenuItems()
        {
            var userContextMenuItems = new List<MenuItem>();
            var queryType = Queries.Peek().QueryType;
            if (queryType != DirectReportsDirectReports &&
                queryType != UsersDirectReports)
                userContextMenuItems.Add(_userGetDirectReportsMenuItem);
            if (queryType != DirectReportsGroups && queryType != UsersGroups)
                userContextMenuItems.Add(_userGetGroupsMenuItem);
            if (queryType != DirectReportsSummaries && 
                queryType != UsersSummaries)
                userContextMenuItems.Add(_userGetSummaryMenuItem);
            return userContextMenuItems;
        }

        private IEnumerable<string> GetComputersDistinguishedNames()
        {
            return (from DataRowView dataRowView in SelectedDataRowViews
                select GetComputerDistinguishedName(dataRowView)).ToList();
        }

        private async void GetContextComputerGroupsCommandExecute()
        {
            await RunQuery(
                ComputersGroups,
                CurrentScope,
                GetComputersDistinguishedNames());
        }

        private async void GetContextComputerSummaryCommandExecute()
        {
            await RunQuery(
                ComputersSummaries,
                CurrentScope,
                GetComputersDistinguishedNames());
        }

        private async void GetContextDirectReportDirectReportsCommandExecute()
        {
            await RunQuery(
                DirectReportsDirectReports,
                CurrentScope,
                GetGroupsDistinguishedNames());
        }

        private async void GetContextDirectReportGroupsCommandExecute()
        {
            await RunQuery(
                DirectReportsGroups,
                CurrentScope,
                GetDirectReportsDistinguishedNames());
        }

        private async void GetContextDirectReportSummaryCommandExecute()
        {
            await RunQuery(
                DirectReportsSummaries,
                CurrentScope,
                GetDirectReportsDistinguishedNames());
        }

        private async void GetContextGroupComputersCommandExecute()
        {
            await RunQuery(
                GroupsComputers,
                CurrentScope,
                GetGroupsDistinguishedNames());
        }

        private async void GetContextGroupSummaryCommandExecute()
        {
            await RunQuery(
                GroupsSummaries,
                CurrentScope,
                GetGroupsDistinguishedNames());
        }

        private async void GetContextGroupUsersCommandExecute()
        {
            await RunQuery(
                GroupsUsers,
                CurrentScope,
                GetGroupsDistinguishedNames());
        }

        private async void GetContextGroupUsersDirectReportsCommandExecute()
        {
            await RunQuery(
                GroupsUsersDirectReports,
                CurrentScope,
                GetGroupsDistinguishedNames());
        }

        private async void GetContextGroupUsersGroupsCommandExecute()
        {
            await RunQuery(
                GroupsUsersGroups,
                CurrentScope,
                GetGroupsDistinguishedNames());
        }

        private async void GetContextUserDirectReportsCommandExecute()
        {
            await RunQuery(
                UsersDirectReports,
                CurrentScope,
                GetUsersDistinguishedNames());
        }

        private async void GetContextUserGroupsCommandExecute()
        {
            await RunQuery(
                UsersGroups,
                CurrentScope,
                GetUsersDistinguishedNames());
        }

        private async void GetContextUserSummaryCommandExecute()
        {
            await RunQuery(
                UsersSummaries,
                CurrentScope,
                GetUsersDistinguishedNames());
        }

        private IEnumerable<string> GetDirectReportsDistinguishedNames()
        {
            return (from DataRowView dataRowView in SelectedDataRowViews
                select GetDirectReportDistinguishedName(dataRowView)).ToList();
        }

        private IEnumerable<string> GetGroupsDistinguishedNames()
        {
            return (from DataRowView dataRowView in SelectedDataRowViews
                select GetGroupDistinguishedName(dataRowView)).ToList();
        }

        private async void GetOuComputersCommandExecute()
        {
            await RunQuery(OuComputers, CurrentScope);
        }

        private async void GetOuGroupsCommandExecute()
        {
            await RunQuery(OuGroups, CurrentScope);
        }

        private async void GetOuGroupsUsersCommandExecute()
        {
            await RunQuery(OuGroupsUsers, CurrentScope);
        }

        private async void GetOuUsersCommandExecute()
        {
            await RunQuery(OuUsers, CurrentScope);
        }

        private async void GetOuUsersDirectReportsCommandExecute()
        {
            await RunQuery(OuUsersDirectReports, CurrentScope);
        }

        private async void GetOuUsersGroupsCommandExecute()
        {
            await RunQuery(OuUsersGroups, CurrentScope);
        }

        private QueryType GetSearchQueryType()
        {
            if (ComputerSearchIsChecked)
                return SearchComputer;
            if (GroupSearchIsChecked)
                return SearchGroup;
            return UserSearchIsChecked ? SearchUser : None;
        }

        private IEnumerable<string> GetUsersDistinguishedNames()
        {
            return (from DataRowView dataRowView in SelectedDataRowViews
                select GetUserDistinguishedName(dataRowView)).ToList();
        }

        private void HideMessage()
        {
            MessageContent = string.Empty;
            MessageVisibility = Visibility.Hidden;
        }

        private void NotifyPropertyChanged(
            [CallerMemberName] string propertyName = "")
        {
            PropertyChanged?.Invoke(
                this,
                new PropertyChangedEventArgs(propertyName));
        }

        private void OpenAboutWindowCommandExecute()
        {
            if (_aboutWindow == null || !_aboutWindow.IsVisible)
            {
                _aboutWindow = new AboutWindow();
                _aboutWindow.Show();
            }
            else
                _aboutWindow.Activate();
        }

        private bool OuCommandCanExecute()
        {
            return CurrentScope != null;
        }

        private bool PreviousQueryCommandCanExecute()
        {
            return Queries.Multiple();
        }

        private async void PreviousQueryCommandExecute()
        {
            Queries.Pop();
            await RunQuery(Queries.Pop());
        }

        private void ResetQuery()
        {
            if (Queries.Any())
                Queries.Pop();
        }

        private async Task RunQuery(
            QueryType queryType,
            Scope scope = null,
            IEnumerable<string> selectedItemDistinguishedNames = null,
            string searchText = null)
        {
            await RunQuery(
                new Query(
                    queryType,
                    scope,
                    selectedItemDistinguishedNames,
                    searchText));
        }

        private async Task RunQuery(Query query)
        {
            StartTask();
            try
            {
                Queries.Push(query);
                await Queries.Peek().Execute();
                Data = Queries.Peek().Data.ToDataTable().AsDataView();
                ContextMenuItems = GenerateContextMenuItems();
            }
            catch (NullReferenceException e)
            {
                ShowMessage(e.Message);
                ResetQuery();
            }
            catch (OperationCanceledException)
            {
                ShowMessage("Operation was cancelled.");
                ResetQuery();
            }
            catch (ArgumentNullException)
            {
                ShowMessage(
                    "No results of desired type found in selected context.");
                ResetQuery();
            }
            catch (OutOfMemoryException)
            {
                ShowMessage("The selected query is too large to run.");
                ResetQuery();
            }
            FinishTask();
        }

        private bool SearchCommandCanExecute()
        {
            return !SearchText.IsNullOrWhiteSpace() && SearchTypeIsChecked();
        }

        private async void SearchCommandExecute()
        {
            await RunQuery(GetSearchQueryType(), searchText: SearchText);
        }

        private bool SearchOuCommandCanExecute()
        {
            return OuCommandCanExecute() && SearchCommandCanExecute();
        }

        private async void SearchOuCommandExecute()
        {
            await RunQuery(
                GetSearchQueryType(), CurrentScope, searchText: SearchText);
        }

        private bool SearchTypeIsChecked()
        {
            return ComputerSearchIsChecked ||
                   GroupSearchIsChecked ||
                   UserSearchIsChecked;
        }

        private void SetUpCommands()
        {
            GetOuComputersCommand = new RelayCommand(
                GetOuComputersCommandExecute, OuCommandCanExecute);

            GetOuGroupsCommand = new RelayCommand(
                GetOuGroupsCommandExecute, OuCommandCanExecute);

            GetOuUsersCommand = new RelayCommand(
                GetOuUsersCommandExecute, OuCommandCanExecute);

            GetOuUsersDirectReportsCommand = new RelayCommand(
                GetOuUsersDirectReportsCommandExecute, OuCommandCanExecute);

            GetOuUsersGroupsCommand = new RelayCommand(
                GetOuUsersGroupsCommandExecute, OuCommandCanExecute);

            WriteToFileCommand = new RelayCommand(
                WriteToFileCommandExecute, WriteToFileCommandCanExecute);

            OpenAboutWindowCommand = new RelayCommand(
                OpenAboutWindowCommandExecute);

            OpenHelpWindowCommand = new RelayCommand(
                OpenHelpWindowCommandExecute);

            GetUserGroupsCommand = new RelayCommand(
                GetContextUserGroupsCommandExecute);

            GetComputerGroupsCommand = new RelayCommand(
                GetContextComputerGroupsCommandExecute);

            GetDirectReportDirectReportsCommand = new RelayCommand(
                GetContextDirectReportDirectReportsCommandExecute);

            GetDirectReportGroupsCommand = new RelayCommand(
                GetContextDirectReportGroupsCommandExecute);

            GetGroupComputersCommand = new RelayCommand(
                GetContextGroupComputersCommandExecute);

            GetGroupUsersCommand = new RelayCommand(
                GetContextGroupUsersCommandExecute);

            GetGroupUsersDirectReportsCommand = new RelayCommand(
                GetContextGroupUsersDirectReportsCommandExecute);

            GetUserDirectReportsCommand = new RelayCommand(
                GetContextUserDirectReportsCommandExecute);

            GetGroupUsersGroupsCommand = new RelayCommand(
                GetContextGroupUsersGroupsCommandExecute);

            GetDirectReportSummaryCommand = new RelayCommand(
                GetContextDirectReportSummaryCommandExecute);

            GetUserSummaryCommand = new RelayCommand(
                GetContextUserSummaryCommandExecute);

            GetComputerSummaryCommand = new RelayCommand(
                GetContextComputerSummaryCommandExecute);

            GetGroupSummaryCommand = new RelayCommand(
                GetContextGroupSummaryCommandExecute);

            CancelCommand = new RelayCommand(CancelCommandExecute);

            PreviousQueryCommand = new RelayCommand(
                PreviousQueryCommandExecute, PreviousQueryCommandCanExecute);

            SelectionChangedCommand =
                new RelayCommand<IList>(
                    items =>
                    {
                        SelectedDataRowViews = items;
                    });

            GetOuGroupsUsersCommand = new RelayCommand(
                GetOuGroupsUsersCommandExecute, OuCommandCanExecute);

            SearchCommand = new RelayCommand(
                SearchCommandExecute, SearchCommandCanExecute);

            SearchOuCommand = new RelayCommand(
                SearchOuCommandExecute, SearchOuCommandCanExecute);
        }

        private void SetViewVariables()
        {
            ProgressBarVisibility = Visibility.Hidden;
            MessageVisibility = Visibility.Hidden;
            CancelButtonVisibility = Visibility.Hidden;
            ViewIsEnabled = true;
            ContextMenuItems = null;
            try
            {
                Version = CurrentDeployment.CurrentVersion.ToString();
            }
            catch (InvalidDeploymentException)
            {
                Version = "Not Installed";
            }
            SetUpCommands();
        }

        private void ShowMessage(string messageContent)
        {
            MessageContent = messageContent + "\n\nDouble-click to dismiss.";
            MessageVisibility = Visibility.Visible;
        }

        private void StartTask()
        {
            HideMessage();
            ViewIsEnabled = false;
            ProgressBarVisibility = Visibility.Visible;
            CancelButtonVisibility = Visibility.Visible;
        }

        private bool WriteToFileCommandCanExecute()
        {
            return Data != null && Data.Count > 0;
        }

        private async void WriteToFileCommandExecute()
        {
            StartTask();
            await Task.Run(
                () =>
                {
                    var fileWriter = new DataFileWriter
                    {
                        Data = Data,
                        Scope = Queries.First().Scope,
                        QueryType = Queries.First().QueryType
                    };
                    ShowMessage("Wrote data to:\n" + fileWriter.WriteToCsv());
                });
            FinishTask();
        }
    }
}