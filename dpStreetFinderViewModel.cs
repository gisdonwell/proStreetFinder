using ActiproSoftware.Windows.Input;
using ArcGIS.Core.CIM;
using ArcGIS.Core.Data;
using ArcGIS.Core.Geometry;
using ArcGIS.Desktop.Catalog;
using ArcGIS.Desktop.Core;
using ArcGIS.Desktop.Editing;
using ArcGIS.Desktop.Extensions;
using ArcGIS.Desktop.Framework;
using ArcGIS.Desktop.Framework.Contracts;
using ArcGIS.Desktop.Framework.Dialogs;
using ArcGIS.Desktop.Framework.Threading.Tasks;
using ArcGIS.Desktop.Layouts;
using ArcGIS.Desktop.Mapping;
using ArcGIS.Desktop.Mapping.Events;
using Oracle.ManagedDataAccess.Client;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;

namespace proStreetFinder
{
    internal class dpStreetFinderViewModel : DockPane
    {
        #region
        OracleConnection con = null;
        string conStr = "Data Source=(DESCRIPTION =" +
            "(ADDRESS = (PROTOCOL = TCP)(HOST = localhost)(PORT = 1521))" +
            "(CONNECT_DATA =" +
            "(SERVER = DEDICATED)" +
            "(SERVICE_NAME = donwellpdb)));" +
            "User Id= proddba;Password=;";
        //string conStr = "Data Source=(DESCRIPTION =" +
        //    "(ADDRESS = (PROTOCOL = TCP)(HOST = CMHORA-3)(PORT = 1521))" +
        //    "(CONNECT_DATA =" +
        //    "(SERVER = DEDICATED)" +
        //    "(SERVICE_NAME = gis.city.medicine-hat.ab.ca)));" +
        //    "User Id= testmap;Password=;";
        #endregion
        private const string _dockPaneID = "proStreetFinder_dpStreetFinder";
        private ObservableCollection<FeatureLayer> _featureLayers = new ObservableCollection<FeatureLayer>();
        private object _lockSelectedList = new object();
        private readonly object _lockLayer = new object();
        protected dpStreetFinderViewModel() 
        {
            this.setConnection();
            BindingOperations.EnableCollectionSynchronization(_selectedList, _lockSelectedList);
            ActiveMapViewChangedEvent.Subscribe(OnActiveMapViewChanged);
            updateCombox();

        }
        private void OnActiveMapViewChanged(ActiveMapViewChangedEventArgs args)
        {
            //if (_this == null) return;
            //var active = false;

            if (args.IncomingView == null) return;

            //else if(args.IncomingView != null)
            GetFeatureLayer();

            //await GetFeatureLayer(args == null ? MapView.Active : args.IncomingView);
            // TODO: BusyVisibility for WaitingCursor Control  
            //BusyVisibility = System.Windows.Visibility.Collapsed;

            //GetFeatureLayer();
        }
        private ObservableCollection<string> _selectedList = new ObservableCollection<string>();

        public ObservableCollection<string> SelectedList
        {
            get { return _selectedList; }
            set
            {
                SetProperty(ref _selectedList, value, () => SelectedList);
                //updateCombox();
            }
        }
        private bool _isRdoCMHChecked = true;
        public bool IsRdoCMHChecked
        {
            get { return _isRdoCMHChecked; }
            set
            {
                SetProperty(ref _isRdoCMHChecked, value, () => IsRdoCMHChecked);
                //SetProperty(ref _isRdoCMHChecked, value); //, () => _isRdoCMHChecked);
                //p_ButtonAIsChecked = value;
                System.Windows.MessageBox.Show(string.Format("Button A is checked: {0}", value));
                //if (_isRdoOtherChecked==true)
                //updateCombox();
            }
        }
        private bool _isRdoOtherChecked;

        /// <summary>
        /// Summary
        /// </summary>
        public bool IsRdoOtherChecked
        {
            get { return _isRdoOtherChecked; }
            set
            {
                SetProperty(ref _isRdoOtherChecked, value, () => IsRdoOtherChecked);
                //SetProperty(ref _isRdoOtherChecked, value); //, () => _isRdoOtherChecked);
                //p_ButtonBIsChecked = value;
                System.Windows.MessageBox.Show(string.Format("Button B is checked: {0}", value));
                //updateCombox();
                //if(_isRdoCMHChecked==true)
                //    this.updateCombox();
            }
        }
        private async void updateCombox()
        {
            // TODO: 2. enable _workorders for updates from background threads
            lock (_lockSelectedList)
            {
                _selectedList.Clear();
            }
            //var resultList = new ObservableCollection<string>();
            //var resultTable = new DataTable();
            String strSQL = "";
            await QueuedTask.Run(() => {

                if (_isRdoCMHChecked == true)
                {
                    //strSQL = "select distinct name, type, direction from PRODDBA.COR_STREETLINES where city='M' and name <> ' ' and name is not null order by name, type, direction";
                    strSQL = "select distinct address from PRODDBA.COR_STREETLINES where city='M' and address <> ' ' and address is not null order by address";
                    //strSQL = "select distinct PARCELID from PRODDBA.TAXALTALISLUDROLL_VIEW where PARCELID is not null order by PARCELID";
                }
                if (_isRdoOtherChecked == true)
                {
                    //strSQL = "select distinct name, type, direction from PRODDBA.COR_STREETLINES where city <>'M' and name <> ' ' and name is not null order by name, type, direction";
                    strSQL = "select distinct address from PRODDBA.COR_STREETLINES where city <>'M' and address <> ' ' and address is not null order by address";
                    //strSQL = "select distinct LINC from PRODDBA.TAXALTALISLUDROLL_VIEW where LINC is not null and LINC <> ' ' order by LINC";
                }

                if (con.State == ConnectionState.Closed)
                {
                    con.Open();
                }

                OracleCommand cmd = con.CreateCommand();
                cmd.CommandText = strSQL;
                cmd.CommandType = System.Data.CommandType.Text;

                OracleDataReader dr = cmd.ExecuteReader();
                //resultList.Clear();
                //resultTable.Columns.Add("address", typeof(string));
                //DataRow dataR = resultTable.NewRow();
                //String strStreetName = "";
                //String strStreetAddress = "";
                while (dr.Read())
                {
                    lock (_lockSelectedList)
                        //address = (dr.GetValue(0)).ToString();
                        //resultList.Add((dr.GetValue(0)).ToString());
                        _selectedList.Add((dr.GetValue(0)).ToString());

                }
                //_selectedList = resultList;
                //SelectedList = resultList;
                SelectedList = _selectedList;
                //DataTable dt = new DataTable();
                //dt.Load(dr);
                //myDataGrid.ItemsSource = dt.DefaultView;
                dr.Close();


            });
        }

        private void setConnection()
        {
            con= new OracleConnection(conStr);
            try
            {
                con.Open();
            }
            catch(Exception exp)
            { }
        }
        private DelegateCommand<object> _checkedCMHChangedCommand;
        public DelegateCommand<object> CheckedCMHChangedCommand => (_checkedCMHChangedCommand = new DelegateCommand<object>(CheckCMHChanged));

        private void CheckCMHChanged(object obj)
        {
            var chekbox = obj as System.Windows.Controls.RadioButton;
            //MessageBox.Show($"RadioCMH is checked : { chekbox.IsChecked}");
            _isRdoCMHChecked = true;
            _isRdoOtherChecked = false;
            updateCombox();

        }

        private DelegateCommand<object> _checkedOtherChangedCommand;
        public DelegateCommand<object> CheckedOtherChangedCommand => (_checkedOtherChangedCommand = new DelegateCommand<object>(CheckOtherChanged));
        private void CheckOtherChanged(object obj)
        {
            var chekbox = obj as System.Windows.Controls.RadioButton;
            //MessageBox.Show($"RadioOther is checked : { chekbox.IsChecked}");
            _isRdoOtherChecked = true;
            _isRdoCMHChecked = false;
            updateCombox();

        }
        private DelegateCommand<object> _findStreetCommand;
        public DelegateCommand<object> FindStreetCommand => (_findStreetCommand = new DelegateCommand<object>(FindStreet));
        private void FindStreet(object obj)
        {
            var findbutton = obj as System.Windows.Controls.Button;
            //OnActiveMapViewChanged(null); //test if after user adds street layer
            // if the map is not available.
            var mapView = MapView.Active;
            if (mapView == null)
            {
                System.Windows.MessageBox.Show("Add or open a map which PRODDBA.cor_Streetlines is on, close Street Finder tool and relaunch the tool!", "Please take the following actions");
                return;
            }
            // if the map is available, but street layer is not available. It should grab GetFeatureLayer by Linq to see if there is a Streetlines on the map. Then trigger the message if no Streetlines.
            else if (featureLayer == null)
            {
                GetFeatureLayer();
                //System.Windows.MessageBox.Show("Add PRODDBA.cor_Streetlines on the map, close Street Finder tool and relaunch the tool!", "Please take the following actions");
                //return;
            }

            selectAttribute();

        }

        private async void selectAttribute()
        {
            try
            {

                //if (featureLayer.Name.ToUpper() != "PRODDBA.COR_STREETLINES")
                //if (featureLayer ==null)
                //{
                //    System.Windows.MessageBox.Show("Add PRODDBA.cor_Streetlines on Map, close Street Finder tool and relaunch the tool!", "Please take the following actions");
                //    //var activeMapView = MapView.Active;
                //    //GetFeatureLayer();
                //    //await GetFeatureLayer(activeMapView); // GetFeatureLyrStreet();
                //    //throw new InvalidOperationException("There is not PRODDBA.cor_Streetlines on Map!");
                //    return;
                //}

                string strName;
                //strName = comStreet.SelectedItem.ToString().ToUpper();
                strName = _selectedStreet.ToString().ToUpper();
                string strCityWhere;
                //if (this.rdoMedicineHat.IsChecked == true)
                if (_isRdoCMHChecked == true)
                    strCityWhere = "CITY = 'M' AND ";
                    //strCityWhere = "PARCELID =  ";
                else
                    strCityWhere = "CITY <> 'M' AND ";
                    //strCityWhere = "LINC = ";

                await QueuedTask.Run(() =>
                {
                    QueryFilter queryFilter = new QueryFilter
                    {
                        //WhereClause = "OBJECTID = 20"
                        WhereClause = strCityWhere + "Address = '" + strName + "'"
                        //WhereClause = strCityWhere + strName
                    };

                    featureLayer.Select(queryFilter);
                    //MapView.Active.ZoomInFixedAsync(new TimeSpan(0, 0, 3));
                    MapView.Active.ZoomToSelectedAsync(TimeSpan.FromSeconds(2));
                    //MapView.Active.ZoomToSelected();
                });
                //MapView.Active.ZoomToSelected();
            }
            catch (Exception e)
            {
                System.Windows.MessageBox.Show("Add PRODDBA.cor_Streetlines on Map, close Street Finder tool and relaunch the tool!", "Please take the following actions");
            }
        }
        private FeatureLayer GetFeatureLyrStreet()
        {
            featureLayer = MapView.Active.Map.GetLayersAsFlattenedList().OfType<FeatureLayer>().Where(fl => fl.Name.Contains("PRODDBA.cor_Streetlines")).FirstOrDefault();
            //featureLayer = MapView.Active.Map.GetLayersAsFlattenedList().OfType<FeatureLayer>().Where(fl => fl.Name.Contains("PRODDBA.TAXALTALISLUDROLL_VIEW")).FirstOrDefault();
            if (featureLayer.Name is null)
            {
                throw new InvalidOperationException("There is not PRODDBA.cor_Streetlines on Map!");
            }
            return featureLayer;
        }
        private string _selectedStreet;
        public string SelectedStreet
        {
            get { return _selectedStreet; }
            set
            {
                SetProperty(ref _selectedStreet, value, () => SelectedStreet);

            }
        }
        private void GetFeatureLayer()
        {
            //Get the active map view.
            var mapView = MapView.Active;
            if (mapView == null)
            {
                //System.Windows.MessageBox.Show("Please add or open a map which PRODDBA.cor_Streetlines layer is on", "Error in GetFeatureLayer", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            lock (_lockLayer)
            {
                if (featureLayer != null)
                    featureLayer = null;
                featureLayer = MapView.Active.Map.GetLayersAsFlattenedList().OfType<FeatureLayer>().Where(fl => fl.Name.Contains("PRODDBA.cor_Streetlines")).FirstOrDefault();
                //featureLayer = MapView.Active.Map.GetLayersAsFlattenedList().OfType<FeatureLayer>().Where(fl => fl.Name.Contains("PRODDBA.TAXALTALISLUDROLL_VIEW")).FirstOrDefault();
            }
            NotifyPropertyChanged(() => FeatureLayer);
        }
        private FeatureLayer featureLayer; // field

        public FeatureLayer FeatureLayer   // property
        {
            get { return featureLayer; }   // get method
            set
            {
                SetProperty(ref featureLayer, value, () => FeatureLayer);
                OnActiveMapViewChanged(null);
            }  // set method
        }

        /// <summary>
        /// Show the DockPane.
        /// </summary>
        internal static void Show()
        {
            DockPane pane = FrameworkApplication.DockPaneManager.Find(_dockPaneID);
            if (pane == null)
                return;

            pane.Activate();
        }

        /// <summary>
        /// Text shown near the top of the DockPane.
        /// </summary>
        private string _heading = "Please select a street name.";
        public string Heading
        {
            get { return _heading; }
            set
            {
                SetProperty(ref _heading, value, () => Heading);
            }
        }
    }

    /// <summary>
    /// Button implementation to show the DockPane.
    /// </summary>
    internal class dpStreetFinder_ShowButton : Button
    {
        protected override void OnClick()
        {
            dpStreetFinderViewModel.Show();
        }
    }
}
