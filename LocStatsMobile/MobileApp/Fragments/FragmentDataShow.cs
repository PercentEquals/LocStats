using Android.OS;
using Android.Views;
using Android.Widget;
using System;
using MobileApp.Managers;
using System.Collections.Generic;
using Android.Util;
using Microcharts;
using Microcharts.Droid;
using SkiaSharp;

namespace MobileApp.Fragments
{
    public class FragmentDataShow : AndroidX.Fragment.App.Fragment
    {
        
        private DateTime selectedDateFrom = DateTime.Now.AddDays(-7);
        private DateTime selectedDateTo = DateTime.Now;

        private Button selectedDateToBtn;
        private Button selectedDateFromBtn;

        private ChartView distanceChart;
        private ChartView timeChart;

        private Action<string, string> infoBoxCallback;

        public FragmentDataShow(Action<string, string> infoBoxCallback)
        {
            this.infoBoxCallback = infoBoxCallback;
        }

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            return inflater.Inflate(Resource.Layout.activity_data_show, container, false);
        }

        public override void OnStart()
        {
            base.OnStart();

            selectedDateFromBtn = View.FindViewById<Button>(Resource.Id.buttonSelectDateFrom);
            selectedDateFromBtn.Click += _buttonClickSelectDateFrom;
            selectedDateToBtn = View.FindViewById<Button>(Resource.Id.buttonSelectDateTo);
            selectedDateToBtn.Click += _buttonClickSelectDateTo;

            selectedDateFromBtn.Text = selectedDateFrom.ToString("dd'-'MM'-'yyyy");
            selectedDateToBtn.Text = selectedDateTo.ToString("dd'-'MM'-'yyyy");

            distanceChart = View.FindViewById<ChartView>(Resource.Id.chartViewDistance);
            timeChart = View.FindViewById<ChartView>(Resource.Id.chartViewTime);

            _loadStats();
        }

        private void _showStats(ref ChartView chartView, Dictionary<string, double> values)
        {
            List<ChartEntry> chartEntries = new List<ChartEntry>();

            
            foreach (var element in values)
            {
                
                chartEntries.Add(new ChartEntry((float)element.Value)
                {
                    Label = element.Key,
                    ValueLabel = element.Value.ToString(),
                    Color = SKColor.Parse("#25a9ba")
                });
            }


            chartView.Chart = new BarChart
            {
                Entries = chartEntries.ToArray(),
                LabelTextSize = 40,
                LabelOrientation = Microcharts.Orientation.Vertical,
                ValueLabelOrientation = Microcharts.Orientation.Vertical,
                Margin = 30,
                BackgroundColor = SKColor.Empty
            };

            
        }

        private void _buttonClickSelectDateFrom(object sender, EventArgs e)
        {
            new DatePickerFragment(delegate (DateTime time) 
            {
                selectedDateFrom = time;
                selectedDateFromBtn.Text = selectedDateFrom.ToString("dd'-'MM'-'yyyy");

               
                _loadStats();

            }, selectedDateFrom)
          .Show(FragmentManager, DatePickerFragment.TAG);
        }

        private async void _loadStats()
        {

            if (selectedDateFrom <= selectedDateTo && ((selectedDateTo - selectedDateFrom).TotalDays <= 14))
            {
                selectedDateFromBtn.Enabled = false;
                selectedDateToBtn.Enabled = false;


                var resultDistance = await ConnectionManager.GetDistanceStats(selectedDateFrom, selectedDateTo);

                Log.Info("DistanceResult", "Getting time res");
                if (resultDistance.success)
                {
                    Log.Info("DistanceResult", "Show stats");
                    foreach (var v in resultDistance.values)
                    {
                        Log.Info("DistanceResult", v.ToString());
                    }
                    _showStats(ref distanceChart, resultDistance.values);
                }
                else
                {
                    infoBoxCallback("Błąd", resultDistance.errors);
                }

                var resultTime = await ConnectionManager.GetTimeStats(selectedDateFrom, selectedDateTo);

                Log.Info("TimeResult", "Getting time res");
                if (resultTime.success)
                {
                    Log.Info("TimeResult", "Show stats");
                    foreach (var v in resultTime.values)
                    {
                        Log.Info("TimeResult", v.ToString());
                    }
                    _showStats(ref timeChart, resultTime.values);
                }
                else
                {
                    infoBoxCallback("Error", resultTime.errors);
                }

                selectedDateFromBtn.Enabled = true;
                selectedDateToBtn.Enabled = true;
            }
            else
            {
                infoBoxCallback("Error", "Date range can contain max 14 days");
                
            }
        }


        private void _buttonClickSelectDateTo(object sender, EventArgs e)
        {
            new DatePickerFragment(delegate (DateTime time)
            {
                selectedDateTo = time;
                selectedDateToBtn.Text = selectedDateTo.ToString("dd'-'MM'-'yyyy");

                
                _loadStats();

            }, selectedDateTo)
          .Show(FragmentManager, DatePickerFragment.TAG);
        }
    }
}